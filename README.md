# OpsPilot

OpsPilot es un asistente de troubleshooting desarrollado como proyecto de portfolio que convierte un incidente de servicio en un diagnóstico estructurado y respaldado por evidencia. Combina la recuperación de runbooks y herramientas operativas deterministas en un flujo sencillo, que permite entender el fundamento de cada recomendación.

## Qué hace

Ingresá el nombre de un servicio y el problema en la interfaz de una sola pantalla. OpsPilot devuelve un resumen, la causa probable, la acción recomendada, el nivel de confianza, las fuentes de los runbooks, las herramientas utilizadas y la evidencia encontrada. Los escenarios incluidos cubren payments, authentication, database y servicios desconocidos.

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

## Arquitectura y stack

El backend utiliza .NET 8 y se organiza en cuatro proyectos con responsabilidades concretas:

- OpsPilot.Domain contiene el modelo de incidente y su severidad.
- OpsPilot.Application contiene el agente basado en reglas, los embeddings deterministas, la búsqueda en Qdrant, las herramientas técnicas y el flujo de trabajo.
- OpsPilot.Contracts contiene los contratos de solicitud y respuesta de la API.
- OpsPilot.Api expone REST, Swagger en Development y MCP mediante Streamable HTTP.

El frontend es una aplicación de Next.js 16 y TypeScript ubicada en src/OpsPilot.Web. Qdrant 1.15 se ejecuta mediante Docker Compose. Los tests utilizan xUnit y WebApplicationFactory; GitHub Actions compila y valida ambos componentes.

## RAG

Los cuatro runbooks Markdown de data/runbooks se convierten en embeddings locales mediante un proveedor determinista y se indexan en Qdrant. Cada análisis recupera contexto relevante antes de ejecutar las reglas. La respuesta identifica los archivos recuperados en sources. La configuración incluye campos reservados para un futuro proveedor de OpenAI o Azure OpenAI, pero la aplicación no requiere APIs pagas ni secretos.

## MCP

El SDK oficial de MCP para C# expone un servidor Streamable HTTP en /mcp. Ofrece tres herramientas locales y deterministas:

- get_service_health devuelve el estado de salud simulado de un servicio.
- search_logs devuelve logs técnicos simulados que coinciden con la consulta.
- get_recent_deployments devuelve los despliegues recientes simulados.

El flujo de troubleshooting invoca en proceso las mismas implementaciones de las herramientas, combina sus resultados con el contexto RAG y expone la trazabilidad mediante toolsUsed y evidence.

## Ejecución local

Requisitos: SDK de .NET 8, Node.js 24+, Docker Desktop y Visual Studio 2022.

Configuración inicial:

~~~powershell
docker compose up -d
dotnet run --project src/OpsPilot.Api --no-launch-profile --urls http://localhost:57672
Invoke-RestMethod -Method Post http://localhost:57672/api/runbooks/index
~~~

Detené la API temporal después de indexar. Abrí OpsPilot.sln, seleccioná OpsPilot.Api como proyecto de inicio y presioná Play. SpaProxy instala las dependencias del frontend cuando hace falta, inicia Next.js y la API, y abre http://localhost:3000. No es necesario iniciar el backend y el frontend con comandos separados.

Los endpoints principales son:

- POST /api/troubleshooting/analyze
- POST /api/runbooks/index
- GET /api/health
- /mcp para MCP Streamable HTTP

Para detener la infraestructura local:

~~~powershell
docker compose down
~~~

## Verificación

~~~powershell
dotnet build
dotnet test
cd src/OpsPilot.Web
npm install
npm run build
~~~

El repositorio cuenta con 20 tests de backend, todos verdes, que cubren el dominio, la API, los puntos de integración de RAG, las herramientas deterministas, la salida del flujo de trabajo y los servicios desconocidos. CI ejecuta restore/build/test del backend e install/build del frontend en cada push y pull request a main.

## Limitaciones intencionales

OpsPilot utiliza reglas, embeddings, logs, estados de salud y datos de despliegues deterministas para que el portfolio pueda ejecutarse sin cuentas externas. No incluye autenticación, persistencia, conexiones a observabilidad en vivo, LLM pago, memoria conversacional, comportamiento multiagente, automatización de despliegues ni preparación para producción. La interfaz se limita intencionalmente a una pantalla de troubleshooting.
