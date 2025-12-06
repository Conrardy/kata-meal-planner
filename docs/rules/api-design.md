# API Design Principles

> Language and framework agnostic rules for designing robust, maintainable APIs.

## Core Principles

- **Intent-revealing**: API should express what, not how
- **Consistent conventions**: Same patterns across the entire API
- **Minimal surface area**: Expose only what's necessary
- **Hard to misuse**: API design prevents common mistakes
- **Self-documenting**: Names and structure explain usage
- **Versioning strategy**: Plan for evolution from day one
- **Backward compatibility**: Don't break existing clients
- **Fail-fast validation**: Reject invalid requests immediately

## Resource Design

### Resource-Oriented Design

- **Model as resources**: Nouns, not verbs (e.g., `/meals`, not `/getMeals`)
- **Resource hierarchy**: Reflect relationships in URLs
- **Consistent naming**: Use plural nouns for collections
- **Predictable patterns**: Similar resources follow similar patterns

### URL Structure

**Good:**
- `/recipes` - Collection
- `/recipes/123` - Specific resource
- `/recipes/123/ingredients` - Sub-collection
- `/users/456/meal-plans` - User's meal plans

**Bad:**
- `/getRecipe?id=123` - RPC-style
- `/recipe/123` - Inconsistent singular/plural
- `/recipes/get/123` - Redundant verb

## HTTP Methods

### Standard Semantics

- **GET**: Retrieve resource(s) - safe, idempotent, cacheable
- **POST**: Create new resource - not idempotent
- **PUT**: Replace entire resource - idempotent
- **PATCH**: Partial update - may or may not be idempotent
- **DELETE**: Remove resource - idempotent

### Method Guidelines

**GET:**
- Never modify state
- No request body
- Results should be cacheable
- Use query parameters for filtering/sorting

**POST:**
- Create new resources
- Non-idempotent (multiple calls create multiple resources)
- Return 201 Created with Location header

**PUT:**
- Replace entire resource
- Idempotent (multiple identical requests = same result)
- Require all fields in request

**PATCH:**
- Update specific fields
- Send only changed fields
- Define patch semantics clearly (JSON Patch, Merge Patch)

**DELETE:**
- Remove resource
- Idempotent
- 204 No Content or 200 OK with response body

## Status Codes

### Success (2xx)

- **200 OK**: Successful GET, PUT, PATCH, DELETE with body
- **201 Created**: Successful POST, include Location header
- **202 Accepted**: Request accepted for async processing
- **204 No Content**: Successful DELETE, PUT with no response body

### Client Errors (4xx)

- **400 Bad Request**: Invalid syntax or validation failure
- **401 Unauthorized**: Authentication required or failed
- **403 Forbidden**: Authenticated but not authorized
- **404 Not Found**: Resource doesn't exist
- **405 Method Not Allowed**: HTTP method not supported
- **409 Conflict**: Resource state conflict (e.g., duplicate)
- **422 Unprocessable Entity**: Semantic errors in request
- **429 Too Many Requests**: Rate limit exceeded

### Server Errors (5xx)

- **500 Internal Server Error**: Generic server error
- **502 Bad Gateway**: Upstream service error
- **503 Service Unavailable**: Temporary unavailability
- **504 Gateway Timeout**: Upstream service timeout

## Request/Response Design

### Request Guidelines

- **Validate early**: Fail fast on invalid input
- **Clear error messages**: Explain what's wrong and how to fix
- **Accept flexible input**: Be lenient in what you accept
- **Use standard formats**: ISO 8601 dates, standard currencies

### Response Guidelines

- **Consistent structure**: Same structure for similar responses
- **Include metadata**: Pagination, timestamps, version
- **Partial responses**: Support field selection for large objects
- **Standard formats**: Use well-known formats (JSON, XML)

### Error Responses

**Structure:**

```json
{
  "error": {
    "code": "INVALID_MEAL_TIME",
    "message": "Meal time must be breakfast, lunch, or dinner",
    "details": {
      "field": "mealTime",
      "value": "midnight-snack",
      "allowed": ["breakfast", "lunch", "dinner"]
    }
  }
}
```

**Guidelines:**
- Machine-readable error codes
- Human-readable messages
- Contextual details for debugging
- Links to documentation when helpful

