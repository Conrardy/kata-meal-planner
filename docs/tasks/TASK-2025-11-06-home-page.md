# TASK - Home Page (Daily Digest & Weekly Meal Plan)

## Contexte

Implémenter la page d'accueil de MealPlanner avec deux vues :
- **Mobile** : Daily Digest affichant les repas de la journée (Breakfast, Lunch, Dinner)
- **Desktop** : This Week's Meal Plan avec calendrier mensuel, tableau hebdomadaire, quick actions et upcoming meals

### Objectifs
- Permettre la visualisation des repas planifiés (vue journalière et hebdomadaire)
- Offrir des actions rapides : Swap Meal, Cook Now, Add Recipe, Create Meal Plan, Generate Grocery List
- Assurer une navigation fluide et un affichage responsive
- Gérer les cas où aucun repas n'est planifié

---

## Backend

### Migrations

- [ ] **Vérifier l'existence des tables** `meal_plans` et `meals`
  - Si absentes, créer les migrations suivantes :

```php
// migration: create_meal_plans_table
Schema::create('meal_plans', function (Blueprint $table) {
    $table->id();
    $table->foreignId('user_id')->constrained()->onDelete('cascade');
    $table->date('start_date');
    $table->date('end_date');
    $table->boolean('is_active')->default(true);
    $table->timestamps();
});
```

```php
// migration: create_meals_table
Schema::create('meals', function (Blueprint $table) {
    $table->id();
    $table->foreignId('meal_plan_id')->constrained()->onDelete('cascade');
    $table->foreignId('recipe_id')->constrained()->onDelete('cascade');
    $table->date('date');
    $table->enum('meal_type', ['breakfast', 'lunch', 'dinner', 'snack']);
    $table->integer('order')->default(0);
    $table->timestamps();
    
    $table->index(['meal_plan_id', 'date', 'meal_type']);
});
```

### Models + Relations

- [ ] **Model `MealPlan`** (`app/Models/MealPlan.php`)
  - Relations :
    - `belongsTo(User::class)`
    - `hasMany(Meal::class)`
  - Scopes :
    - `scopeActive()` : plan actif
    - `scopeForUser($userId)` : plan d'un utilisateur
    - `scopeCurrent()` : plan en cours (date actuelle entre start_date et end_date)

- [ ] **Model `Meal`** (`app/Models/Meal.php`)
  - Relations :
    - `belongsTo(MealPlan::class)`
    - `belongsTo(Recipe::class)`
  - Scopes :
    - `scopeForDate($date)` : repas d'une date spécifique
    - `scopeForWeek($startDate, $endDate)` : repas d'une semaine
    - `scopeByType($type)` : filtre par type de repas

- [ ] **Mise à jour du Model `Recipe`** (si nécessaire)
  - Vérifier l'existence des champs : `name`, `image_url`, `description`, `cooking_time`

### Controllers + Endpoints API

- [ ] **Controller `MealPlanController`** (`app/Http/Controllers/Api/MealPlanController.php`)

**Endpoints à créer :**

1. `GET /api/meal-plans/current`
   - Retourne le plan de repas actif de l'utilisateur
   - Inclut les repas (`with('meals.recipe')`)
   - Si aucun plan actif : retourne `null` ou `404`

