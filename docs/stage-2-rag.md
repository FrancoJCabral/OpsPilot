# Stage 2: RAG con runbooks

OpsPilot indexa cuatro runbooks Markdown locales en Qdrant y recupera contexto antes de ejecutar las reglas de troubleshooting. La respuesta incluye los nombres de los runbooks consultados en `sources`.

- `POST /api/runbooks/index`: crea la colección y reindexa `data/runbooks`.
- `POST /api/troubleshooting/analyze`: genera un embedding de `serviceName + issue`, busca hasta tres runbooks y aplica las reglas con ese contexto.
- Qdrant se ejecuta con `docker compose up -d` y expone HTTP en `localhost:6333`.

El proveedor predeterminado genera embeddings locales, deterministas y sin secretos. `Embeddings` en `appsettings.json` deja configurables los campos para una integración futura con OpenAI o Azure OpenAI; esta etapa no implementa llamadas externas.

Para probar localmente: iniciar Qdrant, ejecutar la API y llamar primero a `/api/runbooks/index`.
