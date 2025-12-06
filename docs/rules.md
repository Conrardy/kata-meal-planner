### Development Rules & Principles

> Language and framework agnostic rules for building robust, maintainable software.

This directory contains comprehensive guidelines for software development following Domain-Driven Design, Software Craftsmanship, and modern engineering practices.

#### Quick Reference

##### Core Files

- **[ddd.md](./ddd.md)** - Domain-Driven Design tactical patterns
  - Value Objects, Entities, Aggregates
  - Domain Events, Repositories, Services
  - CQRS principles
  - Decision guidelines and anti-patterns

- **[craft.md](./craft.md)** - Software craftsmanship principles
  - Naming conventions
  - Function design
  - Error handling and null safety
  - Testing, refactoring, SOLID
  - Immutability and state management
  - Tell Don't Ask, Law of Demeter

- **[functional.md](./functional.md)** - Functional programming principles
  - Pure functions
  - Immutability
  - Function composition
  - Higher-order functions
  - Monads for error handling

- **[performance.md](./performance.md)** - Performance optimization
  - Algorithmic complexity
  - Database patterns
  - Memory management
  - Caching strategies
  - Profiling and monitoring

- **[api-design.md](./api-design.md)** - API design best practices
  - Resource-oriented design
  - HTTP methods and status codes
  - Versioning and pagination
  - Security and rate limiting
  - Error handling

- **[observability.md](./observability.md)** - Logging, metrics, and tracing
  - Structured logging
  - The four golden signals
  - Distributed tracing
  - Health checks and alerting

- **[concurrency.md](./concurrency.md)** - Thread safety and concurrency
  - Avoiding shared mutable state
  - Synchronization patterns
  - Deadlock prevention
  - Race condition handling
  - Thread-safe data structures

#### How to Use

##### For New Projects

1. Start with **ddd.md** to establish domain boundaries
2. Follow **craft.md** for clean code practices
3. Reference **api-design.md** when building APIs
4. Implement **observability.md** from day one
5. Apply **performance.md** when needed (measure first!)
6. Use **functional.md** for data transformations
7. Consult **concurrency.md** when dealing with parallel processing

##### For Code Reviews

Check against:
- Does it follow DDD patterns appropriately?
- Is the code clean and maintainable (craft principles)?
- Are APIs well-designed and documented?
- Is observability built-in?
- Are concurrency issues handled correctly?

##### For Refactoring

1. Add tests first
2. Apply principles incrementally
3. Focus on highest-value improvements
4. Measure before and after (performance)
5. Document decisions (ADRs)

#### Principles Priority

##### Always Apply
- ✅ Clean naming (craft.md)
- ✅ Error handling (craft.md)
- ✅ Testing (craft.md)
- ✅ Logging at boundaries (observability.md)
- ✅ Input validation (api-design.md)

##### Apply When Relevant
- 🟡 DDD patterns for complex domains (ddd.md)
- 🟡 CQRS for read/write separation (ddd.md)
- 🟡 Functional patterns for transformations (functional.md)
- 🟡 Concurrency patterns for parallel work (concurrency.md)

##### Apply When Needed
- ⚠️ Performance optimization (measure first!)
- ⚠️ Advanced caching strategies
- ⚠️ Distributed tracing (start simple)

#### Quick Tips

##### Domain Modeling
- Value Objects for concepts without identity
- Entities for things with lifecycle
- Aggregates for consistency boundaries
- Events for things that happened

##### Clean Code
- Functions < 20 lines
- Max 3 parameters
- One level of abstraction
- Tell, don't ask

##### Performance
- Big-O matters most
- Measure, don't guess
- Database is often the bottleneck
- Cache wisely, not everywhere

##### APIs
- Use HTTP verbs correctly
- Return appropriate status codes
- Version from day one
- Document everything

##### Observability
- Structured logging
- Include correlation IDs
- Track golden signals
- Create actionable alerts

##### Concurrency
- Immutability first
- Minimize shared state
- Use concurrent collections
- Prevent deadlocks by design

#### Anti-Patterns to Avoid

❌ Anemic domain models (logic in services)
❌ God objects (classes that do too much)
❌ Primitive obsession (int instead of UserId)
❌ Premature optimization
❌ Logging sensitive data
❌ Blocking in async code
❌ Nested locks
❌ Missing error handling

#### Contributing

These rules are living documents. Update them when:
- Learning from production issues
- Discovering better patterns
- Team adopts new practices
- Technology evolves

Keep rules:
- **Practical**: Based on real experience
- **Concise**: Easy to scan and reference
- **Agnostic**: Language/framework independent
- **Actionable**: Clear do's and don'ts

#### See Also

- [AGENTS.md](../../AGENTS.md) - Project-specific guidelines
- Architecture Decision Records (ADRs)
- Team coding standards


### API Design Principles

> Language and framework agnostic rules for designing robust, maintainable APIs.

#### Core Principles

- **Intent-revealing**: API should express what, not how
- **Consistent conventions**: Same patterns across the entire API
- **Minimal surface area**: Expose only what's necessary
- **Hard to misuse**: API design prevents common mistakes
- **Self-documenting**: Names and structure explain usage
- **Versioning strategy**: Plan for evolution from day one
- **Backward compatibility**: Don't break existing clients
- **Fail-fast validation**: Reject invalid requests immediately

#### Resource Design

##### Resource-Oriented Design

- **Model as resources**: Nouns, not verbs (e.g., `/meals`, not `/getMeals`)
- **Resource hierarchy**: Reflect relationships in URLs
- **Consistent naming**: Use plural nouns for collections
- **Predictable patterns**: Similar resources follow similar patterns

##### URL Structure

**Good:**
- `/recipes` - Collection
- `/recipes/123` - Specific resource
- `/recipes/123/ingredients` - Sub-collection
- `/users/456/meal-plans` - User's meal plans

**Bad:**
- `/getRecipe?id=123` - RPC-style
- `/recipe/123` - Inconsistent singular/plural
- `/recipes/get/123` - Redundant verb

#### HTTP Methods

##### Standard Semantics

