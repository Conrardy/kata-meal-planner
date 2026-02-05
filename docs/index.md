# MealPlanner

Bienvenue dans la documentation du projet **MealPlanner**.

MealPlanner est une application de planification de repas permettant aux utilisateurs d'organiser leur alimentation, de découvrir des recettes et de générer automatiquement leur liste de courses.

## Stack Technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Frontend  | Angular     | 19      |
| Backend   | .NET        | 9       |
| Base de données | PostgreSQL | 17 |
| Cache     | Redis       | 7       |

## Liens rapides

- [Vision & Domaine](memory-bank/PROJECT_BRIEF.md)
- [Architecture](memory-bank/common/ARCHITECTURE.md)
- [Conventions Backend](memory-bank/backend/CONVENTIONS.md)
- [Conventions Frontend](memory-bank/frontend/CONVENTIONS.md)
- [Deployment & Docker](memory-bank/infra/DEPLOYMENT.md)

## Demarrage rapide

### Backend

```bash
dotnet restore backend/MealPlanner.sln
dotnet run --project backend/src/Api/MealPlanner.Api
```

### Frontend

```bash
cd frontend
npm ci
npm start
```

### Docker Compose

```bash
docker-compose up -d
```
