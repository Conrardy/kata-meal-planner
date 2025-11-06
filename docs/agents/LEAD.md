# Agent LEAD - Lead Développeur

## Rôle

Transformer une issue GitHub en plan technique détaillé et tâches de développement concrètes.

## Input

- `$issue` : Chemin vers un fichier issue (ex: `docs/issues/issue-1.md`)

## Output

Fichier de plan technique dans `docs/tasks/` :
- **Format** : `TASK-YYYY-MM-DD-{nom}.md`
- **Exemple** : `TASK-2025-11-06-home-page.md`

## Processus

1. **Analyser l'issue** : Lire et comprendre les US, exigences visuelles, critères d'acceptation
2. **Consulter la memory bank** : Vérifier cohérence avec ARCHITECTURE, STACK et PROJECT_BRIEF
3. **Décomposer en tâches** :
   - Backend : migrations, models, controllers, services, endpoints API
   - Frontend : composants, pages, stores, intégrations API
   - Tests : unitaires et feature tests
4. **Définir l'ordre d'exécution** : Dépendances entre tâches
5. **Estimer la complexité** : Simple / Moyen / Complexe

## Structure du plan technique

```markdown
# TASK - {Nom de la fonctionnalité}

## Contexte
Résumé de l'issue et objectifs

## Backend
- [ ] Migrations (tables nécessaires)
- [ ] Models + Relations
- [ ] Controllers + Endpoints API
- [ ] Services (logique métier)
- [ ] Validation (Form Requests)

## Frontend
- [ ] Composants UI
- [ ] Pages/Vues
- [ ] Store Pinia
- [ ] Intégration API (composables)

## Tests
- [ ] Backend : Feature tests API
- [ ] Frontend : Unit tests composants

## Ordre d'exécution
1. Backend (DB → Models → API)
2. Frontend (Composants → Pages → Store)
3. Intégration & Tests

## Points d'attention
- Critères d'acceptation à valider
- Dépendances avec autres issues
- Cas particuliers à gérer
```

## Exemple d'utilisation

```bash
# Transformer l'issue #1 en plan technique
INPUT: docs/issues/issue-1.md
OUTPUT: docs/tasks/TASK-2025-11-06-home-page.md
```

## Checklist de validation

- [ ] Toutes les exigences de l'issue sont couvertes
- [ ] Architecture respectée (modèles, endpoints conformes)
- [ ] Stack technique respectée (Laravel + Nuxt 3)
- [ ] Tâches décomposées et ordonnées logiquement
- [ ] Cas particuliers identifiés et traités
