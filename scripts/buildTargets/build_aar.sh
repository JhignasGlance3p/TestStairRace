#!/bin/bash

# Exit on any error
set -e


# --- Configuration ---
# Use the common configuration from parent script
: "${UNITY_VERSION:?UNITY_VERSION is not set in parent script}"
: "${UNITY_PATH:?UNITY_PATH is not set in parent script}"   
: "${PROJECT_PATH:?PROJECT_PATH is not set in parent script}"
: "${LOGS_DIR:?LOGS_DIR is not set in parent script}"

# Define all key paths and names at the top for reuse
LOG_FILE="$LOGS_DIR/unity_android_build.log"
EXPORT_PATH="$PROJECT_PATH/Exports/Android"
AAR_NAME="unityLibrary-release.aar"
LOCAL_AAR_PATH="unityLibrary/build/outputs/aar/$AAR_NAME"

# Print configuration
echo "=== Starting Unity AAR build process ==="
echo "Project path: $PROJECT_PATH"
echo "Logs will be written to: $LOG_FILE"

# Create necessary directories
mkdir -p "$LOGS_DIR"

# Step 1: Unity Export
echo "=== Step 1: Exporting Unity Project to Android ==="
"$UNITY_PATH" \
    -quit \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -logFile "$LOG_FILE" \
    -executeMethod nostra.platform.build.BuildAAR.ExportAndroidAAR

if [ $? -ne 0 ]; then
    echo "Unity export failed! Check $LOG_FILE for details"
    tail -n 50 "$LOG_FILE"
    exit 1
fi

echo "Unity export completed successfully!"

# Step 2: Gradle Build
echo "=== Step 2: Building AAR using Gradle ==="

# Change to export directory for Gradle build
cd "$EXPORT_PATH"

# Initialize Gradle wrapper if needed
if [ ! -f "gradlew" ]; then
    echo "Initializing Gradle wrapper..."
    gradle wrapper
fi

# Ensure gradlew is executable
chmod +x gradlew

# Build AAR
echo "Building AAR..."
./gradlew :unityLibrary:assembleRelease

# Verify AAR was created
if [ -f "$LOCAL_AAR_PATH" ]; then
    echo "=== Build Successful! ==="
    
    # Get AAR file size
    AAR_SIZE=$(ls -lh "$LOCAL_AAR_PATH" | awk '{print $5}')
    echo "AAR file created (Size: $AAR_SIZE)"
    echo "AAR file path: $LOCAL_AAR_PATH"
    
else
    echo "=== Build Failed! ==="
    echo "AAR file not found at: $LOCAL_AAR_PATH"
    echo "Current directory: $(pwd)"
    echo "Contents of expected AAR directory:"
    ls -la unityLibrary/build/outputs/aar/
    exit 1
fi
