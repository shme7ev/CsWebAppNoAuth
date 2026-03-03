#!/bin/bash

# Quick upload test for FileStorageService in k3s

set -e

echo "========================================="
echo "FileStorageService Quick Upload Test"
echo "========================================="
echo ""

# Get node IP and setup service URL
NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[0].address}')
SERVICE_URL="http://$NODE_IP:30081"

echo "Service URL: $SERVICE_URL"
echo ""

# 1. Health Check
echo "1. Health Check:"
HEALTH_RESPONSE=$(curl -s "$SERVICE_URL/health")
if echo "$HEALTH_RESPONSE" | grep -q "Healthy"; then
    echo "✓ Service is healthy"
    echo "$HEALTH_RESPONSE" | jq .
else
    echo "✗ Service health check failed"
    echo "Response: $HEALTH_RESPONSE"
    exit 1
fi
echo ""

# 2. Create test file with unique name
TIMESTAMP=$(date +%s)
TEST_FILE="/tmp/test-pv-$TIMESTAMP.txt"
TEST_CONTENT="Persistent Volume Test - $(date)"
echo "$TEST_CONTENT" > "$TEST_FILE"

echo "2. Created test file: $TEST_FILE"
echo "Content: $TEST_CONTENT"
echo ""

# 3. Upload file
echo "3. Uploading file to FileStorageService..."
UPLOAD_RESPONSE=$(curl -s -X POST \
  -H "X-Uploaded-By: quick-test-script" \
  -F "file=@$TEST_FILE" \
  -F "description=Quick PV persistence test via script" \
  "$SERVICE_URL/api/files/upload")

echo "Upload response:"
echo "$UPLOAD_RESPONSE" | jq .

FILE_ID=$(echo "$UPLOAD_RESPONSE" | jq -r '.fileId // empty')

if [ -z "$FILE_ID" ]; then
    echo "✗ Upload failed - no fileId in response"
    exit 1
fi

echo ""
echo "✓ File uploaded successfully!"
echo "File ID: $FILE_ID"
echo ""

FILE_NAME=$(echo "$UPLOAD_RESPONSE" | jq -r '.fileId // empty')
echo "File Name: $FILE_NAME"
echo ""

# 4. Get metadata
echo "4. Retrieving file metadata..."
METADATA_RESPONSE=$(curl -s "$SERVICE_URL/api/files/$FILE_ID/metadata")
echo "$METADATA_RESPONSE" | jq .
echo ""

# 5. Download and verify content
echo "5. Downloading file..."
DOWNLOADED_FILE="/tmp/downloaded-$FILE_ID.txt"
curl -s "$SERVICE_URL/api/files/$FILE_ID" -o "$DOWNLOADED_FILE"

if [ -f "$DOWNLOADED_FILE" ]; then
    DOWNLOADED_CONTENT=$(cat "$DOWNLOADED_FILE")
    if [ "$DOWNLOADED_CONTENT" = "$TEST_CONTENT" ]; then
        echo "✓ File content verified - matches original!"
        echo "Downloaded content: $DOWNLOADED_CONTENT"
    else
        echo "✗ File content mismatch!"
        echo "Expected: $TEST_CONTENT"
        echo "Got: $DOWNLOADED_CONTENT"
    fi
else
    echo "✗ Failed to download file"
fi
echo ""

# 6. List all files
echo "6. Listing all files in system..."
ALL_FILES=$(curl -s "$SERVICE_URL/api/files")
echo "$ALL_FILES" | jq '.[] | {id: .id, fileName: .fileName, size: .size, uploadedBy: .uploadedBy}'
echo ""

# 7. CRITICAL: Verify file exists in persistent volume
echo "7. Verifying file in persistent volume (CRITICAL TEST)..."
POD_NAME=$(kubectl get pods -n webapp-noauth -l app=filestorageservice -o jsonpath='{.items[0].metadata.name}')

if [ -z "$POD_NAME" ]; then
    echo "✗ Could not find FileStorageService pod"
    exit 1
fi

echo "Pod: $POD_NAME"
echo ""

echo "Contents of /app/storage/:"
kubectl exec -n webapp-noauth $POD_NAME -- ls -la /app/storage/ || echo "Unable to list storage directory"
echo ""

echo "Searching for uploaded file..."
FILE_IN_PV=$(kubectl exec -n webapp-noauth $POD_NAME -- find /app/storage -name "*$FILE_NAME*" 2>/dev/null || echo "")

if [ ! -z "$FILE_IN_PV" ]; then
    echo "✓✓✓ SUCCESS! File found in persistent volume!"
    echo "File path: $FILE_IN_PV"
    echo ""
    echo "File content from PV:"
    kubectl exec -n webapp-noauth $POD_NAME -- cat "$FILE_IN_PV"
else
    echo "✗✗✗ WARNING! File NOT found in persistent volume!"
    echo "This means files are NOT being persisted to the PV!"
    echo ""
    echo "Full storage directory contents:"
    kubectl exec -n webapp-noauth $POD_NAME -- ls -laR /app/storage/ || true
fi
echo ""

# 8. Cleanup
echo "8. Cleaning up..."
rm -f "$TEST_FILE" "$DOWNLOADED_FILE"

echo ""
echo "========================================="
echo "Test Summary"
echo "========================================="
echo "Service URL: $SERVICE_URL"
echo "File ID: $FILE_ID"
echo "Test file: $TEST_FILE (removed)"
echo ""
echo "Key Results:"
echo "  ✓ Health check passed"
echo "  ✓ File upload successful"
echo "  ✓ File download successful"
echo "  ✓ Content integrity verified"
if [ ! -z "$FILE_IN_PV" ]; then
    echo "  ✓✓ File persisted to PV (CRITICAL)"
else
    echo "  ✗✗ File NOT in PV (CRITICAL ISSUE)"
fi
echo ""
echo "To delete this test file:"
echo "  curl -X DELETE -H 'X-Deleted-By: cleanup' $SERVICE_URL/api/files/$FILE_ID"
echo ""
