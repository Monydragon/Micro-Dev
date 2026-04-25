# Micro Dev

## Platform Targets

Micro Dev keeps one shared core project and one shared DesktopGL launcher. The desktop launcher is published with runtime-specific targets for Windows, Linux, and MacOSX, while Web, Android, and iOS have their own host projects.

Fast prototype loop:

```powershell
.\scripts\build-platform.ps1 -Platform web
.\scripts\build-platform.ps1 -Platform windows
.\scripts\build-platform.ps1 -Platform linux
.\scripts\build-platform.ps1 -Platform macosx
.\scripts\build-platform.ps1 -Platform android
.\scripts\build-platform.ps1 -Platform ios
```

Local run loop:

```powershell
.\scripts\run-platform.ps1 -Platform web
.\scripts\run-platform.ps1 -Platform windows
.\scripts\run-platform.ps1 -Platform android
```

Release packaging:

```powershell
.\scripts\publish-releases.ps1 -Version 0.1.1 -Platforms web,windows,linux,macosx
.\scripts\publish-releases.ps1 -Version 0.1.1 -Platforms android -GenerateAndroidSigning
```

Android release signing can also be supplied with `-AndroidKeystorePath`, `-AndroidStorePassword`, `-AndroidKeyAlias`, `-AndroidKeyPassword`, and `-AndroidExpectedSha1`. iOS publishing requires macOS or a configured remote Mac build host.
