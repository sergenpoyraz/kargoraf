# Valid ICO for ApplicationIcon and runtime (256px, CSC-compatible, transparent)
Add-Type -AssemblyName System.Drawing

$png = Join-Path $PSScriptRoot "AppIcon.png"
$ico = Join-Path $PSScriptRoot "AppIcon.ico"

$src = [System.Drawing.Bitmap]::FromFile($png)
$bmp256 = New-Object System.Drawing.Bitmap 256, 256, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp256)
$g.Clear([System.Drawing.Color]::Transparent)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
$g.DrawImage($src, 0, 0, 256, 256)
$g.Dispose()
$src.Dispose()

$handle = $bmp256.GetHicon()
$baseIcon = [System.Drawing.Icon]::FromHandle($handle)
$icon = $baseIcon.Clone()
$baseIcon.Dispose()
$bmp256.Dispose()

$stream = [IO.File]::Create($ico)
$icon.Save($stream)
$stream.Close()
$icon.Dispose()

Write-Host "Created $ico"
