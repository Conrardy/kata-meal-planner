# Definir le mapping des entites vers Firestore

## Objectif
Definir un mapping clair entre les entites du domaine et les collections Firestore, en gardant une API backend unique et un stockage cible oriente document.

## Conventions generales
- Cle utilisateur canonique: AspNetUserId (du JWT interne) comme identifiant de document.
- FirebaseUid est resolu via UserIdentityMap et mappe vers AspNetUserId avant lecture/ecriture.
- Dates en `YYYY-MM-DD` (string) pour compatibilite et tri.
- Identifiants `Guid` stockes en string.
- Value Objects stockes en string/int (ex: `MealType`, `DietaryPreference`).
- Les documents restent petits; eviter des tableaux non bornes dans un document.

## Mapping par entite

### UserPreferences
- Collection: `users/{userId}/preferences`
- Document ID: `current`
- Champs:
  - `dietaryPreference`: string (ex: "Vegetarian")
  - `allergies`: array<string>
  - `mealsPerDay`: number
  - `planLength`: number
  - `includeLeftovers`: boolean
  - `autoGenerateShoppingList`: boolean
  - `excludedIngredients`: array<string>
  - `updatedAt`: timestamp

### ShoppingListState (pilot)
- Collection: `users/{userId}/shoppingListStates`
- Document ID: `{startDate}` (YYYY-MM-DD)
- Champs:
  - `startDate`: string (YYYY-MM-DD)
  - `endDate`: string (YYYY-MM-DD)
  - `checkedItems`: map<string, boolean> (itemId -> isChecked)
  - `customItems`: array<object>
    - `id`: string (ex: "custom-<guid>")
    - `name`: string
    - `quantity`: string
    - `unit`: string | null
    - `category`: string (Produce, Dairy, Meat, Pantry)
  - `updatedAt`: timestamp

### PlannedMeal (weekly plan / daily digest)
- Choix: collection plate (1 document par repas).
- Collection: `users/{userId}/plannedMeals`
- Document ID: `{plannedMealId}`
- Champs:
  - `date`: string (YYYY-MM-DD)
  - `mealType`: string (Breakfast, Lunch, Dinner)
  - `recipeId`: string (Guid)
  - `createdAt`: timestamp
  - `updatedAt`: timestamp

### Recipe
- Collection: `recipes`
- Document ID: `{recipeId}`
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

## Notes de coherence
- Les DTOs restent inchanges; le mapping est interne au backend.
- `ShoppingListDto` est construit a partir des `PlannedMeal` + `ShoppingListState`.
- Les documents `recipes` sont globaux et reutilisables entre utilisateurs.

## Questions ouvertes
- Aucune pour l instant.
