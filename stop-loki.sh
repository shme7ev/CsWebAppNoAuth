#!/bin/bash

# Script to stop Loki logging stack

echo "Stopping Loki logging stack..."

# Stop Loki, Promtail, and Grafana
docker-compose -f docker-compose.loki.yml down

echo "Loki stack stopped!"
echo "Data is preserved in Docker volumes."
echo "To remove volumes as well, run: docker-compose -f docker-compose.loki.yml down -v"