2. `GET /api/meal-plans/current/daily?date={date}`
   - Retourne les repas d'une date spécifique
   - Paramètre optionnel `date` (défaut : aujourd'hui)
   - Format réponse :
```json
{
  "date": "2025-07-15",
  "day_name": "Monday",
  "meals": {
    "breakfast": { "id": 1, "recipe": {...} },
    "lunch": { "id": 2, "recipe": {...} },
    "dinner": { "id": 3, "recipe": {...} }
  }
}
```

3. `GET /api/meal-plans/current/weekly?start_date={date}`
   - Retourne les repas d'une semaine (7 jours)
   - Paramètre optionnel `start_date` (défaut : lundi de la semaine actuelle)
   - Format réponse :
```json
{
  "week_range": "July 15 – July 21",
  "days": [
    {
      "date": "2025-07-15",
      "day_name": "Monday",
      "meals": {
        "breakfast": {...},
        "lunch": {...},
        "dinner": {...}
      }
    },
    // ... 6 autres jours
  ]
}
```

4. `GET /api/meal-plans/current/upcoming?limit={number}`
   - Retourne les prochains repas à venir (ordre chronologique)
   - Paramètre optionnel `limit` (défaut : 5)
   - Filtre : `date >= aujourd'hui`

- [ ] **Controller `MealController`** (`app/Http/Controllers/Api/MealController.php`)

**Endpoints à créer :**

1. `PUT /api/meals/{id}/swap`
   - Remplace le repas par une autre recette
   - Body : `{ "recipe_id": 123 }`
   - Validation : recette existe, compatible avec les préférences utilisateur
   - Retourne le repas mis à jour avec la nouvelle recette

### Services (logique métier)

- [ ] **Service `MealPlanService`** (`app/Services/MealPlanService.php`)

Méthodes :
- `getCurrentPlanForUser($userId)` : récupère le plan actif
- `getDailyMeals($userId, $date)` : repas d'une journée
- `getWeeklyMeals($userId, $startDate)` : repas d'une semaine
- `getUpcomingMeals($userId, $limit = 5)` : prochains repas
- `swapMeal($mealId, $newRecipeId, $userId)` : échange un repas (avec validation)

### Validation (Form Requests)

- [ ] **`SwapMealRequest`** (`app/Http/Requests/SwapMealRequest.php`)
  - Règles :
    - `recipe_id` : required, exists:recipes,id
  - Validation personnalisée : vérifier que la recette est compatible avec les préférences/allergies de l'utilisateur

### Seeders (données de test)

- [ ] **Seeder pour `MealPlans` et `Meals`**
  - Créer un plan actif pour un utilisateur de test
  - Peupler avec des repas pour la semaine en cours et à venir
  - Utiliser des recettes existantes (ou créer un RecipeSeeder si nécessaire)

---

## Frontend

### Composants UI

#### Design System (`components/common/`)

- [ ] **`BaseCard.vue`**
  - Props : `variant` (default, elevated), `padding`, `rounded`
  - Style : fond blanc, bordures arrondies, ombre légère

- [ ] **`BaseButton.vue`**
  - Props : `variant` (primary, secondary, outline), `size`, `icon`, `loading`
  - Variants :
    - `primary` : fond orange (#FF8C00), texte blanc
    - `secondary` : fond orange clair (#FFD700), texte orange
    - `outline` : bordure orange, fond transparent

- [ ] **`AppHeader.vue`**
  - Affichage : logo "MealPrep", titre, icônes notification/menu (desktop), icône profil
  - Position fixe en haut
  - Responsive (mobile/desktop)

#### Composants Home (`components/home/`)

- [ ] **`MealCard.vue`**
  - Props : `meal` (objet avec recipe, meal_type), `compact` (mode mobile/desktop)
  - Affichage :
    - Image du plat (avec fallback si manquante)
    - Nom du plat (gestion troncature si long)
    - Boutons "Swap Meal" et "Cook Now" (si non compact)
  - Événements : `@swap`, `@cook`

- [ ] **`DailyDigest.vue`**
  - Props : `date`, `meals` (breakfast, lunch, dinner)
  - Affichage :
    - Titre "Daily Digest"
    - Date formatée (ex. "July 15, Monday") en orange
    - Liste verticale de `MealCard` pour chaque repas
  - Gestion : message si aucun repas planifié

- [ ] **`WeeklyCalendar.vue`**
  - Props : `currentMonth`, `currentYear`, `selectedDate`
  - Affichage :
    - Titre du mois avec flèches de navigation
    - Grille des jours (7 colonnes × 5-6 lignes)
    - Jour actuel mis en évidence (cercle orange)
  - Événements : `@month-change`, `@day-select`

- [ ] **`WeeklyMealTable.vue`**
  - Props : `weekData` (array de 7 jours avec meals)
  - Affichage :
    - Titre "This Week's Meal Plan" avec période
    - Tableau : colonnes (Day, Breakfast, Lunch, Dinner)
    - Noms de plats cliquables (orange)
  - Événements : `@meal-click`

- [ ] **`QuickActions.vue`**
  - Affichage : 3 boutons verticaux
    - "Add Recipe" (icône +)
    - "Create Meal Plan" (icône calendrier)
    - "Generate Grocery List" (icône liste)
  - Événements : `@add-recipe`, `@create-plan`, `@generate-list`

- [ ] **`UpcomingMeals.vue`**
  - Props : `meals` (liste des prochains repas)
  - Affichage :
    - Titre "Upcoming Meals"
    - Liste de cartes miniatures (image + nom + date/heure)
  - Événement : `@meal-click`

### Pages/Vues

- [ ] **`pages/index.vue`** (Home Page)
  - Layout responsive :
    - **Mobile** : Affichage de `DailyDigest`
    - **Desktop** : Grille 3 colonnes
      - Colonne gauche : `WeeklyCalendar` + `QuickActions`
      - Colonne centrale : `WeeklyMealTable`
      - Colonne droite : `UpcomingMeals`
  - Détection responsive : `useBreakpoints` ou media queries
  - Chargement initial des données au `onMounted`

### Store Pinia

- [ ] **Store `mealPlan`** (`stores/mealPlan.ts`)

**State :**
```typescript
{
  currentPlan: MealPlan | null,
  dailyMeals: DailyMeals | null,
  weeklyMeals: WeeklyMeals | null,
  upcomingMeals: Meal[],
  selectedDate: Date,
  selectedMonth: { month: number, year: number },
  isLoading: boolean,
  error: string | null
}
```

**Getters :**
- `hasMealPlan` : boolean (plan actif existe)
- `formattedDailyDate` : string (ex. "July 15, Monday")
- `formattedWeekRange` : string (ex. "July 15 – July 21")

**Actions :**
- `fetchCurrentPlan()` : récupère le plan actif
- `fetchDailyMeals(date?)` : repas d'une journée
- `fetchWeeklyMeals(startDate?)` : repas d'une semaine
- `fetchUpcomingMeals(limit?)` : prochains repas
- `swapMeal(mealId, newRecipeId)` : échange un repas
- `changeMonth(direction)` : navigation calendrier
- `selectDate(date)` : sélection d'un jour

### Intégration API (composables)

- [ ] **Composable `useMealPlan`** (`composables/useMealPlan.ts`)

Wrapper autour du store avec :
- Accès aux données du store
- Méthodes pour actions fréquentes (swap, refresh, etc.)
- Gestion d'erreurs centralisée
- Exemple :
```typescript
export const useMealPlan = () => {
  const store = useMealPlanStore()
  
  const loadDailyView = async (date?: Date) => {
    await store.fetchDailyMeals(date)
  }
  
  const loadWeeklyView = async (startDate?: Date) => {
    await Promise.all([
      store.fetchWeeklyMeals(startDate),
      store.fetchUpcomingMeals(5)
    ])
  }
  
  return { store, loadDailyView, loadWeeklyView }
}
```

- [ ] **Composable `useApi`** (si non existant)
  - Configuration axios/fetch avec baseURL
  - Gestion tokens Sanctum
  - Intercepteurs pour erreurs

### Gestion des images

- [ ] **Placeholder pour images manquantes**
  - Créer une image par défaut dans `public/images/meal-placeholder.png`
  - Directive ou composant `ImageWithFallback.vue`

---

## Tests

### Backend : Feature tests API

- [ ] **`MealPlanControllerTest.php`**
  - Test `GET /api/meal-plans/current` : retourne le plan actif
  - Test `GET /api/meal-plans/current/daily` : repas d'aujourd'hui
  - Test `GET /api/meal-plans/current/weekly` : repas de la semaine
  - Test `GET /api/meal-plans/current/upcoming` : prochains repas
  - Test cas sans plan actif (404 ou réponse vide)

- [ ] **`MealControllerTest.php`**
  - Test `PUT /api/meals/{id}/swap` : swap réussi
  - Test swap avec recette incompatible (allergies/préférences)
  - Test swap avec meal_id inexistant (404)
  - Test swap par utilisateur non propriétaire (403)

### Frontend : Unit tests composants

- [ ] **`MealCard.test.ts`**
  - Affichage correct des infos du repas
  - Émission événements @swap et @cook

- [ ] **`DailyDigest.test.ts`**
  - Affichage date formatée
  - Affichage 3 repas
  - Message si aucun repas

- [ ] **`WeeklyMealTable.test.ts`**
  - Affichage 7 jours
  - Affichage noms de plats
  - Émission événement @meal-click

- [ ] **Store `mealPlan.test.ts`**
  - Actions : fetchDailyMeals, fetchWeeklyMeals, swapMeal
  - Getters : formattedDailyDate, formattedWeekRange

---

## Ordre d'exécution

### Phase 1 : Backend - Base de données et API
1. Migrations (meal_plans, meals)
2. Models (MealPlan, Meal) + Relations
3. MealPlanService (logique métier)
4. Controllers (MealPlanController, MealController)
5. Validation (SwapMealRequest)
6. Seeders (données de test)
7. Tests API (Feature tests)

### Phase 2 : Frontend - Design System et composants de base
1. Design System (BaseCard, BaseButton, AppHeader)
2. Composable useApi
3. Store Pinia (mealPlan)
4. Composable useMealPlan

### Phase 3 : Frontend - Composants Home
1. MealCard
2. DailyDigest (mobile)
3. WeeklyCalendar, WeeklyMealTable, QuickActions, UpcomingMeals (desktop)

### Phase 4 : Intégration et Responsive
1. Page index.vue (assemblage mobile + desktop)
2. Gestion responsive (breakpoints)
3. Tests composants
4. Placeholder images

### Phase 5 : Tests et optimisation
1. Tests unitaires frontend
2. Tests d'intégration
3. Optimisation performance (lazy loading images, debounce)
4. Vérification accessibilité

---

## Points d'attention

### Critères d'acceptation à valider

**Mobile (Daily Digest) :**
- [ ] Affichage de la date du jour en orange
- [ ] 3 cartes de repas (Breakfast, Lunch, Dinner) avec image, nom, 2 boutons
- [ ] Boutons "Swap Meal" et "Cook Now" fonctionnels
- [ ] Défilement vertical fluide
- [ ] Gestion image manquante (placeholder)
- [ ] Message si aucun repas planifié

**Desktop (Weekly Meal Plan) :**
- [ ] Calendrier mensuel avec jour actuel en évidence (orange)
- [ ] Navigation entre mois fonctionnelle
- [ ] Tableau hebdomadaire avec 7 jours × 3 repas
- [ ] Section "Quick Actions" avec 3 boutons
- [ ] Section "Upcoming Meals" avec 5 prochains repas
- [ ] Responsive design (adaptation tablette/desktop)
- [ ] Noms de plats cliquables (orange)

**Commun :**
- [ ] Chargement rapide des données
- [ ] Gestion erreurs API (affichage message utilisateur)
- [ ] Accessibilité : alt text images, labels boutons, navigation clavier
- [ ] Performance : images optimisées, pas de saccades

### Dépendances avec autres issues

- **Issue #2 (Settings)** : Les préférences utilisateur doivent être consultées lors du swap de repas pour filtrer les recettes compatibles
- **Issue #3 (Recipe Discovery)** : Les recettes affichées dans les repas proviennent de la base recipes
- **Issue #4 (Shopping List)** : Le bouton "Generate Grocery List" dépend de l'implémentation de la génération de liste de courses

### Cas particuliers à gérer

1. **Aucun plan de repas actif**
   - Afficher un message d'incitation : "Create your first meal plan to get started"
   - Bouton CTA : "Create Meal Plan"

2. **Repas non planifié pour un moment de la journée**
   - Afficher une carte vide avec message : "No meal planned" + bouton "Add Meal"

3. **Noms de plats très longs**
   - Troncature à 50 caractères avec ellipsis (...)
   - Tooltip au survol pour voir le nom complet (desktop)

4. **Image de plat manquante**
   - Afficher placeholder avec icône de plat générique

5. **Erreur réseau/API**
   - Toast notification avec message d'erreur
   - Bouton "Retry" pour recharger les données

6. **Swap de repas - recette incompatible**
   - Validation backend : rejette si allergène présent
   - Message frontend : "This recipe is not compatible with your dietary preferences"

7. **Navigation calendrier**
   - Désactiver flèche gauche si mois passé (optionnel)
   - Charger données semaine lors changement de mois

8. **Performance - nombreuses images**
   - Lazy loading pour images (v-lazy ou Intersection Observer)
   - Compression images côté backend
   - Format WebP avec fallback

---

## Estimation de complexité

**Complexité globale : Moyen à Complexe**

- **Backend** : Moyen
  - Modèles et relations simples
  - API REST classique
  - Service métier pour logique de swap (validation préférences/allergies)

- **Frontend** : Complexe
  - Nombreux composants à créer
  - Deux layouts différents (mobile/desktop)
  - Gestion state complexe (calendrier, semaine, jour)
  - Responsive design sur plusieurs breakpoints
  - Interactions multiples (swap, cook, navigation calendrier)

- **Tests** : Moyen
  - Feature tests API standards
  - Tests composants avec mocks store et API

**Durée estimée** : 3-5 jours pour un développeur fullstack expérimenté

