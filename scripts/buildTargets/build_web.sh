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
EXPORT_PATH="$PROJECT_PATH/Exports/Web"
BUILD_PATH="$PROJECT_PATH/Build/Web"
LOG_PATH="$LOGS_DIR/unity_web_build.log"
ZIP_NAME="qp-platform-web.zip"


# Create directories
mkdir -p "$EXPORT_PATH"
mkdir -p "$BUILD_PATH"
mkdir -p "$LOGS_DIR"


# Clean build directory
echo -e "\n=== Cleaning Build Directory ==="
rm -f "$BUILD_PATH"/*.zip && echo "✓ Cleaned $BUILD_PATH"

# Export from Unity
echo "Building WebGL..."
"$UNITY_PATH" \
    -quit \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -logFile "$LOG_PATH" \
    -executeMethod nostra.platform.build.BuildWeb.ExportWebGL

# Package the exported files
echo "Creating zip file..."
cd "$EXPORT_PATH"
zip -r "../../../$BUILD_PATH/$ZIP_NAME" .

echo "=== Build Complete ==="
echo "- Web files: $EXPORT_PATH"
echo "- Zip file: $BUILD_PATH/$ZIP_NAME"
echo "- Build log: $LOG_PATH"