## Idempotency

### Definition

Same request executed multiple times produces same result as executing once.

### Idempotent Methods

- GET, PUT, DELETE - naturally idempotent
- POST - not idempotent by default
- PATCH - depends on implementation

### Implementing Idempotency

**For POST:**
- Use idempotency keys in headers
- Client generates unique key per logical operation
- Server stores key and response
- Subsequent requests with same key return cached response

**Example:**

```
POST /meal-plans
Idempotency-Key: a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

## Versioning

### Versioning Strategies

**URL versioning:**
- `/v1/recipes`, `/v2/recipes`
- Pros: Clear, simple, easy to route
- Cons: URL change required

**Header versioning:**
- `Accept: application/vnd.mealplanner.v1+json`
- Pros: Cleaner URLs, follows REST principles
- Cons: Less discoverable

**Query parameter:**
- `/recipes?version=1`
- Pros: Easy to test
- Cons: Caching issues

### Version Management

- Maintain backward compatibility when possible
- Deprecate, don't break
- Document breaking changes clearly
- Provide migration guides
- Support N-1 versions minimum

## Pagination

### Offset-based Pagination

```
GET /recipes?limit=20&offset=40
```

**Response:**

```json
{
  "data": [...],
  "pagination": {
    "limit": 20,
    "offset": 40,
    "total": 150
  }
}
```

### Cursor-based Pagination

```
GET /recipes?limit=20&cursor=eyJpZCI6MTIzfQ
```

**Benefits:**
- Handles concurrent modifications better
- More efficient for large datasets
- Prevents skipped/duplicate items

### Guidelines

- Default and maximum page sizes
- Include pagination metadata
- Provide next/previous links
- Consider cursor-based for real-time data

## Filtering, Sorting, Searching

### Filtering

```
GET /recipes?dietaryPreference=vegan&maxCalories=500
```

- Use query parameters
- Support multiple filters
- Document available filters
- Use consistent operators (eq, gt, lt, in)

### Sorting

```
GET /recipes?sort=createdAt:desc,name:asc
```

- Specify field and direction
- Support multiple sort fields
- Default sort order

### Searching

```
GET /recipes?q=pasta&fields=title,ingredients
```

- Use `q` parameter for full-text search
- Support field-specific search
- Return relevance scores

## Security

### Authentication & Authorization

- **Authentication**: Who you are (OAuth, JWT, API keys)
- **Authorization**: What you can do (role-based, attribute-based)
- Use HTTPS exclusively
- Never pass credentials in URLs
- Implement rate limiting
- Validate all input

### Input Validation

- Validate types, formats, ranges
- Sanitize string inputs
- Check array/object sizes
- Use allow-lists, not deny-lists
- Reject unexpected fields

### Output Encoding

- Escape output appropriately
- Use Content-Type headers correctly
- Prevent injection attacks
- Don't expose sensitive data
- Remove internal error details in production

## Rate Limiting

### Implementation

- Limit requests per time window
- Per user, per IP, or per API key
- Use token bucket or sliding window

### Response Headers

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 500
X-RateLimit-Reset: 1640000000
```

### Response

```
HTTP/1.1 429 Too Many Requests
Retry-After: 3600
```

## Documentation

### What to Document

- Authentication mechanism
- Available endpoints and methods
- Request/response schemas
- Error codes and meanings
- Rate limits
- Pagination strategy
- Examples for each endpoint

### Documentation Tools

- OpenAPI/Swagger specifications
- Interactive API explorers
- Code examples in multiple languages
- Changelog for versions
- Migration guides

## Best Practices

### Do

✅ Use nouns for resources, not verbs
✅ Be consistent across endpoints
✅ Version your API from day one
✅ Validate input rigorously
✅ Return appropriate status codes
✅ Document everything
✅ Support pagination
✅ Implement rate limiting
✅ Use HTTPS everywhere
✅ Make APIs idempotent where possible

### Don't

❌ Use verbs in URLs (`/getRecipe`)
❌ Return different structures for similar resources
❌ Break backward compatibility without versioning
❌ Trust client input
❌ Return 200 for errors
❌ Expose internal implementation details
❌ Forget error handling
❌ Ignore security
❌ Make non-idempotent operations idempotent by default
❌ Use synchronous APIs for long-running operations
