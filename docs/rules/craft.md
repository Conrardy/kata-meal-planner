# Software Craftsmanship Principles

> Language and framework agnostic rules for writing clean, maintainable code.

## Naming

- **Reveal intent**: Names should explain why something exists, what it does
- **Use domain language**: Prefer domain terms over technical terms
- **Avoid abbreviations**: Write `Customer` not `Cust`, `Recipe` not `Rcp`
- **Be consistent**: Same word for same concept across the codebase
- **No redundant context**: In `Recipe` class, use `title` not `recipeTitle`

## Functions/Methods

- **Single responsibility**: One function = one thing at one level of abstraction
- **Small size**: Aim for < 20 lines, definitely < 50 lines
- **Few parameters**: Max 3 parameters; more = use a parameter object
- **No side effects**: Don't modify state unexpectedly
- **Command-Query Separation**: Methods either do something OR return something, not both

## Error Handling

- **Fail fast**: Validate at boundaries, fail immediately on invalid input
- **Use exceptions for exceptional cases**: Not for control flow
- **Specific exceptions**: Create domain-specific exception types
- **Meaningful messages**: Include context about what failed and why
- **No silent failures**: Never catch and ignore exceptions without logging

## Null Handling

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

## Testing

- **Test behavior, not implementation**: Focus on what, not how
- **One assertion concept per test**: Tests should verify one logical concept
- **Given-When-Then**: Structure tests clearly: setup, action, verification
- **Fast tests**: Unit tests run in milliseconds
- **Independent tests**: No test depends on another test's execution
- **DAMP over DRY**: Tests can repeat setup for clarity (Descriptive And Meaningful Phrases)

## Boundaries

- **Dependency inversion**: Domain depends on nothing, all dependencies point inward
- **Ports and Adapters**: Isolate external systems behind interfaces
- **Anti-corruption layer**: Protect your domain from external model pollution
- **DTOs at boundaries**: Never pass domain objects across architectural boundaries
- **Explicit contracts**: Use interfaces to define contracts between layers

## Code Organization

- **Package by feature**: Group by business capability, not technical layer
- **Screaming architecture**: Structure should reveal the system's intent
- **Dependency direction**: Always point toward domain, never outward
- **Cohesion**: Things that change together belong together
- **Coupling**: Minimize dependencies between modules

## Refactoring Discipline

- **Red-Green-Refactor**: Make it work, make it right, make it fast (in that order)
- **Boy Scout Rule**: Leave code cleaner than you found it
- **Ruthless refactoring**: Don't accumulate technical debt
- **Preparatory refactoring**: Refactor to make the change easy, then make the change
- **Test-protected refactoring**: Only refactor with tests in place

## SOLID Principles

- **Single Responsibility**: A class should have only one reason to change
- **Open-Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Subtypes must be substitutable for their base types
- **Interface Segregation**: Many specific interfaces > one general interface
- **Dependency Inversion**: Depend on abstractions, not concretions

## Immutability & State Management

### Immutability Rules

- **Prefer immutable by default**: Make objects immutable unless there's a compelling reason
- **Copy-on-write**: Return new instances with modified state instead of mutating
- **No temporal coupling**: Avoid order-dependent method calls
- **Thread-safety by design**: Immutable objects are inherently thread-safe
- **Share freely**: Immutable objects can be safely shared without defensive copying

### State Changes

- **Minimize mutable state**: Keep stateful objects to a minimum
- **Local mutability**: If mutation is needed, keep it local and contained
- **Atomic state transitions**: State should move from one valid state to another atomically
- **No half-baked objects**: Never expose objects in partially initialized state

## Tell, Don't Ask Principle

- **Push behavior down**: Tell objects what to do, don't ask for their state
- **Avoid getter chains**: `order.getCustomer().getAddress().getCity()` is a smell
- **Encapsulate decisions**: Let objects make decisions about their own data
- **Reduce coupling**: Asking for data creates dependencies on structure

**Example:**

- ❌ Bad: `if (meal.getCalories() > limit) meal.setStatus(REJECTED)`
- ✅ Good: `meal.rejectIfExceedsCalories(limit)`

## Law of Demeter (Principle of Least Knowledge)

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

## Temporal Patterns (Time Handling)

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

## Defensive Programming

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

## Documentation Principles

- **Code explains how, comments explain why**: Don't duplicate what code already says
- **Comment the non-obvious**: Explain business rules, gotchas, workarounds
- **Keep docs close to code**: Documentation should live near what it documents
- **Update with code**: Outdated docs are worse than no docs
- **Architecture Decision Records (ADRs)**: Document significant decisions
- **Public API docs**: Every public API should have usage documentation
- **Examples over explanations**: Show how to use, don't just describe

### What to Document

- ✅ Why decisions were made
- ✅ Business rules and constraints
- ✅ Complex algorithms
- ✅ Workarounds and hacks (with context)
- ✅ Public API usage
- ❌ What the code obviously does
- ❌ Change history (use git)
- ❌ Author names (use git)
