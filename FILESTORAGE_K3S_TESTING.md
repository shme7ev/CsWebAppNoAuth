# FileStorageService k3s Testing Guide

## Quick Start

### 1. Deploy FileStorageService to k3s

```bash
./deploy-filestorage-to-k3s.sh
```

This will:
- Build the Docker image
- Deploy to k3s with persistent volume
- Expose via NodePort (port 30081)
- Verify health endpoint

### 2. Test FileStorageService

```bash
./quick-upload-test.sh
or more comprehensive
./test-filestorage-k3s.sh
```

This comprehensive test will verify:
- ✓ Kubernetes resources (PVC, PV, Pod, Service)
- ✓ Health endpoint
- ✓ File upload/download
- ✓ **File persistence in volume**
- ✓ Database metadata storage
- ✓ File deletion

## Manual Testing

### Access the Service

**Via NodePort (recommended):**
```bash
NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[0].address}')
SERVICE_URL="http://$NODE_IP:30081"
curl $SERVICE_URL/health
```

**Via port-forward:**
```bash
kubectl port-forward service/filestorageservice-service -n webapp-noauth 8081:8080
curl http://localhost:8081/health
```

### Test File Upload

```bash
# Create test file
echo "Test content $(date)" > test.txt

# Upload
curl -X POST \
  -H "X-Uploaded-By: test-user" \
  -F "file=@test.txt" \
  -F "description=Manual test" \
  http://localhost:8081/api/files/upload

# Example response: {"message":"File uploaded successfully","fileId":"guid-here"}
```

### Verify File in Persistent Volume

```bash
# Get pod name
POD_NAME=$(kubectl get pods -n webapp-noauth -l app=filestorageservice -o jsonpath='{.items[0].metadata.name}')

# Check storage directory
kubectl exec -n webapp-noauth $POD_NAME -- ls -la /app/storage/

# Find specific file (replace FILE_ID with actual GUID)
kubectl exec -n webapp-noauth $POD_NAME -- find /app/storage -name "*FILE_ID*"

# View file content
kubectl exec -n webapp-noauth $POD_NAME -- cat /app/storage/YOUR_FILE_ID
```

### Test File Download

```bash
# Replace FILE_ID with actual GUID from upload response
curl http://localhost:8081/api/files/YOUR_FILE_ID
```

### Test List Files

```bash
curl http://localhost:8081/api/files
```

### Test Delete File

```bash
curl -X DELETE \
  -H "X-Deleted-By: test-user" \
  http://localhost:8081/api/files/YOUR_FILE_ID
```

## Verification Checklist

### Kubernetes Resources
- [ ] PVC is Bound: `kubectl get pvc file-storage-pvc -n webapp-noauth`
- [ ] PV is Bound: `kubectl get pv`
- [ ] Pod is Running: `kubectl get pods -n webapp-noauth`
- [ ] Service is NodePort: `kubectl get service filestorageservice-service -n webapp-noauth`

### Application Level
- [ ] Health endpoint responds
- [ ] Can upload files
- [ ] Can download files
- [ ] File content matches
- [ ] Metadata in database
- [ ] Files listed correctly
- [ ] Delete works

### Persistent Volume (CRITICAL)
- [ ] Files exist in `/app/storage/` inside pod
- [ ] Files persist after pod restart
- [ ] Files are removed on delete

## Testing Persistence

To verify files survive pod restart:

```bash
# 1. Upload a file
curl -X POST \
  -H "X-Uploaded-By: persistence-test" \
  -F "file=@test.txt" \
  -F "description=Persistence test" \
  http://localhost:8081/api/files/upload

# Note the FILE_ID

# 2. Verify file exists in PV
POD_NAME=$(kubectl get pods -n webapp-noauth -l app=filestorageservice -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n webapp-noauth $POD_NAME -- ls -la /app/storage/

# 3. Delete the pod
kubectl delete pod -n webapp-noauth -l app=filestorageservice

# 4. Wait for new pod
kubectl wait --for=condition=ready pod -l app=filestorageservice -n webapp-noauth --timeout=60s

# 5. Verify file still exists in new pod
NEW_POD_NAME=$(kubectl get pods -n webapp-noauth -l app=filestorageservice -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n webapp-noauth $NEW_POD_NAME -- ls -la /app/storage/

# 6. Try to download the file
curl http://localhost:8081/api/files/YOUR_FILE_ID
```

## Troubleshooting

### Pod not starting
```bash
kubectl describe pod -l app=filestorageservice -n webapp-noauth
kubectl logs -n webapp-noauth <pod-name>
```

### PVC not binding
```bash
kubectl describe pvc file-storage-pvc -n webapp-noauth
kubectl get storageclass
```

### Cannot access service
```bash
kubectl get service filestorageservice-service -n webapp-noauth
kubectl get endpoints filestorageservice-service -n webapp-noauth
```

### Files not persisting
Check volume mount:
```bash
kubectl get pod <pod-name> -n webapp-noauth -o jsonpath='{.spec.volumes}'
kubectl exec -n webapp-noauth <pod-name> -- mount | grep storage
```

## Database Verification

Connect to PostgreSQL to verify metadata:

```bash
DB_POD=$(kubectl get pods -n webapp-noauth -l app=postgres -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n webapp-noauth $DB_POD -- psql -U postgres -d webapp_db -c "SELECT * FROM \"Files\";"
```

## Cleanup

To remove FileStorageService:

```bash
kubectl delete deployment filestorageservice -n webapp-noauth
kubectl delete service filestorageservice-service -n webapp-noauth
kubectl delete pvc file-storage-pvc -n webapp-noauth
```

## API Endpoints Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /health | Health check |
| POST | /api/files/upload | Upload file |
| GET | /api/files/{id} | Download file |
| GET | /api/files/{id}/metadata | Get metadata |
| GET | /api/files | List all files |
| DELETE | /api/files/{id} | Delete file |

All endpoints accept:
- `X-Uploaded-By` header (optional): Username of uploader
- `X-Deleted-By` header (optional): Username of deleter
