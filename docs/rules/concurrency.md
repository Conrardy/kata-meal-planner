# Concurrency & Thread Safety

> Language and framework agnostic rules for writing concurrent, thread-safe code.

## Core Principles

- **Immutability first**: Immutable objects are thread-safe by default
- **No shared mutable state**: Avoid shared state between threads
- **Synchronization boundaries**: Clearly mark synchronized regions
- **Minimize lock scope**: Lock for shortest time possible
- **Avoid deadlocks**: Always acquire locks in same order
- **Use higher-level abstractions**: Prefer concurrent collections, async patterns
- **Race conditions**: Identify and protect critical sections
- **Thread-local storage**: Use for thread-specific state
- **Non-blocking algorithms**: Prefer lock-free when possible

## Immutability & Thread Safety

### Why Immutability Matters

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

### Guidelines

- Make objects immutable by default
- Use immutable collections
- Copy-on-write for modifications
- No setters, only constructors
- Mark fields as final/const/readonly

## Avoiding Shared Mutable State

### Problems with Shared State

- Race conditions
- Memory visibility issues
- Coordination overhead
- Difficult debugging

### Solutions

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

## Synchronization

### When to Synchronize

✅ Accessing shared mutable state
✅ Coordinating between threads
✅ Ensuring visibility of changes
✅ Protecting invariants

❌ Reading immutable data
❌ Thread-local data
❌ Pure computations
❌ Already thread-safe operations

### Synchronization Mechanisms

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

### Lock Guidelines

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

## Deadlock Prevention

### Deadlock Conditions

Deadlock requires ALL four:
1. Mutual exclusion (locks are exclusive)
2. Hold and wait (hold lock while waiting for another)
3. No preemption (can't force lock release)
4. Circular wait (circular dependency of locks)

### Prevention Strategies

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

## Race Conditions

### Types

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

### Detection

- Code review
- Static analysis tools
- Thread safety annotations
- Stress testing
- Race condition detectors (ThreadSanitizer, etc.)

## Thread-Safe Data Structures

### Use Built-in Concurrent Collections

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

### When to Synchronize Collections

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

## Asynchronous Patterns

### Benefits

- No thread blocking
- Efficient resource usage
- Simpler than callbacks
- Exception handling preserved

### Guidelines

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

### Error Handling

```
process():
  try:
    result = await_result(operation())
    return result
  catch error:
    // handle or propagate
    throw error
```

## Actor Model

### Principles

- Actors are isolated units
- Communicate via messages
- Process one message at a time
- No shared state

### Benefits

- No locks needed
- Natural concurrency
- Location transparency
- Fault tolerance

### Pattern

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

## Thread Pools

### Sizing

**CPU-bound tasks:**
- Pool size = number of cores
- More threads = more context switching

**I/O-bound tasks:**
- Pool size > number of cores
- Threads can wait during I/O
- Formula: cores * (1 + wait time / compute time)

### Best Practices

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

## Memory Visibility

### The Problem

Changes in one thread may not be visible to others due to:
- CPU caches
- Compiler optimizations
- Out-of-order execution

### Solutions

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

## Testing Concurrent Code

### Strategies

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

### Example

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

## Common Pitfalls

### Avoid

❌ Assuming operations are atomic
❌ Forgetting to synchronize
❌ Over-synchronizing
❌ Acquiring locks in different orders
❌ Holding locks during I/O
❌ Accessing shared state without synchronization
❌ Using mutable objects as keys
❌ Thread.stop() or killing threads
❌ Ignoring memory visibility

### Do

✅ Prefer immutability
✅ Use concurrent collections
✅ Minimize shared state
✅ Synchronize sparingly but correctly
✅ Use established patterns
✅ Test with multiple threads
✅ Document thread-safety guarantees
✅ Use static analysis tools
✅ Profile and measure

## Best Practices Summary

1. **Immutable by default**: Eliminate most concurrency issues
2. **Isolate mutable state**: Minimize shared mutable state
3. **Use high-level abstractions**: Let libraries handle complexity
4. **Lock carefully**: Minimize scope, avoid nesting
5. **Prevent deadlocks**: Consistent lock ordering
6. **Test thoroughly**: Concurrent bugs are hard to find
7. **Document guarantees**: Specify thread-safety expectations
8. **Measure performance**: Concurrency can hurt if done wrong
