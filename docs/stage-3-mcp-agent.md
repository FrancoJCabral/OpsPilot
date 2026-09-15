# Stage 3: MCP + agent workflow

La API expone un servidor MCP compatible con Streamable HTTP en /mcp, usando el SDK oficial ModelContextProtocol.AspNetCore. Los clientes MCP pueden inicializarse, listar herramientas y ejecutarlas mediante el protocolo.

Herramientas disponibles:
- get_service_health(serviceName): estado local Healthy, Degraded o Down. Servicios desconocidos devuelven status null y knownService false.
- search_logs(serviceName, query): logs simulados que coinciden con términos de la consulta; sin coincidencias devuelve una lista vacía.
- get_recent_deployments(serviceName): deployments simulados con fechas UTC fijas; desconocidos devuelven una lista vacía.

POST /api/troubleshooting/analyze ejecuta un workflow simple: busca runbooks en Qdrant, consulta salud, logs y deployments para servicios conocidos, y combina el contexto y la evidencia con el agente basado en reglas. Para un servicio desconocido consulta salud y conserva el diagnóstico controlado.

El workflow invoca en proceso las mismas implementaciones locales que expone MCP, sin una llamada HTTP a sí mismo. La respuesta incluye sources, evidence y toolsUsed, además del diagnóstico. Las recomendaciones incorporan la guía del runbook recuperado. Toda la telemetría está explícitamente marcada como simulada.

No hay LLM pago, multi-agent, memoria conversacional ni aprobación humana. Qdrant sigue siendo necesario para RAG; los tests de API usan el reemplazo local existente para búsqueda.
