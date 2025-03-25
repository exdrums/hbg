#!/bin/bash

# Angular Build Script
# This script handles the Angular build process with environment selection
# and optional parameters

# Default configuration
ENV="development"
BUILD_TYPE="build"
AOT=true
STATS=false
SOURCE_MAP=false
OPTIMIZATION=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --env=*)
      ENV="${1#*=}"
      ;;
    --prod)
      ENV="production"
      OPTIMIZATION=true
      SOURCE_MAP=false
      ;;
    --aot=*)
      AOT="${1#*=}"
      ;;
    --stats)
      STATS=true
      ;;
    --source-map)
      SOURCE_MAP=true
      ;;
    --watch)
      BUILD_TYPE="watch"
      ;;
    --help)
      echo "Usage: ./start.sh [options]"
      echo "Options:"
      echo "  --env=<environment>   Specify environment (development, staging, production)"
      echo "  --prod                Shorthand for production environment with optimizations"
      echo "  --aot=<true|false>    Enable/disable ahead-of-time compilation"
      echo "  --stats               Generate build statistics"
      echo "  --source-map          Generate source maps"
      echo "  --watch               Watch for changes and rebuild"
      echo "  --help                Show this help message"
      exit 0
      ;;
    *)
      echo "Unknown option: $1"
      echo "Use --help for available options"
      exit 1
      ;;
  esac
  shift
done

# Log configuration
echo "Starting Angular build with configuration:"
echo "- Environment: $ENV"
echo "- Build type: $BUILD_TYPE"
echo "- AOT: $AOT"
echo "- Stats: $STATS"
echo "- Source maps: $SOURCE_MAP"
echo "- Optimization: $OPTIMIZATION"

# Build the Angular command
CMD="ng $BUILD_TYPE --configuration=$ENV"

# Add additional flags based on options
if [ "$AOT" = true ]; then
  CMD="$CMD --aot"
fi

if [ "$STATS" = true ]; then
  CMD="$CMD --stats-json"
fi

if [ "$SOURCE_MAP" = true ]; then
  CMD="$CMD --source-map"
fi

if [ "$OPTIMIZATION" = true ]; then
  CMD="$CMD --optimization"
fi

# Execute the command
echo "Executing: $CMD"
eval "cd ./src/Web/Web.SPA/Client"
eval $CMD

# Check the build result
if [ $? -eq 0 ]; then
  echo "Build completed successfully."
  
  # If it's a production build, show the bundle sizes
  if [ "$ENV" = "production" ]; then
    echo "Bundle sizes:"
    du -sh dist/*
  fi
else
  echo "Build failed with error code $?."
  exit 1
fi