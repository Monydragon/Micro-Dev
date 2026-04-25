param(
    [string]$Configuration = 'Release',
    [string]$Version,
    [string[]]$Platforms = @('web', 'windows', 'linux', 'macosx', 'android', 'ios'),
    [switch]$GenerateAndroidSigning,
    [string]$AndroidKeystorePath,
    [string]$AndroidStorePassword,
    [string]$AndroidKeyAlias,
    [string]$AndroidKeyPassword,
    [string]$AndroidExpectedSha1,
    [switch]$SaveAndroidSigning,
    [string]$ServerAddress,
    [string]$ServerUser,
    [string]$ServerPassword,
    [int]$TcpPort = 58181,
    [string]$RemoteDotNetRoot,
    [string]$IosRuntimeIdentifier,
    [switch]$BuildIosIpa,
    [string]$IosCodesignKey,
    [string]$IosCodesignProvision
)

$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Test-HostOS([string]$name) {
    return [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::$name
    )
}

function Get-PropsDocument([string]$PropsPath) {
    return [xml](Get-Content -LiteralPath $PropsPath -Raw)
}

function Get-XmlPropertyValue([xml]$Document, [string]$PropertyName) {
    $node = $Document.SelectSingleNode("/Project/PropertyGroup/$PropertyName")
    if ($null -eq $node) {
        return $null
    }

    return $node.InnerText
}

function Set-XmlPropertyValue([xml]$Document, [string]$PropertyName, [string]$Value) {
    $projectNode = $Document.SelectSingleNode('/Project')
    $propertyGroup = $Document.SelectSingleNode('/Project/PropertyGroup')
    if ($null -eq $propertyGroup) {
        $propertyGroup = $Document.CreateElement('PropertyGroup')
        [void]$projectNode.AppendChild($propertyGroup)
    }

    $propertyNode = $Document.SelectSingleNode("/Project/PropertyGroup/$PropertyName")
    if ($null -eq $propertyNode) {
        $propertyNode = $Document.CreateElement($PropertyName)
        [void]$propertyGroup.AppendChild($propertyNode)
    }

    $propertyNode.InnerText = $Value
}

function Save-XmlDocument([xml]$Document, [string]$Path) {
    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Indent = $true
    $settings.IndentChars = '  '
    $settings.OmitXmlDeclaration = $true
    $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
    $settings.NewLineChars = "`r`n"
    $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

    $writer = [System.Xml.XmlWriter]::Create($Path, $settings)
    try {
        $Document.Save($writer)
    }
    finally {
        $writer.Dispose()
    }
}

function Convert-VersionStringToParts([string]$VersionText) {
    if ($VersionText -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
        throw "Version '$VersionText' must use major.minor.patch format."
    }

    return [pscustomobject]@{
        major = [int]$Matches[1]
        minor = [int]$Matches[2]
        patch = [int]$Matches[3]
    }
}

function Convert-VersionPartsToString([object]$VersionParts) {
    return "{0}.{1}.{2}" -f $VersionParts.major, $VersionParts.minor, $VersionParts.patch
}

function Convert-VersionPartsToBuildNumber([object]$VersionParts) {
    return ($VersionParts.major * 10000) + ($VersionParts.minor * 100) + $VersionParts.patch
}

function Resolve-TargetVersion([string]$CurrentVersion, [string]$RequestedVersion) {
    if (-not [string]::IsNullOrWhiteSpace($RequestedVersion)) {
        [void](Convert-VersionStringToParts $RequestedVersion)
        return $RequestedVersion
    }

    return $CurrentVersion
}

