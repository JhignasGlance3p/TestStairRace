#!/bin/bash

# Error handling
set -e

# Source shell configuration to get updated PATH
if [ -f ~/.zshrc ]; then
    source ~/.zshrc
fi

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
CLOUDSMITH_ORG="glance"
CLOUDSMITH_REPO="nostra-qp-games-dlls"
CLOUDSMITH_API_KEY="6bd94d07f0c1a843eed01a200c9c531e6118e872"
GAMES_BASE_DIR="Platform/Assets/Games"
TEMP_DIR="/tmp/game_dll"

# Check if game name was provided as argument
if [ -n "$1" ]; then
    GAME_DLL_NAME="$1"
    # Ensure it has .dll extension
    if [[ "$GAME_DLL_NAME" != *.dll ]]; then
        GAME_DLL_NAME="${GAME_DLL_NAME}.dll"
    fi
    # Extract game name without extension for target directory
    GAME_NAME="${GAME_DLL_NAME%.dll}"
    TARGET_DIR="$GAMES_BASE_DIR/$GAME_NAME"
    echo -e "Game DLL: ${YELLOW}$GAME_DLL_NAME${NC}"
    echo -e "Target directory: ${YELLOW}$TARGET_DIR${NC}"
else
    echo -e "${RED}No game name provided.${NC}"
    echo -e "Usage: $0 <GameName>"
    echo -e "Example: $0 ColorClash"
    exit 1
fi

# Check if Cloudsmith CLI is installed
if ! command -v cloudsmith &> /dev/null; then
    echo -e "${RED}Cloudsmith CLI is not installed${NC}"
    echo "Install it using: pip3 install --user --upgrade cloudsmith-cli"
    exit 1
fi

# Verify API access
function verify_cloudsmith_access() {
    echo -e "Verifying Cloudsmith access..."
    local API_RESPONSE
    API_RESPONSE=$(curl -s -H "X-Api-Key: $CLOUDSMITH_API_KEY" "https://api.cloudsmith.io/v1/repos/$CLOUDSMITH_ORG/$CLOUDSMITH_REPO/")
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to access Cloudsmith repository. API Response:${NC}"
        echo "$API_RESPONSE"
        exit 1
    fi
    
    echo -e "${GREEN}Cloudsmith access verified successfully${NC}"
}

# Create temporary directory
function setup_temp_dir() {
    echo -e "Setting up temporary directory..."
    rm -rf "$TEMP_DIR"
    mkdir -p "$TEMP_DIR"
}

# Download the latest game DLL
function download_latest_game_dll() {
    echo -e "\n${YELLOW}Fetching latest $GAME_DLL_NAME from Cloudsmith...${NC}"
    
    # Query API for the latest version of the specific DLL
    local API_URL="https://api.cloudsmith.io/v1/packages/$CLOUDSMITH_ORG/$CLOUDSMITH_REPO/?page=1&page_size=25&query=$GAME_DLL_NAME&ordering=-version"
    
    local API_RESPONSE
    API_RESPONSE=$(curl -s -H "X-Api-Key: $CLOUDSMITH_API_KEY" "$API_URL")
    
    # Extract the CDN URL and version of the first (latest) result
    local CDN_URL
    CDN_URL=$(echo "$API_RESPONSE" | grep -o '"cdn_url": *"[^"]*' | head -n1 | cut -d'"' -f4 | tr -d '\r\n')
    
    local VERSION
    VERSION=$(echo "$API_RESPONSE" | grep -o '"version": *"[^"]*' | head -n1 | cut -d'"' -f4 | tr -d '\r\n')
    
    if [ -z "$CDN_URL" ] || [ -z "$VERSION" ]; then
        echo -e "${RED}Failed to retrieve download URL or version from Cloudsmith API${NC}"
        echo -e "${RED}Make sure $GAME_DLL_NAME exists in the repository${NC}"
        exit 1
    fi
    
    echo -e "Latest version: ${GREEN}$VERSION${NC}"
    
    # Download the DLL with authentication header
    echo -e "Downloading $GAME_DLL_NAME..."
    curl -fL -H "X-Api-Key: $CLOUDSMITH_API_KEY" "$CDN_URL" -o "$TEMP_DIR/$GAME_DLL_NAME"
    
    if [ $? -ne 0 ] || [ ! -s "$TEMP_DIR/$GAME_DLL_NAME" ]; then
        echo -e "${RED}Download failed or file is empty${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Download successful!${NC}"
}

# Update the game DLL in the target directory
function update_game_dll() {
    echo -e "\n${YELLOW}Updating $GAME_DLL_NAME in $TARGET_DIR...${NC}"
    
    # Create target directory if it doesn't exist
    mkdir -p "$TARGET_DIR"
    
    # Copy the DLL to the target directory
    cp "$TEMP_DIR/$GAME_DLL_NAME" "$TARGET_DIR/"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to copy $GAME_DLL_NAME to $TARGET_DIR${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}$GAME_DLL_NAME updated successfully!${NC}"
}

# Cleanup temporary files
function cleanup() {
    echo -e "\nCleaning up temporary files..."
    rm -rf "$TEMP_DIR"
}

# === Main Execution ===

echo -e "Starting game DLL download process..."

verify_cloudsmith_access
setup_temp_dir
download_latest_game_dll
update_game_dll
cleanup

echo -e "\n${GREEN}Game DLL download and update completed successfully!${NC}"