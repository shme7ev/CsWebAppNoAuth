#!/bin/bash

# WebAppNoAuth Kubernetes Deployment Script

set -e

# Set kubectl context
export KUBECONFIG=~/.kube/config

echo "Starting WebAppNoAuth deployment to k3s..."

# Check if k3s is accessible
if ! kubectl get nodes >/dev/null 2>&1; then
    echo "⚠k3s is not accessible. Please ensure k3s is running."
    exit 1
fi

export KUBECONFIG=~/.kube/config

cd /home/petr/RiderProjects/WebAppNoAuth
docker build -t webappnoauth:latest -f WebAppNoAuth/Dockerfile .


kubectl apply -f k8s/01-namespace.yaml
kubectl apply -f k8s/02-postgres-config.yaml
kubectl apply -f k8s/04-postgres-init.yaml
kubectl apply -f k8s/03-postgres-deployment.yaml
kubectl apply -f k8s/05-app-config.yaml
kubectl apply -f k8s/06-app-deployment.yaml
kubectl apply -f k8s/07-ingress.yaml


echo "Waiting for PostgreSQL..."
kubectl wait --for=condition=ready pod -l app=postgres -n webapp-noauth --timeout=120s

echo "Waiting for WebAppNoAuth..."
kubectl wait --for=condition=ready pod -l app=webappnoauth -n webapp-noauth --timeout=120s

echo "   Local URL: http://localhost:30080"
echo "   Or add '127.0.0.1 webapp.local' to /etc/hosts and visit http://webapp.local"