function Update-VersionMetadata([string]$PropsPath, [string]$VersionText) {
    $document = Get-PropsDocument $PropsPath
    $currentVersion = Get-XmlPropertyValue $document 'Version'
    if ([string]::IsNullOrWhiteSpace($currentVersion)) {
        throw 'Directory.Build.props is missing the Version property.'
    }

    $targetVersion = Resolve-TargetVersion -CurrentVersion $currentVersion -RequestedVersion $VersionText
    $versionParts = Convert-VersionStringToParts $targetVersion
    $buildNumber = Convert-VersionPartsToBuildNumber $versionParts

    Set-XmlPropertyValue $document 'Version' $targetVersion
    Set-XmlPropertyValue $document 'AssemblyVersion' "$targetVersion.0"
    Set-XmlPropertyValue $document 'FileVersion' "$targetVersion.0"
    Set-XmlPropertyValue $document 'InformationalVersion' $targetVersion
    Set-XmlPropertyValue $document 'MicroDevAndroidVersionCode' ([string]$buildNumber)
    Set-XmlPropertyValue $document 'MicroDevIosBuildNumber' ([string]$buildNumber)
    Save-XmlDocument $document $PropsPath

    return [pscustomobject]@{
        previousVersion = $currentVersion
        version = $targetVersion
        buildNumber = $buildNumber
    }
}

function New-RandomSecret([int]$Length = 24) {
    $chars = 'abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!@$%*_-+='.ToCharArray()
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $bytes = New-Object byte[] ($Length * 2)
    $rng.GetBytes($bytes)
    $builder = New-Object System.Text.StringBuilder

    for ($index = 0; $index -lt $Length; $index++) {
        [void]$builder.Append($chars[$bytes[$index] % $chars.Length])
    }

    return $builder.ToString()
}

function Get-AndroidSigningConfigPath([string]$RepoRoot) {
    return Join-Path $RepoRoot 'artifacts\signing\android-release-signing.json'
}

function Normalize-CertificateFingerprint([string]$Fingerprint) {
    if ([string]::IsNullOrWhiteSpace($Fingerprint)) {
        return $null
    }

    $hex = ($Fingerprint -replace '[^0-9A-Fa-f]', '').ToUpperInvariant()
    if ($hex.Length -eq 0) {
        return $null
    }

    if ($hex.Length % 2 -ne 0) {
        throw "Invalid certificate fingerprint '$Fingerprint'."
    }

    $pairs = for ($index = 0; $index -lt $hex.Length; $index += 2) {
        $hex.Substring($index, 2)
    }

    return ($pairs -join ':')
}

