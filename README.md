# OpsPilot

OpsPilot is a portfolio troubleshooting assistant that turns a service incident into a structured, evidence-backed diagnosis. It brings runbook retrieval and deterministic operational tools into one small workflow, making the reasoning behind each recommendation visible.

## What it does

Enter a service name and issue in the single-page UI. OpsPilot returns a summary, probable cause, recommended action, confidence, runbook sources, tools used, and the evidence it found. The included scenarios cover payments, authentication, database, and unknown services.

~~~mermaid
flowchart LR
    UI[Next.js UI] --> API[ASP.NET Core API]
    API --> WF[Troubleshooting workflow]
    WF --> RAG[RAG search]
    RAG --> Q[(Qdrant)]
    WF --> MCP[MCP tools]
    RAG --> D[Structured diagnosis]
    MCP --> D
    D --> UI
~~~

## Architecture and stack

The backend uses .NET 8 with four focused projects:

- OpsPilot.Domain contains the incident model and severity.
- OpsPilot.Application contains the rule-based agent, deterministic embeddings, Qdrant search, technical tools, and workflow.
- OpsPilot.Contracts contains API request and response contracts.
- OpsPilot.Api exposes REST, Swagger in Development, and MCP over Streamable HTTP.

The frontend is a Next.js 16 and TypeScript application in src/OpsPilot.Web. Qdrant 1.15 runs through Docker Compose. Tests use xUnit and WebApplicationFactory; GitHub Actions builds both stacks.

## RAG

Four Markdown runbooks in data/runbooks are embedded locally with a deterministic provider and indexed in Qdrant. Each analysis retrieves relevant context before the rules run. The response names the retrieved files in sources. Configuration includes placeholders for a future OpenAI or Azure OpenAI provider, but the application requires no paid API or secret.

## MCP

The official C# MCP SDK exposes a Streamable HTTP server at /mcp. It provides three deterministic local tools:

- get_service_health returns simulated health for a service.
- search_logs returns matching simulated technical logs.
- get_recent_deployments returns simulated recent releases.

The troubleshooting workflow calls the same tool implementations in process, combines their output with RAG context, and exposes the trace through toolsUsed and evidence.

## Run locally

Requirements: .NET 8 SDK, Node.js 24+, Docker Desktop, and Visual Studio 2022.

First-time setup:

~~~powershell
docker compose up -d
dotnet run --project src/OpsPilot.Api --no-launch-profile --urls http://localhost:57672
Invoke-RestMethod -Method Post http://localhost:57672/api/runbooks/index
~~~

Stop the temporary API after indexing. Open OpsPilot.sln, select OpsPilot.Api as the startup project, and press Play. SpaProxy installs frontend dependencies when needed, starts Next.js, starts the API, and opens http://localhost:3000. Backend and frontend do not need separate launch commands.

The main endpoints are:

- POST /api/troubleshooting/analyze
- POST /api/runbooks/index
- GET /api/health
- /mcp for MCP Streamable HTTP

To stop local infrastructure:

~~~powershell
docker compose down
~~~

## Verification

~~~powershell
dotnet build
dotnet test
cd src/OpsPilot.Web
npm install
npm run build
~~~

The repository contains 19 focused backend tests covering the domain, API, RAG integration seams, deterministic tools, workflow output, and unknown services. CI repeats backend restore/build/test and frontend install/build on pushes and pull requests to main.

## Intentional limitations

OpsPilot uses deterministic rules, embeddings, logs, health, and deployment data so the portfolio can run without external accounts. It has no authentication, persistence, live observability connections, paid LLM, conversational memory, multi-agent behavior, deployment automation, or production hardening. The UI is intentionally one troubleshooting screen.
