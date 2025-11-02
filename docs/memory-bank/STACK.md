Stack

## Frontend
- **Framework**: Angular 20 (standalone components)
- **Styling**: Tailwind CSS 3.4+ (custom design system)
- **Forms**: Angular Reactive Forms
- **E2E Testing**: Playwright
- **Package Manager**: npm (Node.js 20.x LTS)

## Backend
- **Framework**: ASP.NET Core 8 Web API
- **Database**: PostgreSQL 16+
- **ORM**: Entity Framework Core (Npgsql)
- **Unit Testing**: xUnit
- **Integration Testing**: Testcontainers for .NET

## Infrastructure
- **Hosting**: Azure App Service + PostgreSQL Flexible Server
- **CI/CD**: GitHub Actions
- **Containerization**: Docker (for Testcontainers, optional for deployment)
- **Email**: SendGrid (future)

## Development Tools
- **Version Control**: Git + GitHub
- **IDE**: Visual Studio Code / Visual Studio 2022
- **Database Client**: psql / pgAdmin (optional)

## Notes
- Cross-platform development (Windows primary, Linux for CI/CD)
- Authentication deferred for MVP
- Design system built with Tailwind CSS for AI-assisted development
- Keep versions generic (e.g., Node.js 20, .NET 8) — avoid minor versions
