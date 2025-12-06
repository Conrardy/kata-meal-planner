# Performance Patterns & Optimization

> Language and framework agnostic rules for writing performant code.

## General Rules

- **Measure, don't guess**: Profile before optimizing
- **Big-O matters**: Understand algorithmic complexity
- **Lazy initialization**: Defer expensive operations until needed
- **Caching strategy**: Cache expensive computations, not everything
- **Batch operations**: Process in batches when dealing with collections
- **Avoid premature optimization**: Make it work, make it right, then make it fast

## Algorithmic Complexity

### Know Your Big-O

- **O(1)**: Constant time - hash lookups, array access by index
- **O(log n)**: Logarithmic - binary search, balanced trees
- **O(n)**: Linear - single loop through data
- **O(n log n)**: Log-linear - efficient sorting algorithms
- **O(n²)**: Quadratic - nested loops, naive sorting
- **O(2ⁿ)**: Exponential - recursive fibonacci, backtracking

### Guidelines

- Aim for O(n log n) or better for large datasets
- Avoid nested loops when possible
- Use appropriate data structures (hash maps vs arrays)
- Consider space-time tradeoffs

## Database Patterns

### Query Optimization

- **N+1 query problem**: Load related data in one query, not in a loop
- **Select only needed fields**: Don't use `SELECT *`
- **Pagination**: Always paginate large result sets
- **Database indexes**: Index fields used in WHERE, JOIN, ORDER BY
- **Query plan analysis**: Use EXPLAIN to understand query execution
- **Avoid SELECT DISTINCT**: Often indicates data model issues

### Connection Management

- **Connection pooling**: Reuse connections, don't create per request
- **Connection limits**: Set appropriate pool sizes
- **Timeout configuration**: Set reasonable timeouts
- **Close connections**: Always release connections back to pool

### Data Access Patterns

- **Lazy vs Eager loading**: Choose based on use case
- **Batch fetching**: Load multiple records in one query
- **Projection queries**: Select only what you need
- **Caching strategy**: Cache frequently accessed, rarely changed data
- **Read replicas**: Distribute read load across replicas

## Memory Patterns

### Memory Management

- **Stream large datasets**: Don't load everything into memory
- **Release resources**: Close connections, file handles, streams
- **Avoid memory leaks**: Clear references, unsubscribe listeners
- **Object pooling**: Reuse expensive objects when appropriate
- **Weak references**: Use for caches that shouldn't prevent garbage collection

### Data Structure Selection

- **Arrays**: Fast access by index, contiguous memory
- **Linked lists**: Fast insertion/deletion, slow access
- **Hash maps**: O(1) lookup, more memory overhead
- **Sets**: Fast membership testing
- **Trees**: Ordered data, O(log n) operations

## Caching Strategies

### When to Cache

✅ Expensive computations
✅ Frequently accessed data
✅ Rarely changing data
✅ Remote API responses
✅ Database query results

❌ User-specific data (unless per-user cache)
❌ Fast operations
❌ Rapidly changing data
❌ Large objects with low reuse

### Cache Patterns

- **Cache-aside**: Application manages cache explicitly
- **Read-through**: Cache automatically loads on miss
- **Write-through**: Write to cache and database synchronously
- **Write-behind**: Write to cache immediately, database asynchronously
- **TTL (Time To Live)**: Expire cache entries after time period
- **LRU (Least Recently Used)**: Evict oldest unused entries

### Cache Invalidation

- Set appropriate expiration times
- Invalidate on updates
- Use cache keys that reflect data dependencies
- Consider event-driven invalidation

## Lazy Loading

- **Defer expensive operations**: Only do work when needed
- **Lazy initialization**: Initialize objects on first use
- **Virtual scrolling**: Render only visible items
- **Lazy module loading**: Load code only when required
- **Lazy property evaluation**: Compute properties on access

**Example:**


- Load images only when they enter viewport
- Load menu items only when dropdown opens
- Initialize expensive services only when first used

## Batch Operations

- **Batch database operations**: Insert/update multiple records at once
- **Batch API calls**: Send multiple requests together
- **Debouncing**: Delay execution until activity stops
- **Throttling**: Limit frequency of execution
- **Chunking**: Process large datasets in smaller batches


**Guidelines:**

- Batch size: 100-1000 items depending on operation
- Monitor memory usage with large batches
- Consider partial failure handling

## Network Optimization

### Request Optimization

- **Minimize requests**: Batch, combine, or eliminate requests
- **Compression**: Use gzip/brotli compression
- **CDN usage**: Serve static assets from CDN
- **Connection reuse**: Keep-alive connections
- **Parallel requests**: Fetch independent data simultaneously

### Data Transfer

- **Pagination**: Don't send all data at once
- **Partial responses**: Return only requested fields
- **Incremental updates**: Send deltas, not full state
- **Compression**: Compress large payloads
- **Binary formats**: Consider Protocol Buffers, MessagePack

## Frontend Performance

### Rendering Optimization

- **Virtual DOM**: Minimize actual DOM manipulations
- **Reconciliation**: Batch DOM updates
- **Memoization**: Cache component renders
- **Code splitting**: Load code on demand
- **Tree shaking**: Remove unused code

### Resource Loading

- **Lazy loading**: Load images/components on demand
- **Preloading**: Load critical resources early
- **Prefetching**: Load likely-needed resources
- **Resource hints**: dns-prefetch, preconnect
- **Asset optimization**: Minify, compress, optimize images

## Profiling & Monitoring

### What to Measure

- Response time (P50, P95, P99)
- Throughput (requests per second)
- Memory usage
- CPU utilization
- Database query time
- Cache hit rate

### Profiling Tools

- Use language-specific profilers
- Monitor production metrics
- Set up alerting for anomalies
- Profile under realistic load
- Test with production-like data

## Common Pitfalls

### Avoid

❌ String concatenation in loops (use builders)
❌ Premature optimization
❌ Optimizing the wrong thing
❌ Trading readability for micro-optimizations
❌ Not measuring before/after optimization
❌ Ignoring Big-O complexity
❌ Over-caching
❌ Synchronous I/O in hot paths

### Do

✅ Profile first, optimize second
✅ Focus on algorithmic improvements
✅ Optimize critical paths
✅ Use appropriate data structures
✅ Measure impact of changes
✅ Consider maintainability
✅ Document performance requirements
✅ Set performance budgets

## Performance Testing

- **Load testing**: Test with realistic traffic
- **Stress testing**: Find breaking points
- **Spike testing**: Handle traffic spikes
- **Endurance testing**: Sustained load over time
- **Benchmarking**: Compare alternatives objectively

## Optimization Priorities

1. **Correctness first**: Don't sacrifice correctness for speed
2. **Algorithmic improvements**: O(n²) → O(n log n) matters most
3. **Database optimization**: Often the bottleneck
4. **Caching**: High impact, low effort
