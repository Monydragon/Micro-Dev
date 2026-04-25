param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('web', 'windows', 'linux', 'macosx', 'macos', 'android', 'ios')]
    [string]$Platform,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$ServerAddress,
    [string]$ServerUser,
    [string]$ServerPassword,
    [int]$TcpPort = 58181,
    [string]$RemoteDotNetRoot,
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

function Assert-WorkloadInstalled([string]$WorkloadId) {
    $workloads = (& dotnet workload list | Out-String)
    if ($workloads -notmatch "(?m)^$WorkloadId\s") {
        throw "The $WorkloadId workload is not installed. Run 'dotnet workload install $WorkloadId' first."
    }
}

function Resolve-IosRuntimeIdentifier([string]$RequestedRuntimeIdentifier) {
    if (-not [string]::IsNullOrWhiteSpace($RequestedRuntimeIdentifier)) {
        return $RequestedRuntimeIdentifier
    }

    if ((Test-HostOS 'OSX') -and
        [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::X64) {
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
        & dotnet build 'src/MicroDev.WebGL/MicroDev.WebGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'windows' {
        if (-not $isWindows) {
            Write-Host 'Building the Windows target on a non-Windows host; this validates the shared DesktopGL launcher only.'
        }

        & dotnet build 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'linux' {
        if (-not $isLinux) {
            Write-Host 'Building the Linux target on a non-Linux host; this validates the shared DesktopGL launcher only.'
        }

        & dotnet build 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'macosx' {
        if (-not $isMacOS) {
            Write-Host 'Building the MacOSX target on a non-macOS host; this validates the shared DesktopGL launcher only.'
        }

        & dotnet build 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -c $Configuration
        exit $LASTEXITCODE
    }
    'android' {
        Assert-WorkloadInstalled 'android'
        & dotnet build 'src/MicroDev.AndroidGL/MicroDev.AndroidGL.csproj' -c $Configuration -f net10.0-android
        exit $LASTEXITCODE
    }
    'ios' {
        Assert-WorkloadInstalled 'ios'

        $iosArgs = @(
            'build',
            'src/MicroDev.iOSGL/MicroDev.iOSGL.csproj',
            '-c', $Configuration,
            '-f', 'net10.0-ios',
            '-r', (Resolve-IosRuntimeIdentifier $IosRuntimeIdentifier)
        )

        if (-not $isMacOS) {
            if ([string]::IsNullOrWhiteSpace($ServerAddress) -or [string]::IsNullOrWhiteSpace($ServerUser)) {
                Write-Host 'Skipping iOS: native iOS builds require macOS or a remote Mac build host.'
                Write-Host 'Pass -ServerAddress and -ServerUser to use a remote Mac build host.'
                exit 0
            }

            if ([string]::IsNullOrWhiteSpace($RemoteDotNetRoot)) {
                $RemoteDotNetRoot = "/Users/$ServerUser/Library/Caches/Xamarin/XMA/SDKs/dotnet/"
            }

            $iosArgs += @(
                '-p:ServerAddress=' + $ServerAddress,
                '-p:ServerUser=' + $ServerUser,
                '-p:TcpPort=' + $TcpPort,
                '-p:_DotNetRootRemoteDirectory=' + $RemoteDotNetRoot
            )

            if (-not [string]::IsNullOrWhiteSpace($ServerPassword)) {
                $iosArgs += '-p:ServerPassword=' + $ServerPassword
            }
        }
        else {
            $iosArgs += '-p:CodesignKey='
        }

        & dotnet @iosArgs
        exit $LASTEXITCODE
    }
}