- **GET**: Retrieve resource(s) - safe, idempotent, cacheable
- **POST**: Create new resource - not idempotent
- **PUT**: Replace entire resource - idempotent
- **PATCH**: Partial update - may or may not be idempotent
- **DELETE**: Remove resource - idempotent

##### Method Guidelines

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

#### Status Codes

##### Success (2xx)

- **200 OK**: Successful GET, PUT, PATCH, DELETE with body
- **201 Created**: Successful POST, include Location header
- **202 Accepted**: Request accepted for async processing
- **204 No Content**: Successful DELETE, PUT with no response body

##### Client Errors (4xx)

- **400 Bad Request**: Invalid syntax or validation failure
- **401 Unauthorized**: Authentication required or failed
- **403 Forbidden**: Authenticated but not authorized
- **404 Not Found**: Resource doesn't exist
- **405 Method Not Allowed**: HTTP method not supported
- **409 Conflict**: Resource state conflict (e.g., duplicate)
- **422 Unprocessable Entity**: Semantic errors in request
- **429 Too Many Requests**: Rate limit exceeded

##### Server Errors (5xx)

- **500 Internal Server Error**: Generic server error
- **502 Bad Gateway**: Upstream service error
- **503 Service Unavailable**: Temporary unavailability
- **504 Gateway Timeout**: Upstream service timeout

#### Request/Response Design

##### Request Guidelines

- **Validate early**: Fail fast on invalid input
- **Clear error messages**: Explain what's wrong and how to fix
- **Accept flexible input**: Be lenient in what you accept
- **Use standard formats**: ISO 8601 dates, standard currencies

##### Response Guidelines

- **Consistent structure**: Same structure for similar responses
- **Include metadata**: Pagination, timestamps, version
- **Partial responses**: Support field selection for large objects
- **Standard formats**: Use well-known formats (JSON, XML)

##### Error Responses

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

#### Idempotency

##### Definition

Same request executed multiple times produces same result as executing once.

##### Idempotent Methods

- GET, PUT, DELETE - naturally idempotent
- POST - not idempotent by default
- PATCH - depends on implementation

##### Implementing Idempotency

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

#### Versioning

##### Versioning Strategies

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

##### Version Management

- Maintain backward compatibility when possible
- Deprecate, don't break
- Document breaking changes clearly
- Provide migration guides
- Support N-1 versions minimum

#### Pagination

##### Offset-based Pagination

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

##### Cursor-based Pagination

```
GET /recipes?limit=20&cursor=eyJpZCI6MTIzfQ
```

**Benefits:**
- Handles concurrent modifications better
- More efficient for large datasets
- Prevents skipped/duplicate items

##### Guidelines

- Default and maximum page sizes
- Include pagination metadata
- Provide next/previous links
- Consider cursor-based for real-time data

#### Filtering, Sorting, Searching

##### Filtering

```
GET /recipes?dietaryPreference=vegan&maxCalories=500
```

- Use query parameters
- Support multiple filters
- Document available filters
- Use consistent operators (eq, gt, lt, in)

##### Sorting

```
GET /recipes?sort=createdAt:desc,name:asc
```

- Specify field and direction
- Support multiple sort fields
- Default sort order

##### Searching

```
GET /recipes?q=pasta&fields=title,ingredients
```

- Use `q` parameter for full-text search
- Support field-specific search
- Return relevance scores

#### Security

##### Authentication & Authorization

- **Authentication**: Who you are (OAuth, JWT, API keys)
- **Authorization**: What you can do (role-based, attribute-based)
- Use HTTPS exclusively
- Never pass credentials in URLs
- Implement rate limiting
- Validate all input

##### Input Validation

- Validate types, formats, ranges
- Sanitize string inputs
- Check array/object sizes
- Use allow-lists, not deny-lists
- Reject unexpected fields

##### Output Encoding

- Escape output appropriately
- Use Content-Type headers correctly
- Prevent injection attacks
- Don't expose sensitive data
- Remove internal error details in production

#### Rate Limiting

##### Implementation

- Limit requests per time window
- Per user, per IP, or per API key
- Use token bucket or sliding window

##### Response Headers

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 500
X-RateLimit-Reset: 1640000000
```

##### Response

```
HTTP/1.1 429 Too Many Requests
Retry-After: 3600
```

#### Documentation

##### What to Document

- Authentication mechanism
- Available endpoints and methods
- Request/response schemas
- Error codes and meanings
- Rate limits
- Pagination strategy
- Examples for each endpoint

##### Documentation Tools

- OpenAPI/Swagger specifications
- Interactive API explorers
- Code examples in multiple languages
- Changelog for versions
- Migration guides

#### Best Practices

##### Do

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

##### Don't

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


### Concurrency & Thread Safety

> Language and framework agnostic rules for writing concurrent, thread-safe code.

#### Core Principles

- **Immutability first**: Immutable objects are thread-safe by default
- **No shared mutable state**: Avoid shared state between threads
- **Synchronization boundaries**: Clearly mark synchronized regions
- **Minimize lock scope**: Lock for shortest time possible
- **Avoid deadlocks**: Always acquire locks in same order
- **Use higher-level abstractions**: Prefer concurrent collections, async patterns
- **Race conditions**: Identify and protect critical sections
- **Thread-local storage**: Use for thread-specific state
- **Non-blocking algorithms**: Prefer lock-free when possible

#### Immutability & Thread Safety

##### Why Immutability Matters

**Immutable objects are:**
- Thread-safe without synchronization
- Can be shared freely
- Cannot introduce race conditions
- Easier to reason about

**Example:**

```
// Mutable - NOT thread-safe
class Counter:
  private count = 0
  
  increment():
    count = count + 1  // Race condition!

// Immutable - Thread-safe
class Counter:
  private final count
  
  Counter(count):
    this.count = count
  
  increment():
    return new Counter(count + 1)
