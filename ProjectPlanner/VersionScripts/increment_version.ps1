param(
    [string]$Path
)

if (-not (Test-Path $Path)) {
    Set-Content -Path $Path -Value '0'
}

# Read first non-empty line
$val = Get-Content -Path $Path | Where-Object { $_ -match '\S' } | Select-Object -First 1
if (-not [int]::TryParse($val, [ref]$null)) {
    $val = '0'
}

$new = [int]$val + 1
Set-Content -Path $Path -Value $new
Write-Output $new
