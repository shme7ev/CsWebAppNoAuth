#!/bin/bash

# Script to start Loki logging stack for testing

echo "Starting Loki logging stack..."

# Create necessary directories
mkdir -p logs

# Start Loki, Promtail, and Grafana
docker-compose -f docker-compose.loki.yml up -d

echo "Loki stack started!"
echo "Access Grafana at: http://localhost:3000"
echo "Default credentials: admin/admin"
echo "Loki endpoint: http://localhost:3100"
echo ""
echo "To stop the stack, run: docker-compose -f docker-compose.loki.yml down"