```

##### Guidelines

- Make objects immutable by default
- Use immutable collections
- Copy-on-write for modifications
- No setters, only constructors
- Mark fields as final/const/readonly

#### Avoiding Shared Mutable State

##### Problems with Shared State

- Race conditions
- Memory visibility issues
- Coordination overhead
- Difficult debugging

##### Solutions

**Message Passing**: Send copies of data between threads

```
thread1 -> message queue -> thread2
```

**Thread Confinement**: Data owned by single thread

```
- Thread-local storage
- Actor model
- Request-scoped data
```

**Immutable Sharing**: Share immutable references

```
data = create_immutable_data()
for each thread in threads:
  thread.process(data)  // Safe to share
```

**Synchronized Access**: Use locks/mutexes when necessary

```
lock.acquire()
try {
  // modify shared state
} finally {
  lock.release()
}
```

#### Synchronization

##### When to Synchronize

✅ Accessing shared mutable state
✅ Coordinating between threads
✅ Ensuring visibility of changes
✅ Protecting invariants

❌ Reading immutable data
❌ Thread-local data
❌ Pure computations
❌ Already thread-safe operations

##### Synchronization Mechanisms

**Mutexes/Locks**: Exclusive access

```
mutex.lock()
// critical section
mutex.unlock()
```

**Read-Write Locks**: Multiple readers OR one writer

```
rwlock.readLock()   // multiple readers OK
rwlock.writeLock()  // exclusive write
```

**Semaphores**: Limit concurrent access

```
semaphore.acquire()  // wait for permit
// use resource
semaphore.release()  // return permit
```

**Atomic Operations**: Lock-free single operations

```
atomicCounter.incrementAndGet()
compareAndSwap(expected, new)
```

##### Lock Guidelines

**Minimize lock scope:**

```
// Bad - holding lock too long
lock.acquire()
data = fetchFromDatabase()  // slow!
cache.put(key, data)
lock.release()

// Good - lock only critical section
data = fetchFromDatabase()
lock.acquire()
cache.put(key, data)
lock.release()
```

**Avoid nested locks:**

```
// Bad - potential deadlock
lock1.acquire()
  lock2.acquire()
    // work
  lock2.release()
lock1.release()

// Good - acquire in consistent order
acquireLocks(lock1, lock2)  // always same order
// work
releaseLocks(lock2, lock1)
```

**Use try-finally:**

```
lock.acquire()
try {
  // critical section
} finally {
  lock.release()  // always release
}
```

#### Deadlock Prevention

##### Deadlock Conditions

Deadlock requires ALL four:
1. Mutual exclusion (locks are exclusive)
2. Hold and wait (hold lock while waiting for another)
3. No preemption (can't force lock release)
4. Circular wait (circular dependency of locks)

##### Prevention Strategies

**Ordered lock acquisition:**

```
// Always acquire locks in same order
def transfer(from, to, amount):
  first, second = sorted([from, to], key=lambda x: x.id)
  first.lock()
  second.lock()
  // transfer
  second.unlock()
  first.unlock()
```

**Timeout-based:**

```
if lock1.tryLock(timeout):
  if lock2.tryLock(timeout):
    // work
    lock2.unlock()
  else:
    lock1.unlock()
    // retry or fail
```

**Lock-free algorithms:**

```
// Use atomic compare-and-swap
do {
  expected = value.get()
  newValue = expected + 1
} while (!value.compareAndSet(expected, newValue))
```

**Single lock:**

```
// Use one lock for related resources
globalLock.acquire()
// access any account
globalLock.release()
```

#### Race Conditions

##### Types

**Check-Then-Act:**

```
// Bad
if (!map.containsKey(key)) {  // check
  map.put(key, value)         // act - race!
}

// Good
map.putIfAbsent(key, value)   // atomic
```

**Read-Modify-Write:**

```
// Bad
count = counter.get()         // read
count++                       // modify
counter.set(count)           // write - race!

// Good
counter.incrementAndGet()     // atomic
```

**Double-Checked Locking:**

```
// Bad (broken in some languages)
if instance is null:
  lock.acquire()
  if instance is null:
    instance = create_singleton()  // not atomic!
  lock.release()

// Good
lock.acquire()
if instance is null:
  instance = create_singleton()
lock.release()
```

##### Detection

- Code review
- Static analysis tools
- Thread safety annotations
- Stress testing
- Race condition detectors (ThreadSanitizer, etc.)

#### Thread-Safe Data Structures

##### Use Built-in Concurrent Collections

**Instead of:**
- Regular HashMap → ConcurrentHashMap
- Regular List → CopyOnWriteArrayList
- Regular Queue → ConcurrentLinkedQueue
- Regular Set → ConcurrentHashSet

**Benefits:**
- Already thread-safe
- Optimized performance
- Well-tested
- Lock-free or fine-grained locking

##### When to Synchronize Collections

```
// Bad - not thread-safe
List<String> list = new ArrayList<>();
// multiple threads add/remove

// Option 1: Synchronized wrapper
List<String> list = Collections.synchronizedList(new ArrayList<>());

// Option 2: Concurrent collection
List<String> list = new CopyOnWriteArrayList<>();

// Option 3: External synchronization
synchronized(list) {
  list.add(item)
}
```

#### Asynchronous Patterns

##### Benefits

- No thread blocking
- Efficient resource usage
- Simpler than callbacks
- Exception handling preserved

##### Guidelines

```
// Good - asynchronous all the way
processOrder(orderId):
  order = await_result(fetchOrder(orderId))
  validation = await_result(validateOrder(order))
  result = await_result(saveOrder(order))
  return result

// Bad - blocking in async context
process():
  data = await_result(fetch())
  sleep(1000)  // blocks thread!
  return data
```

##### Error Handling

```
process():
  try:
    result = await_result(operation())
    return result
  catch error:
    // handle or propagate
    throw error
