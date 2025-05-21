#!/bin/zsh

# Exit on any error
set -e

# --- Ensure Xcode is selected and license is accepted ---
XCODE_PATH="/Applications/Xcode.app/Contents/Developer"
if [ -d "$XCODE_PATH" ]; then
    echo "Selecting Xcode at $XCODE_PATH"
    sudo xcode-select -s "$XCODE_PATH"
    sudo xcodebuild -license accept
else
    echo "Xcode not found at $XCODE_PATH. Please install Xcode from the App Store."
    exit 1
fi

# --- Configuration ---
# Use the common configuration from parent script
: "${UNITY_VERSION:?UNITY_VERSION is not set in parent script}"
: "${UNITY_PATH:?UNITY_PATH is not set in parent script}"   
: "${PROJECT_PATH:?PROJECT_PATH is not set in parent script}"
: "${LOGS_DIR:?LOGS_DIR is not set in parent script}"

# Define all key paths and names at the top for reuse
LOG_FILE="$LOGS_DIR/unity_ios_build.log"
EXPORT_PATH="$PROJECT_PATH/Exports/iOS"
FRAMEWORK_NAME="UnityFramework.framework"
FRAMEWORK_BUILT_PATH="build/Release-iphoneos/$FRAMEWORK_NAME"

# Print configuration
echo "=== Starting Unity iOS Framework build process ==="
echo "Project path: $PROJECT_PATH"
echo "Logs will be written to: $LOG_FILE"

# Create necessary directories
mkdir -p "$LOGS_DIR"

# Step 1: Unity Export
echo "=== Step 1: Exporting Unity Project to iOS (Xcode project) ==="
"$UNITY_PATH" \
    -quit \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -logFile "$LOG_FILE" \
    -executeMethod nostra.platform.build.BuildIOSFramework.ExportIOSFramework

if [ $? -ne 0 ]; then
    echo "Unity export failed! Check $LOG_FILE for details"
    tail -n 50 "$LOG_FILE"
    exit 1
fi

echo "Unity export completed successfully!"

# Step 2: Xcode Build
echo "=== Step 2: Building .framework using xcodebuild ==="
cd "$EXPORT_PATH"

# Build the framework
xcodebuild \
  -project Unity-iPhone.xcodeproj \
  -scheme UnityFramework \
  -configuration Release \
  -sdk iphoneos \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
  BUILD_DIR="build" \
  SUPPORTED_PLATFORMS="iphoneos" \
  clean build


# Copy the built .framework to the desired location
if [ -d "$FRAMEWORK_BUILT_PATH" ]; then
    echo "=== Build Successful! ==="
    echo ".framework created at: $FRAMEWORK_BUILT_PATH"

else
    echo "=== Build Failed! ==="
    echo ".framework not found at: $FRAMEWORK_BUILT_PATH"
    exit 1
fi

