param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('web', 'windows', 'linux', 'macosx', 'macos', 'android', 'ios')]
    [string]$Platform,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$IosRuntimeIdentifier
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

function Test-HostOS([string]$name) {
    return [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::$name
    )
}

function Resolve-IosRuntimeIdentifier([string]$RequestedRuntimeIdentifier) {
    if (-not [string]::IsNullOrWhiteSpace($RequestedRuntimeIdentifier)) {
        return $RequestedRuntimeIdentifier
    }

    if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::X64) {
        return 'iossimulator-x64'
    }

    return 'iossimulator-arm64'
}

$isWindows = Test-HostOS 'Windows'
$isLinux = Test-HostOS 'Linux'
$isMacOS = Test-HostOS 'OSX'
$normalizedPlatform = if ($Platform -eq 'macos') { 'macosx' } else { $Platform }

switch ($normalizedPlatform) {
    'web' {
        & dotnet run --project 'src/MicroDev.WebGL/MicroDev.WebGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'windows' {
        if (-not $isWindows) {
            Write-Host 'Skipping Windows run: this host is not Windows.'
            exit 0
        }

        & dotnet run --project 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'linux' {
        if (-not $isLinux) {
            Write-Host 'Skipping Linux run: this host is not Linux.'
            exit 0
        }

        & dotnet run --project 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'macosx' {
        if (-not $isMacOS) {
            Write-Host 'Skipping MacOSX run: this host is not macOS.'
            exit 0
        }

        & dotnet run --project 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'android' {
        $adb = Get-Command adb -ErrorAction SilentlyContinue
        if (-not $adb) {
            Write-Host 'Skipping Android run: adb is not available on PATH.'
            exit 0
        }

        $devices = & adb devices
        if ($devices -notmatch "device`r?$") {
            Write-Host 'Skipping Android run: no Android device or emulator is connected.'
            exit 0
        }

        & dotnet build 'src/MicroDev.AndroidGL/MicroDev.AndroidGL.csproj' -t:Run -c $Configuration -f net10.0-android
        exit $LASTEXITCODE
    }
    'ios' {
        if (-not $isMacOS) {
            Write-Host 'Skipping iOS run: simulator runs require macOS.'
            exit 0
        }

        & dotnet build 'src/MicroDev.iOSGL/MicroDev.iOSGL.csproj' -t:Run -c $Configuration -f net10.0-ios -r (Resolve-IosRuntimeIdentifier $IosRuntimeIdentifier) -p:CodesignKey=
        exit $LASTEXITCODE
    }
}
