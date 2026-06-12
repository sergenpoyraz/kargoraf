# Kargo Raf - professional app icon (shelf + package)
Add-Type -AssemblyName System.Drawing

$size = 512
$pngPath = Join-Path $PSScriptRoot "AppIcon.png"

$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

function New-RoundedRectPath([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

$margin = 28.0
$bodyW = $size - 2 * $margin
$bodyH = $size - 2 * $margin
$body = [System.Drawing.RectangleF]::new($margin, $margin, $bodyW, $bodyH)
$radius = 108.0

$shadowRect = [System.Drawing.RectangleF]::new($body.X + 6, $body.Y + 10, $body.Width, $body.Height)
$shadowPath = New-RoundedRectPath $shadowRect.X $shadowRect.Y $shadowRect.Width $shadowRect.Height ($radius - 4)
$shadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(48, 15, 23, 42))
$g.FillPath($shadowBrush, $shadowPath)
$shadowBrush.Dispose()
$shadowPath.Dispose()

$bgPath = New-RoundedRectPath $body.X $body.Y $body.Width $body.Height $radius
$bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $body,
    [System.Drawing.Color]::FromArgb(255, 249, 115, 22),
    [System.Drawing.Color]::FromArgb(255, 234, 88, 12),
    [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal)
$g.FillPath($bgBrush, $bgPath)
$bgBrush.Dispose()

$shineRect = [System.Drawing.RectangleF]::new($body.X + 24, $body.Y + 18, $body.Width - 48, $body.Height * 0.42)
$shinePath = New-RoundedRectPath $shineRect.X $shineRect.Y $shineRect.Width $shineRect.Height 72
$shineBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $shineRect,
    [System.Drawing.Color]::FromArgb(56, 255, 255, 255),
    [System.Drawing.Color]::FromArgb(0, 255, 255, 255),
    [System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
$g.FillPath($shineBrush, $shinePath)
$shineBrush.Dispose()
$shinePath.Dispose()

$borderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(90, 255, 255, 255)), 2
$g.DrawPath($borderPen, $bgPath)
$borderPen.Dispose()
$bgPath.Dispose()

$unitLeft = 118.0
$unitTop = 132.0
$unitWidth = 276.0
$unitHeight = 228.0

$framePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(210, 255, 255, 255)), 7
$framePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$framePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$framePen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

$g.DrawLine($framePen, $unitLeft, $unitTop, $unitLeft, ($unitTop + $unitHeight))
$g.DrawLine($framePen, ($unitLeft + $unitWidth), $unitTop, ($unitLeft + $unitWidth), ($unitTop + $unitHeight))

$shelfYs = @(168.0, 228.0, 288.0)
foreach ($y in $shelfYs) {
    $g.DrawLine($framePen, ($unitLeft + 8), $y, ($unitLeft + $unitWidth - 8), $y)
}

$divPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(130, 255, 255, 255)), 5
$divPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$divPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($divPen, 210, ($unitTop + 8), 210, ($unitTop + $unitHeight - 8))
$g.DrawLine($divPen, 302, ($unitTop + 8), 302, ($unitTop + $unitHeight - 8))
$divPen.Dispose()
$framePen.Dispose()

$boxX = 314.0
$boxY = 248.0
$boxW = 68.0
$boxH = 52.0

$boxShadow = [System.Drawing.RectangleF]::new($boxX + 4, $boxY + 6, $boxW, $boxH)
$boxShadowPath = New-RoundedRectPath $boxShadow.X $boxShadow.Y $boxShadow.Width $boxShadow.Height 8
$boxShadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(55, 15, 23, 42))
$g.FillPath($boxShadowBrush, $boxShadowPath)
$boxShadowBrush.Dispose()
$boxShadowPath.Dispose()

$boxRect = [System.Drawing.RectangleF]::new($boxX, $boxY, $boxW, $boxH)
$boxPath = New-RoundedRectPath $boxRect.X $boxRect.Y $boxRect.Width $boxRect.Height 8
$boxBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
$g.FillPath($boxBrush, $boxPath)
$boxBrush.Dispose()

$boxBorderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 191, 219, 254)), 3
$g.DrawPath($boxBorderPen, $boxPath)
$boxBorderPen.Dispose()

$flapBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 239, 246, 255))
$flap = @(
    [System.Drawing.PointF]::new($boxX, $boxY),
    [System.Drawing.PointF]::new($boxX + $boxW * 0.5, $boxY - 16),
    [System.Drawing.PointF]::new($boxX + $boxW, $boxY)
)
$g.FillPolygon($flapBrush, $flap)
$flapPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 191, 219, 254)), 3
$g.DrawPolygon($flapPen, $flap)
$flapPen.Dispose()
$flapBrush.Dispose()

$tapeBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 194, 65, 12))
$g.FillRectangle($tapeBrush, ($boxX + $boxW * 0.5 - 5), $boxY, 10, $boxH)
$tapeBrush.Dispose()

$dotBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 245, 158, 11))
$g.FillEllipse($dotBrush, ($boxX + 12), ($boxY + 14), 10, 10)
$dotBrush.Dispose()

$boxPath.Dispose()

$bmp.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()

Write-Host "Created $pngPath"
