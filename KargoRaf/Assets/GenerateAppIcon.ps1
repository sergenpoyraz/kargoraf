# Temiz kare logo — şeffaf arka plan, ekstra padding yok
Add-Type -AssemblyName System.Drawing

$size = 512
$path = Join-Path $PSScriptRoot "AppIcon.png"

$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

$margin = 24
$radius = 96
$rect = New-Object System.Drawing.RectangleF $margin, $margin, ($size - 2 * $margin), ($size - 2 * $margin)
$blue = [System.Drawing.Color]::FromArgb(255, 37, 99, 235)
$dark = [System.Drawing.Color]::FromArgb(255, 29, 78, 216)

$pathBg = New-Object System.Drawing.Drawing2D.GraphicsPath
$pathBg.AddArc($rect.X, $rect.Y, $radius, $radius, 180, 90)
$pathBg.AddArc($rect.Right - $radius, $rect.Y, $radius, $radius, 270, 90)
$pathBg.AddArc($rect.Right - $radius, $rect.Bottom - $radius, $radius, $radius, 0, 90)
$pathBg.AddArc($rect.X, $rect.Bottom - $radius, $radius, $radius, 90, 90)
$pathBg.CloseFigure()

$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $rect, $blue, $dark, [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal)
$g.FillPath($brush, $pathBg)
$brush.Dispose()
$pathBg.Dispose()

$white = [System.Drawing.Color]::White
$pen = New-Object System.Drawing.Pen $white, 14
$pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

# Kutu
$boxLeft = 150
$boxTop = 170
$boxW = 212
$boxH = 150
$g.DrawLine($pen, $boxLeft, ($boxTop + 40), ($boxLeft + $boxW), ($boxTop + 40))
$g.DrawLine($pen, $boxLeft, ($boxTop + 40), ($boxLeft + 30), $boxTop)
$g.DrawLine($pen, ($boxLeft + $boxW), ($boxTop + 40), ($boxLeft + $boxW - 30), $boxTop)
$g.DrawLine($pen, ($boxLeft + 30), $boxTop, ($boxLeft + $boxW - 30), $boxTop)
$g.DrawRectangle($pen, ($boxLeft + 8), ($boxTop + 40), ($boxW - 16), ($boxH - 40))

# Numaralı kartlar
$font = New-Object System.Drawing.Font "Segoe UI", 44, ([System.Drawing.FontStyle]::Bold)
$sf = New-Object System.Drawing.StringFormat
$sf.Alignment = [System.Drawing.StringAlignment]::Center
$sf.LineAlignment = [System.Drawing.StringAlignment]::Center

$cardW = 46
$cardH = 62
$startX = 188
$cardY = 198
foreach ($i in 1..3) {
    $x = $startX + ($i - 1) * 58
    $g.FillRectangle([System.Drawing.Brushes]::White, $x, $cardY, $cardW, $cardH)
    $g.DrawString("$i", $font, (New-Object System.Drawing.SolidBrush $blue), ($x + $cardW / 2), ($cardY + $cardH / 2), $sf)
}

$font.Dispose()
$pen.Dispose()
$g.Dispose()
$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "Created $path"
