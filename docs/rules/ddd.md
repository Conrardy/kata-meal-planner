# Domain-Driven Design Tactical Patterns

> Language and framework agnostic rules for DDD tactical patterns.

## Value Objects

- **Immutability**: Value Objects MUST be immutable once created
- **Equality by value**: Two Value Objects are equal if all their attributes are equal
- **Self-validation**: Value Objects validate themselves in their constructor/factory
- **No identity**: Value Objects have no conceptual identity, only their values matter
- **Small and focused**: Represent a single concept from the domain (Email, Money, DateRange)
- **Methods express behavior**: Add domain methods that operate on the value (e.g., `Money.add()`, `DateRange.overlaps()`)

## Entities

- **Identity**: Entities have a unique identifier that remains constant throughout their lifecycle
- **Mutable state**: State can change, but identity persists
- **Equality by ID**: Two Entities are equal if they have the same identifier
- **Encapsulation**: Hide internal state, expose behavior through methods
- **Business logic location**: Put domain logic that changes Entity state inside the Entity itself
- **No anemic models**: Entities should contain behavior, not just getters/setters

## Aggregates

- **Single root**: Each Aggregate has ONE root Entity that controls access
- **Consistency boundary**: All invariants must be satisfied within the Aggregate
- **Transaction boundary**: One transaction = one Aggregate modification
- **Reference by ID**: External Aggregates reference each other by ID only, not by direct object reference
- **Small boundaries**: Keep Aggregates as small as possible while maintaining consistency
- **Lazy loading**: Load related Aggregates separately, don't cascade load

## Domain Events

- **Past tense naming**: Events represent something that happened (e.g., `MealPlanned`, `RecipeSwapped`)
- **Immutable**: Events cannot be changed once created
- **Contain minimal data**: Include only what subscribers need to react
- **Represent business facts**: Express what happened in domain terms, not technical terms
- **Publish after persistence**: Emit events only after the Aggregate is successfully persisted

## Repositories

- **Collection metaphor**: Act like an in-memory collection of Aggregates
- **One per Aggregate root**: Create repositories only for Aggregate roots
- **Domain-oriented interface**: Use domain language (`findByEmail()` not `selectByEmailColumn()`)
- **Hide persistence**: Keep infrastructure concerns out of the domain
- **Return domain objects**: Always return Entities/Aggregates, never raw data structures

## Domain Services

- **Stateless**: Domain Services hold no state, only behavior
- **Multi-Aggregate operations**: Use when logic spans multiple Aggregates
- **Domain logic only**: No infrastructure concerns (no DB, no HTTP, no file system)
- **Explicit dependencies**: Require repositories or other services as parameters
- **Verb-based naming**: Name after the action performed (e.g., `PlanMealService`, `SwapMealService`)

## Application Services

- **Use case orchestration**: One Application Service = one use case
- **Transaction management**: Handle transaction boundaries here
- **DTO transformation**: Convert domain objects to/from DTOs
- **Thin layer**: Coordinate, don't contain business logic
- **Exception translation**: Catch domain exceptions, translate to application-appropriate responses

## Factories

- **Complex creation**: Use when object creation is complex or involves rules
- **Encapsulate creation logic**: Hide how objects are constructed
- **Return valid objects**: Never return partially constructed or invalid objects
- **Reconstitution**: Separate factory methods for creating new vs reconstituting from storage

## CQRS (Command Query Responsibility Segregation)

### Command Side

- **Intent-based commands**: Name commands after business intent (`PlanMeal`, not `CreateMealPlan`)
- **Commands never return domain data**: Return acknowledgment/ID only
- **Validate commands**: Check business rules before execution
- **One transaction per command**: Command = unit of work

### Query Side

- **Queries never mutate**: Pure read operations with no side effects
- **Optimize for reading**: Query models can denormalize for performance
- **Eventual consistency**: Accept that read models may lag behind writes
- **Multiple read models**: Create specialized projections for different use cases

### General

- **Clear separation**: Never mix command and query in same operation
- **Different models**: Write model (domain) ≠ read model (projection)

## Decision Making Guidelines

### When to Create a Value Object

✅ Create when:
- The concept has no identity (Email, Money, Quantity)
- Equality is determined by value, not ID
- Multiple attributes form a cohesive concept
- There are validation rules or invariants

❌ Don't create when:
- It's a primitive obsession reflex (not everything needs to be wrapped)
- The value has no domain meaning
- It would add complexity without benefit

### When to Create an Entity vs Value Object

**Entity** if:
- It has a lifecycle and identity matters over time
- You track it, reference it, update it
- Example: Recipe, MealPlan, User

**Value Object** if:
- Identity doesn't matter, only the values
- Immutable and replaceable
- Example: Ingredient, CalorieCount, ServingSize

### When to Create a Domain Service

✅ Create when:
- Operation spans multiple Aggregates
- Logic doesn't naturally fit in any single Entity
- The operation is a significant domain concept

❌ Don't create when:
- Logic belongs to a single Entity
- It's just a utility function (use static/module functions)

### When to Emit a Domain Event

✅ Emit when:
- Something significant happened in the domain
- Other parts of the system need to react
- You want to decouple components

❌ Don't emit when:
- It's just a CRUD operation with no domain meaning
- Only used for logging/auditing (use infrastructure events)
- Would create unnecessary coupling

## Anti-Patterns to Avoid

- **Anemic Domain Model**: Entities with only getters/setters, logic in services
- **God Objects**: Classes that know/do too much
- **Feature Envy**: Methods that use more of another class than their own
- **Primitive Obsession**: Using primitives instead of small domain objects
- **Shotgun Surgery**: One change requires modifications everywhere
- **Leaky Abstractions**: Implementation details bleeding through interfaces
- **Smart UI**: Business logic in presentation layer
- **Transaction Script**: Procedural code with no domain model
