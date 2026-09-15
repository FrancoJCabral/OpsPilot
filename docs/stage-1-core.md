# Stage 1: OpsPilot core

Proyecto de GitHub/portfolio con .NET 8.

- Domain: TechnicalIncident inmutable, campos obligatorios, Severity válida y fecha UTC generada.
- Application: ITroubleshootingAgent y reglas deterministas para payments, authentication, database y servicios desconocidos.
- Contracts: request y response con JSON camelCase.
- API: POST /api/troubleshooting/analyze y GET /api/health.
- Swagger en Development; validaciones con ProblemDetails (400).
- Confidence es una heurística fija entre 0 y 1. Las reglas se seleccionan por servicio; issue se conserva en el incidente.

Referencias: Application → Domain; Api → Application y Contracts. Tests de dominio y API con WebApplicationFactory.

Validar desde la raíz con dotnet restore, dotnet build y dotnet test.
Desarrollo: dotnet run --project src/OpsPilot.Api -- --environment Development.

Esta etapa no incluye LLM real, persistencia, frontend, Docker, CI, RAG ni MCP.
