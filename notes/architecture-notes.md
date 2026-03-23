# Architecture Notes

Initial structure:
- Controllers
- Models / Entities
- Data / DbContext
- DTOs (later if needed)
- Services (later, after endpoints exist)

Rule for early implementation:
- get the API running first
- avoid over-architecting on day one
- move logic into services only after basic flow works