# Project Brief - MealPlanner/MealPrep

## Vue d'ensemble du projet

**MealPlanner** (aussi appelé **MealPrep**) est une application de planification de repas qui aide les utilisateurs à organiser leurs menus hebdomadaires, découvrir des recettes adaptées à leurs préférences alimentaires, et générer automatiquement des listes de courses.

## Objectif métier

Simplifier la planification des repas quotidiens et hebdomadaires en offrant :
- Une vision claire et organisée des repas à venir
- Une personnalisation selon les préférences alimentaires et contraintes (allergies, régimes)
- Une génération automatique de listes de courses basée sur les repas planifiés
- Une découverte facilitée de nouvelles recettes correspondant aux besoins de l'utilisateur

## Valeur ajoutée

- **Gain de temps** : Planification centralisée et liste de courses automatique
- **Personnalisation** : Adaptation aux régimes alimentaires et allergies
- **Flexibilité** : Possibilité d'échanger facilement les repas et d'ajuster le plan
- **Organisation** : Vue claire des repas de la semaine et synchronisation entre planning et courses

## Personas cibles

- **Familles actives** : Besoin d'organiser les repas de la semaine pour gagner du temps
- **Personnes avec contraintes alimentaires** : Régimes spécifiques (végétarien, vegan, keto, sans gluten, etc.)
- **Individus soucieux de leur santé** : Souhaitant planifier des repas équilibrés
- **Personnes allergiques** : Nécessitant d'exclure certains ingrédients

## Fonctionnalités principales

### 1. Page d'accueil (Home)

#### Version Mobile - Daily Digest
- **Affichage journalier** : Vue des repas du jour (Breakfast, Lunch, Dinner)
- **Actions rapides** :
  - **Swap Meal** : Échanger un repas contre une autre suggestion
  - **Cook Now** : Accéder à la recette détaillée ou lancer un minuteur
- **Visualisation** : Cartes avec image du plat et nom du repas

#### Version Desktop - This Week's Meal Plan
- **Calendrier mensuel interactif** : Navigation entre les mois, sélection de jour
- **Tableau hebdomadaire** : Affichage des repas (Breakfast, Lunch, Dinner) pour chaque jour de la semaine
- **Quick Actions** :
  - Add Recipe : Ajouter une recette au plan
  - Create Meal Plan : Créer ou modifier le plan de la semaine
  - Generate Grocery List : Générer la liste de courses automatiquement
- **Upcoming Meals** : Prévisualisation des prochains repas avec images

### 2. Settings (Paramètres du plan de repas)

#### Fonctionnalités communes (Mobile & Desktop)
- **Préférences alimentaires** : Sélection d'un régime (Omnivore, Vegetarian, Vegan, Pescatarian, Keto, Paleo, Low Carb, Mediterranean)
- **Allergies** : Sélection multiple via menu déroulant (gluten, noix, produits laitiers, etc.)
- **Nombre de repas par jour** : Choix entre 2, 3 ou 4 repas

#### Fonctionnalités supplémentaires (Desktop)
- **Exclure des ingrédients** : Liste d'ingrédients à éviter
- **Durée du plan** : Sélection de la période (1 semaine, 2 semaines, etc.)
- **Autres préférences** :
  - Inclure les restes dans le plan
  - Générer automatiquement une liste de courses

### 3. Shopping List (Liste de courses)

- **Organisation par catégories** : Produce, Dairy, Meat, Pantry
- **Fonctionnalités de gestion** :
  - Cocher/décocher les articles achetés (case orange quand cochée)
  - Ajouter manuellement des articles (nom + quantité + catégorie)
  - Imprimer la liste (PDF ou impression directe)
- **Synchronisation automatique** : Mise à jour basée sur les recettes sélectionnées dans le plan de repas
- **Affichage** : Quantité et nom de chaque article (ex. "1 head Lettuce", "2 lbs Apples")

### 4. Recipe Discovery (Découverte de recettes)

- **Barre de recherche** : Recherche par mots-clés en temps réel
- **Filtres personnalisables** : 
  - Par type de régime : Quick & Easy, Vegetarian, Gluten-Free, Low Carb, Family-Friendly, Desserts
  - Par moment de la journée : Breakfast, Lunch, Dinner
  - Sélection multiple possible
- **Affichage des résultats** : Liste de cartes avec :
  - Image de la recette
  - Titre
  - Description courte
- **Interaction** : Clic sur une carte pour accéder aux détails complets de la recette

