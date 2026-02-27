# WebAppNoAuth Kubernetes Deployment Summary

### Via NodePort (Direct Access)
- **URL**: http://localhost:30080
- **Description**: Direct access through NodePort service

### Via Ingress (Hostname-based)
1. Add this entry to your `/etc/hosts` file:
   ```
   127.0.0.1 webapp.local
   ```
2. Access via: http://webapp.local

## Deployment Details

### Components Deployed
- **Namespace**: `webapp-noauth`
- **PostgreSQL Database**: Running with sample data
- **Web Application**: 2 replicas running
- **Services**: 
  - Internal ClusterIP service for app
  - NodePort service on port 30080
- **Ingress**: Traefik ingress controller configured

### Health Checks
- **Readiness Probe**: HTTP GET to `/` path
- **Liveness Probe**: HTTP GET to `/` path
- Both probes are functioning correctly

## Image Management

### Pushing to Local Registry

The deployment uses a local Docker registry running on port 5000:

1. **Start local registry** (if not already running):
   ```bash
   docker run -d -p 5000:5000 --restart=always --name registry registry:2
   ```

2. **Tag the image for local registry**:
   ```bash
   docker tag webappnoauth:latest localhost:5000/webappnoauth:latest
   ```

3. **Push to local registry**:
   ```bash
   docker push localhost:5000/webappnoauth:latest
   ```

4. **Verify image in registry**:
   ```bash
   curl -s http://localhost:5000/v2/_catalog
   ```

### Rebuilding and Redeploying

To rebuild and redeploy after code changes:

```bash
# Build new image
docker build -t webappnoauth:latest -f WebAppNoAuth/Dockerfile .

# Tag and push to local registry
docker tag webappnoauth:latest localhost:5000/webappnoauth:latest
docker push localhost:5000/webappnoauth:latest

# Restart deployment to pick up new image
kubectl rollout restart deployment/webappnoauth -n webapp-noauth
```

## Configuration Applied

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Production
- `ASPNETCORE_URLS`: http://+:8080
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection
- JWT configuration for authentication

### Security
- Secrets stored in Kubernetes Secrets
- ConfigMaps for non-sensitive configuration
- Proper RBAC and namespace isolation

## 🧪 Verification Commands

Check deployment status:
```bash
kubectl get pods -n webapp-noauth
kubectl get services -n webapp-noauth
kubectl get ingress -n webapp-noauth
```

View application logs:
```bash
kubectl logs -l app=webappnoauth -n webapp-noauth
```

Scale the application:
```bash
kubectl scale deployment webappnoauth --replicas=3 -n webapp-noauth
```

## Cleanup

To remove the entire deployment:
```bash
./cleanup-k3s.sh
```

Or manually:
```bash
kubectl delete namespace webapp-noauth
```

## Troubleshooting

If you encounter issues:
1. Check pod status: `kubectl get pods -n webapp-noauth`
2. Check logs: `kubectl logs -n webapp-noauth <pod-name>`
3. Verify services: `kubectl get services -n webapp-noauth`
4. Ensure k3s is running: `systemctl status k3s`