```

#### Actor Model

##### Principles

- Actors are isolated units
- Communicate via messages
- Process one message at a time
- No shared state

##### Benefits

- No locks needed
- Natural concurrency
- Location transparency
- Fault tolerance

##### Pattern

```
actor MealPlanActor {
  private state
  
  receive(message) {
    match message {
      CreateMeal(data) -> 
        this.state.meals.add(data)
        reply(Success)
      
      GetMeals() ->
        reply(this.state.meals)
    }
  }
}
```

#### Thread Pools

##### Sizing

**CPU-bound tasks:**
- Pool size = number of cores
- More threads = more context switching

**I/O-bound tasks:**
- Pool size > number of cores
- Threads can wait during I/O
- Formula: cores * (1 + wait time / compute time)

##### Best Practices

- Use bounded queues
- Set reasonable timeouts
- Handle rejected tasks
- Monitor pool metrics
- Name threads for debugging

```
threadPool = ThreadPool(
  coreSize = 10,
  maxSize = 50,
  queueSize = 1000,
  keepAlive = 60 seconds,
  threadNamePrefix = "meal-planner-"
)
```

#### Memory Visibility

##### The Problem

Changes in one thread may not be visible to others due to:
- CPU caches
- Compiler optimizations
- Out-of-order execution

##### Solutions

**Volatile/Atomic variables:**

```
volatile boolean flag = false  // visible across threads
```

**Synchronization:**

```
synchronized {
  // changes visible after lock release
}
```

**Memory barriers:**

```
// Language-specific memory fences
```

#### Testing Concurrent Code

##### Strategies

**Unit tests:**
- Test thread-safe components in isolation
- Use multiple threads in tests
- Repeat tests many times

**Stress tests:**
- High thread count
- Long duration
- Look for deadlocks, race conditions

**Tools:**
- Thread sanitizers
- Race condition detectors
- Deadlock detectors
- Load testing tools

##### Example

```
test_concurrent_increments():
  counter = new AtomicCounter()
  threads = empty_list
  
  for i from 1 to 10:
    thread = create_thread():
      for j from 1 to 1000:
        counter.increment()
    threads.add(thread)
  
  for each thread in threads:
    thread.join()
  
  assert(counter.get() == 10000)
```

#### Common Pitfalls

##### Avoid

❌ Assuming operations are atomic
❌ Forgetting to synchronize
❌ Over-synchronizing
❌ Acquiring locks in different orders
❌ Holding locks during I/O
❌ Accessing shared state without synchronization
❌ Using mutable objects as keys
❌ Thread.stop() or killing threads
❌ Ignoring memory visibility

##### Do

✅ Prefer immutability
✅ Use concurrent collections
✅ Minimize shared state
✅ Synchronize sparingly but correctly
✅ Use established patterns
✅ Test with multiple threads
✅ Document thread-safety guarantees
✅ Use static analysis tools
✅ Profile and measure

#### Best Practices Summary

1. **Immutable by default**: Eliminate most concurrency issues
2. **Isolate mutable state**: Minimize shared mutable state
3. **Use high-level abstractions**: Let libraries handle complexity
4. **Lock carefully**: Minimize scope, avoid nesting
5. **Prevent deadlocks**: Consistent lock ordering
6. **Test thoroughly**: Concurrent bugs are hard to find
7. **Document guarantees**: Specify thread-safety expectations
8. **Measure performance**: Concurrency can hurt if done wrong


### Software Craftsmanship Principles

> Language and framework agnostic rules for writing clean, maintainable code.

#### Naming

- **Reveal intent**: Names should explain why something exists, what it does
- **Use domain language**: Prefer domain terms over technical terms
- **Avoid abbreviations**: Write `Customer` not `Cust`, `Recipe` not `Rcp`
- **Be consistent**: Same word for same concept across the codebase
- **No redundant context**: In `Recipe` class, use `title` not `recipeTitle`

#### Functions/Methods

- **Single responsibility**: One function = one thing at one level of abstraction
- **Small size**: Aim for < 20 lines, definitely < 50 lines
- **Few parameters**: Max 3 parameters; more = use a parameter object
- **No side effects**: Don't modify state unexpectedly
- **Command-Query Separation**: Methods either do something OR return something, not both

#### Error Handling

- **Fail fast**: Validate at boundaries, fail immediately on invalid input
- **Use exceptions for exceptional cases**: Not for control flow
- **Specific exceptions**: Create domain-specific exception types
- **Meaningful messages**: Include context about what failed and why
- **No silent failures**: Never catch and ignore exceptions without logging

#### Null Handling

- **Avoid null returns**: Return empty collections, Optional/Maybe, or Null Object pattern
- **Fail fast on null**: Validate nulls at boundaries, never propagate them
- **No defensive null checks everywhere**: Fix the root cause instead
- **Explicit optionality**: If something can be absent, make it explicit in the type system
- **Null Object pattern**: Return a safe default object instead of null when appropriate
- **Never return null collections**: Empty collection is always better than null

**Strategies:**

- Use Optional/Maybe monads
- Return empty arrays/collections
- Use Null Object pattern for strategy/polymorphism
- Throw exceptions for programmer errors

#### Testing

- **Test behavior, not implementation**: Focus on what, not how
- **One assertion concept per test**: Tests should verify one logical concept
- **Given-When-Then**: Structure tests clearly: setup, action, verification
- **Fast tests**: Unit tests run in milliseconds
- **Independent tests**: No test depends on another test's execution
- **DAMP over DRY**: Tests can repeat setup for clarity (Descriptive And Meaningful Phrases)

#### Boundaries

- **Dependency inversion**: Domain depends on nothing, all dependencies point inward
- **Ports and Adapters**: Isolate external systems behind interfaces
- **Anti-corruption layer**: Protect your domain from external model pollution
- **DTOs at boundaries**: Never pass domain objects across architectural boundaries
- **Explicit contracts**: Use interfaces to define contracts between layers

#### Code Organization

- **Package by feature**: Group by business capability, not technical layer
- **Screaming architecture**: Structure should reveal the system's intent
- **Dependency direction**: Always point toward domain, never outward
- **Cohesion**: Things that change together belong together
- **Coupling**: Minimize dependencies between modules

#### Refactoring Discipline

- **Red-Green-Refactor**: Make it work, make it right, make it fast (in that order)
- **Boy Scout Rule**: Leave code cleaner than you found it
- **Ruthless refactoring**: Don't accumulate technical debt
- **Preparatory refactoring**: Refactor to make the change easy, then make the change
- **Test-protected refactoring**: Only refactor with tests in place

#### SOLID Principles

- **Single Responsibility**: A class should have only one reason to change
- **Open-Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Subtypes must be substitutable for their base types
- **Interface Segregation**: Many specific interfaces > one general interface
- **Dependency Inversion**: Depend on abstractions, not concretions

#### Immutability & State Management

##### Immutability Rules

- **Prefer immutable by default**: Make objects immutable unless there's a compelling reason
- **Copy-on-write**: Return new instances with modified state instead of mutating
- **No temporal coupling**: Avoid order-dependent method calls
- **Thread-safety by design**: Immutable objects are inherently thread-safe
- **Share freely**: Immutable objects can be safely shared without defensive copying

##### State Changes

- **Minimize mutable state**: Keep stateful objects to a minimum
- **Local mutability**: If mutation is needed, keep it local and contained
- **Atomic state transitions**: State should move from one valid state to another atomically
- **No half-baked objects**: Never expose objects in partially initialized state

#### Tell, Don't Ask Principle

- **Push behavior down**: Tell objects what to do, don't ask for their state
- **Avoid getter chains**: `order.getCustomer().getAddress().getCity()` is a smell
- **Encapsulate decisions**: Let objects make decisions about their own data
- **Reduce coupling**: Asking for data creates dependencies on structure

**Example:**

- ❌ Bad: `if (meal.getCalories() > limit) meal.setStatus(REJECTED)`
- ✅ Good: `meal.rejectIfExceedsCalories(limit)`

#### Law of Demeter (Principle of Least Knowledge)

- **Talk to friends, not strangers**: Only talk to immediate dependencies
- **One dot rule**: Avoid chaining method calls (exceptions: fluent builders, query APIs)
- **Don't reach through**: Don't access nested objects

**Allowed to call methods on:**

- The object itself (`this`)
- Method parameters
- Objects created in the method
- Direct attributes/properties

**Not allowed:**

- Methods on objects returned by other methods

#### Temporal Patterns (Time Handling)

- **Use UTC everywhere**: Convert to local time only at presentation layer
- **Explicit time zones**: Never assume time zone, always specify
- **Immutable time objects**: Time values should be immutable
- **Business time vs system time**: Separate when something is scheduled vs when it's processed
- **Audit trails**: Record when things happened with timestamps
- **Effective dating**: Support past/future effective dates for business rules
- **Avoid time-based coupling**: Don't use current time in domain logic, inject it

**Example:**

- ❌ Bad: `meal.isExpired()` (uses current system time internally)
- ✅ Good: `meal.isExpiredAt(clock.now())`

#### Defensive Programming

- **Validate all inputs**: Trust no external data
- **Fail explicitly**: Throw specific exceptions with clear messages
- **Guard clauses**: Check preconditions at the start of methods
- **Invariant assertions**: Assert class invariants are maintained
- **Design by contract**: Explicit preconditions, postconditions, invariants
- **Safe parsing**: Never assume parsing will succeed
- **Range checks**: Validate numeric ranges and array bounds
- **Type safety**: Use types to prevent invalid states

**Guard Clause Pattern:**

```
process(data):
  if not data:
    throw InvalidArgumentException("data required")
  
  if data.items.is_empty():
    return EmptyResult()
  
  // main logic here
