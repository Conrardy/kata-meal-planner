# Architecture - MealPlanner

## Stack technique

- **Frontend** : Nuxt 3 (Vue.js)
- **Backend** : Laravel (PHP)
- **Database** : MySQL
- **API** : REST (JSON)
- **Auth** : Laravel Sanctum

## Vue d'ensemble

```
Frontend (Nuxt 3) ←─── HTTP/REST/JSON ───→ Backend (Laravel) ←─── ORM ───→ MySQL
```

---

## Structure Backend

```
backend/
├── app/
│   ├── Http/Controllers/Api/
│   │   ├── AuthController
│   │   ├── RecipeController
│   │   ├── MealPlanController
│   │   ├── ShoppingListController
│   │   └── UserPreferenceController
│   ├── Models/
│   │   ├── User, UserPreference, Allergy
│   │   ├── Recipe, Ingredient
│   │   ├── MealPlan, Meal
│   │   └── ShoppingList, ShoppingListItem
│   └── Services/
│       ├── MealPlanService          # Logique de génération de plans
│       ├── ShoppingListService      # Génération liste de courses
│       └── RecipeFilterService      # Filtrage intelligent
├── database/migrations/
└── routes/api.php
```

---

## Structure Frontend

```
frontend/
├── components/
│   ├── common/              # Design system (buttons, cards, etc.)
│   ├── home/                # Daily Digest, Weekly Calendar
│   ├── recipes/             # Search, Filters, Cards
│   ├── settings/            # Preferences, Allergies
│   └── shopping/            # List items, Categories
├── composables/
│   ├── useApi.ts
│   ├── useAuth.ts
│   ├── useMealPlan.ts
│   └── useRecipes.ts
├── pages/
│   ├── index.vue            # Home (Daily/Weekly view)
│   ├── recipes/
│   │   ├── index.vue        # Discovery
│   │   └── [id].vue         # Detail
│   ├── shopping-list.vue
│   └── settings.vue
└── stores/                  # Pinia
    ├── auth.ts
    ├── mealPlan.ts
    ├── recipes.ts
    ├── shoppingList.ts
    └── userPreferences.ts
```

---

## Modèle de données

### Relations principales

```
User
  ├─ 1:1 → UserPreference (dietary_preference, meals_per_day, etc.)
  ├─ N:M → Allergy
  ├─ 1:N → MealPlan
  └─ 1:N → ShoppingList

MealPlan
  ├─ 1:N → Meal (date, meal_type, recipe_id)
  └─ 1:1 ← ShoppingList

Recipe
  └─ N:M → Ingredient (via recipe_ingredients: quantity, unit)

ShoppingList
  └─ 1:N → ShoppingListItem (ingredient, quantity, is_checked, category)
```

### Tables clés

- `users`, `user_preferences`, `allergies`, `user_allergies`
- `recipes`, `ingredients`, `recipe_ingredients`
- `meal_plans`, `meals`
- `shopping_lists`, `shopping_list_items`

---

## API REST - Endpoints principaux

```
# Auth
POST   /api/register
POST   /api/login

# Preferences
GET    /api/preferences
PUT    /api/preferences

# Recipes
GET    /api/recipes              # Params: dietary_preference, meal_type, tags, search
GET    /api/recipes/{id}

# Meal Plans
GET    /api/meal-plans/current
POST   /api/meal-plans
POST   /api/meal-plans/{id}/meals
PUT    /api/meals/{id}           # Swap meal
POST   /api/meal-plans/generate  # Auto-generate

# Shopping Lists
POST   /api/shopping-lists/generate   # From meal plan
PUT    /api/shopping-list-items/{id}/toggle
POST   /api/shopping-lists/{id}/items
```

---

## Flux métier principaux

### 1. Configuration initiale
User → Settings → Backend (save preferences) → Ready for meal planning

### 2. Création plan de repas
Browse recipes (filtered) → Add to plan → Backend saves Meal entries → Display weekly view

### 3. Génération liste de courses
Click "Generate" → Backend aggregates ingredients from meal plan → Create shopping list → Display by category

### 4. Swap de repas
Click "Swap Meal" → Fetch alternatives → Select new recipe → Update Meal → Refresh display

---

## Considérations clés

### Performance
- **Backend** : Eager loading, caching (Redis optionnel), pagination
- **Frontend** : Lazy loading images, code splitting (auto Nuxt), debounce search

### Sécurité
- Sanctum tokens, CORS strict, validation Form Requests, Policies Laravel

### Scalabilité
- Indexation DB (user_id, date, meal_type)
- Queues Laravel pour génération plans complexes
- Cache résultats filtres fréquents
