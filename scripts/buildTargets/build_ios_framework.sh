#!/bin/zsh

# Exit on any error
set -e

# --- Ensure Xcode is selected and license is accepted ---
# XCODE_PATH="/Applications/Xcode.app/Contents/Developer"
# if [ -d "$XCODE_PATH" ]; then
#     echo "Selecting Xcode at $XCODE_PATH"
#     sudo xcode-select -s "$XCODE_PATH"
#     sudo xcodebuild -license accept
# else
#     echo "Xcode not found at $XCODE_PATH. Please install Xcode from the App Store."
#     exit 1
# fi

# --- Configuration ---
# Use the common configuration from parent script
: "${UNITY_VERSION:?UNITY_VERSION is not set in parent script}"
: "${UNITY_PATH:?UNITY_PATH is not set in parent script}"   
: "${PROJECT_PATH:?PROJECT_PATH is not set in parent script}"
: "${LOGS_DIR:?LOGS_DIR is not set in parent script}"

# Define all key paths and names at the top for reuse
LOG_FILE="$LOGS_DIR/unity_ios_build.log"

# Print configuration
echo "=== Starting Unity iOS Framework build process ==="
echo "Project path: $PROJECT_PATH"
echo "Logs will be written to: $LOG_FILE"

# Create necessary directories
mkdir -p "$LOGS_DIR"

# ======================== Step 1: Unity Export =========================
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

# ========================= Step 2: Framework Build ========================

echo "=== Step 2: Building UnityFramework ==="
ROOT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)
echo "Root directory: $ROOT_DIR"
cd "$ROOT_DIR/Platform/Exports/iOS"

echo "=== Building Unity Framework ==="

# Configuration parameters
FRAMEWORK_NAME="UnityFramework"
OUTPUT_DIR="$ROOT_DIR/Platform/Build/Products/iphoneos"
FRAMEWORK_DIR="$OUTPUT_DIR/$FRAMEWORK_NAME.framework"
TEMP_DIR="$ROOT_DIR/temp_build"
UNITY_DATA_DIR="$ROOT_DIR/Platform/Exports/iOS/Data"

# Clean up
echo "=== Cleaning up output directory ==="
rm -rf "$OUTPUT_DIR"

# Create necessary directories
mkdir -p "$OUTPUT_DIR"
mkdir -p "$TEMP_DIR"

# Check if XCode project exists
if [ ! -d "Unity-iPhone.xcodeproj" ]; then
    echo "Error: Unity-iPhone.xcodeproj not found. Make sure you've exported your Unity project for iOS."
    exit 1
fi

echo "=== Building Unity Framework using XCode ==="
# Build the framework using xcodebuild
xcodebuild clean build \
    -project Unity-iPhone.xcodeproj \
    -scheme UnityFramework \
    -sdk iphoneos \
    -configuration Release \
    ENABLE_BITCODE=NO \
    ENABLE_TESTABILITY=NO \
    ARCHS="arm64" \
    SKIP_INSTALL=NO \
    BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
    DEFINES_MODULE=YES \
    -derivedDataPath "$TEMP_DIR"

# Check if build was successful
if [ ! -d "$TEMP_DIR/Build/Products/Release-iphoneos/$FRAMEWORK_NAME.framework" ]; then
    echo "Error: Framework build failed"
    exit 1
fi

# Copy the built framework to output directory
echo "=== Copying Framework to output directory ==="
cp -R "$TEMP_DIR/Build/Products/Release-iphoneos/$FRAMEWORK_NAME.framework" "$OUTPUT_DIR/"

# Make sure the framework exists
if [ ! -d "$FRAMEWORK_DIR" ]; then
    echo "Error: Framework not found at $FRAMEWORK_DIR"
    exit 1
fi

# Fix framework structure if needed
echo "=== Ensuring correct framework structure ==="

# Make sure Info.plist is in the right location
if [ -f "UnityFramework/Info.plist" ] && [ ! -f "$FRAMEWORK_DIR/Info.plist" ]; then
    cp "UnityFramework/Info.plist" "$FRAMEWORK_DIR/"
fi

# Fix the framework binary - iOS frameworks should have the binary directly in the root
echo "Ensuring binary is correctly placed in the framework..."

# First, completely remove the existing framework directory to avoid any lingering symbolic links
echo "Recreating framework structure..."
rm -rf "$FRAMEWORK_DIR"
mkdir -p "$FRAMEWORK_DIR"

# Copy the entire framework from temp_build - this ensures we get all needed files
echo "Copying complete framework from temp build..."
cp -R "$TEMP_DIR/Build/Products/Release-iphoneos/$FRAMEWORK_NAME.framework/"* "$FRAMEWORK_DIR/"