```

#### Documentation Principles

- **Code explains how, comments explain why**: Don't duplicate what code already says
- **Comment the non-obvious**: Explain business rules, gotchas, workarounds
- **Keep docs close to code**: Documentation should live near what it documents
- **Update with code**: Outdated docs are worse than no docs
- **Architecture Decision Records (ADRs)**: Document significant decisions
- **Public API docs**: Every public API should have usage documentation
- **Examples over explanations**: Show how to use, don't just describe

##### What to Document

- ✅ Why decisions were made
- ✅ Business rules and constraints
- ✅ Complex algorithms
- ✅ Workarounds and hacks (with context)
- ✅ Public API usage
- ❌ What the code obviously does
- ❌ Change history (use git)
- ❌ Author names (use git)


### Domain-Driven Design Tactical Patterns

> Language and framework agnostic rules for DDD tactical patterns.

#### Value Objects

- **Immutability**: Value Objects MUST be immutable once created
- **Equality by value**: Two Value Objects are equal if all their attributes are equal
- **Self-validation**: Value Objects validate themselves in their constructor/factory
- **No identity**: Value Objects have no conceptual identity, only their values matter
- **Small and focused**: Represent a single concept from the domain (Email, Money, DateRange)
- **Methods express behavior**: Add domain methods that operate on the value (e.g., `Money.add()`, `DateRange.overlaps()`)

#### Entities

- **Identity**: Entities have a unique identifier that remains constant throughout their lifecycle
- **Mutable state**: State can change, but identity persists
- **Equality by ID**: Two Entities are equal if they have the same identifier
- **Encapsulation**: Hide internal state, expose behavior through methods
- **Business logic location**: Put domain logic that changes Entity state inside the Entity itself
- **No anemic models**: Entities should contain behavior, not just getters/setters

#### Aggregates

- **Single root**: Each Aggregate has ONE root Entity that controls access
- **Consistency boundary**: All invariants must be satisfied within the Aggregate
- **Transaction boundary**: One transaction = one Aggregate modification
- **Reference by ID**: External Aggregates reference each other by ID only, not by direct object reference
- **Small boundaries**: Keep Aggregates as small as possible while maintaining consistency
- **Lazy loading**: Load related Aggregates separately, don't cascade load

#### Domain Events

- **Past tense naming**: Events represent something that happened (e.g., `MealPlanned`, `RecipeSwapped`)
- **Immutable**: Events cannot be changed once created
- **Contain minimal data**: Include only what subscribers need to react
- **Represent business facts**: Express what happened in domain terms, not technical terms
- **Publish after persistence**: Emit events only after the Aggregate is successfully persisted

#### Repositories

- **Collection metaphor**: Act like an in-memory collection of Aggregates
- **One per Aggregate root**: Create repositories only for Aggregate roots
- **Domain-oriented interface**: Use domain language (`findByEmail()` not `selectByEmailColumn()`)
- **Hide persistence**: Keep infrastructure concerns out of the domain
- **Return domain objects**: Always return Entities/Aggregates, never raw data structures

#### Domain Services

- **Stateless**: Domain Services hold no state, only behavior
- **Multi-Aggregate operations**: Use when logic spans multiple Aggregates
- **Domain logic only**: No infrastructure concerns (no DB, no HTTP, no file system)
- **Explicit dependencies**: Require repositories or other services as parameters
- **Verb-based naming**: Name after the action performed (e.g., `PlanMealService`, `SwapMealService`)

#### Application Services

- **Use case orchestration**: One Application Service = one use case
- **Transaction management**: Handle transaction boundaries here
- **DTO transformation**: Convert domain objects to/from DTOs
- **Thin layer**: Coordinate, don't contain business logic
- **Exception translation**: Catch domain exceptions, translate to application-appropriate responses

#### Factories

- **Complex creation**: Use when object creation is complex or involves rules
- **Encapsulate creation logic**: Hide how objects are constructed
- **Return valid objects**: Never return partially constructed or invalid objects
- **Reconstitution**: Separate factory methods for creating new vs reconstituting from storage

#### CQRS (Command Query Responsibility Segregation)

##### Command Side

- **Intent-based commands**: Name commands after business intent (`PlanMeal`, not `CreateMealPlan`)
- **Commands never return domain data**: Return acknowledgment/ID only
- **Validate commands**: Check business rules before execution
- **One transaction per command**: Command = unit of work

##### Query Side

- **Queries never mutate**: Pure read operations with no side effects
- **Optimize for reading**: Query models can denormalize for performance
- **Eventual consistency**: Accept that read models may lag behind writes
- **Multiple read models**: Create specialized projections for different use cases

##### General

- **Clear separation**: Never mix command and query in same operation
- **Different models**: Write model (domain) ≠ read model (projection)

#### Decision Making Guidelines

##### When to Create a Value Object

✅ Create when:
- The concept has no identity (Email, Money, Quantity)
- Equality is determined by value, not ID
- Multiple attributes form a cohesive concept
- There are validation rules or invariants

❌ Don't create when:
- It's a primitive obsession reflex (not everything needs to be wrapped)
- The value has no domain meaning
- It would add complexity without benefit

##### When to Create an Entity vs Value Object

**Entity** if:
- It has a lifecycle and identity matters over time
- You track it, reference it, update it
- Example: Recipe, MealPlan, User

**Value Object** if:
- Identity doesn't matter, only the values
- Immutable and replaceable
- Example: Ingredient, CalorieCount, ServingSize

##### When to Create a Domain Service

✅ Create when:
- Operation spans multiple Aggregates
- Logic doesn't naturally fit in any single Entity
- The operation is a significant domain concept

❌ Don't create when:
- Logic belongs to a single Entity
- It's just a utility function (use static/module functions)

##### When to Emit a Domain Event

✅ Emit when:
- Something significant happened in the domain
- Other parts of the system need to react
- You want to decouple components

❌ Don't emit when:
- It's just a CRUD operation with no domain meaning
- Only used for logging/auditing (use infrastructure events)
- Would create unnecessary coupling

#### Anti-Patterns to Avoid

- **Anemic Domain Model**: Entities with only getters/setters, logic in services
- **God Objects**: Classes that know/do too much
- **Feature Envy**: Methods that use more of another class than their own
- **Primitive Obsession**: Using primitives instead of small domain objects
- **Shotgun Surgery**: One change requires modifications everywhere
- **Leaky Abstractions**: Implementation details bleeding through interfaces
- **Smart UI**: Business logic in presentation layer
- **Transaction Script**: Procedural code with no domain model


### Functional Programming Principles

> Language and framework agnostic rules for functional programming practices.

#### Core Principles

- **Pure functions**: Same input = same output, no side effects
- **Referential transparency**: Expression can be replaced with its value
- **Higher-order functions**: Functions that take/return functions
- **Function composition**: Build complex behavior from simple functions
- **Avoid shared mutable state**: Pass data through function chains
- **Immutable data structures**: Transform, don't mutate
- **Declarative over imperative**: Express what, not how

#### Functional Patterns

- **Map**: Transform each element in a collection
- **Filter**: Select elements matching criteria
- **Reduce**: Aggregate collection to single value
- **Pipeline**: Chain operations on data streams
- **Monads**: Optional, Result, Either for error handling

#### Pure Functions

**Characteristics:**
- No side effects (no mutations, no I/O, no exceptions)
- Deterministic (same input always produces same output)
- No dependency on external state
- Easy to test and reason about

**Benefits:**
- Thread-safe by default
- Cacheable (memoization)
- Testable without mocks
- Composable

**Example:**
- ❌ Bad: `addToCart(item) -> cart.append(item)` (mutates global cart)
- ✅ Good: `addToCart(cart, item) -> return new_cart_with(cart, item)` (returns new cart)

#### Function Composition

- **Small building blocks**: Write small, focused functions
- **Compose for complexity**: Build complex operations from simple ones
- **Left-to-right or right-to-left**: Be consistent in composition direction
- **Type safety**: Ensure output of one function matches input of next

**Example:**

```
toLowerCase(str):
  return str.to_lower_case()

