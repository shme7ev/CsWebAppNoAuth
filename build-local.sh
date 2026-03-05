#!/bin/bash

# Build script for local development with localhost:5000 registry
# Usage: ./build-local.sh [version] [service]
#   version: Optional version tag (default: dev-$(date +%Y%m%d-%H%M%S))
#   service: Optional service name (webapp, filestorage, or both)

set -e

# Configuration
REGISTRY="localhost:5000"
WEBAPP_IMAGE="$REGISTRY/webappnoauth"
FILESTORAGE_IMAGE="$REGISTRY/filestorageservice"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "running in $SCRIPT_DIR"

# Parse arguments
VERSION="${1:-dev-$(date +%Y%m%d-%H%M%S)}"
SERVICE="${2:-both}"

echo "========================================="
echo "Local Build Script"
echo "========================================="
echo "Version: $VERSION"
echo "Service: $SERVICE"
echo "Registry: $REGISTRY"
echo "========================================="

# Function to check if registry is running
check_registry() {
    echo "Checking if local registry is running..."
    if ! docker ps | grep -q "registry"; then
        echo "Warning: Local registry not found. Starting one..."
        docker run -d -p 5000:5000 --name registry registry:2
        sleep 3
    fi
    
    # Test if registry is accessible
    if ! curl -s http://localhost:5000/v2/_catalog > /dev/null; then
        echo "Error: Cannot connect to local registry at localhost:5000"
        echo "Please ensure Docker registry is running:"
        echo "  docker run -d -p 5000:5000 --name registry registry:2"
        exit 1
    fi
    echo "Success: Local registry is accessible"
}

# Function to build and push WebAppNoAuth
build_webapp() {
    echo ""
    echo "Building WebAppNoAuth..."
#    cd "$SCRIPT_DIR/WebAppNoAuth"
    
    echo "  Building Docker image...$WEBAPP_IMAGE:$VERSION"
    docker build -t "$WEBAPP_IMAGE:$VERSION" -f WebAppNoAuth/Dockerfile .
    
    echo "  Pushing to local registry..."
    docker push "$WEBAPP_IMAGE:$VERSION"
    
    echo "  Success: WebAppNoAuth built and pushed: $WEBAPP_IMAGE:$VERSION"
}

# Function to build and push FileStorageService
build_filestorage() {
    echo ""
    echo "Building FileStorageService... $FILESTORAGE_IMAGE:$VERSION"
#    cd "$SCRIPT_DIR/FileStorageService"
    
    echo "  Building Docker image..."
    docker build -t "$FILESTORAGE_IMAGE:$VERSION" -f FileStorageService/Dockerfile .
    
    echo "  Pushing to local registry..."
    docker push "$FILESTORAGE_IMAGE:$VERSION"
    
    echo "  Success: FileStorageService built and pushed: $FILESTORAGE_IMAGE:$VERSION"
}

# Function to update Kubernetes manifests
update_k8s_manifests() {
    local webapp_tag="$1"
    local filestorage_tag="$2"
    
    echo ""
    echo "Updating Kubernetes manifests..."
    
    # Update WebAppNoAuth deployment
    if [ "$webapp_tag" != "" ]; then
        echo "  Updating WebAppNoAuth image in k8s/06-app-deployment.yaml"
        sed -i "s|image: $WEBAPP_IMAGE:.*|image: $WEBAPP_IMAGE:$webapp_tag|" \
            "$SCRIPT_DIR/k8s/06-app-deployment.yaml"
    fi
    
    # Update FileStorageService deployment
    if [ "$filestorage_tag" != "" ]; then
        echo "  Updating FileStorageService image in k8s/08-filestorage-deployment.yaml"
        sed -i "s|image: $FILESTORAGE_IMAGE:.*|image: $FILESTORAGE_IMAGE:$filestorage_tag|" \
            "$SCRIPT_DIR/k8s/08-filestorage-deployment.yaml"
    fi
    
    echo "  Success: Kubernetes manifests updated"
}

# Main execution
check_registry

case "$SERVICE" in
    webapp)
        build_webapp
        update_k8s_manifests "$VERSION" ""
        ;;
    filestorage)
        build_filestorage
        update_k8s_manifests "" "$VERSION"
        ;;
    both|*)
        build_webapp
        build_filestorage
        update_k8s_manifests "$VERSION" "$VERSION"
        ;;
esac

echo ""
echo "========================================="
echo "Build Complete!"
echo "========================================="
echo ""
echo "Next steps:"
echo "  1. Review changes: git diff k8s/"
echo "  2. Apply to cluster: kubectl apply -f k8s/"
echo "  3. Commit and push: git add k8s/*.yaml && git commit -m 'Update to $VERSION'"
echo ""
echo "Images available at:"
if [ "$SERVICE" = "webapp" ] || [ "$SERVICE" = "both" ]; then
    echo "  - $WEBAPP_IMAGE:$VERSION"
fi
if [ "$SERVICE" = "filestorage" ] || [ "$SERVICE" = "both" ]; then
    echo "  - $FILESTORAGE_IMAGE:$VERSION"
fi
echo ""
