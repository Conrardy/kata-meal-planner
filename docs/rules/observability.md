# Observability: Logging, Metrics & Tracing

> Language and framework agnostic rules for making systems observable and debuggable.

## The Three Pillars

### Logs
- Record discrete events
- Debug and audit
- Unstructured to structured data

### Metrics
- Aggregate measurements over time
- Monitor system health
- Alert on anomalies

### Traces
- Track requests across services
- Understand latency and bottlenecks
- Distributed system debugging

## Logging Best Practices

### Logging Levels

**DEBUG**: Detailed diagnostic information
- Variable values
- Method entry/exit
- Detailed flow information
- Only in development/troubleshooting

**INFO**: General informational events
- Application start/stop
- Configuration loaded
- State transitions
- Business events (order placed, meal planned)

**WARN**: Something unexpected but handled
- Deprecated API usage
- Retry attempts
- Fallback behavior activated
- Configuration issues

**ERROR**: Error that prevents operation but system continues
- Failed API calls
- Database errors
- Validation failures
- Business rule violations

**FATAL/CRITICAL**: Critical error requiring system shutdown
- Cannot connect to database
- Critical dependency unavailable
- Corrupted data detected

### What to Log

✅ **Do log:**
- Application start/shutdown
- Configuration changes
- State transitions
- Business events
- External API calls
- Database operations
- Authentication attempts
- Authorization failures
- Performance issues
- Errors and exceptions

❌ **Don't log:**
- Passwords or secrets
- Credit card numbers
- Personal Identifiable Information (PII)
- Security tokens
- Session IDs
- Raw request/response bodies (may contain sensitive data)

### Structured Logging

**Bad (Unstructured):**

```
logger.info("User 123 placed order for $45.50")
```

**Good (Structured):**

```
logger.info("Order placed", {
  userId: "123",
  orderId: "ord_789",
  amount: 45.50,
  currency: "USD",
  timestamp: "2024-01-15T10:30:00Z"
})
```

**Benefits:**
- Machine-parseable
- Easy to search and filter
- Enables aggregation
- Supports dashboards

### Contextual Logging

**Include relevant context:**
- User ID
- Request ID / Correlation ID
- Session ID
- Transaction ID
- Tenant ID (multi-tenant apps)
- Operation name
- Timestamp

**Example:**

```
{
  "level": "INFO",
  "message": "Meal plan created",
  "userId": "user_123",
  "requestId": "req_abc789",
  "mealPlanId": "plan_456",
  "timestamp": "2024-01-15T10:30:00Z",
  "service": "meal-planner-api"
}
```

### Correlation IDs

- Generate unique ID per request
- Pass through all services
- Include in all logs for that request
- Enables request tracing across system

**Implementation:**
- Generate at API gateway/entry point
- Include in headers (`X-Correlation-ID`)
- Store in thread-local/async context
- Log with every message

## Metrics

### Types of Metrics

**Counters**: Always increasing values
- Total requests
- Total errors
- Items created

**Gauges**: Point-in-time values that go up and down
- Current memory usage
- Active connections
- Queue size

**Histograms**: Distribution of values
- Request duration
- Response size
- Processing time

**Summaries**: Similar to histograms with percentiles
- Response time percentiles (P50, P95, P99)

### What to Measure

**System Metrics:**
- CPU usage
- Memory usage
- Disk I/O
- Network I/O
- Thread count

**Application Metrics:**
- Request rate (requests/second)
- Error rate
- Response time (latency)
- Active users/sessions

**Business Metrics:**
- Orders placed
- Meals planned
- Users registered
- Revenue

**Database Metrics:**
- Query time
- Connection pool size
- Slow queries
- Deadlocks

### The Four Golden Signals

**Latency**: Time to service requests
- Measure all requests
- Separate success and error latency
- Track percentiles (P50, P95, P99)

**Traffic**: Demand on system
- Requests per second
- Transactions per second
- Concurrent users

**Errors**: Rate of failed requests
- 4xx errors (client errors)
- 5xx errors (server errors)
- Failed validations
- Exceptions

**Saturation**: How "full" your service is
- CPU utilization
- Memory usage
- Queue depth
- Thread pool utilization

### Metric Naming Conventions

**Pattern**: `namespace.subsystem.metric.unit`

**Examples:**
- `mealplanner.api.requests.total`
- `mealplanner.api.response_time.seconds`
- `mealplanner.db.connections.active`
- `mealplanner.business.meals_planned.total`