removeSpaces(str):
  return str.replace_all(whitespace, '')

slugify = compose(removeSpaces, toLowerCase)
```

#### Immutability

- **Never mutate**: Always return new data structures
- **Structural sharing**: Use persistent data structures for efficiency
- **Immutable by default**: Make immutability the default, mutation explicit
- **Copy-on-write**: Clone and modify instead of mutating in place

**Transformations:**
- Arrays: Use `map`, `filter`, `reduce`, spread operator
- Objects: Use spread operator, `Object.assign`, or libraries
- Deep updates: Use lenses or utility libraries

#### Higher-Order Functions

**Characteristics:**
- Accept functions as arguments
- Return functions as results
- Enable abstraction and code reuse

**Common patterns:**
- Callback functions
- Partial application
- Currying
- Decorators/Wrappers

**Example:**

```
withLogging(fn):
  return function(args...):
    log("Calling " + fn.name + " with " + args)
    return fn(args...)
```

#### Currying & Partial Application

**Currying**: Transform `f(a, b, c)` into `f(a)(b)(c)`
- Enables partial application
- Creates specialized functions from general ones
- Improves reusability

**Partial Application**: Pre-fill some arguments
- Fix common parameters
- Create specialized functions
- Reduce repetition

**Example:**

```
add(a):
  return function(b):
    return a + b

