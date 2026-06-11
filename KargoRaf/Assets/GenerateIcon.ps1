# Valid ICO uretici - Assets klasorunde calistirin
Add-Type -AssemblyName System.Drawing
$png = Join-Path $PSScriptRoot "AppIcon.png"
$ico = Join-Path $PSScriptRoot "AppIcon.ico"
$sizeDims = @(16, 32, 48, 256)
$mem = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($mem)
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$sizeDims.Count)
$offset = 6 + (16 * $sizeDims.Count)
$pngData = New-Object System.Collections.Generic.List[Object]
$src = [System.Drawing.Image]::FromFile($png)
foreach ($dim in $sizeDims) {
    $bmp = New-Object System.Drawing.Bitmap($dim, $dim)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.DrawImage($src, 0, 0, $dim, $dim)
    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngData.Add(@($dim, $ms.ToArray()))
    $bmp.Dispose()
}
$src.Dispose()
foreach ($entry in $pngData) {
    $dim = [int]$entry[0]
    $b = if ($dim -eq 256) { [byte]0 } else { [byte]$dim }
    $writer.Write($b)
    $writer.Write($b)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([byte]1)
    $writer.Write([byte]32)
    $writer.Write([uint32]$entry[1].Length)
    $writer.Write([uint32]$offset)
    $offset += $entry[1].Length
}
foreach ($entry in $pngData) { $writer.Write($entry[1]) }
$writer.Flush()
[IO.File]::WriteAllBytes($ico, $mem.ToArray())
Write-Host "Created $ico"
