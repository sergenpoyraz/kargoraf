# App icon from master logo - preserves real alpha or removes checkerboard globally
Add-Type -AssemblyName System.Drawing

$source = Join-Path $PSScriptRoot "LogoSource.png"
$pngPath = Join-Path $PSScriptRoot "AppIcon.png"

if (-not (Test-Path $source)) {
    throw "LogoSource.png bulunamadi: $source"
}

function Test-BackgroundColor([int]$r, [int]$g, [int]$b) {
    $max = [Math]::Max($r, [Math]::Max($g, $b))
    $min = [Math]::Min($r, [Math]::Min($g, $b))
    return ($max -ge 205) -and (($max - $min) -le 40)
}

function Test-SourceHasAlpha([System.Drawing.Bitmap]$bitmap) {
    if ($bitmap.PixelFormat -notmatch 'Argb') {
        return $false
    }

    $samples = @(
        @(0, 0), @(($bitmap.Width - 1), 0), @(0, ($bitmap.Height - 1)),
        @(($bitmap.Width - 1), ($bitmap.Height - 1))
    )

    $transparentCorners = 0
    foreach ($sample in $samples) {
        if ($bitmap.GetPixel($sample[0], $sample[1]).A -lt 16) {
            $transparentCorners++
        }
    }

    return $transparentCorners -ge 2
}

$src = [System.Drawing.Bitmap]::FromFile($source)
$width = $src.Width
$height = $src.Height
$hasAlpha = Test-SourceHasAlpha $src
$dest = New-Object System.Drawing.Bitmap $width, $height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

$rect = New-Object System.Drawing.Rectangle 0, 0, $width, $height
$srcData = $src.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$destData = $dest.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::WriteOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

try {
    $bytes = [Math]::Abs($srcData.Stride) * $height
    $srcBuffer = New-Object byte[] $bytes
    $destBuffer = New-Object byte[] $bytes
    [System.Runtime.InteropServices.Marshal]::Copy($srcData.Scan0, $srcBuffer, 0, $bytes)

    for ($y = 0; $y -lt $height; $y++) {
        $row = $y * $srcData.Stride
        for ($x = 0; $x -lt $width; $x++) {
            $i = $row + ($x * 4)
            $b = $srcBuffer[$i + 0]
            $g = $srcBuffer[$i + 1]
            $r = $srcBuffer[$i + 2]
            $a = $srcBuffer[$i + 3]

            $makeTransparent = $false
            if ($hasAlpha) {
                $makeTransparent = ($a -lt 16)
            }
            else {
                $makeTransparent = (Test-BackgroundColor $r $g $b)
            }

            if ($makeTransparent) {
                $destBuffer[$i + 0] = 0
                $destBuffer[$i + 1] = 0
                $destBuffer[$i + 2] = 0
                $destBuffer[$i + 3] = 0
            }
            else {
                $destBuffer[$i + 0] = $b
                $destBuffer[$i + 1] = $g
                $destBuffer[$i + 2] = $r
                $destBuffer[$i + 3] = 255
            }
        }
    }

    [System.Runtime.InteropServices.Marshal]::Copy($destBuffer, 0, $destData.Scan0, $bytes)
}
finally {
    $src.UnlockBits($srcData)
    $dest.UnlockBits($destData)
}

$src.Dispose()
$dest.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$dest.Dispose()

Write-Host "Created transparent $pngPath (sourceAlpha=$hasAlpha)"