add5 = add(5)
add5(3)  // returns 8
```

#### Monads for Error Handling

##### Optional/Maybe Monad

- Represents presence or absence of a value
- Avoids null/undefined checks
- Chains operations safely

##### Result/Either Monad

- Represents success or failure
- Captures error information
- Enables railway-oriented programming

**Benefits:**
- No exception throwing
- Explicit error handling
- Composable error paths
- Type-safe error propagation

#### Declarative vs Imperative

**Imperative (How):**

```
result = empty_list
for i from 0 to items.length:
  if items[i].active:
    result.append(items[i].name)
```

**Declarative (What):**

```
result = items
  .filter(item -> item.active)
  .map(item -> item.name)
```

#### Function Purity Guidelines

##### Pure (Allowed)

✅ Operating on parameters
✅ Calling other pure functions
✅ Creating new local variables
✅ Returning values

##### Impure (Avoid in pure functions)

❌ Mutating parameters or external state
❌ Reading/writing files
❌ Making network calls
❌ Reading system time
❌ Generating random numbers
❌ Logging to console
❌ Throwing exceptions

#### Practical Application

##### When to Use FP

✅ Data transformations
✅ Business logic
✅ Calculations and algorithms
✅ Data validation
✅ Query operations

##### When to Use Imperative

✅ I/O operations
✅ Performance-critical sections
✅ Interfacing with imperative APIs
✅ Low-level optimizations

#### Best Practices

- **Isolate side effects**: Push them to the edges
- **Small functions**: Each does one thing well
- **Descriptive names**: Function name describes what it returns
- **Avoid deep nesting**: Use composition or early returns
- **Prefer expressions over statements**: Return values, don't mutate state
- **Type annotations**: Make function signatures explicit
- **Test pure functions**: Easy to test with simple inputs/outputs


### Observability: Logging, Metrics & Tracing

> Language and framework agnostic rules for making systems observable and debuggable.

#### The Three Pillars

##### Logs
- Record discrete events
- Debug and audit
- Unstructured to structured data

##### Metrics
- Aggregate measurements over time
- Monitor system health
- Alert on anomalies

##### Traces
- Track requests across services
- Understand latency and bottlenecks
- Distributed system debugging

#### Logging Best Practices

##### Logging Levels

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

##### What to Log

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

##### Structured Logging

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

##### Contextual Logging

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

##### Correlation IDs

- Generate unique ID per request
- Pass through all services
- Include in all logs for that request
- Enables request tracing across system

**Implementation:**
- Generate at API gateway/entry point
- Include in headers (`X-Correlation-ID`)
- Store in thread-local/async context
- Log with every message

#### Metrics

##### Types of Metrics

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

##### What to Measure

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

##### The Four Golden Signals

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

##### Metric Naming Conventions

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

#### Distributed Tracing

##### Trace Components

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

##### Span Best Practices

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

##### Sampling

- Trace 100% in development
- Sample in production (1-10%)
- Always trace errors
- Support debug flag for full tracing

#### Health Checks

##### Types

**Liveness**: Is service alive?
- Can it receive traffic?
- Should it be restarted?

**Readiness**: Is service ready?
- Dependencies available?
- Can it handle requests?

**Startup**: Has service finished starting?
- Initialization complete?
- Resources loaded?

##### Health Check Endpoints

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

#### Alerting

##### Alert Guidelines

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

##### Alert Best Practices

- **Actionable**: Alert should suggest action
- **Contextual**: Include relevant information
- **Avoid fatigue**: Don't alert on noise
- **Tuned thresholds**: Adjust based on patterns
- **Escalation**: Define escalation path

**Bad alert:**
- "Server down" (which server? why? what to do?)

**Good alert:**
- "API server pod-123 unresponsive for 5 minutes. Current CPU: 98%. Last logs show database connection timeout. Runbook: https://wiki/db-timeout"

#### Dashboards

##### Dashboard Design

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

#### Observability Patterns

##### Correlation

- Link logs, metrics, traces
- Use common identifiers
- Enable drill-down from metric to logs to traces

##### Aggregation

- Roll up metrics over time
- Aggregate by dimensions (service, endpoint, user)
- Retain detail for recent data, aggregate old data

##### Retention

- Logs: 30-90 days
- Metrics: Forever (with aggregation)
- Traces: 7-30 days (sample older)

#### Best Practices Summary

##### Do

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

##### Don't

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

#### Tools & Standards

##### Logging
- Structured: JSON, logfmt
- Standards: Syslog, Common Log Format

##### Metrics
- Prometheus, StatsD, InfluxDB
- OpenMetrics standard

##### Tracing
- OpenTelemetry (recommended)
- Jaeger, Zipkin
- W3C Trace Context standard

##### Visualization
- Grafana (metrics/logs)
- Kibana (logs)
- Jaeger UI (traces)


### Performance Patterns & Optimization

> Language and framework agnostic rules for writing performant code.

#### General Rules

- **Measure, don't guess**: Profile before optimizing
- **Big-O matters**: Understand algorithmic complexity
- **Lazy initialization**: Defer expensive operations until needed
- **Caching strategy**: Cache expensive computations, not everything
- **Batch operations**: Process in batches when dealing with collections
- **Avoid premature optimization**: Make it work, make it right, then make it fast

#### Algorithmic Complexity

##### Know Your Big-O

- **O(1)**: Constant time - hash lookups, array access by index
- **O(log n)**: Logarithmic - binary search, balanced trees
- **O(n)**: Linear - single loop through data
- **O(n log n)**: Log-linear - efficient sorting algorithms
- **O(n²)**: Quadratic - nested loops, naive sorting
- **O(2ⁿ)**: Exponential - recursive fibonacci, backtracking

##### Guidelines

- Aim for O(n log n) or better for large datasets
- Avoid nested loops when possible
- Use appropriate data structures (hash maps vs arrays)
- Consider space-time tradeoffs

#### Database Patterns

##### Query Optimization

- **N+1 query problem**: Load related data in one query, not in a loop
- **Select only needed fields**: Don't use `SELECT *`
- **Pagination**: Always paginate large result sets
- **Database indexes**: Index fields used in WHERE, JOIN, ORDER BY
- **Query plan analysis**: Use EXPLAIN to understand query execution
- **Avoid SELECT DISTINCT**: Often indicates data model issues

##### Connection Management

- **Connection pooling**: Reuse connections, don't create per request
- **Connection limits**: Set appropriate pool sizes
- **Timeout configuration**: Set reasonable timeouts
- **Close connections**: Always release connections back to pool

##### Data Access Patterns

- **Lazy vs Eager loading**: Choose based on use case
- **Batch fetching**: Load multiple records in one query
- **Projection queries**: Select only what you need
- **Caching strategy**: Cache frequently accessed, rarely changed data
- **Read replicas**: Distribute read load across replicas

#### Memory Patterns

##### Memory Management

- **Stream large datasets**: Don't load everything into memory
- **Release resources**: Close connections, file handles, streams
- **Avoid memory leaks**: Clear references, unsubscribe listeners
- **Object pooling**: Reuse expensive objects when appropriate
- **Weak references**: Use for caches that shouldn't prevent garbage collection

##### Data Structure Selection

- **Arrays**: Fast access by index, contiguous memory
- **Linked lists**: Fast insertion/deletion, slow access
- **Hash maps**: O(1) lookup, more memory overhead
- **Sets**: Fast membership testing
- **Trees**: Ordered data, O(log n) operations

#### Caching Strategies

##### When to Cache

✅ Expensive computations
✅ Frequently accessed data
✅ Rarely changing data
✅ Remote API responses
✅ Database query results

❌ User-specific data (unless per-user cache)
❌ Fast operations
❌ Rapidly changing data
❌ Large objects with low reuse

##### Cache Patterns

- **Cache-aside**: Application manages cache explicitly
- **Read-through**: Cache automatically loads on miss
- **Write-through**: Write to cache and database synchronously
- **Write-behind**: Write to cache immediately, database asynchronously
- **TTL (Time To Live)**: Expire cache entries after time period
- **LRU (Least Recently Used)**: Evict oldest unused entries

##### Cache Invalidation

- Set appropriate expiration times
- Invalidate on updates
- Use cache keys that reflect data dependencies
- Consider event-driven invalidation

#### Lazy Loading

- **Defer expensive operations**: Only do work when needed
- **Lazy initialization**: Initialize objects on first use
- **Virtual scrolling**: Render only visible items
- **Lazy module loading**: Load code only when required
- **Lazy property evaluation**: Compute properties on access

**Example:**


- Load images only when they enter viewport
- Load menu items only when dropdown opens
- Initialize expensive services only when first used

#### Batch Operations

- **Batch database operations**: Insert/update multiple records at once
- **Batch API calls**: Send multiple requests together
- **Debouncing**: Delay execution until activity stops
- **Throttling**: Limit frequency of execution
- **Chunking**: Process large datasets in smaller batches


**Guidelines:**

- Batch size: 100-1000 items depending on operation
- Monitor memory usage with large batches
- Consider partial failure handling

#### Network Optimization

##### Request Optimization

- **Minimize requests**: Batch, combine, or eliminate requests
- **Compression**: Use gzip/brotli compression
- **CDN usage**: Serve static assets from CDN
- **Connection reuse**: Keep-alive connections
- **Parallel requests**: Fetch independent data simultaneously

##### Data Transfer

- **Pagination**: Don't send all data at once
- **Partial responses**: Return only requested fields
- **Incremental updates**: Send deltas, not full state
- **Compression**: Compress large payloads
- **Binary formats**: Consider Protocol Buffers, MessagePack

#### Frontend Performance

##### Rendering Optimization

- **Virtual DOM**: Minimize actual DOM manipulations
- **Reconciliation**: Batch DOM updates
- **Memoization**: Cache component renders
- **Code splitting**: Load code on demand
- **Tree shaking**: Remove unused code

##### Resource Loading

- **Lazy loading**: Load images/components on demand
- **Preloading**: Load critical resources early
- **Prefetching**: Load likely-needed resources
- **Resource hints**: dns-prefetch, preconnect
- **Asset optimization**: Minify, compress, optimize images

#### Profiling & Monitoring

##### What to Measure

- Response time (P50, P95, P99)
- Throughput (requests per second)
- Memory usage
- CPU utilization
- Database query time
- Cache hit rate

##### Profiling Tools

- Use language-specific profilers
- Monitor production metrics
- Set up alerting for anomalies
- Profile under realistic load
- Test with production-like data

#### Common Pitfalls

##### Avoid

❌ String concatenation in loops (use builders)
❌ Premature optimization
❌ Optimizing the wrong thing
❌ Trading readability for micro-optimizations
❌ Not measuring before/after optimization
❌ Ignoring Big-O complexity
❌ Over-caching
❌ Synchronous I/O in hot paths

##### Do

✅ Profile first, optimize second
✅ Focus on algorithmic improvements
✅ Optimize critical paths
✅ Use appropriate data structures
✅ Measure impact of changes
✅ Consider maintainability
✅ Document performance requirements
✅ Set performance budgets

#### Performance Testing

- **Load testing**: Test with realistic traffic
- **Stress testing**: Find breaking points
- **Spike testing**: Handle traffic spikes
- **Endurance testing**: Sustained load over time
- **Benchmarking**: Compare alternatives objectively

#### Optimization Priorities

1. **Correctness first**: Don't sacrifice correctness for speed
2. **Algorithmic improvements**: O(n²) → O(n log n) matters most
3. **Database optimization**: Often the bottleneck
4. **Caching**: High impact, low effort
