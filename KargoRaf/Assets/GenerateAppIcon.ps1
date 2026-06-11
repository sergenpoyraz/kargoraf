# Kargo paketi logosu — şeffaf arka plan, kare ikon
Add-Type -AssemblyName System.Drawing

$size = 512
$path = Join-Path $PSScriptRoot "AppIcon.png"

$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

$margin = 20.0
$radius = 100.0
$rect = New-Object System.Drawing.RectangleF $margin, $margin, ($size - 2 * $margin), ($size - 2 * $margin)

$blue = [System.Drawing.Color]::FromArgb(255, 14, 116, 144)
$teal = [System.Drawing.Color]::FromArgb(255, 8, 145, 178)

$pathBg = New-Object System.Drawing.Drawing2D.GraphicsPath
$pathBg.AddArc($rect.X, $rect.Y, $radius, $radius, 180, 90)
$pathBg.AddArc($rect.Right - $radius, $rect.Y, $radius, $radius, 270, 90)
$pathBg.AddArc($rect.Right - $radius, $rect.Bottom - $radius, $radius, $radius, 0, 90)
$pathBg.AddArc($rect.X, $rect.Bottom - $radius, $radius, $radius, 90, 90)
$pathBg.CloseFigure()

$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $rect, $teal, $blue, [System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
$g.FillPath($brush, $pathBg)
$brush.Dispose()
$pathBg.Dispose()

# Karton kutu gövdesi
$box = New-Object System.Drawing.RectangleF 138, 168, 236, 176
$cardboard = [System.Drawing.Color]::FromArgb(255, 245, 158, 11)
$cardDark = [System.Drawing.Color]::FromArgb(255, 217, 119, 6)
$cardBrush = New-Object System.Drawing.SolidBrush $cardboard
$g.FillRectangle($cardBrush, $box)
$cardBrush.Dispose()

$penDark = New-Object System.Drawing.Pen $cardDark, 6
$penDark.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.DrawRectangle($penDark, $box.X, $box.Y, $box.Width, $box.Height)

# Üst kapak (3D)
$topPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 251, 191, 36)), 5
$g.DrawLine($topPen, 138, 168, 190, 128)
$g.DrawLine($topPen, 374, 168, 322, 128)
$g.DrawLine($topPen, 190, 128, 322, 128)
$topPen.Dispose()

# Bant
$tape = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(220, 255, 255, 255))
$g.FillRectangle($tape, 248, 168, 16, 176)
$tape.Dispose()

# Etiket
$labelRect = New-Object System.Drawing.RectangleF 168, 214, 120, 72
$g.FillRectangle([System.Drawing.Brushes]::White, $labelRect)
$labelPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 203, 213, 225)), 3
$g.DrawRectangle($labelPen, $labelRect.X, $labelRect.Y, $labelRect.Width, $labelRect.Height)
$labelPen.Dispose()

$barPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 100, 116, 139)), 4
$g.DrawLine($barPen, 182, 232, 274, 232)
$g.DrawLine($barPen, 182, 248, 258, 248)
$g.DrawLine($barPen, 182, 264, 240, 264)
$barPen.Dispose()

# Ok (teslimat)
$arrowPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), 10
$arrowPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$arrowPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($arrowPen, 392, 118, 432, 118)
$g.DrawLine($arrowPen, 418, 102, 438, 118)
$g.DrawLine($arrowPen, 418, 134, 438, 118)
$arrowPen.Dispose()

$penDark.Dispose()
$g.Dispose()
$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "Created $path"
