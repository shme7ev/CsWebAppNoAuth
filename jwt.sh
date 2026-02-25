#!/bin/bash

# Simple script to get JWT token and access admin resources
# Usage: ./jwt.sh [username] [auth_url] [token_url]

USERNAME=${1:-"admin"}
AUTH_URL=${2:-"http://localhost:5033/Admin"}
TOKEN_URL=${3:-"http://localhost:5033/api/login/token"}

echo "Getting token for user: $USERNAME"

TOKEN_RESPONSE=$(curl -s -X POST \
    -H "Content-Type: application/json" \
    -d "{\"username\":\"$USERNAME\"}" \
    $TOKEN_URL)

JWT_TOKEN=$(echo "$TOKEN_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

if [ -z "$JWT_TOKEN" ]; then
    echo "Error: Could not get token"
    echo "Response: $TOKEN_RESPONSE"
    exit 1
fi

echo "Token: $JWT_TOKEN"

echo "Accessing $AUTH_URL..."
curl -fsSL -H "Authorization: Bearer $JWT_TOKEN" \
    -H "Accept: text/html" \
    $AUTH_URL | head
