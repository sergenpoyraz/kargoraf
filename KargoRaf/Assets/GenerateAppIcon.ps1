# Kargo Raf — raf + paket logosu (mavi, profesyonel)
Add-Type -AssemblyName System.Drawing

$size = 512
$path = Join-Path $PSScriptRoot "AppIcon.png"

$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

$margin = 24.0
$radius = 96.0
$rect = New-Object System.Drawing.RectangleF $margin, $margin, ($size - 2 * $margin), ($size - 2 * $margin)

$blueDark = [System.Drawing.Color]::FromArgb(255, 29, 78, 216)
$blue = [System.Drawing.Color]::FromArgb(255, 37, 99, 235)

$pathBg = New-Object System.Drawing.Drawing2D.GraphicsPath
$pathBg.AddArc($rect.X, $rect.Y, $radius, $radius, 180, 90)
$pathBg.AddArc($rect.Right - $radius, $rect.Y, $radius, $radius, 270, 90)
$pathBg.AddArc($rect.Right - $radius, $rect.Bottom - $radius, $radius, $radius, 0, 90)
$pathBg.AddArc($rect.X, $rect.Bottom - $radius, $radius, $radius, 90, 90)
$pathBg.CloseFigure()

$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $rect, $blue, $blueDark, [System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
$g.FillPath($brush, $pathBg)
$brush.Dispose()
$pathBg.Dispose()

$shelfPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(220, 255, 255, 255)), 8
$shelfPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$shelfPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$left = 88; $right = 424
$g.DrawLine($shelfPen, $left, 168, $right, 168)
$g.DrawLine($shelfPen, $left, 256, $right, 256)
$g.DrawLine($shelfPen, $left, 344, $right, 344)
$shelfPen.Dispose()

$divPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(140, 255, 255, 255)), 6
$g.DrawLine($divPen, 256, 148, 256, 364)
$divPen.Dispose()

$boxRect = New-Object System.Drawing.RectangleF 300, 280, 96, 72
$boxBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
$g.FillRectangle($boxBrush, $boxRect.X, $boxRect.Y, $boxRect.Width, $boxRect.Height)
$boxPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 191, 219, 254)), 4
$g.DrawRectangle($boxPen, $boxRect.X, $boxRect.Y, $boxRect.Width, $boxRect.Height)
$boxPen.Dispose()
$boxBrush.Dispose()

$tape = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 37, 99, 235))
$g.FillRectangle($tape, 344, 280, 8, 72)
$tape.Dispose()

$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()

Write-Host "Created $path"
