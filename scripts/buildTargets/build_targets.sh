#!/bin/zsh
# build_targets.sh
# Usage: ./scripts/buildTargets/build_targets.sh [android] [web] [ios]
# If no arguments are given, all builds will run.

set -e  # Exit on any error

# Common variables
export UNITY_VERSION="6000.0.30f1"
export UNITY_PATH="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
export PROJECT_PATH="Platform"
export LOGS_DIR="logs"

# Individual build script paths
ANDROID_BUILD_SCRIPT="./scripts/buildTargets/build_aar.sh"
IOS_BUILD_SCRIPT="./scripts/buildTargets/build_ios_framework.sh"
WEB_BUILD_SCRIPT="./scripts/buildTargets/build_web.sh"

# Map arguments to build scripts
BUILD_ANDROID=false
BUILD_WEB=false
BUILD_IOS=false

if [[ $# -eq 0 ]]; then
  BUILD_ANDROID=true
  BUILD_WEB=true
  BUILD_IOS=true
else
  for arg in "$@"; do
    case $arg in
      android)
        BUILD_ANDROID=true
        ;;
      web)
        BUILD_WEB=true
        ;;
      ios)
        BUILD_IOS=true
        ;;
      *)
        echo "Unknown build target: $arg"
        echo "Usage: $0 [android] [web] [ios]"
        exit 1
        ;;
    esac
  done
fi

echo "==== Build Script Started ===="

if $BUILD_ANDROID; then
  echo "=== Android Build ==="
  if [[ -f "$ANDROID_BUILD_SCRIPT" ]]; then
    chmod +x "$ANDROID_BUILD_SCRIPT"
    "$ANDROID_BUILD_SCRIPT"
  else
    echo "Android build script ($ANDROID_BUILD_SCRIPT) not found!"
    exit 1
  fi
fi

if $BUILD_IOS; then
  echo "=== iOS Build ==="
  if [[ -f "$IOS_BUILD_SCRIPT" ]]; then
    chmod +x "$IOS_BUILD_SCRIPT"
    "$IOS_BUILD_SCRIPT"
  else
    echo "iOS build script ($IOS_BUILD_SCRIPT) not found!"
    exit 1
  fi
fi

if $BUILD_WEB; then
  echo "=== Web Build ==="
  if [[ -f "$WEB_BUILD_SCRIPT" ]]; then
    chmod +x "$WEB_BUILD_SCRIPT"
    "$WEB_BUILD_SCRIPT"
  else
    echo "Web build script ($WEB_BUILD_SCRIPT) not found!"
    exit 1
  fi
fi

echo "==== All selected builds completed successfully! ===="
