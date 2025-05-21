#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Create hooks directory if it doesn't exist
mkdir -p .git/hooks

# Make hooks executable
chmod +x GitHooks/hooks/*

# Create symlinks
ln -sf ../../GitHooks/hooks/pre-push .git/hooks/pre-push

echo -e "${GREEN}✅ Git hooks installed successfully${NC}"
