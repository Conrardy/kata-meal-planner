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

- [Vision & Domaine](project/brief.md)
- [Architecture](architecture/overview.md)
- [Conventions Backend](conventions/backend.md)
- [Conventions Frontend](conventions/frontend.md)
- [Deployment & Docker](deployment/deployment.md)

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
