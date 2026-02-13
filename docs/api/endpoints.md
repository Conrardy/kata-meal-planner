# API Endpoints

Reference complète de tous les endpoints de l'API MealPlanner.

**Base URL** : `http://localhost:5000` (développement) | Via nginx proxy en production

**Version** : `/api/v1/`

---

## Vue d'ensemble

| Catégorie | Endpoints | Authentification |
|-----------|-----------|-----------------|
| [Health & Monitoring](#health-monitoring) | 2 | Aucune |
| [Authentification](#authentification) | 5 | Publique |
| [Administration](#administration) | 2 | Admin requis |
| [Daily Digest](#daily-digest) | 1 | Bearer JWT |
| [Meals](#meals) | 3 | Bearer JWT |
| [Weekly Plan](#weekly-plan) | 1 | Bearer JWT |
| [Recipes](#recipes) | 3 | Bearer JWT |
| [Shopping List](#shopping-list) | 4 | Bearer JWT |
| [Preferences](#preferences) | 2 | Bearer JWT |
| [Stock](#stock) | 5 | Bearer JWT |

**Total : 28 endpoints**

---

## Health & Monitoring

### GET `/health/live` - Liveness Check

Vérifie que l'application est en cours d'exécution.

**Authentification** : Aucune

**Réponse** `200 OK` :

```json
{
  "status": "Healthy",
  "totalDuration": 0.5,
  "checks": []
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Application saine |
| `503` | Application non disponible |

```bash
curl http://localhost:5000/health/live
```

---

### GET `/health/ready` - Readiness Check

Vérifie que l'application peut traiter des requêtes (PostgreSQL).

**Authentification** : Aucune

**Réponse** `200 OK` :

```json
{
  "status": "Healthy",
  "totalDuration": 45.2,
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "duration": 23.1
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Toutes les dépendances sont prêtes |
| `503` | Une ou plusieurs dépendances indisponibles |

```bash
curl http://localhost:5000/health/ready
```

---

## Authentification

!!! note "Rate Limiting"
    Les endpoints d'authentification sont limités à **10 requêtes par minute** par IP.

### POST `/api/v1/auth/register` - Inscription

Crée un nouveau compte utilisateur avec email et mot de passe.

**Authentification** : Bearer JWT requis

**Corps de la requête** :

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Réponse** `201 Created` :

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com"
}
```

**Header** : `Location: /api/v1/users/{userId}`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `201` | Utilisateur créé |
| `400` | Erreur de validation (mot de passe faible, email invalide) |
| `401` | Token manquant ou invalide |
| `409` | Email déjà utilisé |

```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"email": "user@example.com", "password": "SecurePassword123!"}'
```

---

### POST `/api/v1/auth/login` - Connexion (username)

Authentifie un utilisateur avec son nom d'utilisateur et mot de passe.

**Authentification** : Aucune

**Corps de la requête** :

```json
{
  "username": "Emmanuel",
  "password": "MealPlanner123!"
}
```

**Réponse** `200 OK` :

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "accessTokenExpiresAt": "2026-02-05T15:30:00Z",
  "refreshTokenExpiresAt": "2026-02-12T14:00:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "Emmanuel"
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Authentification réussie |
| `400` | Erreur de validation |
| `401` | Identifiants invalides |

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "Emmanuel", "password": "MealPlanner123!"}'
```

---

### POST `/api/v1/auth/login/email` - Connexion (email)

Authentifie un utilisateur avec son email et mot de passe.

**Authentification** : Aucune

**Corps de la requête** :

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Réponse** `200 OK` :

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "accessTokenExpiresAt": "2026-02-05T15:30:00Z",
  "refreshTokenExpiresAt": "2026-02-12T14:00:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com"
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Authentification réussie |
| `400` | Erreur de validation |
| `401` | Identifiants invalides |

```bash
curl -X POST http://localhost:5000/api/v1/auth/login/email \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "SecurePassword123!"}'
```

---

### POST `/api/v1/auth/refresh` - Renouveler le token (username)

Génère un nouveau access token à partir d'un refresh token.

**Authentification** : Aucune

**Corps de la requête** :

```json
{
  "refreshToken": "eyJhbGc..."
}
```

**Réponse** `200 OK` :

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "accessTokenExpiresAt": "2026-02-05T15:30:00Z",
  "refreshTokenExpiresAt": "2026-02-12T14:00:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "Emmanuel"
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Token renouvelé |
| `400` | Erreur de validation |
| `401` | Refresh token invalide ou expiré |

```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "eyJhbGc..."}'
```

---

### POST `/api/v1/auth/refresh/email` - Renouveler le token (email)

Génère un nouveau access token à partir d'un refresh token (flux email).

**Authentification** : Aucune

**Corps de la requête** :

```json
{
  "refreshToken": "eyJhbGc..."
}
```

**Réponse** `200 OK` :

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "accessTokenExpiresAt": "2026-02-05T15:30:00Z",
  "refreshTokenExpiresAt": "2026-02-12T14:00:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com"
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Token renouvelé |
| `400` | Erreur de validation |
| `401` | Refresh token invalide ou expiré |

```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh/email \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "eyJhbGc..."}'
```

---

## Administration

!!! warning "Accès restreint"
    Ces endpoints nécessitent le claim `IsAdmin=true`. Seuls les utilisateurs seeded (Emmanuel, Gabrielle) sont admins.

### GET `/api/v1/admin/users` - Liste des utilisateurs

Récupère la liste de tous les utilisateurs.

**Authentification** : Bearer JWT + Policy `RequireAdmin`

**Réponse** `200 OK` :

```json
{
  "users": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "Emmanuel",
      "isAdmin": true
    },
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "username": "Gabrielle",
      "isAdmin": true
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Liste récupérée |
| `401` | Token manquant ou invalide |
| `403` | Droits admin insuffisants |

```bash
curl http://localhost:5000/api/v1/admin/users \
  -H "Authorization: Bearer <admin_token>"
```

---

### POST `/api/v1/admin/users` - Créer un utilisateur

Crée un nouveau compte utilisateur (admin uniquement).

**Authentification** : Bearer JWT + Policy `RequireAdmin`

**Corps de la requête** :

```json
{
  "username": "newuser",
  "password": "SecurePassword123!"
}
```

**Réponse** `201 Created` :

```json
{
  "userId": "770e8400-e29b-41d4-a716-446655440002",
  "username": "newuser"
}
```

**Header** : `Location: /api/v1/admin/users/{userId}`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `201` | Utilisateur créé |
| `400` | Erreur de validation (mot de passe faible) |
| `401` | Token manquant ou invalide |
| `403` | Droits admin insuffisants |
| `409` | Nom d'utilisateur déjà pris |

```bash
curl -X POST http://localhost:5000/api/v1/admin/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <admin_token>" \
  -d '{"username": "newuser", "password": "SecurePassword123!"}'
```

---

## Daily Digest

### GET `/api/v1/daily-digest/{date}` - Résumé du jour

Récupère les repas planifiés pour une date donnée.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Format | Description |
|-----------|------|--------|-------------|
| `date` | `DateOnly` | `YYYY-MM-DD` | Date du digest |

**Réponse** `200 OK` :

```json
{
  "date": "2026-02-05",
  "meals": [
    {
      "id": "880e8400-e29b-41d4-a716-446655440003",
      "mealType": "Breakfast",
      "recipeId": "990e8400-e29b-41d4-a716-446655440004",
      "recipeName": "Pancakes",
      "imageUrl": "https://example.com/pancakes.jpg"
    },
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440005",
      "mealType": "Lunch",
      "recipeId": "bb0e8400-e29b-41d4-a716-446655440006",
      "recipeName": "Caesar Salad",
      "imageUrl": null
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Digest récupéré |
| `401` | Token manquant ou invalide |

```bash
curl http://localhost:5000/api/v1/daily-digest/2026-02-05 \
  -H "Authorization: Bearer <token>"
```

---

## Meals

### GET `/api/v1/meals/{mealId}/suggestions` - Suggestions de remplacement

Récupère des recettes alternatives pour un repas planifié.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `mealId` | `Guid` | Identifiant du repas planifié |

**Réponse** `200 OK` :

```json
{
  "mealId": "880e8400-e29b-41d4-a716-446655440003",
  "mealType": "Breakfast",
  "suggestions": [
    {
      "id": "cc0e8400-e29b-41d4-a716-446655440007",
      "name": "Oatmeal",
      "imageUrl": "https://example.com/oatmeal.jpg",
      "description": "Healthy oatmeal with berries"
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Suggestions récupérées |
| `401` | Token manquant ou invalide |
| `404` | Repas non trouvé |

```bash
curl http://localhost:5000/api/v1/meals/880e8400-e29b-41d4-a716-446655440003/suggestions \
  -H "Authorization: Bearer <token>"
```

---

### POST `/api/v1/meals/{mealId}/swap` - Échanger un repas

Remplace un repas planifié par une nouvelle recette.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `mealId` | `Guid` | Identifiant du repas à remplacer |

**Corps de la requête** :

```json
{
  "newRecipeId": "cc0e8400-e29b-41d4-a716-446655440007"
}
```

**Réponse** `200 OK` :

```json
{
  "mealId": "880e8400-e29b-41d4-a716-446655440003",
  "mealType": "Breakfast",
  "recipeName": "Oatmeal",
  "imageUrl": "https://example.com/oatmeal.jpg",
  "shoppingListUpdated": true
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Repas échangé |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |
| `404` | Repas ou recette non trouvé |

```bash
curl -X POST http://localhost:5000/api/v1/meals/880e8400-e29b-41d4-a716-446655440003/swap \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"newRecipeId": "cc0e8400-e29b-41d4-a716-446655440007"}'
```

---

### POST `/api/v1/meal-plan` - Planifier un repas

Ajoute une recette au plan de repas pour une date et un type de repas.

**Authentification** : Bearer JWT

**Corps de la requête** :

```json
{
  "recipeId": "cc0e8400-e29b-41d4-a716-446655440007",
  "date": "2026-02-05",
  "mealType": "Lunch"
}
```

**Réponse** `201 Created` :

```json
{
  "mealId": "dd0e8400-e29b-41d4-a716-446655440008",
  "recipeName": "Caesar Salad",
  "date": "2026-02-05",
  "mealType": "Lunch",
  "shoppingListUpdated": true
}
```

**Header** : `Location: /api/v1/meals/{mealId}`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `201` | Repas planifié |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |
| `409` | Créneau déjà occupé |

```bash
curl -X POST http://localhost:5000/api/v1/meal-plan \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"recipeId": "cc0e8400-...", "date": "2026-02-05", "mealType": "Lunch"}'
```

---

## Weekly Plan

### GET `/api/v1/weekly-plan/{startDate}` - Plan hebdomadaire

Récupère le plan de repas pour une période de 7 jours.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Format | Description |
|-----------|------|--------|-------------|
| `startDate` | `DateOnly` | `YYYY-MM-DD` | Date de début de la semaine |

**Réponse** `200 OK` :

```json
{
  "startDate": "2026-02-02",
  "endDate": "2026-02-08",
  "days": [
    {
      "date": "2026-02-02",
      "dayName": "Monday",
      "breakfast": {
        "id": "880e8400-e29b-41d4-a716-446655440003",
        "recipeId": "990e8400-e29b-41d4-a716-446655440004",
        "recipeName": "Pancakes",
        "imageUrl": "https://example.com/pancakes.jpg"
      },
      "lunch": null,
      "dinner": null
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Plan récupéré |
| `401` | Token manquant ou invalide |

```bash
curl http://localhost:5000/api/v1/weekly-plan/2026-02-02 \
  -H "Authorization: Bearer <token>"
```

---

## Recipes

### GET `/api/v1/recipes` - Rechercher des recettes

Recherche des recettes par nom et/ou filtres de tags.

**Authentification** : Bearer JWT

**Paramètres de requête** :

| Paramètre | Type | Requis | Description |
|-----------|------|--------|-------------|
| `search` | `string` | Non | Texte de recherche dans les noms |
| `tags` | `string` | Non | Tags séparés par des virgules |

**Réponse** `200 OK` :

```json
{
  "recipes": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440004",
      "name": "Pancakes",
      "imageUrl": "https://example.com/pancakes.jpg",
      "description": "Fluffy pancakes with maple syrup",
      "tags": ["Breakfast", "Quick", "Family-Friendly"]
    }
  ],
  "availableTags": ["Breakfast", "Lunch", "Dinner", "Vegetarian", "Quick"]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Recherche effectuée |
| `401` | Token manquant ou invalide |

```bash
# Recherche par texte
curl "http://localhost:5000/api/v1/recipes?search=pancake" \
  -H "Authorization: Bearer <token>"

# Filtre par tags
curl "http://localhost:5000/api/v1/recipes?tags=Vegetarian,Quick" \
  -H "Authorization: Bearer <token>"

# Combiné
curl "http://localhost:5000/api/v1/recipes?search=salad&tags=Lunch" \
  -H "Authorization: Bearer <token>"
```

---

### GET `/api/v1/recipes/{recipeId}` - Détails d'une recette

Récupère les détails complets d'une recette (ingrédients, étapes).

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `recipeId` | `Guid` | Identifiant de la recette |

**Réponse** `200 OK` :

```json
{
  "id": "990e8400-e29b-41d4-a716-446655440004",
  "name": "Pancakes",
  "imageUrl": "https://example.com/pancakes.jpg",
  "description": "Fluffy pancakes with maple syrup",
  "tags": ["Breakfast", "Quick"],
  "mealType": "Breakfast",
  "ingredients": [
    {
      "name": "Flour",
      "quantity": "2",
      "unit": "cups"
    },
    {
      "name": "Eggs",
      "quantity": "2",
      "unit": null
    }
  ],
  "steps": [
    {
      "stepNumber": 1,
      "instruction": "Mix flour and eggs in a bowl"
    },
    {
      "stepNumber": 2,
      "instruction": "Cook on griddle until golden brown"
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Recette récupérée |
| `401` | Token manquant ou invalide |
| `404` | Recette non trouvée |

```bash
curl http://localhost:5000/api/v1/recipes/990e8400-e29b-41d4-a716-446655440004 \
  -H "Authorization: Bearer <token>"
```

---

### POST `/api/v1/recipes` - Créer une recette

Crée une nouvelle recette avec ingrédients et étapes de préparation.

**Authentification** : Bearer JWT

**Corps de la requête** :

```json
{
  "name": "Grilled Chicken",
  "imageUrl": "https://example.com/chicken.jpg",
  "description": "Herb-marinated grilled chicken breast",
  "ingredients": [
    {
      "name": "Chicken Breast",
      "quantity": "2",
      "unit": "lbs"
    }
  ],
  "steps": [
    {
      "stepNumber": 1,
      "instruction": "Marinate chicken with herbs"
    }
  ],
  "tags": ["Dinner", "Healthy"],
  "mealType": "Dinner"
}
```

**Réponse** `201 Created` :

```json
{
  "id": "ee0e8400-e29b-41d4-a716-446655440009"
}
```

**Header** : `Location: /api/v1/recipes/{id}`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `201` | Recette créée |
| `400` | Erreur de validation (nom manquant, ingrédients vides) |
| `401` | Token manquant ou invalide |

```bash
curl -X POST http://localhost:5000/api/v1/recipes \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "name": "Grilled Chicken",
    "description": "Herb-marinated grilled chicken",
    "ingredients": [{"name": "Chicken Breast", "quantity": "2", "unit": "lbs"}],
    "steps": [{"stepNumber": 1, "instruction": "Marinate chicken"}],
    "tags": ["Dinner"],
    "mealType": "Dinner"
  }'
```

---

## Shopping List

### GET `/api/v1/shopping-list/{startDate}` - Générer la liste de courses

Génère la liste de courses agrégée depuis le plan de repas (7 jours).

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Format | Description |
|-----------|------|--------|-------------|
| `startDate` | `DateOnly` | `YYYY-MM-DD` | Date de début de la période |

**Réponse** `200 OK` :

```json
{
  "startDate": "2026-02-02",
  "endDate": "2026-02-08",
  "categories": [
    {
      "category": "Produce",
      "items": [
        {
          "id": "item-flour",
          "name": "Flour",
          "quantity": "2",
          "unit": "cups",
          "isChecked": false,
          "isCustom": false
        }
      ]
    },
    {
      "category": "Dairy",
      "items": [
        {
          "id": "item-eggs",
          "name": "Eggs",
          "quantity": "4",
          "unit": null,
          "isChecked": true,
          "isCustom": false
        }
      ]
    }
  ],
  "wasUpdated": false,
  "updateNotice": null
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Liste générée |
| `401` | Token manquant ou invalide |

```bash
curl http://localhost:5000/api/v1/shopping-list/2026-02-02 \
  -H "Authorization: Bearer <token>"
```

---

### PATCH `/api/v1/shopping-list/{startDate}/items/{itemId}` - Cocher/Décocher un article

Marque un article de la liste comme acheté ou non.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `startDate` | `DateOnly` | Date de début de la période |
| `itemId` | `string` | Identifiant de l'article |

**Corps de la requête** :

```json
{
  "isChecked": true
}
```

**Réponse** : `204 No Content`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `204` | Article mis à jour |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |

```bash
curl -X PATCH http://localhost:5000/api/v1/shopping-list/2026-02-02/items/item-flour \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"isChecked": true}'
```

---

### POST `/api/v1/shopping-list/{startDate}/items` - Ajouter un article personnalisé

Ajoute un article personnalisé à la liste de courses.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `startDate` | `DateOnly` | Date de début de la période |

**Corps de la requête** :

```json
{
  "name": "Milk",
  "quantity": "1",
  "unit": "gallon",
  "category": "Dairy"
}
```

**Réponse** `201 Created` :

```json
{
  "id": "custom-milk-001",
  "name": "Milk",
  "quantity": "1",
  "unit": "gallon",
  "isChecked": false,
  "isCustom": true
}
```

**Header** : `Location: /api/v1/shopping-list/{startDate}/items/{itemId}`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `201` | Article ajouté |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |

```bash
curl -X POST http://localhost:5000/api/v1/shopping-list/2026-02-02/items \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"name": "Milk", "quantity": "1", "unit": "gallon", "category": "Dairy"}'
```

---

### DELETE `/api/v1/shopping-list/{startDate}/items/{itemId}` - Supprimer un article

Supprime un article de la liste de courses.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `startDate` | `DateOnly` | Date de début de la période |
| `itemId` | `string` | Identifiant de l'article |

**Réponse** : `204 No Content`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `204` | Article supprimé |
| `401` | Token manquant ou invalide |
| `404` | Article non trouvé |

```bash
curl -X DELETE http://localhost:5000/api/v1/shopping-list/2026-02-02/items/custom-milk-001 \
  -H "Authorization: Bearer <token>"
```

---

## Preferences

### GET `/api/v1/preferences` - Récupérer les préférences

Récupère les préférences alimentaires de l'utilisateur.

**Authentification** : Bearer JWT

**Réponse** `200 OK` :

```json
{
  "dietaryPreference": "Vegetarian",
  "allergies": ["Peanuts", "Dairy"],
  "availableDietaryPreferences": ["Omnivore", "Vegetarian", "Vegan", "Pescatarian", "Keto", "Paleo"],
  "availableAllergies": ["Peanuts", "Dairy", "Gluten", "Shellfish"],
  "mealsPerDay": 3,
  "planLength": 7,
  "includeLeftovers": true,
  "autoGenerateShoppingList": true,
  "excludedIngredients": ["Cilantro"],
  "availableMealsPerDay": [2, 3, 4],
  "availablePlanLengths": [7, 14]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Préférences récupérées |
| `401` | Token manquant ou invalide |

```bash
curl http://localhost:5000/api/v1/preferences \
  -H "Authorization: Bearer <token>"
```

---

### PUT `/api/v1/preferences` - Mettre à jour les préférences

Met à jour les préférences alimentaires de l'utilisateur.

**Authentification** : Bearer JWT

**Corps de la requête** :

```json
{
  "dietaryPreference": "Vegan",
  "allergies": ["Peanuts"],
  "mealsPerDay": 3,
  "planLength": 14,
  "includeLeftovers": false,
  "autoGenerateShoppingList": true,
  "excludedIngredients": ["Cilantro", "Onions"]
}
```

| Champ | Type | Requis | Description |
|-------|------|--------|-------------|
| `dietaryPreference` | `string` | Oui | Régime alimentaire |
| `allergies` | `string[]` | Oui | Liste des allergies |
| `mealsPerDay` | `int` | Non | Nombre de repas par jour (2, 3, 4) |
| `planLength` | `int` | Non | Durée du plan en jours (7, 14) |
| `includeLeftovers` | `bool` | Non | Inclure les restes |
| `autoGenerateShoppingList` | `bool` | Non | Générer la liste automatiquement |
| `excludedIngredients` | `string[]` | Non | Ingrédients à exclure |

**Réponse** `200 OK` : Retourne les préférences mises à jour (même format que GET).

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Préférences mises à jour |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |

```bash
curl -X PUT http://localhost:5000/api/v1/preferences \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "dietaryPreference": "Vegan",
    "allergies": ["Peanuts"],
    "mealsPerDay": 3,
    "planLength": 14
  }'
```

---

## Stock

### GET `/api/v1/stock` - Liste du stock

Récupère tous les articles du stock de l'utilisateur connecté.

**Authentification** : Bearer JWT

**Réponse** `200 OK` :

```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "ingredientName": "Flour",
      "quantity": 2.5,
      "unit": "kg",
      "category": "Pantry",
      "expirationDate": "2026-06-15",
      "lowStockThreshold": 1.0
    }
  ]
}
```

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Stock récupéré |
| `401` | Token manquant ou invalide |

```bash
curl http://localhost:5000/api/v1/stock \
  -H "Authorization: Bearer <token>"
```

---

### POST `/api/v1/stock` - Ajouter un article au stock

Ajoute un nouvel article au stock de l'utilisateur.

**Authentification** : Bearer JWT

**Corps de la requête** :

```json
{
  "ingredientName": "Flour",
  "quantity": 2.5,
  "unit": "kg",
  "category": "Pantry",
  "expirationDate": "2026-06-15",
  "lowStockThreshold": 1.0
}
```

| Champ | Type | Requis | Description |
|-------|------|--------|-------------|
| `ingredientName` | `string` | Oui | Nom de l'ingrédient (max 200 car.) |
| `quantity` | `decimal` | Oui | Quantité (>= 0) |
| `unit` | `string` | Oui | Unité de mesure (max 50 car.) |
| `category` | `string` | Oui | Catégorie : Produce, Dairy, Meat, Pantry |
| `expirationDate` | `DateOnly` | Non | Date de péremption |
| `lowStockThreshold` | `decimal` | Non | Seuil d'alerte de stock bas (>= 0) |

**Réponse** `201 Created` :

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "ingredientName": "Flour",
  "quantity": 2.5,
  "unit": "kg",
  "category": "Pantry",
  "expirationDate": "2026-06-15",
  "lowStockThreshold": 1.0
}
```

**Header** : `Location: /api/v1/stock/{id}`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `201` | Article créé |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |

```bash
curl -X POST http://localhost:5000/api/v1/stock \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"ingredientName": "Flour", "quantity": 2.5, "unit": "kg", "category": "Pantry"}'
```

---

### PUT `/api/v1/stock/{id}` - Modifier un article du stock

Met à jour un article existant dans le stock.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` | Identifiant de l'article |

**Corps de la requête** :

```json
{
  "ingredientName": "Flour",
  "quantity": 3.0,
  "unit": "kg",
  "category": "Pantry",
  "expirationDate": "2026-06-15",
  "lowStockThreshold": 1.0
}
```

**Réponse** `200 OK` : Retourne l'article mis à jour (même format que POST).

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Article mis à jour |
| `400` | Erreur de validation |
| `401` | Token manquant ou invalide |
| `404` | Article non trouvé |

```bash
curl -X PUT http://localhost:5000/api/v1/stock/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"ingredientName": "Flour", "quantity": 3.0, "unit": "kg", "category": "Pantry"}'
```

---

### DELETE `/api/v1/stock/{id}` - Supprimer un article du stock

Supprime un article du stock.

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` | Identifiant de l'article |

**Réponse** : `204 No Content`

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `204` | Article supprimé |
| `401` | Token manquant ou invalide |
| `404` | Article non trouvé |

```bash
curl -X DELETE http://localhost:5000/api/v1/stock/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer <token>"
```

---

### PATCH `/api/v1/stock/{id}/quantity` - Ajuster la quantité

Ajuste rapidement la quantité d'un article (+/-).

**Authentification** : Bearer JWT

**Paramètres de chemin** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `id` | `Guid` | Identifiant de l'article |

**Corps de la requête** :

```json
{
  "adjustment": -1.0
}
```

| Champ | Type | Requis | Description |
|-------|------|--------|-------------|
| `adjustment` | `decimal` | Oui | Valeur d'ajustement (positif pour ajouter, négatif pour retirer) |

**Réponse** `200 OK` : Retourne l'article mis à jour.

**Codes d'erreur** :

| Code | Description |
|------|-------------|
| `200` | Quantité ajustée |
| `400` | Erreur de validation (résultat négatif, ajustement nul) |
| `401` | Token manquant ou invalide |
| `404` | Article non trouvé |

```bash
curl -X PATCH http://localhost:5000/api/v1/stock/550e8400-e29b-41d4-a716-446655440000/quantity \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"adjustment": -1.0}'
```

---

## Informations transversales

### Authentification JWT

Tous les endpoints protégés nécessitent un header `Authorization: Bearer <token>`.

| Paramètre | Valeur |
|-----------|--------|
| Access Token TTL | 15 minutes |
| Refresh Token TTL | 7 jours |
| Algorithm | HMAC SHA-256 |
| Issuer | `MealPlanner` |
| Audience | `MealPlannerApp` |

### Gestion des erreurs

Les erreurs suivent le format [Problem Details (RFC 9457)](https://www.rfc-editor.org/rfc/rfc9457) :

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "errors": {
    "Password": ["Password must be at least 8 characters"]
  }
}
```

### Localisation des erreurs

Les messages d'erreur sont localisés selon le header `Accept-Language`.

| Langue | Code |
|--------|------|
| Anglais (défaut) | `en` |
| Français | `fr` |

```bash
curl http://localhost:5000/api/v1/preferences \
  -H "Authorization: Bearer <token>" \
  -H "Accept-Language: fr"
```

### Rate Limiting

| Politique | Limite | Fenêtre |
|-----------|--------|---------|
| Default | 100 requêtes | 60 secondes |
| Auth (`/api/v1/auth/*`) | 10 requêtes | 60 secondes |

En cas de dépassement, l'API retourne `429 Too Many Requests`.

### Correlation ID

Toutes les requêtes sont tracées via un header `X-Correlation-ID`. Si absent, l'API en génère un automatiquement.

```bash
curl http://localhost:5000/api/v1/preferences \
  -H "Authorization: Bearer <token>" \
  -H "X-Correlation-ID: my-trace-id"
```
