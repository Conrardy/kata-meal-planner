# Decision Matrix - Ecrans pilotes et sensibles

## Objectif
Definir une matrice simple pour classer les ecrans en pilotes ou sensibles, afin de planifier la bascule Firestore par feature.

## Regles de lecture
- Score par critere: 1 (faible) a 5 (fort)
- Score total = somme des criteres
- Classification:
  - Pilote: score <= 12
  - Sensible: score >= 13

## Criteres
| Critere | Description |
|---|---|
| Criticite business | Impact utilisateur si incident ou divergence |
| Complexite data | Volume, relations, transformations |
| Risque divergence | Probabilite de desync en dual-write |
| Observabilite | Capacite a detecter/rollback rapidement (score haut = difficile) |

## Matrice
| Ecran/Feature | Criticite business | Complexite data | Risque divergence | Observabilite | Score total | Classification | Notes |
|---|---:|---:|---:|---:|---:|---|---|
| Preferences utilisateur | 2 | 2 | 2 | 2 | 8 | Pilote | Pilote propose dans Mikado |
| Shopping list | 3 | 2 | 2 | 2 | 9 | Pilote | Pilote propose dans Mikado |
| Weekly plan | 4 | 4 | 4 | 4 | 16 | Sensible | A migrer plus tard |
| Recipes | 4 | 4 | 3 | 3 | 14 | Sensible | A migrer plus tard |

## Notes
- Ajuster les scores si la complexite ou la criticite evolue.
- Utiliser cette matrice pour alimenter le plan de migration ecran par ecran.
