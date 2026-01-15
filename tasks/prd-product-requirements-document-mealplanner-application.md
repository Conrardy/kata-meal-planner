# Product Requirements Document: MealPlanner Application

## 1. Executive Summary

**Product Name:** MealPlanner (MealPrep)  
**Version:** 1.0 MVP  
**Date:** 2026-01-15  
**Tech Stack:** Angular (Frontend) + .NET (Backend)

### Vision
Simplify daily and weekly meal planning with an intuitive, responsive web application that helps users organize their meals, discover recipes, and manage grocery shopping.

### Mission
Deliver a full-stack application enabling users to:
- View daily meal digests with quick actions (swap/cook)
- Plan meals on a weekly calendar
- Discover and filter recipes
- Auto-generate shopping lists synchronized with meal plans
- Customize dietary preferences and restrictions

---

## 2. Scope & Features

### 2.1 Issue #1: Home Page
**Priority:** High

#### Mobile View - Daily Digest
- Header with "MealPrep" logo, notification icon, hamburger menu
- "Daily Digest" section showing current date (orange #FF8C00)
- Meal cards for Breakfast, Lunch, Dinner containing:
  - Meal image (full-width, rounded corners)
  - Meal type label (e.g., "Breakfast")
  - Meal name
  - "Swap Meal" button (outlined, orange border)
  - "Cook Now" button (filled, orange #FF8C00)
- Vertical scrolling for overflow

#### Desktop View - Weekly Plan
- Top navigation: Home, Recipes, Grocery List, Settings
- Left sidebar:
  - Monthly calendar with day selection (current day highlighted orange)
  - Quick Actions: Add Recipe, Create Meal Plan, Generate Grocery List
- Main content:
  - "This Week's Meal Plan" table (Day | Breakfast | Lunch | Dinner)
  - Meal names displayed as orange links
- Right section: "Upcoming Meals" list with thumbnails

### 2.2 Issue #2: Settings
**Priority:** Medium

#### Dietary Preferences
- Selection buttons (single-select): Omnivore, Vegetarian, Vegan, Pescatarian, Keto, Paleo, Low Carb, Mediterranean
- Selected = orange background (#FF8C00), white text
- Unselected = gray background, black text

#### Allergies
- Dropdown multi-select (gluten, nuts, dairy, etc.)

#### Number of Meals Per Day
- Selection buttons: 2, 3, 4

#### Desktop Additional Options
- Exclude Ingredients dropdown
- Meal Plan Length dropdown (1 Week, 2 Weeks)
- Checkboxes:
  - Include leftovers in the plan
  - Generate a shopping list automatically
- "Save Settings" button (orange)

### 2.3 Issue #3: Shopping List
**Priority:** Medium

#### Features
- Categorized items: Produce, Dairy, Meat, Pantry
- Interactive checkboxes (orange when checked, strikethrough text)
- Item format: "quantity unit Item" (e.g., "2 lbs Apples")
- "Add Item" button (orange, opens input modal)
- "Print" button (gray, generates printable version)
- Auto-sync with meal plan changes

### 2.4 Issue #4: Recipe Discovery
**Priority:** High

#### Search & Filters
- Search bar with placeholder "Search for recipes"
- Horizontal scrollable filter chips:
  - Quick & Easy, Vegetarian, Gluten-Free, Low Carb, Family-Friendly, Desserts, Breakfast, Lunch, Dinner
- Selected filter = orange background with orange text
- Multi-select supported

#### Recipe List
- "All Recipes" section header
- Recipe cards containing:
  - Square thumbnail (80x80px, rounded corners)
  - Recipe title (bold)
  - Short description
- Click navigates to recipe detail

---

## 3. Technical Requirements

### 3.1 Frontend (Angular)
- **Framework:** Angular 18+ with standalone components
- **Styling:** SCSS with CSS variables for theming
- **State Management:** Angular Signals or NgRx (if complexity warrants)
- **Routing:** Angular Router with lazy-loaded feature modules
- **HTTP:** HttpClient with interceptors for auth/error handling
- **Responsive:** Mobile-first design with breakpoints at 768px, 1024px

### 3.2 Backend (.NET)
- **Framework:** .NET 8 with ASP.NET Core Web API
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API layers)
- **API Style:** RESTful with consistent JSON responses
- **Validation:** FluentValidation
- **ORM:** Entity Framework Core
- **Documentation:** OpenAPI/Swagger

### 3.3 Database Schema (Core Entities)

```
Recipe
├── Id (GUID)
├── Name (string)
├── Description (string)
├── ImageUrl (string)
├── PrepTimeMinutes (int)
├── Tags (string[]) // Quick & Easy, Vegetarian, etc.
├── MealType (enum: Breakfast, Lunch, Dinner)
└── Ingredients (Ingredient[])

Ingredient
├── Id (GUID)
├── Name (string)
├── Category (enum: Produce, Dairy, Meat, Pantry)
└── DefaultUnit (string)

MealPlan
├── Id (GUID)
├── UserId (GUID)
├── Date (DateTime)
├── MealType (enum)
└── RecipeId (GUID)

ShoppingListItem
├── Id (GUID)
├── UserId (GUID)
├── IngredientId (GUID)
├── Quantity (decimal)
├── Unit (string)
├── IsChecked (bool)
└── Category (enum)

UserPreferences
├── UserId (GUID)
├── DietaryPreference (enum)
├── Allergies (string[])
├── MealsPerDay (int)
├── ExcludedIngredients (string[])
├── MealPlanLengthWeeks (int)
├── IncludeLeftovers (bool)
└── AutoGenerateShoppingList (bool)
```

### 3.4 API Endpoints

```
GET    /api/recipes                    # List recipes with filtering
GET    /api/recipes/{id}               # Get recipe details
GET    /api/meal-plans/daily/{date}    # Get daily digest
GET    /api/meal-plans/weekly/{date}   # Get weekly plan
POST   /api/meal-plans                 # Create/update meal plan entry
PUT    /api/meal-plans/{id}/swap       # Swap meal suggestion
GET    /api/shopping-list              # Get shopping list
POST   /api/shopping-list/items        # Add item
PATCH  /api/shopping-list/items/{id}   # Toggle checked status
DELETE /api/shopping-list/items/{id}   # Remove item
POST   /api/shopping-list/generate     # Generate from meal plan
GET    /api/preferences                # Get user preferences
PUT    /api/preferences                # Update preferences
```

---

## 4. Design System

### 4.1 Colors
| Token | Hex | Usage |
|-------|-----|-------|
| Primary | #FF8C00 | Buttons, links, selected states |
| Primary Light | #FFD700 | Hover states, borders |
| Background | #F8F8F8 | Page background |
| Surface | #FFFFFF | Cards, inputs |
| Text Primary | #000000 | Headings |
| Text Secondary | #616161 | Descriptions |
| Border | #E0E0E0 | Input borders, separators |

### 4.2 Typography
- **Title Large:** 24px, Bold
- **Subtitle Bold:** 16px, Bold
- **Body Regular:** 14px, Normal
- **Caption:** 12px, Normal

### 4.3 Components
- **Button Primary:** Orange fill, white text, 8px radius
- **Button Secondary:** Transparent, orange border, orange text
- **Card:** White background, subtle shadow, 8px radius
- **Checkbox:** Orange fill when checked
- **Dropdown:** White background, gray border, arrow indicator

---

## 5. Acceptance Criteria Summary

### Home Page
- [ ] Mobile: Daily Digest displays today's meals with Swap/Cook buttons
- [ ] Desktop: Weekly calendar view with meal table
- [ ] Responsive layout switches at 768px breakpoint
- [ ] Quick Actions functional (Add Recipe, Create Plan, Generate List)

### Settings
- [ ] Single dietary preference selection with visual feedback
- [ ] Multi-select allergies dropdown
- [ ] Meals per day selector (2/3/4)
- [ ] Desktop: Additional options visible
- [ ] Save persists preferences to backend

### Shopping List
- [ ] Items grouped by category (Produce, Dairy, Meat, Pantry)
- [ ] Checkbox toggles with visual feedback
- [ ] Add Item opens modal/input
- [ ] Print generates printable format
- [ ] Auto-syncs when meal plan changes

### Recipe Discovery
- [ ] Search filters recipes in real-time
- [ ] Filter chips are multi-selectable
- [ ] Selected filter shows orange highlight
- [ ] Recipe cards display image, title, description
- [ ] Empty state message when no matches

---

## 6. Project Structure

```
/
├── src/
│   ├── MealPlanner.Api/           # .NET Web API
│   ├── MealPlanner.Application/   # Use cases, DTOs
│   ├── MealPlanner.Domain/        # Entities, enums
│   ├── MealPlanner.Infrastructure/# EF Core, repositories
│   └── mealplanner-web/           # Angular app
│       ├── src/app/
│       │   ├── core/              # Services, guards, interceptors
│       │   ├── shared/            # Shared components, pipes
│       │   ├── features/
│       │   │   ├── home/
│       │   │   ├── recipes/
│       │   │   ├── shopping-list/
│       │   │   └── settings/
│       │   └── app.routes.ts
│       └── assets/
└── tests/
```

---

## 7. Out of Scope (Future Iterations)
- User authentication/registration
- Recipe creation by users
- Nutritional information
- Social sharing features
- Push notifications
- Multi-language support
- Dark mode

---

## 8. Success Metrics
- Users can complete daily meal planning workflow in < 2 minutes
- Page load time < 2 seconds
- Mobile responsiveness on all major breakpoints
- Zero critical accessibility violations (WCAG 2.1 AA)

---

## 9. User Stories

### US-001: Daily Digest - View today's meals
As a user, I want to see today's meals with quick actions so that I can act quickly on my plan.

- Displays Breakfast, Lunch, Dinner with image, name, and actions
- Includes "Swap Meal" and "Cook Now" actions on each card
- Mobile-first layout with vertical scrolling

### US-002: Swap a planned meal
As a user, I want to swap a planned meal so that I can replace it with a suggested alternative.

- Clicking "Swap Meal" shows suggested alternatives
- Selecting an alternative updates the plan for that meal and date
- Shopping list is updated accordingly

### US-003: Cook Now from Daily Digest
As a user, I want to start cooking a planned meal so that I can access the recipe quickly.

- "Cook Now" navigates to the recipe details page
- Preserves navigation state to return to Daily Digest

### US-004: Weekly plan view (desktop)
As a user, I want to see a weekly plan grid so that I can review and edit my week at a glance.

- Desktop shows a table Day × (Breakfast, Lunch, Dinner)
- Meal names are clickable to open details
- Supports quick replace/edit from the grid

### US-005: Desktop quick actions
As a user, I want quick actions (Add Recipe, Create Plan, Generate List) so that I can perform frequent tasks faster.

- Visible in the desktop sidebar
- Each action triggers the corresponding flow

### US-006: Discover recipes with search and filters
As a user, I want to search and filter recipes so that I can find suitable options.

- Search bar filters by title/description
- Multi-select filter chips (e.g., Quick & Easy, Vegetarian, Gluten-Free)
- Results update in real-time

### US-007: View recipe details
As a user, I want to view a recipe's details so that I can decide to cook or plan it.

- Shows title, image, description, ingredients, and steps
- Displays tags and meal type
- Action to add to a specific day/meal slot

### US-008: Generate shopping list from meal plan
As a user, I want to generate a shopping list so that I can buy what I need for my planned meals.

- Generates categorized items from the current meal plan
- Groups by Produce, Dairy, Meat, Pantry
- Deduplicates and sums quantities when possible

### US-009: Manage shopping list items
As a user, I want to manage shopping list items so that I can track purchases and modify the list.

- Check/uncheck items with visual feedback
- Add custom items (quantity, unit, name)
- Remove items and print the list

### US-010: Set dietary preferences and allergies
As a user, I want to set my dietary preferences and allergies so that recommendations match my needs.

- Single-select dietary preference (e.g., Vegetarian, Keto)
- Multi-select allergies (e.g., gluten, nuts, dairy)
- Preferences affect recipe suggestions and planning

### US-011: Configure meal plan options
As a user, I want to configure meals per day, plan length, leftovers, and excluded ingredients so that my plan fits my routine.

- Select 2/3/4 meals per day
- Set plan length (1 or 2 weeks)
- Toggle leftovers and auto-generate shopping list
- Exclude ingredients from suggestions

### US-012: Auto-sync shopping list on plan changes
As a user, I want my shopping list to update automatically when I change the plan so that it remains accurate.

- Any plan change reflects in the shopping list
- Badge/notice indicates updates applied