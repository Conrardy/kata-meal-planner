# Development Rules & Principles

> Language and framework agnostic rules for building robust, maintainable software.

This directory contains comprehensive guidelines for software development following Domain-Driven Design, Software Craftsmanship, and modern engineering practices.

## Quick Reference

### Core Files

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

## How to Use

### For New Projects

1. Start with **ddd.md** to establish domain boundaries
2. Follow **craft.md** for clean code practices
3. Reference **api-design.md** when building APIs
4. Implement **observability.md** from day one
5. Apply **performance.md** when needed (measure first!)
6. Use **functional.md** for data transformations
7. Consult **concurrency.md** when dealing with parallel processing

### For Code Reviews

Check against:
- Does it follow DDD patterns appropriately?
- Is the code clean and maintainable (craft principles)?
- Are APIs well-designed and documented?
- Is observability built-in?
- Are concurrency issues handled correctly?

### For Refactoring

1. Add tests first
2. Apply principles incrementally
3. Focus on highest-value improvements
4. Measure before and after (performance)
5. Document decisions (ADRs)

## Principles Priority

### Always Apply
- ✅ Clean naming (craft.md)
- ✅ Error handling (craft.md)
- ✅ Testing (craft.md)
- ✅ Logging at boundaries (observability.md)
- ✅ Input validation (api-design.md)

### Apply When Relevant
- 🟡 DDD patterns for complex domains (ddd.md)
- 🟡 CQRS for read/write separation (ddd.md)
- 🟡 Functional patterns for transformations (functional.md)
- 🟡 Concurrency patterns for parallel work (concurrency.md)

### Apply When Needed
- ⚠️ Performance optimization (measure first!)
- ⚠️ Advanced caching strategies
- ⚠️ Distributed tracing (start simple)

## Quick Tips

### Domain Modeling
- Value Objects for concepts without identity
- Entities for things with lifecycle
- Aggregates for consistency boundaries
- Events for things that happened

### Clean Code
- Functions < 20 lines
- Max 3 parameters
- One level of abstraction
- Tell, don't ask

### Performance
- Big-O matters most
- Measure, don't guess
- Database is often the bottleneck
- Cache wisely, not everywhere

### APIs
- Use HTTP verbs correctly
- Return appropriate status codes
- Version from day one
- Document everything

### Observability
- Structured logging
- Include correlation IDs
- Track golden signals
- Create actionable alerts

### Concurrency
- Immutability first
- Minimize shared state
- Use concurrent collections
- Prevent deadlocks by design

## Anti-Patterns to Avoid

❌ Anemic domain models (logic in services)
❌ God objects (classes that do too much)
❌ Primitive obsession (int instead of UserId)
❌ Premature optimization
❌ Logging sensitive data
❌ Blocking in async code
❌ Nested locks
❌ Missing error handling

## Contributing

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

## See Also

- [AGENTS.md](../../AGENTS.md) - Project-specific guidelines
- Architecture Decision Records (ADRs)
- Team coding standards
