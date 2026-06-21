# Generates topory's application icon: a violet->indigo rounded square with a
# white push-pin — the universal "keep this on top / pinned" mark.
#
# Frames are written as uncompressed 32-bit BMP (DIB) entries via GDI+ itself,
# because System.Drawing.Icon / the WinForms NotifyIcon load BMP frames
# reliably, whereas PNG-compressed frames can fail to decode.
#
# Run from anywhere; it writes ../topory/Assets/topory.ico.
Add-Type -AssemblyName System.Drawing

function New-RoundedRect([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconBitmap([int]$S) {
    $bmp = New-Object System.Drawing.Bitmap($S, $S, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # Background rounded square, filled with a diagonal violet -> indigo gradient.
    $m = [single]($S * 0.06)
    $side = [single]($S - 2 * $m)
    $bg = New-RoundedRect $m $m $side $side ([single]($S * 0.22))
    $violet = [System.Drawing.Color]::FromArgb(255, 139, 92, 246)   # #8B5CF6
    $indigo = [System.Drawing.Color]::FromArgb(255, 99, 102, 241)   # #6366F1
    $rect = New-Object System.Drawing.RectangleF(0, 0, $S, $S)
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $violet, $indigo, 45.0)
    $g.FillPath($grad, $bg)

    $white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $cx = [single]($S / 2)

    # Push-pin head (round grip).
    $headR = [single]($S * 0.15)
    $headCy = [single]($S * 0.34)
    $g.FillEllipse($white, ($cx - $headR), ($headCy - $headR), ($headR * 2), ($headR * 2))

    # Collar across the pin.
    $collarW = [single]($S * 0.34)
    $collarH = [single]($S * 0.075)
    $collarY = [single]($S * 0.46)
    $collar = New-RoundedRect ($cx - $collarW / 2) $collarY $collarW $collarH ([single]($collarH / 2))
    $g.FillPath($white, $collar)

    # Needle tapering to a point.
    $needle = New-Object System.Drawing.Drawing2D.GraphicsPath
    $pts = @(
        (New-Object System.Drawing.PointF(($cx - $S * 0.06), ($collarY + $collarH))),
        (New-Object System.Drawing.PointF(($cx + $S * 0.06), ($collarY + $collarH))),
        (New-Object System.Drawing.PointF($cx, [single]($S * 0.84)))
    )
    $needle.AddPolygon($pts)
    $g.FillPath($white, $needle)

    $g.Dispose()
    return $bmp
}

# Returns a complete single-frame .ico (as bytes) for one size, produced by
# GDI+ itself via GetHicon -> Icon.Save, so the pixel data and its directory
# entry are guaranteed mutually consistent; we only repackage them below.
function Get-SingleFrameIco([System.Drawing.Bitmap]$bmp) {
    $hicon = $bmp.GetHicon()
    $icon = [System.Drawing.Icon]::FromHandle($hicon)
    $ms = New-Object System.IO.MemoryStream
    $icon.Save($ms)
    $icon.Dispose()
    $bytes = $ms.ToArray()
    $ms.Dispose()
    return , $bytes
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)

$singles = New-Object 'System.Collections.Generic.List[byte[]]'
foreach ($s in $sizes) {
    $bmp = New-IconBitmap $s
    $singles.Add((Get-SingleFrameIco $bmp))
    $bmp.Dispose()
}

$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter($out)
$w.Write([uint16]0)
$w.Write([uint16]1)
$w.Write([uint16]$sizes.Count)

$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $single = $singles[$i]
    $blobLength = $single.Length - 22
    $entry = New-Object byte[] 16
    [System.Array]::Copy($single, 6, $entry, 0, 16)
    [System.BitConverter]::GetBytes([uint32]$blobLength).CopyTo($entry, 8)
    [System.BitConverter]::GetBytes([uint32]$offset).CopyTo($entry, 12)
    $w.Write($entry, 0, 16)
    $offset += $blobLength
}
foreach ($single in $singles) {
    $w.Write($single, 22, $single.Length - 22)
}
$w.Flush()

$target = Join-Path $PSScriptRoot '..\topory\Assets\topory.ico'
[System.IO.File]::WriteAllBytes($target, $out.ToArray())
$w.Dispose()
Write-Output "Wrote $((Resolve-Path $target).Path) ($((Get-Item $target).Length) bytes)"
