#!/bin/bash

# Test script for FileStorageService in k3s with persistent volume verification

set -e

# Set kubectl config
export KUBECONFIG=~/.kube/config

echo "========================================="
echo "FileStorageService k3s Integration Test"
echo "========================================="
echo ""
echo "Using kubeconfig: $KUBECONFIG"
echo ""

# Configuration
NAMESPACE="webapp-noauth"
FILE_SERVICE_POD_LABEL="app=filestorageservice"
POSTGRES_POD_LABEL="app=postgres"
FILE_SERVICE_URL="http://localhost:30081"  # Assuming NodePort or port-forward

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_status() {
    if [ "$1" -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
    fi
}

print_info() {
    echo -e "${YELLOW}ℹ${NC} $1"
}

# Test 1: Check k3s cluster accessibility
echo "1. Checking k3s cluster accessibility..."
if kubectl get nodes >/dev/null 2>&1; then
    print_status 0 "k3s cluster is accessible"
    kubectl get nodes
else
    print_status 1 "k3s cluster is not accessible"
    exit 1
fi
echo ""

# Test 2: Check namespace exists
echo "2. Checking namespace..."
if kubectl get namespace $NAMESPACE >/dev/null 2>&1; then
    print_status 0 "Namespace $NAMESPACE exists"
else
    print_status 1 "Namespace $NAMESPACE does not exist"
    echo "Creating namespace..."
    kubectl create namespace $NAMESPACE
fi
echo ""

# Test 3: Check PersistentVolumeClaim
echo "3. Checking PersistentVolumeClaim..."
PVC_STATUS=$(kubectl get pvc file-storage-pvc -n $NAMESPACE -o jsonpath='{.status.phase}' 2>/dev/null || echo "NotFound")
if [ "$PVC_STATUS" = "Bound" ]; then
    print_status 0 "PVC file-storage-pvc is Bound"
    kubectl get pvc file-storage-pvc -n $NAMESPACE
    PVC_STORAGE_CLASS=$(kubectl get pvc file-storage-pvc -n $NAMESPACE -o jsonpath='{.spec.storageClassName}')
    PVC_CAPACITY=$(kubectl get pvc file-storage-pvc -n $NAMESPACE -o jsonpath='{.status.capacity.storage}')
    print_info "Storage Class: $PVC_STORAGE, Capacity: $PVC_CAPACITY"
else
    print_status 1 "PVC file-storage-pvc status: $PVC_STATUS"
    echo "PVC details:"
    kubectl describe pvc file-storage-pvc -n $NAMESPACE
fi
echo ""

# Test 4: Check PersistentVolume
echo "4. Checking PersistentVolume..."
PV_NAME=$(kubectl get pvc file-storage-pvc -n $NAMESPACE -o jsonpath='{.spec.volumeName}' 2>/dev/null || echo "")
if [ ! -z "$PV_NAME" ]; then
    print_status 0 "PV found: $PV_NAME"
    PV_STATUS=$(kubectl get pv $PV_NAME -o jsonpath='{.status.phase}')
    print_info "PV Status: $PV_STATUS"
    kubectl get pv $PV_NAME
else
    print_status 1 "No PV bound to PVC"
fi
echo ""

# Test 5: Check FileStorageService pod
echo "5. Checking FileStorageService pod..."
FILE_POD=$(kubectl get pods -n $NAMESPACE -l $FILE_SERVICE_POD_LABEL -o jsonpath='{.items[0].metadata.name}' 2>/dev/null || echo "")
if [ ! -z "$FILE_POD" ]; then
    print_status 0 "FileStorageService pod found: $FILE_POD"
    POD_STATUS=$(kubectl get pod $FILE_POD -n $NAMESPACE -o jsonpath='{.status.phase}')
    print_info "Pod Status: $POD_STATUS"
    
    if [ "$POD_STATUS" = "Running" ]; then
        print_status 0 "Pod is running"
    else
        print_status 1 "Pod is not running: $POD_STATUS"
        echo "Pod events:"
        kubectl describe pod $FILE_POD -n $NAMESPACE | grep -A 10 "Events:"
    fi
    
    # Check volume mount
    print_info "Checking volume mount..."
    kubectl get pod $FILE_POD -n $NAMESPACE -o jsonpath='{.spec.volumes}' | jq .
    echo ""
else
    print_status 1 "FileStorageService pod not found"
    echo "Available pods in namespace:"
    kubectl get pods -n $NAMESPACE
fi
echo ""

# Test 6: Check service endpoint
echo "6. Checking FileStorageService service..."
SERVICE_EXISTS=$(kubectl get service filestorageservice-service -n $NAMESPACE >/dev/null 2>&1 && echo "true" || echo "false")
if [ "$SERVICE_EXISTS" = "true" ]; then
    print_status 0 "Service filestorageservice-service exists"
    kubectl get service filestorageservice-service -n $NAMESPACE
    SERVICE_TYPE=$(kubectl get service filestorageservice-service -n $NAMESPACE -o jsonpath='{.spec.type}')
    print_info "Service Type: $SERVICE_TYPE"
    
    if [ "$SERVICE_TYPE" = "NodePort" ] || [ "$SERVICE_TYPE" = "LoadBalancer" ]; then
        NODE_PORT=$(kubectl get service filestorageservice-service -n $NAMESPACE -o jsonpath='{.spec.ports[0].nodePort}')
        print_info "Node Port: $NODE_PORT"
        FILE_SERVICE_URL="http://$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[0].address}'):${NODE_PORT}"
        print_info "Service URL: $FILE_SERVICE_URL"
    fi
else
    print_status 1 "Service filestorageservice-service not found"
fi
echo ""

# Test 7: Port-forward if needed
echo "7. Setting up port-forward (if needed)..."
if [ "$SERVICE_TYPE" = "ClusterIP" ]; then
    print_info "Service is ClusterIP, setting up port-forward..."
    # Kill any existing port-forward
    pkill -f "kubectl port-forward.*filestorageservice-service" 2>/dev/null || true
    sleep 2
    
    # Start new port-forward in background
    kubectl port-forward service/filestorageservice-service -n $NAMESPACE 8081:8080 &
    PORT_FORWARD_PID=$!
    sleep 3
    
    if kill -0 $PORT_FORWARD_PID 2>/dev/null; then
        print_status 0 "Port-forward started on port 8081"
        FILE_SERVICE_URL="http://localhost:8081"
    else
        print_status 1 "Failed to start port-forward"
    fi
else
    print_info "Using direct service URL: $FILE_SERVICE_URL"
fi
echo ""

# Test 8: Health check
echo "8. Testing health endpoint..."
HEALTH_RESPONSE=$(curl -s "$FILE_SERVICE_URL/health" 2>/dev/null || echo "")
if echo "$HEALTH_RESPONSE" | grep -q "Healthy"; then
    print_status 0 "Health endpoint responded: $HEALTH_RESPONSE"
else
    print_status 1 "Health endpoint failed"
    echo "Response: $HEALTH_RESPONSE"
fi
echo ""

# Test 9: Upload a test file
echo "9. Testing file upload..."
TEST_CONTENT="Test file content for k3s persistent volume verification - $(date)"
echo "$TEST_CONTENT" > /tmp/test-k3s-file.txt

UPLOAD_RESPONSE=$(curl -s -X POST \
  -H "X-Uploaded-By: k3s-test-user" \
  -F "file=@/tmp/test-k3s-file.txt" \
  -F "description=Test file for k3s PV verification" \
  "$FILE_SERVICE_URL/api/files/upload" 2>/dev/null || echo "")

echo "Upload response: $UPLOAD_RESPONSE"

FILE_ID=$(echo "$UPLOAD_RESPONSE" | grep -o '"fileId":"[^"]*' | cut -d'"' -f4 || echo "")

if [ ! -z "$FILE_ID" ]; then
    print_status 0 "File uploaded successfully with ID: $FILE_ID"
else
    print_status 1 "File upload failed"
    exit 1
fi
echo ""

# Test 10: Verify file metadata
echo "10. Verifying file metadata..."
METADATA_RESPONSE=$(curl -s "$FILE_SERVICE_URL/api/files/$FILE_ID/metadata" 2>/dev/null || echo "")
echo "Metadata: $METADATA_RESPONSE"

if echo "$METADATA_RESPONSE" | grep -q "$FILE_ID"; then
    print_status 0 "File metadata retrieved successfully"
else
    print_status 1 "Failed to retrieve file metadata"
fi
echo ""

# Test 11: Download and verify file content
echo "11. Testing file download..."
curl -s "$FILE_SERVICE_URL/api/files/$FILE_ID" -o /tmp/downloaded-test-file.txt 2>/dev/null

if [ -f "/tmp/downloaded-test-file.txt" ]; then
    DOWNLOADED_CONTENT=$(cat /tmp/downloaded-test-file.txt)
    if [ "$DOWNLOADED_CONTENT" = "$TEST_CONTENT" ]; then
        print_status 0 "File content verified successfully"
        print_info "Content: $DOWNLOADED_CONTENT"
    else
        print_status 1 "File content mismatch"
        echo "Expected: $TEST_CONTENT"
        echo "Got: $DOWNLOADED_CONTENT"
    fi
else
    print_status 1 "Failed to download file"
fi
echo ""

# Test 12: List files
echo "12. Testing list files..."
LIST_RESPONSE=$(curl -s "$FILE_SERVICE_URL/api/files" 2>/dev/null || echo "")
print_info "Files in system: $LIST_RESPONSE"

if echo "$LIST_RESPONSE" | grep -q "$FILE_ID"; then
    print_status 0 "File appears in file list"
else
    print_status 1 "File not found in file list"
fi
echo ""

# Test 13: Verify file in persistent volume (CRITICAL TEST)
echo "13. Verifying file in persistent volume..."
if [ ! -z "$FILE_POD" ]; then
    print_info "Checking storage directory in pod..."
    kubectl exec -n $NAMESPACE $FILE_POD -- ls -la /app/storage/ || true
    
    print_info "Looking for uploaded file..."
    FILE_IN_PV=$(kubectl exec -n $NAMESPACE $FILE_POD -- find /app/storage -name "*${FILE_ID}*" 2>/dev/null || echo "")
    
    if [ ! -z "$FILE_IN_PV" ]; then
        print_status 0 "File found in persistent volume!"
        echo "File path in PV: $FILE_IN_PV"
        
        # Show file content from PV
        print_info "File content from PV:"
        kubectl exec -n $NAMESPACE $FILE_POD -- cat "$FILE_IN_PV"
    else
        print_status 1 "File NOT found in persistent volume"
        echo "This indicates files are NOT being persisted!"
        echo "Contents of /app/storage:"
        kubectl exec -n $NAMESPACE $FILE_POD -- ls -laR /app/storage/
    fi
else
    print_status 1 "Cannot verify PV - pod not available"
fi
echo ""

# Test 14: Verify database record
echo "14. Verifying database record..."
DB_POD=$(kubectl get pods -n $NAMESPACE -l $POSTGRES_POD_LABEL -o jsonpath='{.items[0].metadata.name}' 2>/dev/null || echo "")

if [ ! -z "$DB_POD" ]; then
    print_info "Checking Files table in PostgreSQL..."
    DB_RECORD=$(kubectl exec -n $NAMESPACE $DB_POD -- psql -U postgres -d webapp_db -c "SELECT id, original_file_name, size, uploaded_by FROM \"Files\" WHERE id = '$FILE_ID';" 2>/dev/null || echo "")
    
    if echo "$DB_RECORD" | grep -q "$FILE_ID"; then
        print_status 0 "Database record found"
        echo "$DB_RECORD"
    else
        print_status 1 "Database record not found"
        echo "All records:"
        kubectl exec -n $NAMESPACE $DB_POD -- psql -U postgres -d webapp_db -c "SELECT * FROM \"Files\";" 2>/dev/null || echo "Unable to query database"
    fi
else
    print_status 1 "Database pod not available"
fi
echo ""

# Test 15: Delete test file
echo "15. Cleaning up test file..."
DELETE_RESPONSE=$(curl -s -X DELETE \
  -H "X-Deleted-By: k3s-test-user" \
  "$FILE_SERVICE_URL/api/files/$FILE_ID" 2>/dev/null || echo "")
echo "Delete response: $DELETE_RESPONSE"

if echo "$DELETE_RESPONSE" | grep -q "deleted successfully"; then
    print_status 0 "File deleted successfully"
else
    print_status 1 "File deletion failed"
fi
echo ""

# Test 16: Verify file deletion from PV
echo "16. Verifying file deletion from persistent volume..."
if [ ! -z "$FILE_POD" ]; then
    DELETED_FILE_IN_PV=$(kubectl exec -n $NAMESPACE $FILE_POD -- find /app/storage -name "*${FILE_ID}*" 2>/dev/null || echo "")
    
    if [ -z "$DELETED_FILE_IN_PV" ]; then
        print_status 0 "File removed from persistent volume after deletion"
    else
        print_status 1 "File still exists in PV after deletion"
        echo "Remaining files:"
        kubectl exec -n $NAMESPACE $FILE_POD -- ls -la /app/storage/
    fi
fi
echo ""

# Cleanup
echo "Cleaning up..."
rm -f /tmp/test-k3s-file.txt /tmp/downloaded-test-file.txt

# Stop port-forward if started
if [ ! -z "$PORT_FORWARD_PID" ]; then
    kill $PORT_FORWARD_PID 2>/dev/null || true
fi

echo ""
echo "========================================="
echo "Test Summary"
echo "========================================="
echo ""
echo "Service URL tested: $FILE_SERVICE_URL"
echo "Test File ID: $FILE_ID"
echo ""
echo "Key Verification Points:"
echo "  ✓ PVC status and capacity"
echo "  ✓ Pod volume mount configuration"
echo "  ✓ File upload via API"
echo "  ✓ File download and content verification"
echo "  ✓ File physical presence in PV"
echo "  ✓ Database metadata persistence"
echo "  ✓ File deletion from PV"
echo ""
echo "If all tests passed, FileStorageService is correctly configured with persistent storage in k3s!"
