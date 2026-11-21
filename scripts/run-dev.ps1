$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$projectFile = Join-Path $projectRoot 'MediSlot.csproj'
$listenerProcessIds = Get-NetTCPConnection -LocalPort 5113 -State Listen -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess -Unique

foreach ($processId in $listenerProcessIds)
{
    $process = Get-CimInstance Win32_Process -Filter "ProcessId = $processId"
    if ($null -eq $process)
    {
        continue
    }

    if ([string]::IsNullOrWhiteSpace($process.CommandLine))
    {
        throw "Cannot verify the process owning port 5113 (PID $processId); it was not stopped."
    }

    $isMediSlot = $process.Name -eq 'MediSlot.exe' -or
        ($process.Name -eq 'dotnet.exe' -and $process.CommandLine -match 'MediSlot\.(dll|exe)')
    $belongsToProject = $process.CommandLine.IndexOf($projectRoot, [StringComparison]::OrdinalIgnoreCase) -ge 0

    if (-not ($isMediSlot -and $belongsToProject))
    {
        throw "Port 5113 is owned by a different process (PID $processId); it was not stopped."
    }

    Stop-Process -Id $processId
}

& dotnet run --project $projectFile --launch-profile MediSlot
exit $LASTEXITCODE
