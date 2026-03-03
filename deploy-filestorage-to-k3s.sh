#!/bin/bash

# Deploy FileStorageService to k3s

set -e

# Set kubectl config
export KUBECONFIG=~/.kube/config

echo "========================================="
echo "FileStorageService k3s Deployment"
echo "========================================="
echo ""
echo "Using kubeconfig: $KUBECONFIG"
echo ""

NAMESPACE="webapp-noauth"

# Check if k3s is accessible
if ! kubectl get nodes >/dev/null 2>&1; then
    echo "✗ k3s is not accessible. Please ensure k3s is running."
    echo "Try: sudo systemctl start k3s"
    exit 1
fi

echo "✓ k3s cluster is accessible"
echo ""

# Build Docker image
echo "Building FileStorageService Docker image..."
cd /home/petr/RiderProjects/WebAppNoAuth
docker build -t localhost:5000/filestorageservice:latest -f FileStorageService/Dockerfile .
echo "✓ Docker image built"
echo ""

# Push to local registry (if needed)
echo "Pushing image to local registry..."
docker push localhost:5000/filestorageservice:latest || {
    echo "⚠ Failed to push to localhost:5000, trying alternative approach..."
    # For k3s, we might need to import the image directly
    docker save filestorageservice:latest | k3d image import filestorageservice:latest --cluster k3s-default 2>/dev/null || true
}
echo ""

# Apply namespace
echo "Applying namespace..."
kubectl apply -f k8s/01-namespace.yaml
echo ""

# Apply PVC first
echo "Applying PersistentVolumeClaim..."
kubectl apply -f k8s/09-filestorage-pvc.yaml
echo "Waiting for PVC to be bound..."
kubectl wait --for=jsonpath='{.status.phase}'=Bound pvc/file-storage-pvc -n $NAMESPACE --timeout=60s || {
    echo "⚠ PVC binding timeout, continuing anyway..."
}
echo ""

# Apply ConfigMap for FileStorageService (if needed)
echo "Applying FileStorageService configuration..."
# You might need to create a configmap with appsettings
kubectl create configmap filestorage-config \
  --from-literal=ASPNETCORE_ENVIRONMENT=Production \
  --from-literal=FileStorage__StoragePath=/app/storage \
  -n $NAMESPACE \
  --dry-run=client -o yaml | kubectl apply -f -
echo ""

# Apply deployment and service
echo "Applying FileStorageService deployment..."
kubectl apply -f k8s/08-filestorage-deployment.yaml
echo ""

# Wait for deployment
echo "Waiting for FileStorageService pod to be ready..."
kubectl wait --for=condition=ready pod -l app=filestorageservice -n $NAMESPACE --timeout=120s || {
    echo "⚠ Pod not ready within timeout. Checking status..."
    kubectl get pods -n $NAMESPACE
    kubectl describe pod -l app=filestorageservice -n $NAMESPACE
    exit 1
}
echo ""

# Show deployment info
echo "========================================="
echo "Deployment Summary"
echo "========================================="
echo ""
kubectl get deployment filestorageservice -n $NAMESPACE
echo ""
kubectl get pods -l app=filestorageservice -n $NAMESPACE
echo ""
kubectl get service filestorageservice-service -n $NAMESPACE
echo ""
kubectl get pvc file-storage-pvc -n $NAMESPACE
echo ""

# Get access information
SERVICE_TYPE=$(kubectl get service filestorageservice-service -n $NAMESPACE -o jsonpath='{.spec.type}')
NODE_PORT=$(kubectl get service filestorageservice-service -n $NAMESPACE -o jsonpath='{.spec.ports[0].nodePort}' 2>/dev/null || echo "")

echo "Access Information:"
if [ "$SERVICE_TYPE" = "NodePort" ] && [ ! -z "$NODE_PORT" ]; then
    NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[0].address}')
    echo "  Service Type: NodePort"
    echo "  Node Port: $NODE_PORT"
    echo "  Access URL: http://$NODE_IP:$NODE_PORT"
    echo "  Health Check: http://$NODE_IP:$NODE_PORT/health"
elif [ "$SERVICE_TYPE" = "ClusterIP" ]; then
    echo "  Service Type: ClusterIP"
    echo "  Use port-forward: kubectl port-forward service/filestorageservice-service -n $NAMESPACE 8081:8080"
    echo "  Then access: http://localhost:8081"
fi
echo ""

echo "Testing deployment..."
sleep 5

# Try health check
if [ "$SERVICE_TYPE" = "NodePort" ] && [ ! -z "$NODE_PORT" ]; then
    HEALTH_URL="http://$NODE_IP:$NODE_PORT/health"
else
    # Start port-forward for testing
    kubectl port-forward service/filestorageservice-service -n $NAMESPACE 8081:8080 &
    PORT_FORWARD_PID=$!
    sleep 3
    HEALTH_URL="http://localhost:8081/health"
    
    trap "kill $PORT_FORWARD_PID 2>/dev/null" EXIT
fi

echo "Checking health endpoint: $HEALTH_URL"
curl -s "$HEALTH_URL" | jq . || echo "⚠ Health check failed or jq not installed"

echo ""
echo "✓ FileStorageService deployed successfully!"
echo ""
echo "Next steps:"
echo "  1. Run: ./test-filestorage-k3s.sh"
echo "  2. Or manually test: curl $HEALTH_URL"
