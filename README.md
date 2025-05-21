# QP-Platform

Unity project for QP-Platform, used for building and exporting Android, iOS, and Web builds.

## Prerequisites

- Unity 6000.0.30f1
  - Must be installed at: `/Applications/Unity/Hub/Editor/6000.0.30f1/Unity.app`
- OpenJDK 17
- Gradle
- Android SDK
- Xcode (for iOS builds)

## Project Structure

```
QP-Platform/
├── Platform/                 # Unity project directory
│   ├── Assets/              # Unity assets
│   └── Build/               # Output directory for final builds
├── logs/                    # Build logs directory
├── scripts/
│   └── buildTargets/
│       ├── build_targets.sh           # Main build entry script
│       ├── build_aar.sh               # Android AAR build script
│       ├── build_ios_framework.sh     # iOS Framework build script
│       └── build_web.sh               # WebGL build script
└── ...
```

## Building (Android, iOS, Web)

1. Make the main build script executable:
   ```zsh
   chmod +x scripts/buildTargets/build_targets.sh
   ```

2. Run the build script from the project root:
   ```zsh
   # To build all targets:
   ./scripts/buildTargets/build_targets.sh

   # To build only Android:
   ./scripts/buildTargets/build_targets.sh android

   # To build Android and iOS:
   ./scripts/buildTargets/build_targets.sh android ios
   ```

3. The script will:
   - Export Unity project for the selected targets
   - Build AAR (Android), Framework (iOS), or WebGL (Web)
   - Copy final artifacts to the appropriate build folders

## Build Artifacts

- **Android AAR**: `Platform/Build/unityLibrary-release.aar`
- **iOS Framework**: `Platform/Exports/iOS/build/Release-iphoneos/UnityFramework.framework`
- **WebGL Zip**: `Platform/Build/Web/qp-platform-web.zip`
- **Build Logs**: `logs/`

## Troubleshooting

If build fails, check:
1. Unity installation path is correct
2. Java version is OpenJDK 17
3. Gradle is installed and in PATH
4. Android SDK is properly set up
5. Xcode is installed and selected (for iOS)
6. `logs/` for build logs
7. File permissions (scripts need execute permission)

## Notes

- All scripts use relative paths from the project root
- Always run build scripts from the project root directory
- Exits immediately on any error (`set -e`)
- Creates necessary directories automatically
- Verifies artifact creation and copy operations

Platform repo for quickplay
