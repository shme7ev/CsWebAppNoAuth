# Loki Integration with Serilog

This document explains how Loki logging has been integrated into the WebAppNoAuth project using Serilog.

## Overview

The application now sends logs to a Grafana Loki instance in addition to console and file outputs. Logs are enriched with contextual information and can be viewed in Grafana.

## Configuration

### appsettings.json

The Loki configuration is defined in `appsettings.json` under the `Serilog` section:

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Grafana.Loki" ],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/webapp-.log",
        "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 7
      }
    },
    {
      "Name": "GrafanaLoki",
      "Args": {
        "uri": "http://localhost:3100",
        "labels": [
          { "key": "app", "value": "webapp-noauth" },
          { "key": "environment", "value": "development" }
        ],
        "propertiesAsLabels": [ "SourceContext", "RequestId" ]
      }
    }
  ],
  "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
}
```

## Key Features

### Log Enrichment
- **Machine Name**: Adds the hostname to each log entry
- **Thread ID**: Adds the thread identifier for troubleshooting
- **Context Properties**: Automatically includes SourceContext and RequestId as labels

### Structured Logging
The application uses structured logging throughout, for example:
```csharp
_logger.LogInformation("HomeController.Index() called - Processing home page request");
_logger.LogDebug("Retrieved products using raw SQL {@ProductCount}", viewModel.RawSqlCount);
_logger.LogError("Error occurred in HomeController. Request ID: {RequestId}, Path: {RequestPath}", 
    requestId, HttpContext.Request.Path);
```

## Setting up Loki and Grafana

### Option 1: Using Docker Compose

Create a `docker-compose.loki.yml` file:

```yaml
version: '3.8'

services:
  loki:
    image: grafana/loki:latest
    ports:
      - "3100:3100"
    command: -config.file=/etc/loki/local-config.yaml

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    depends_on:
      - loki
```

Run with:
```bash
docker-compose -f docker-compose.loki.yml up -d
```

### Option 2: Manual Setup

1. Download and install Loki from https://grafana.com/oss/loki/
2. Download and install Grafana from https://grafana.com/grafana/download
3. Configure Grafana to use Loki as a data source:
   - URL: http://localhost:3100
   - Access: Server (default)

## Testing the Integration

1. Start the Loki and Grafana services
2. Run the application:
   ```bash
   cd WebAppNoAuth
   dotnet run
   ```
3. Access the application and perform some actions
4. Open Grafana at http://localhost:3000
5. Navigate to Explore and select the Loki data source
6. Query logs using labels:
   ```
   {app="webapp-noauth"}
   ```

## Configuration Options

### Loki URI
Change the Loki endpoint in `appsettings.json`:
```json
"uri": "http://your-loki-server:3100"
```

### Labels
Add custom labels to categorize your logs:
```json
"labels": [
  { "key": "app", "value": "webapp-noauth" },
  { "key": "environment", "value": "production" },
  { "key": "team", "value": "backend" }
]
```

### Log Levels
Adjust minimum log levels:
```json
"MinimumLevel": {
  "Default": "Debug",  // Change to Debug for more verbose logging
  "Override": {
    "Microsoft": "Information",
    "Microsoft.AspNetCore": "Information"
  }
}
```

## Troubleshooting

### Common Issues

1. **Connection Refused**: Ensure Loki is running and accessible at the configured URI
2. **No Logs Appearing**: Check that the application has the correct permissions and network access
3. **Authentication Issues**: Loki can be configured with authentication if needed

### Log Verification

You can verify logs are being sent by checking:
1. Application console output (should show Serilog initialization)
2. Log files in the `logs/` directory
3. Grafana Loki dashboard

## Production Considerations

For production deployments:
- Use HTTPS for Loki connections
- Implement proper authentication/authorization
- Consider log retention policies
- Monitor Loki performance and storage usage
- Set appropriate log levels to balance verbosity and performance

## Additional Resources

- [Serilog Documentation](https://github.com/serilog/serilog)
- [Serilog.Sinks.Grafana.Loki](https://github.com/serilog-contrib/serilog-sinks-grafana-loki)
- [Grafana Loki Documentation](https://grafana.com/docs/loki/latest/)