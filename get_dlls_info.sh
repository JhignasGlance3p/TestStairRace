#!/bin/bash
set -e

# Load npm properly (like in update_dlls.sh)
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

if ! command_exists npm; then
    export NVM_DIR="$HOME/.nvm"
    if [ -s "$NVM_DIR/nvm.sh" ]; then
        . "$NVM_DIR/nvm.sh"
        nvm use default >/dev/null 2>&1 || true
    fi

    if ! command_exists npm; then
        if [[ "$OSTYPE" == "darwin"* ]] && command_exists brew; then
            BREW_NPM_PATH=$(brew --prefix node@20 2>/dev/null || brew --prefix node)
            if [ -x "$BREW_NPM_PATH/bin/npm" ]; then
                export PATH="$BREW_NPM_PATH/bin:$PATH"
            fi
        fi
    fi
fi

if ! command_exists npm; then
    echo "❌ npm not found"
    exit 1
fi

echo "npm available at: $(which npm)"
echo "npm version: $(npm -v)"

PACKAGE_NAME="quickplay-core"
NPM_REGISTRY="https://inmobiartifactory.jfrog.io/artifactory/api/npm/npm-prod"

echo "Fetching version of $PACKAGE_NAME..."
PACKAGE_VERSION=$(npm view "$PACKAGE_NAME" version --registry "$NPM_REGISTRY" || echo "Not logged in")
echo "$PACKAGE_NAME version: $PACKAGE_VERSION"