# Handle the case where UnityFramework might be a symbolic link
if [ -L "$FRAMEWORK_DIR/$FRAMEWORK_NAME" ]; then
    echo "Found symbolic link instead of actual binary. Fixing..."
    
    # Get the link target
    LINK_TARGET=$(readlink "$FRAMEWORK_DIR/$FRAMEWORK_NAME")
    echo "Symbolic link points to: $LINK_TARGET"
    
    # Check if this is a macOS-style framework symlink (pointing to Versions/Current/...)
    if [[ "$LINK_TARGET" == "Versions/Current/$FRAMEWORK_NAME" ]]; then
        echo "This is a macOS-style framework symlink. Creating proper iOS framework structure..."
        
        # Remove the symlink
        rm -f "$FRAMEWORK_DIR/$FRAMEWORK_NAME"
        
        # Find the actual binary in temp build
        TEMP_BINARY="$TEMP_DIR/Build/Products/Release-iphoneos/$FRAMEWORK_NAME.framework/$FRAMEWORK_NAME"
        
        if [ -f "$TEMP_BINARY" ]; then
            echo "Found binary in temp build at $TEMP_BINARY"
            cp -f "$TEMP_BINARY" "$FRAMEWORK_DIR/"
            echo "Copied binary to framework root"
        else
            echo "ERROR: Could not locate the UnityFramework binary file in expected location."
            exit 1
        fi
    else
        # If it's some other kind of symlink, try to resolve it
        echo "Non-standard symlink found. Attempting to resolve..."
        
        # Try to find the actual binary in the temp directory
        TEMP_BINARY="$TEMP_DIR/Build/Products/Release-iphoneos/$FRAMEWORK_NAME.framework/$FRAMEWORK_NAME"
        
        if [ -f "$TEMP_BINARY" ] && [ ! -L "$TEMP_BINARY" ]; then
            echo "Found actual binary in temp build. Copying to output..."
            cp -f "$TEMP_BINARY" "$FRAMEWORK_DIR/"
        else
            # Look harder for the binary
            FOUND_BINARY=$(find "$TEMP_DIR" -name "$FRAMEWORK_NAME" -type f | head -1)
            
            if [ -n "$FOUND_BINARY" ]; then
                echo "Found binary at $FOUND_BINARY. Copying to framework root..."
                cp -f "$FOUND_BINARY" "$FRAMEWORK_DIR/"
            else
                echo "ERROR: Could not locate the UnityFramework binary file. Build will fail."
                exit 1
            fi
        fi
    fi
elif [ ! -f "$FRAMEWORK_DIR/$FRAMEWORK_NAME" ]; then
    echo "Binary not found - looking for it..."
    
    # Look for the binary in the temp build directory
    TEMP_BINARY="$TEMP_DIR/Build/Products/Release-iphoneos/$FRAMEWORK_NAME.framework/$FRAMEWORK_NAME"
    
    if [ -f "$TEMP_BINARY" ]; then
        echo "Found binary in temp build at $TEMP_BINARY"
        cp -f "$TEMP_BINARY" "$FRAMEWORK_DIR/"
        echo "Copied binary to framework root"
    else
        # Try to find it elsewhere in temp directory
        FOUND_BINARY=$(find "$TEMP_DIR" -name "$FRAMEWORK_NAME" -type f | head -1)
        
        if [ -n "$FOUND_BINARY" ]; then
            echo "Found binary at $FOUND_BINARY. Copying to framework root..."
            cp -f "$FOUND_BINARY" "$FRAMEWORK_DIR/"
        else
            echo "ERROR: Could not locate the UnityFramework binary file. This will cause linking issues."
            exit 1
        fi
    fi
fi

# Make sure the binary is executable
if [ -f "$FRAMEWORK_DIR/$FRAMEWORK_NAME" ]; then
    chmod +x "$FRAMEWORK_DIR/$FRAMEWORK_NAME"
fi
cd -

# Verify the framework structure
echo "=== Verifying framework content ==="
ls -la "$FRAMEWORK_DIR"

# Clean up
echo "=== Cleaning up temporary files ==="
rm -rf "$TEMP_DIR"

# # Validate framework
# echo "=== Validating framework ==="
# if [ -f "$FRAMEWORK_DIR/$FRAMEWORK_NAME" ]; then
#     # Check if framework has expected architectures
#     ARCHS=$(lipo -info "$FRAMEWORK_DIR/$FRAMEWORK_NAME" | sed -e 's/.*are: //')
#     echo "Framework architectures: $ARCHS"
    
#     # Check for bitcode
#     otool -l "$FRAMEWORK_DIR/$FRAMEWORK_NAME" | grep LLVM 
#     if [ $? -eq 0 ]; then
#         echo "Framework includes bitcode"
#     else
#         echo "Framework does not include bitcode (which is expected if ENABLE_BITCODE=NO)"
#     fi
# else
#     echo "Warning: Framework binary not found at $FRAMEWORK_DIR/$FRAMEWORK_NAME"
# fi


# Create Data directory inside the framework if it doesn't exist
echo "=== Adding Data files to the framework ==="
mkdir -p "$FRAMEWORK_DIR/Data"

# Copy Data files to the framework
if [ -d "$UNITY_DATA_DIR" ]; then
    echo "Copying data files from $UNITY_DATA_DIR to $FRAMEWORK_DIR/Data/"
    cp -Rf "$UNITY_DATA_DIR"/* "$FRAMEWORK_DIR/Data/"
    echo "Data files copied successfully"
    
    # Verify Data folder contents were copied properly
    DATA_FILES_COUNT=$(find "$FRAMEWORK_DIR/Data" -type f | wc -l)
    echo "Copied $DATA_FILES_COUNT files to Data directory"
    
    if [ "$DATA_FILES_COUNT" -eq 0 ]; then
        echo "WARNING: No data files were copied. This may cause runtime issues."
    fi
else
    echo "Warning: Data directory not found at $UNITY_DATA_DIR"
fi

echo "=== Framework build completed ==="
echo "The framework is available at: $FRAMEWORK_DIR"
echo ""
echo "To use this framework in your iOS app:"
echo "1. Drag $FRAMEWORK_NAME.framework into your Xcode project"
echo "2. Add it to your target's 'Frameworks, Libraries, and Embedded Content' section"
echo "3. Set 'Embed & Sign' for the framework"
echo "4. Import it in your code with: #import <$FRAMEWORK_NAME/$FRAMEWORK_NAME.h>"
echo ""
