#!/bin/bash

# Exit on error
set -e

#------------------------------------------
# Helper: Check or set up npm
check_or_setup_npm() {
    # 1. Check if npm is already available
    if command -v npm >/dev/null 2>&1; then
        echo "✅ npm found at: $(which npm)"
        echo "npm version: $(npm -v)"
        return 0
    fi

    echo "npm not found. Trying to load via NVM..."

    export NVM_DIR="$HOME/.nvm"
    if [ -s "$NVM_DIR/nvm.sh" ]; then
        . "$NVM_DIR/nvm.sh"
        nvm use default >/dev/null 2>&1 || nvm use node >/dev/null 2>&1 || true
    fi

    if command -v npm >/dev/null 2>&1; then
        echo "✅ npm loaded via NVM"
        return 0
    fi

    echo "npm still not found. Trying Homebrew fallback (macOS only)..."
    if [[ "$OSTYPE" == "darwin"* ]] && command -v brew >/dev/null 2>&1; then
        BREW_NPM_PATH=$(brew --prefix node@20 2>/dev/null || brew --prefix node)
        if [ -n "$BREW_NPM_PATH" ] && [ -x "$BREW_NPM_PATH/bin/npm" ]; then
            export PATH="$BREW_NPM_PATH/bin:$PATH"
            echo "✅ npm loaded from Homebrew at: $BREW_NPM_PATH"
            return 0
        fi
    fi

    if [ -t 1 ]; then
        # User is in a terminal
        echo "npm not found. Installing dependencies via Homebrew..."

        if ! command -v brew &>/dev/null; then
            echo "Installing Homebrew..."
            /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
            eval "$(/opt/homebrew/bin/brew shellenv)"
        fi

        echo "Installing Node.js..."
        brew install node

        if command -v npm >/dev/null 2>&1; then
            echo "✅ npm installed successfully!"
            return 0
        else
            echo "❌ Failed to install npm"
            return 1
        fi
    else
        echo "❌ npm not found and cannot auto-install inside Unity/Editor context."
        echo ""
        echo "To install Node.js (includes npm), run:"
        echo "   1. curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash"
        echo "   2. source ~/.nvm/nvm.sh"
        echo "   3. nvm install --lts"
        exit 1
    fi
}

#------------------------------------------
# Step 1: Check or install npm
check_or_setup_npm || exit 1

#------------------------------------------------------------
# Config
PACKAGE_NAME="quickplay-core"
NPM_REGISTRY="https://inmobiartifactory.jfrog.io/artifactory/api/npm/npm-prod"
TEMP_DIR="temp_dlls"
UNITY_DLL_DIR="Platform/Assets/Plugins/Nostra"
REQUIRED_DLLS=("NostraCore.dll" "NostraRemote.dll" "QuickPlay.dll")

#------------------------------------------
# Setup temp dir
rm -rf "$TEMP_DIR"
mkdir -p "$TEMP_DIR"

# Move into temp dir and init dummy npm project
cd "$TEMP_DIR"
npm init -y > /dev/null
npm set registry "$NPM_REGISTRY"

#------------------------------------------
# Verify npm login
if ! npm whoami --registry="$NPM_REGISTRY" &> /dev/null; then
    echo "❌ Not logged into Artifactory. Please run:"
    echo "npm login --registry=$NPM_REGISTRY"
    exit 1
fi

#------------------------------------------
# Fetch and install latest version
echo "📦 Fetching latest version of $PACKAGE_NAME from Artifactory..."
LATEST_VERSION=$(npm view "$PACKAGE_NAME" version)
if [ -z "$LATEST_VERSION" ]; then
    echo "❌ Failed to get latest version"
    exit 1
fi

echo "📌 Latest version is: $LATEST_VERSION"

# Install the latest version
if ! npm install "${PACKAGE_NAME}@${LATEST_VERSION}"; then
    echo "❌ Failed to install package"
    exit 1
fi

# Go back to root
cd ..

#------------------------------------------
# Prepare Unity DLL destination
mkdir -p "$UNITY_DLL_DIR"

# Save version to a file inside Unity's Assets folder
echo "$LATEST_VERSION" > "$UNITY_DLL_DIR/dll-version.txt"

# Copy only required DLLs to Unity project
echo "📦 Copying new DLLs..."
UPDATED_COUNT=0
for dll in "${REQUIRED_DLLS[@]}"; do
    SOURCE_DLL=$(find "$TEMP_DIR/node_modules/$PACKAGE_NAME" -name "$dll")
    if [ -n "$SOURCE_DLL" ]; then
        cp "$SOURCE_DLL" "$UNITY_DLL_DIR/"
        ((UPDATED_COUNT++))
    else
        echo "⚠️ Warning: $dll not found in package"
    fi
done

if [ "$UPDATED_COUNT" -eq 0 ]; then
    echo "❌ No required DLLs found in package"
    exit 1
fi

echo "✅ DLLs from $PACKAGE_NAME@$LATEST_VERSION copied to $UNITY_DLL_DIR"
echo "📂 Updated DLLs:"
for dll in "${REQUIRED_DLLS[@]}"; do
    if [ -f "$UNITY_DLL_DIR/$dll" ]; then
        ls -lh "$UNITY_DLL_DIR/$dll"
    fi
done

# Cleanup
rm -rf "$TEMP_DIR"
echo "🧹 Cleaned up temporary files"
