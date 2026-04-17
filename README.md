# YoutubeClone

Proyecto académico final que implementa un clon funcional de YouTube, desarrollado como ejercicio de arquitectura backend con base de datos relacional. El sistema expone una API REST en ASP.NET Core con autenticación JWT, caché en memoria y persistencia en SQL Server.

## Colaboradores

- Juliet Morales
- Neider Velez

## Recursos de diseño

- Diagrama de arquitectura: [Excalidraw](https://excalidraw.com/#json=Ac39waOTlC1vGvEelTVgl,3o7MYImjDWIB5MEH8V0ABQ)

---

## Estructura del repositorio

```
YoutubeClone/
├── database/               # Scripts SQL para la base de datos
│   ├── ddl.sql             # Definición de tablas y esquema
│   ├── dml.sql             # Datos de prueba / seed data
│   └── queries.sql         # Consultas de ejemplo y referencia
│
├── docker/                 # Infraestructura Docker
│   ├── docker-compose.yml  # Levanta SQL Server en contenedor
│   └── data/               # Backups .bak para restaurar bases de datos de ejemplo
│       ├── AdventureWorksLT2022.bak
│       └── leaderboard.bak
│
└── backend/
    ├── withcache/          # Versión inicial del backend (solo usuarios + caché básica)
    └── connect-with-database/   # Versión completa del backend (producción)
        └── YoutubeClone/   # Solución .NET con arquitectura por capas
```

---

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| .NET SDK | 9.0 |
| SQL Server | 2019 o superior (o Docker) |
| Docker (opcional) | 24.x |

---

## Levantamiento rápido

### 1. Base de datos con Docker

```bash
cd docker
docker compose up
```

Esto levanta un contenedor de SQL Server en el puerto **1433** con las credenciales:

- **Usuario:** `sa`
- **Contraseña:** `Admin1234@`

### 2. Crear el esquema de base de datos

Conectarse a SQL Server y ejecutar en orden:

```sql
-- 1. Crear tablas
database/ddl.sql

-- 2. Insertar datos de referencia y seed
database/dml.sql
```

### 3. Configurar la aplicación

Editar `backend/connect-with-database/YoutubeClone/YoutubeClone.WebApp/appsettings.json` o definir variables de entorno:

| Variable de entorno | Clave en appsettings | Descripción |
|---|---|---|
| `CONNECTION_STRING_DATABASE` | `ConnectionStrings:YoutubeClone` | Cadena de conexión a SQL Server |
| `JWT_ISSUER` | `Jwt:Issuer` | Emisor del token JWT |
| `JWT_AUDIENCE` | `Jwt:Audience` | Audiencia del token JWT |
| `JWT_PRIVATE_KEY` | `Jwt:PrivateKey` | Clave secreta para firmar tokens |
| `JWT_EXPIRATION_IN_MINUTES_MIN` | `Jwt:ExpirationInMinutesMin` | Expiración mínima del token (minutos) |
| `JWT_EXPIRATION_IN_MINUTES_MAX` | `Jwt:ExpirationInMinutesMax` | Expiración máxima del token (minutos) |

### 4. Ejecutar la API

```bash
cd backend/connect-with-database/YoutubeClone
dotnet run --project YoutubeClone.WebApp
```

En modo desarrollo, la documentación Scalar/OpenAPI estará disponible en `https://localhost:{puerto}/scalar`.

---

## Arquitectura general

El backend sigue una **arquitectura limpia por capas** (Clean Architecture / Layered Architecture):

```
YoutubeClone.WebApp          ← Capa de presentación (Controllers, Middlewares)
       │
YoutubeClone.Application     ← Capa de aplicación (Services, DTOs, Interfaces)
       │
YoutubeClone.Domain          ← Capa de dominio (Entities, Context EF Core, Interfaces de repositorios)
       │
YoutubeClone.Infraestructure ← Capa de infraestructura (Repositorios concretos, UnitOfWork)
       │
YoutubeClone.Shared          ← Utilidades transversales (Hasher, Generate, Constants, Helpers)
```

Cada capa solo puede depender de las capas inferiores, nunca hacia arriba.

---

## Funcionalidades implementadas

- **Gestión de usuarios:** CRUD completo (crear, listar con paginación, obtener por ID, actualizar, eliminar lógico).
- **Autenticación JWT:** Login con email y contraseña, generación de access token y refresh token almacenado en caché.
- **Renovación de token:** Endpoint para renovar el access token usando el refresh token.
- **Gestión de canales:** Creación y consulta de canales asociados a usuarios.
- **Caché en memoria:** Tokens JWT y refresh tokens almacenados en `IMemoryCache`.
- **Manejo global de errores:** Middleware centralizado que convierte excepciones de dominio en respuestas HTTP estructuradas.
- **Primer usuario automático:** Al iniciar la aplicación, se crea un usuario administrador inicial si la base de datos está vacía.
- **Logging con Serilog:** Logs escritos en consola y archivo rotativo diario.
- **Validación de modelos:** Validaciones con Data Annotations y respuesta estructurada para errores de validación.

---

## Historial de versiones del backend

| Carpeta | Descripción |
|---|---|
| `backend/withcache` | Versión inicial. Solo manejo de usuarios con caché básico. Sin autenticación JWT. |
| `backend/connect-with-database` | Versión completa. Incluye autenticación, canales, middleware de errores, logging y arquitectura limpia completa. |

---

## Disclaimer

Todos los derechos reservados a Google. Este proyecto es únicamente con fines educativos.
