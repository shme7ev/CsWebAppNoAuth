#!/bin/bash

# Test script for File Storage Service

echo "Testing File Storage Service..."

# Base URLs
#WEBAPP_URL="http://localhost:8080"
FILE_SERVICE_URL="http://localhost:5274"

# Test 1: Check if services are running
echo "1. Checking service health..."
#curl -s "$WEBAPP_URL" | grep -q "WebAppNoAuth" && echo "✓ Main web app is running" || echo "✗ Main web app is not responding"
curl -s "$FILE_SERVICE_URL/health" | grep -q "Healthy" && echo "✓ File storage service is running" || echo "✗ File storage service is not responding"

# Test 2: Create a test file
echo "2. Creating test file..."
echo "This is a test file for the file storage service." > test-file.txt

# Test 3: Upload file
echo "3. Uploading file..."
UPLOAD_RESPONSE=$(curl -s -X POST \
  -H "X-Uploaded-By: test-user" \
  -F "file=@test-file.txt" \
  -F "description=Test file upload" \
  "$FILE_SERVICE_URL/api/files/upload")

echo "Upload response: $UPLOAD_RESPONSE"

# Extract file ID from response
FILE_ID=$(echo "$UPLOAD_RESPONSE" | grep -o '"fileId":"[^"]*' | cut -d'"' -f4)

echo "fileId: $FILE_ID"

if [ ! -z "$FILE_ID" ]; then
    echo "✓ File uploaded successfully with ID: $FILE_ID"
    
    # Test 4: Get file metadata
    echo "4. Getting file metadata..."
    METADATA_RESPONSE=$(curl -s "$FILE_SERVICE_URL/api/files/$FILE_ID/metadata")
    echo "Metadata: $METADATA_RESPONSE"
    
    # Test 5: Download file
    echo "5. Downloading file..."
    curl -s "$FILE_SERVICE_URL/api/files/$FILE_ID" -o downloaded-test-file.txt
    if [ -f "downloaded-test-file.txt" ]; then
        echo "✓ File downloaded successfully"
        echo "Downloaded content:"
        cat downloaded-test-file.txt
        echo ""
    else
        echo "✗ Failed to download file"
    fi
    
    # Test 6: List files
    echo "6. Listing files..."
    LIST_RESPONSE=$(curl -s "$FILE_SERVICE_URL/api/files")
    echo "Files list: $LIST_RESPONSE"
    
    # Test 7: Delete file
    echo "7. Deleting file..."
    DELETE_RESPONSE=$(curl -s -X DELETE \
      -H "X-Deleted-By: test-user" \
      "$FILE_SERVICE_URL/api/files/$FILE_ID")
    echo "Delete response: $DELETE_RESPONSE"
    
else
    echo "✗ File upload failed"
fi

# Cleanup
rm -f test-file.txt downloaded-test-file.txt

echo "File storage service tests completed!"
