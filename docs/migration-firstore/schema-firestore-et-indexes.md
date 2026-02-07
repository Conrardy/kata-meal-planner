# Schema Firestore et indexes

## Objectif
Definir le schema Firestore (collections, documents, champs) et les indexes necessaires pour les ecrans pilotes, sur la base du mapping valide.

## Conventions
- `userId` = AspNetUserId (source de verite JWT interne).
- Dates au format `YYYY-MM-DD` (string) pour requetes de plages.
- `Guid` stockes en string.
- Champs `createdAt` / `updatedAt` en `timestamp`.

## Schema (collections principales)

### `users/{userId}/preferences`
- Document: `current`
- Champs:
  - `dietaryPreference`: string
  - `allergies`: array<string>
  - `mealsPerDay`: number
  - `planLength`: number
  - `includeLeftovers`: boolean
  - `autoGenerateShoppingList`: boolean
  - `excludedIngredients`: array<string>
  - `updatedAt`: timestamp

### `users/{userId}/shoppingListStates`
- Document: `{startDate}`
- Champs:
  - `startDate`: string
  - `endDate`: string
  - `checkedItems`: map<string, boolean>
  - `customItems`: array<object>
    - `id`: string
    - `name`: string
    - `quantity`: string
    - `unit`: string | null
    - `category`: string
  - `updatedAt`: timestamp

### `users/{userId}/plannedMeals`
- Document: `{plannedMealId}`
- Champs:
  - `date`: string (YYYY-MM-DD)
  - `mealType`: string
  - `recipeId`: string
  - `createdAt`: timestamp
  - `updatedAt`: timestamp

### `recipes`
- Document: `{recipeId}`
- Champs:
  - `name`: string
  - `imageUrl`: string | null
  - `description`: string | null
  - `tags`: array<string>
  - `mealType`: string
  - `ingredients`: array<object>
    - `name`: string
    - `quantity`: string
    - `unit`: string | null
  - `steps`: array<object>
    - `stepNumber`: number
    - `instruction`: string
  - `createdAt`: timestamp
  - `updatedAt`: timestamp

## Indexes requis

### Planned meals (weekly plan, daily digest)
- Collection: `users/{userId}/plannedMeals`
- Requetes:
  - Par plage de dates: `where date >= startDate` + `where date <= endDate`
- Index:
  - `date` (ASC)

### Shopping list state
- Collection: `users/{userId}/shoppingListStates`
- Requetes:
  - Par document ID (`startDate`)
- Index:
  - Aucun index composite requis (lookup direct par ID).

### Recipes (browse)
- Collection: `recipes`
- Requetes:
  - Filtre par `mealType` (egalite)
  - Filtre par `tags` (array-contains)
- Indexes:
  - Composite: `mealType` (ASC), `tags` (ARRAY_CONTAINS)

## Notes
- Les indexes ci-dessus couvrent les ecrans pilotes; ajouter des indexes uniquement si necessaire.
- Garder une liste courte pour limiter les couts et la complexite d evolution.