**Guidelines:**
- Use dots or underscores consistently
- Include units in name
- Be descriptive but concise
- Follow conventions of your metrics system

## Distributed Tracing

### Trace Components

**Trace**: End-to-end request journey
- Unique trace ID
- Spans across services
- Shows complete request path

**Span**: Single operation within trace
- Span ID
- Parent span ID
- Start and end time
- Operation name
- Tags and logs

### Span Best Practices

**What to trace:**
- External API calls
- Database queries
- Message queue operations
- Service-to-service calls
- Significant business operations

**Span attributes:**
- Service name
- Operation name
- HTTP method and URL
- Status code
- Error details
- Custom business context

**Example span:**

```
{
  "traceId": "abc123",
  "spanId": "xyz789",
  "parentSpanId": "parent456",
  "name": "GET /recipes/:id",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:30:00.150Z",
  "duration": "150ms",
  "tags": {
    "http.method": "GET",
    "http.url": "/recipes/123",
    "http.status_code": 200,
    "recipe.id": "123"
  }
}
```

### Sampling

- Trace 100% in development
- Sample in production (1-10%)
- Always trace errors
- Support debug flag for full tracing

## Health Checks

### Types

**Liveness**: Is service alive?
- Can it receive traffic?
- Should it be restarted?

**Readiness**: Is service ready?
- Dependencies available?
- Can it handle requests?

**Startup**: Has service finished starting?
- Initialization complete?
- Resources loaded?

### Health Check Endpoints

**Basic:**

```
GET /health
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Detailed:**

```
GET /health/detailed
{
  "status": "healthy",
  "version": "1.2.3",
  "uptime": "5d 3h 25m",
  "dependencies": {
    "database": "healthy",
    "cache": "healthy",
    "messageQueue": "degraded"
  }
}
```

## Alerting

### Alert Guidelines

**When to alert:**
- Error rate exceeds threshold
- Latency above acceptable
- Resource saturation high
- Business metrics abnormal
- Dependencies unavailable

**Alert severity:**
- **Critical**: Immediate action required (page on-call)
- **Warning**: Attention needed soon
- **Info**: Informational only

### Alert Best Practices

- **Actionable**: Alert should suggest action
- **Contextual**: Include relevant information
- **Avoid fatigue**: Don't alert on noise
- **Tuned thresholds**: Adjust based on patterns
- **Escalation**: Define escalation path

**Bad alert:**
- "Server down" (which server? why? what to do?)

**Good alert:**
- "API server pod-123 unresponsive for 5 minutes. Current CPU: 98%. Last logs show database connection timeout. Runbook: https://wiki/db-timeout"

## Dashboards

### Dashboard Design

**Key metrics dashboard:**
- Request rate
- Error rate
- Response time (P50, P95, P99)
- Resource utilization

**Business metrics dashboard:**
- Active users
- Orders/meals planned
- Revenue
- Conversion rates

**Guidelines:**
- One dashboard per audience
- Most important metrics at top
- Use consistent time ranges
- Include context (annotations, links)
- Avoid clutter

## Observability Patterns

### Correlation

- Link logs, metrics, traces
- Use common identifiers
- Enable drill-down from metric to logs to traces

### Aggregation

- Roll up metrics over time
- Aggregate by dimensions (service, endpoint, user)
- Retain detail for recent data, aggregate old data

### Retention

- Logs: 30-90 days
- Metrics: Forever (with aggregation)
- Traces: 7-30 days (sample older)

## Best Practices Summary

### Do

✅ Use structured logging
✅ Include correlation IDs
✅ Log at boundaries
✅ Track the four golden signals
✅ Implement distributed tracing
✅ Create actionable alerts
✅ Build focused dashboards
✅ Test observability in development
✅ Sanitize sensitive data
✅ Document runbooks

### Don't

❌ Log sensitive information
❌ Log at excessive volume
❌ Alert on every issue
❌ Create noisy dashboards
❌ Ignore log levels
❌ Hard-code log messages
❌ Sample critical errors
❌ Forget to add context
❌ Overlook business metrics
❌ Make alerts non-actionable

## Tools & Standards

### Logging
- Structured: JSON, logfmt
- Standards: Syslog, Common Log Format

### Metrics
- Prometheus, StatsD, InfluxDB
- OpenMetrics standard

### Tracing
- OpenTelemetry (recommended)
- Jaeger, Zipkin
- W3C Trace Context standard

### Visualization
- Grafana (metrics/logs)
- Kibana (logs)
- Jaeger UI (traces)
