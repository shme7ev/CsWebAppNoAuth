#!/bin/bash

# WebAppNoAuth Kubernetes Cleanup Script

echo "Cleaning up WebAppNoAuth deployment from k3s..."

# Set kubectl context
export KUBECONFIG=~/.kube/config

# Delete the namespace (this will cascade delete all resources)
echo "Deleting namespace webapp-noauth..."
kubectl delete namespace webapp-noauth --ignore-not-found=true

# Remove the entry from /etc/hosts if it exists
if grep -q "webapp.local" /etc/hosts; then
    echo "Removing webapp.local from /etc/hosts..."
    sudo sed -i '/webapp.local/d' /etc/hosts
fi

echo "Cleanup complete!"