function Get-AndroidSigningCertificateSha1([object]$Signing) {
    $args = @(
        '-list',
        '-v',
        '-keystore', $Signing.keystorePath,
        '-alias', $Signing.keyAlias,
        '-storepass', $Signing.storePassword
    )

    if (-not [string]::IsNullOrWhiteSpace($Signing.keyPassword)) {
        $args += @('-keypass', $Signing.keyPassword)
    }

    $output = & keytool @args 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to inspect Android signing keystore '$($Signing.keystorePath)'.`n$output"
    }

    $sha1Line = $output | Where-Object { $_ -match 'SHA1:\s*(.+)$' } | Select-Object -First 1
    if (-not $sha1Line) {
        throw "Unable to determine the SHA1 fingerprint for Android signing alias '$($Signing.keyAlias)'."
    }

    return Normalize-CertificateFingerprint $Matches[1]
}

function Save-AndroidSigningConfig([string]$ConfigPath, [object]$Signing) {
    $persisted = [ordered]@{
        keystorePath = $Signing.keystorePath
        storePassword = $Signing.storePassword
        keyAlias = $Signing.keyAlias
        keyPassword = $Signing.keyPassword
    }

    if (-not [string]::IsNullOrWhiteSpace($Signing.expectedSha1)) {
        $persisted.expectedSha1 = $Signing.expectedSha1
    }

    $persisted | ConvertTo-Json | Set-Content -LiteralPath $ConfigPath
}

function Assert-AndroidSigningConfig([object]$Signing) {
    foreach ($propertyName in @('keystorePath', 'storePassword', 'keyAlias', 'keyPassword')) {
        if ([string]::IsNullOrWhiteSpace($Signing.$propertyName)) {
            throw "Android signing config is missing '$propertyName'."
        }
    }

    if (-not (Test-Path -LiteralPath $Signing.keystorePath)) {
        throw "Android signing keystore was not found at '$($Signing.keystorePath)'."
    }

    $normalizedExpectedSha1 = Normalize-CertificateFingerprint $Signing.expectedSha1
    $actualSha1 = Get-AndroidSigningCertificateSha1 $Signing
    Add-Member -InputObject $Signing -NotePropertyName 'expectedSha1' -NotePropertyValue $normalizedExpectedSha1 -Force
    Add-Member -InputObject $Signing -NotePropertyName 'actualSha1' -NotePropertyValue $actualSha1 -Force

    if (-not [string]::IsNullOrWhiteSpace($Signing.expectedSha1) -and $Signing.actualSha1 -ne $Signing.expectedSha1) {
        throw "Android signing certificate mismatch. Expected SHA1 $($Signing.expectedSha1), actual SHA1 $($Signing.actualSha1)."
    }
}

function Initialize-AndroidSigning {
    param(
        [string]$RepoRoot,
        [switch]$Generate,
        [string]$KeystorePath,
        [string]$StorePassword,
        [string]$KeyAlias,
        [string]$KeyPassword,
        [string]$ExpectedSha1,
        [switch]$Save
    )

    $configPath = Get-AndroidSigningConfigPath $RepoRoot
    $configDirectory = Split-Path -Parent $configPath
    New-Item -ItemType Directory -Force -Path $configDirectory | Out-Null

    $signing = $null
    if (Test-Path -LiteralPath $configPath) {
        $signing = Get-Content -LiteralPath $configPath | ConvertFrom-Json
    }

    if ($null -eq $signing) {
        $signing = [pscustomobject]@{
            keystorePath = $null
            storePassword = $null
            keyAlias = $null
            keyPassword = $null
            expectedSha1 = $null
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($KeystorePath)) {
        $resolvedKeystorePath = $KeystorePath
        if (-not [System.IO.Path]::IsPathRooted($resolvedKeystorePath)) {
            $resolvedKeystorePath = Join-Path $RepoRoot $resolvedKeystorePath
        }

        $signing.keystorePath = [System.IO.Path]::GetFullPath($resolvedKeystorePath)
    }

    if (-not [string]::IsNullOrWhiteSpace($StorePassword)) {
        $signing.storePassword = $StorePassword
    }

    if (-not [string]::IsNullOrWhiteSpace($KeyAlias)) {
        $signing.keyAlias = $KeyAlias
    }

    if (-not [string]::IsNullOrWhiteSpace($KeyPassword)) {
        $signing.keyPassword = $KeyPassword
    }

    if (-not [string]::IsNullOrWhiteSpace($ExpectedSha1)) {
        $signing.expectedSha1 = $ExpectedSha1
    }

    if ([string]::IsNullOrWhiteSpace($signing.keyPassword) -and -not [string]::IsNullOrWhiteSpace($signing.storePassword)) {
        $signing.keyPassword = $signing.storePassword
    }

    $hasExplicitSigningInput = -not [string]::IsNullOrWhiteSpace($KeystorePath) -or
        -not [string]::IsNullOrWhiteSpace($StorePassword) -or
        -not [string]::IsNullOrWhiteSpace($KeyAlias) -or
        -not [string]::IsNullOrWhiteSpace($KeyPassword) -or
        -not [string]::IsNullOrWhiteSpace($ExpectedSha1)

    if ([string]::IsNullOrWhiteSpace($signing.keystorePath)) {
        if (-not $Generate) {
            throw @"
Android signing is not configured.

For Google Play uploads, point the script at the upload keystore:
  .\scripts\publish-releases.ps1 -Platforms android -AndroidKeystorePath <path> -AndroidStorePassword <password> -AndroidKeyAlias <alias> -AndroidKeyPassword <password> -AndroidExpectedSha1 <sha1> -SaveAndroidSigning

Use -GenerateAndroidSigning only for a brand-new app or local test signing.
"@
        }

        $keystorePath = Join-Path $configDirectory 'MicroDev.Release.keystore'
        $storePassword = New-RandomSecret
        $keyPassword = $storePassword
        $alias = 'microdev-upload'
        $dname = 'CN=Micro Dev, OU=Release, O=Micro Dev, L=Chicago, S=Illinois, C=US'

        & keytool -genkeypair `
            -v `
            -keystore $keystorePath `
            -storetype PKCS12 `
            -storepass $storePassword `
            -keypass $keyPassword `
            -alias $alias `
            -keyalg RSA `
            -keysize 4096 `
            -validity 10000 `
            -dname $dname

        if ($LASTEXITCODE -ne 0) {
            throw 'Failed to generate Android release keystore.'
        }

        $signing = [pscustomobject]@{
            keystorePath = $keystorePath
            storePassword = $storePassword
            keyAlias = $alias
            keyPassword = $keyPassword
            expectedSha1 = $null
        }

        $Save = $true
    }

    Assert-AndroidSigningConfig $signing

    if ($Save -or ($hasExplicitSigningInput -and (Test-Path -LiteralPath $configPath))) {
        Save-AndroidSigningConfig -ConfigPath $configPath -Signing $signing
    }

    return $signing
}

function Write-AndroidSigningPasswordFiles([string]$RepoRoot, [object]$Signing) {
    $storePasswordFile = Join-Path $RepoRoot 'artifacts\signing\android-storepass.txt'
    $keyPasswordFile = Join-Path $RepoRoot 'artifacts\signing\android-keypass.txt'
    Set-Content -LiteralPath $storePasswordFile -Value $Signing.storePassword
    Set-Content -LiteralPath $keyPasswordFile -Value $Signing.keyPassword

    return [pscustomobject]@{
        storePasswordFile = $storePasswordFile
        keyPasswordFile = $keyPasswordFile
    }
}

function Get-AndroidPublishSigningArguments([string]$RepoRoot, [object]$Signing) {
    $passwordFiles = Write-AndroidSigningPasswordFiles -RepoRoot $RepoRoot -Signing $Signing

    return @(
        '-p:AndroidKeyStore=true',
        "-p:AndroidSigningKeyStore=$($Signing.keystorePath)",
        "-p:AndroidSigningStorePass=file:$($passwordFiles.storePasswordFile)",
        "-p:AndroidSigningKeyAlias=$($Signing.keyAlias)",
        "-p:AndroidSigningKeyPass=file:$($passwordFiles.keyPasswordFile)"
    )
}

function Clear-AndroidPackageArtifacts([string]$ProjectDirectory) {
    $binPath = Join-Path $ProjectDirectory 'bin'
    if (-not (Test-Path -LiteralPath $binPath)) {
        return
    }

    Get-ChildItem -LiteralPath $binPath -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Extension -in @('.aab', '.apk', '.idsig') } |
        Remove-Item -Force -ErrorAction SilentlyContinue
}

function Resolve-AndroidPackageArtifact([string]$Directory, [string]$Extension) {
    $artifact = Get-ChildItem -LiteralPath $Directory -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Extension -eq $Extension } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $artifact) {
        throw "Unable to find Android package '$Extension' in '$Directory'."
    }

    return $artifact.FullName
}

function Publish-Project {
    param(
        [string]$Project,
        [string[]]$Arguments,
        [string]$OutputPath
    )

    if (Test-Path -LiteralPath $OutputPath) {
        Remove-Item -LiteralPath $OutputPath -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

    $publishArgs = @(
        'publish',
        $Project,
        '-c', $Configuration,
        '-o', $OutputPath
    ) + $Arguments

    Write-Host ''
    Write-Host ('> dotnet ' + ($publishArgs -join ' ')) -ForegroundColor Cyan
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for $Project"
    }
}

function Compress-Release([string]$SourcePath, [string]$ZipPath) {
    if (Test-Path -LiteralPath $ZipPath) {
        Remove-Item -LiteralPath $ZipPath -Force
    }

    Compress-Archive -LiteralPath $SourcePath -DestinationPath $ZipPath -Force
}

function Copy-ReleaseArtifact([string]$SourcePath, [string]$DestinationPath) {
    if (Test-Path -LiteralPath $DestinationPath) {
        Remove-Item -LiteralPath $DestinationPath -Force
    }

    Copy-Item -LiteralPath $SourcePath -Destination $DestinationPath
}

function Write-PlatformNote([string]$Directory, [string[]]$Lines, [string]$FileName = 'artifact-status.txt') {
    New-Item -ItemType Directory -Force -Path $Directory | Out-Null
    Set-Content -LiteralPath (Join-Path $Directory $FileName) -Value $Lines
}

function Resolve-IosRuntimeIdentifier([string]$RequestedRuntimeIdentifier, [bool]$IsMacOS, [bool]$HasRemoteBuildHost) {
    if (-not [string]::IsNullOrWhiteSpace($RequestedRuntimeIdentifier)) {
        return $RequestedRuntimeIdentifier
    }

    if ($IsMacOS -and [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::X64) {
        return 'iossimulator-x64'
    }

    if ($IsMacOS -or $HasRemoteBuildHost) {
        return 'iossimulator-arm64'
    }

    return $null
}

function Get-IosArtifactDescription([string]$RuntimeIdentifier, [switch]$BuildIpa) {
    if ($BuildIpa) {
        return 'device IPA'
    }

    if ($RuntimeIdentifier -like 'iossimulator-*') {
        return 'simulator app bundle'
    }

    return 'device app bundle'
}

function Write-ReleaseNotes {
    param(
        [string]$ReleaseRoot,
        [string]$ZipRoot,
        [string]$Version
    )

    $notes = @(
        "Micro Dev v$Version",
        '',
        'Publish layout:',
        'publish\Web',
        'publish\Windows',
        'publish\Linux',
        'publish\MacOSX',
        'publish\Android',
        'publish\iOS',
        '',
        'Zip packages:',
        (Get-ChildItem -Path $ZipRoot -File -ErrorAction SilentlyContinue | Sort-Object Name | ForEach-Object { $_.Name })
    )

    Set-Content -LiteralPath (Join-Path $ReleaseRoot 'RELEASES.txt') -Value $notes
}

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

$propsPath = Join-Path $repoRoot 'Directory.Build.props'
$versionInfo = Update-VersionMetadata -PropsPath $propsPath -VersionText $Version
$Version = $versionInfo.version

$releaseRoot = Join-Path $repoRoot "artifacts\releases\v$Version"
$publishRoot = Join-Path $releaseRoot 'publish'
$zipRoot = Join-Path $releaseRoot 'zips'

if (Test-Path -LiteralPath $releaseRoot) {
    Remove-Item -LiteralPath $releaseRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $publishRoot, $zipRoot | Out-Null

$isMacOS = Test-HostOS 'OSX'
$hasRemoteMacBuildHost = -not [string]::IsNullOrWhiteSpace($ServerAddress) -and -not [string]::IsNullOrWhiteSpace($ServerUser)
$validPlatforms = @('web', 'windows', 'linux', 'macosx', 'macos', 'android', 'ios')
$normalizedPlatforms = $Platforms |
    ForEach-Object { $_ -split '[,\s]+' } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    ForEach-Object { $_.ToLowerInvariant() } |
    ForEach-Object {
        if ($_ -notin $validPlatforms) {
            throw "Unknown platform '$_'. Valid platforms: $($validPlatforms -join ', ')."
        }

        if ($_ -eq 'macos') { 'macosx' } else { $_ }
    } |
    Select-Object -Unique

foreach ($platform in $normalizedPlatforms) {
    switch ($platform) {
        'web' {
            $outputPath = Join-Path $publishRoot 'Web'
            Publish-Project -Project 'src/MicroDev.WebGL/MicroDev.WebGL.csproj' -OutputPath $outputPath -Arguments @()
            Compress-Release -SourcePath $outputPath -ZipPath (Join-Path $zipRoot "MicroDev-Web-v$Version.zip")
        }
        'windows' {
            $outputPath = Join-Path $publishRoot 'Windows'
            Publish-Project -Project 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -OutputPath $outputPath -Arguments @(
                '-r', 'win-x64',
                '--self-contained', 'true',
                '-p:PublishSingleFile=true',
                '-p:IncludeNativeLibrariesForSelfExtract=true',
                '-p:PublishAot=false',
                '-p:PublishTrimmed=false'
            )
            Compress-Release -SourcePath $outputPath -ZipPath (Join-Path $zipRoot "MicroDev-Windows-win-x64-v$Version.zip")
        }
        'linux' {
            $outputPath = Join-Path $publishRoot 'Linux'
            Publish-Project -Project 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -OutputPath $outputPath -Arguments @(
                '-r', 'linux-x64',
                '--self-contained', 'true',
                '-p:PublishSingleFile=true',
                '-p:IncludeNativeLibrariesForSelfExtract=true',
                '-p:PublishAot=false',
                '-p:PublishTrimmed=false'
            )
            Compress-Release -SourcePath $outputPath -ZipPath (Join-Path $zipRoot "MicroDev-Linux-linux-x64-v$Version.zip")
        }
        'macosx' {
            $outputPath = Join-Path $publishRoot 'MacOSX'
            Publish-Project -Project 'src/MicroDev.DesktopGL/MicroDev.DesktopGL.csproj' -OutputPath $outputPath -Arguments @(
                '-r', 'osx-x64',
                '--self-contained', 'true',
                '-p:PublishSingleFile=true',
                '-p:IncludeNativeLibrariesForSelfExtract=true',
                '-p:PublishAot=false',
                '-p:PublishTrimmed=false'
            )
            Compress-Release -SourcePath $outputPath -ZipPath (Join-Path $zipRoot "MicroDev-MacOSX-osx-x64-v$Version.zip")
        }
        'android' {
            $androidOutput = Join-Path $publishRoot 'Android'
            $apkOutput = Join-Path $androidOutput 'apk'
            $aabOutput = Join-Path $androidOutput 'aab'
            $androidProjectDir = Join-Path $repoRoot 'src\MicroDev.AndroidGL'

            $androidSigning = Initialize-AndroidSigning `
                -RepoRoot $repoRoot `
                -Generate:$GenerateAndroidSigning `
                -KeystorePath $AndroidKeystorePath `
                -StorePassword $AndroidStorePassword `
                -KeyAlias $AndroidKeyAlias `
                -KeyPassword $AndroidKeyPassword `
                -ExpectedSha1 $AndroidExpectedSha1 `
                -Save:$SaveAndroidSigning

            $androidSigningArguments = Get-AndroidPublishSigningArguments -RepoRoot $repoRoot -Signing $androidSigning
            $apkPublishArguments = @('-f', 'net10.0-android', '-p:AndroidPackageFormat=apk') + $androidSigningArguments
            $aabPublishArguments = @('-f', 'net10.0-android', '-p:AndroidPackageFormat=aab') + $androidSigningArguments

            Clear-AndroidPackageArtifacts $androidProjectDir
            Publish-Project -Project 'src/MicroDev.AndroidGL/MicroDev.AndroidGL.csproj' -OutputPath $apkOutput -Arguments $apkPublishArguments

            Clear-AndroidPackageArtifacts $androidProjectDir
            Publish-Project -Project 'src/MicroDev.AndroidGL/MicroDev.AndroidGL.csproj' -OutputPath $aabOutput -Arguments $aabPublishArguments

            $publishedApk = Resolve-AndroidPackageArtifact -Directory $apkOutput -Extension '.apk'
            $publishedAab = Resolve-AndroidPackageArtifact -Directory $aabOutput -Extension '.aab'
            $signedApk = Join-Path $androidOutput "MicroDev-v$Version-release.apk"
            $signedAab = Join-Path $androidOutput "MicroDev-v$Version-release.aab"
            Copy-ReleaseArtifact -SourcePath $publishedApk -DestinationPath $signedApk
            Copy-ReleaseArtifact -SourcePath $publishedAab -DestinationPath $signedAab

            Write-PlatformNote -Directory $androidOutput -Lines @(
                'Micro Dev Android publish summary',
                "Version: $Version",
                'Signing mode: Release signing.',
                "Signing SHA1: $($androidSigning.actualSha1)",
                "APK: $(Split-Path -Leaf $signedApk)",
                "AAB: $(Split-Path -Leaf $signedAab)"
            )
            Compress-Release -SourcePath $androidOutput -ZipPath (Join-Path $zipRoot "MicroDev-Android-v$Version.zip")
        }
        'ios' {
            $outputPath = Join-Path $publishRoot 'iOS'
            New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

            if (-not $isMacOS -and -not $hasRemoteMacBuildHost) {
                Write-PlatformNote -Directory $outputPath -Lines @(
                    'Micro Dev iOS publish was skipped.',
                    'Reason: publish requires macOS or a configured remote Mac build host.',
                    'Simulator artifact on macOS:',
                    '  .\scripts\publish-releases.ps1 -Platforms ios',
                    'Device IPA:',
                    '  .\scripts\publish-releases.ps1 -Platforms ios -IosRuntimeIdentifier ios-arm64 -BuildIosIpa -IosCodesignKey <identity> -IosCodesignProvision <profile>'
                )
                Write-Host 'Skipping MicroDev.iOSGL: publish requires macOS or a configured remote Mac build host.'
                Compress-Release -SourcePath $outputPath -ZipPath (Join-Path $zipRoot "MicroDev-iOS-v$Version.zip")
                continue
            }

            if ([string]::IsNullOrWhiteSpace($RemoteDotNetRoot) -and $hasRemoteMacBuildHost) {
                $RemoteDotNetRoot = "/Users/$ServerUser/Library/Caches/Xamarin/XMA/SDKs/dotnet/"
            }

            $resolvedIosRuntimeIdentifier = Resolve-IosRuntimeIdentifier `
                -RequestedRuntimeIdentifier $IosRuntimeIdentifier `
                -IsMacOS:$isMacOS `
                -HasRemoteBuildHost:$hasRemoteMacBuildHost

            if ([string]::IsNullOrWhiteSpace($resolvedIosRuntimeIdentifier)) {
                throw 'Unable to determine an iOS runtime identifier. Pass -IosRuntimeIdentifier explicitly.'
            }

            if ($BuildIosIpa -and $resolvedIosRuntimeIdentifier -like 'iossimulator-*') {
                throw "BuildIosIpa requires a device runtime identifier such as 'ios-arm64'."
            }

            $iosArguments = @(
                '-f', 'net10.0-ios',
                '-r', $resolvedIosRuntimeIdentifier
            )

            if ($BuildIosIpa) {
                $ipaPath = Join-Path $outputPath "MicroDev-v$Version.ipa"
                $iosArguments += @('-p:BuildIpa=true', "-p:IpaPackagePath=$ipaPath")
            }
            elseif ($resolvedIosRuntimeIdentifier -like 'iossimulator-*') {
                $iosArguments += '-p:CodesignKey='
            }

            if (-not [string]::IsNullOrWhiteSpace($IosCodesignKey)) {
                $iosArguments += "-p:CodesignKey=$IosCodesignKey"
            }

            if (-not [string]::IsNullOrWhiteSpace($IosCodesignProvision)) {
                $iosArguments += "-p:CodesignProvision=$IosCodesignProvision"
            }

            if ($hasRemoteMacBuildHost) {
                $iosArguments += @(
                    "-p:ServerAddress=$ServerAddress",
                    "-p:ServerUser=$ServerUser",
                    "-p:TcpPort=$TcpPort",
                    "-p:_DotNetRootRemoteDirectory=$RemoteDotNetRoot"
                )

                if (-not [string]::IsNullOrWhiteSpace($ServerPassword)) {
                    $iosArguments += "-p:ServerPassword=$ServerPassword"
                }
            }

            Publish-Project -Project 'src/MicroDev.iOSGL/MicroDev.iOSGL.csproj' -OutputPath $outputPath -Arguments $iosArguments
            Write-PlatformNote -Directory $outputPath -Lines @(
                'Micro Dev iOS publish summary',
                "Version: $Version",
                "Runtime identifier: $resolvedIosRuntimeIdentifier",
                "Artifact type: $(Get-IosArtifactDescription -RuntimeIdentifier $resolvedIosRuntimeIdentifier -BuildIpa:$BuildIosIpa)",
                "Host mode: $(if ($hasRemoteMacBuildHost -and -not $isMacOS) { 'remote Mac build host' } else { 'local macOS host' })"
            )
            Compress-Release -SourcePath $outputPath -ZipPath (Join-Path $zipRoot "MicroDev-iOS-v$Version.zip")
        }
    }
}

Write-ReleaseNotes -ReleaseRoot $releaseRoot -ZipRoot $zipRoot -Version $Version

Write-Host ''
Write-Host "Micro Dev release version: $($versionInfo.previousVersion) -> $Version" -ForegroundColor Green
Write-Host "Release artifacts created in $releaseRoot" -ForegroundColor Green
