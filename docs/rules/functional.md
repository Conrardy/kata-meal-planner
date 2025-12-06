# Functional Programming Principles

> Language and framework agnostic rules for functional programming practices.

## Core Principles

- **Pure functions**: Same input = same output, no side effects
- **Referential transparency**: Expression can be replaced with its value
- **Higher-order functions**: Functions that take/return functions
- **Function composition**: Build complex behavior from simple functions
- **Avoid shared mutable state**: Pass data through function chains
- **Immutable data structures**: Transform, don't mutate
- **Declarative over imperative**: Express what, not how

## Functional Patterns

- **Map**: Transform each element in a collection
- **Filter**: Select elements matching criteria
- **Reduce**: Aggregate collection to single value
- **Pipeline**: Chain operations on data streams
- **Monads**: Optional, Result, Either for error handling

## Pure Functions

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

## Function Composition

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

## Immutability

- **Never mutate**: Always return new data structures
- **Structural sharing**: Use persistent data structures for efficiency
- **Immutable by default**: Make immutability the default, mutation explicit
- **Copy-on-write**: Clone and modify instead of mutating in place

**Transformations:**
- Arrays: Use `map`, `filter`, `reduce`, spread operator
- Objects: Use spread operator, `Object.assign`, or libraries
- Deep updates: Use lenses or utility libraries

## Higher-Order Functions

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

## Currying & Partial Application

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

## Monads for Error Handling

### Optional/Maybe Monad

- Represents presence or absence of a value
- Avoids null/undefined checks
- Chains operations safely

### Result/Either Monad

- Represents success or failure
- Captures error information
- Enables railway-oriented programming

**Benefits:**
- No exception throwing
- Explicit error handling
- Composable error paths
- Type-safe error propagation

## Declarative vs Imperative

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

## Function Purity Guidelines

### Pure (Allowed)

✅ Operating on parameters
✅ Calling other pure functions
✅ Creating new local variables
✅ Returning values

### Impure (Avoid in pure functions)

❌ Mutating parameters or external state
❌ Reading/writing files
❌ Making network calls
❌ Reading system time
❌ Generating random numbers
❌ Logging to console
❌ Throwing exceptions

## Practical Application

### When to Use FP

✅ Data transformations
✅ Business logic
✅ Calculations and algorithms
✅ Data validation
✅ Query operations

### When to Use Imperative

✅ I/O operations
✅ Performance-critical sections
✅ Interfacing with imperative APIs
✅ Low-level optimizations

## Best Practices

- **Isolate side effects**: Push them to the edges
- **Small functions**: Each does one thing well
- **Descriptive names**: Function name describes what it returns
- **Avoid deep nesting**: Use composition or early returns
- **Prefer expressions over statements**: Return values, don't mutate state
- **Type annotations**: Make function signatures explicit
- **Test pure functions**: Easy to test with simple inputs/outputs
