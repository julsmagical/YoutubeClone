# Backend — connect-with-database

Esta es la versión completa y principal del backend del proyecto YoutubeClone. Implementa una API REST con ASP.NET Core 9 siguiendo una **arquitectura limpia por capas**. Incluye autenticación JWT con refresh tokens, caché en memoria, manejo global de errores, logging con Serilog y gestión de usuarios y canales.

---

## Tecnologías y dependencias principales

| Paquete / Tecnología | Capa | Propósito |
|---|---|---|
| ASP.NET Core 9 | WebApp | Framework HTTP |
| Entity Framework Core | Domain / Infraestructure | ORM para SQL Server |
| Microsoft.EntityFrameworkCore.SqlServer | Domain | Proveedor EF Core para SQL Server |
| Microsoft.AspNetCore.Authentication.JwtBearer | WebApp | Validación de tokens JWT |
| System.IdentityModel.Tokens.Jwt | Application | Generación de tokens JWT |
| Serilog.AspNetCore | WebApp | Logging estructurado |
| Scalar.AspNetCore | WebApp | Documentación OpenAPI (reemplaza Swagger UI) |
| IMemoryCache | Application | Caché de tokens en memoria |

---

## Estructura de la solución

```
YoutubeClone/
├── YoutubeClone.WebApp/              ← Capa de presentación
├── YoutubeClone.Application/         ← Capa de aplicación
├── YoutubeClone.Domain/              ← Capa de dominio
├── YoutubeClone.Infraestructure/     ← Capa de infraestructura
└── YoutubeClone.Shared/              ← Utilidades transversales
```

La dependencia entre proyectos respeta la dirección de la arquitectura limpia:

```
WebApp → Application → Domain ← Infraestructure
             ↑                         ↑
           Shared ─────────────────────┘
```

---

## Proyectos en detalle

---

### `YoutubeClone.Shared`

Proyecto de utilidades transversales sin dependencias de negocio. Puede ser referenciado por cualquier otra capa.

**Archivos y responsabilidades:**

#### `Hasher.cs`
Utilidad estática para hasheo y verificación de contraseñas. Usa PBKDF2 con SHA-256, salt aleatorio de 16 bytes y 100.000 iteraciones.

- `HashPassword(string password) → string`: genera un hash seguro en formato `hash:salt` (ambos en Base64).
- `ComparePassword(string password, string storedHash) → bool`: compara una contraseña en texto plano contra el hash almacenado.

#### `Generate.cs`
Utilidad estática para generación de valores aleatorios.

- `RandomText(int length) → string`: genera un string aleatorio de longitud dada, útil para crear refresh tokens opacos.

#### `Helpers/DateTimeHelper.cs`
Helper para fechas.

- `UtcNow() → DateTime`: retorna la fecha/hora actual en UTC. Centraliza el uso de `DateTime.UtcNow` para facilitar pruebas y consistencia.

#### `Constants/ClaimsConstants.cs`
Constantes con los nombres de los claims usados en el JWT.

- `USERACCOUNT_ID`: nombre del claim que almacena el `UserID` del usuario autenticado.

#### `Constants/ConfigurationConstants.cs`
Constantes que mapean las claves de configuración (`appsettings.json` / variables de entorno):

| Constante | Clave en configuración | Descripción |
|---|---|---|
| `CONNECTION_STRING_DATABASE` | `ConnectionStrings:YoutubeClone` | Cadena de conexión a SQL Server |
| `JWT_ISSUER` | `Jwt:Issuer` | Emisor del JWT |
| `JWT_AUDIENCE` | `Jwt:Audience` | Audiencia del JWT |
| `JWT_PRIVATE_KEY` | `Jwt:PrivateKey` | Clave secreta para firmar el JWT |
| `JWT_EXPIRATION_IN_MINUTES_MIN` | `Jwt:ExpirationInMinutesMin` | Tiempo mínimo de vida del token |
| `JWT_EXPIRATION_IN_MINUTES_MAX` | `Jwt:ExpirationInMinutesMax` | Tiempo máximo de vida del token |
| `FIRST_APP_TIME_USER_USERNAME` | `FirstAppTime:User:UserName` | Username del usuario administrador inicial |
| `FIRST_APP_TIME_USER_DISPLAYNAME` | `FirstAppTime:User:DisplayName` | DisplayName del usuario administrador inicial |
| `FIRST_APP_TIME_USER_EMAIL` | `FirstAppTime:User:Email` | Email del usuario administrador inicial |
| `FIRST_APP_TIME_USER_PASSWORD` | `FirstAppTime:User:Password` | Contraseña del usuario administrador inicial |
| `AUTH_REFRESH_TOKEN_EXPIRATION_IN_DAYS` | `Auth:RefreshToken:ExpirationInDays` | Días de vida del refresh token |

#### `Constants/ResponseConstants.cs`
Constantes de mensajes de respuesta usados en toda la aplicación (errores, mensajes de éxito, etc.).

- Mensajes de autenticación: `AUTH_TOKEN_NOT_FOUND`, `AUTH_USER_OR_PASSWORD_NOT_FOUND`, `AUTH_REFRESH_TOKEN_NOT_FOUND`.
- Mensajes de usuario: `USER_NOT_EXIST`.
- Error genérico con traceId: `ERROR_UNEXPECTED(traceId)`.
- Helper: `ConfigurationPropertyNotFound(key)`.

#### `Constants/ValidationConstants.cs`
Constantes usadas en la validación de modelos.

- `VALIDATION_MESSAGE`: mensaje genérico que se retorna cuando el modelo de la request es inválido.

---

### `YoutubeClone.Domain`

Capa de dominio. Contiene las entidades de base de datos, el `DbContext` de Entity Framework Core, las interfaces de repositorios y las excepciones de negocio.

**No depende de ningún otro proyecto del sistema** (excepto paquetes de EF Core).

#### `Database/SqlServer/Entities/`

Clases POCO que representan las tablas de la base de datos.

| Entidad | Tabla | Descripción |
|---|---|---|
| `UserAccount` | `UserAccount` | Usuario del sistema con soft delete |
| `Channel` | `Channel` | Canal de video (1 usuario → N canales) |
| `Video` | `Video` | Video publicado por un canal |
| `VideoAccessibility` | `VideoAccessibility` | Catálogo de visibilidad (público/privado/etc.) |
| `Tag` | `Tag` | Etiqueta para clasificar videos |
| `ReactionType` | `ReactionType` | Catálogo de reacciones (like/dislike) |
| `VideoReaction` | `VideoReaction` | Reacción de un usuario a un video |
| `Comment` | `Comment` | Comentario con autorreferencia (replies) |
| `Subscription` | `Subscription` | Suscripción de usuario a canal/video |
| `ViewHistory` | `ViewHistory` | Historial de reproducción con porcentaje de completitud |
| `CreatorType` | `CreatorType` | Catálogo tipo de creador de playlist |
| `Playlist` | `Playlist` | Lista de reproducción creada por usuario o canal |

Todas las entidades con `DeletedAt` soportan **borrado lógico (soft delete)**.

#### `Database/SqlServer/Context/YoutubeCloneContext.cs`

`DbContext` de Entity Framework Core. Generado por scaffolding desde la base de datos. Configura:

- `DbSet<T>` para cada entidad.
- Relaciones (`HasOne`, `WithMany`, `HasForeignKey`) con comportamiento `ClientSetNull` en cascada.
- Restricciones de unicidad e índices (handles de canales, emails y usernames).
- Tablas de unión muchos a muchos: `VideoTags` y `PlaylistVideos`.
- Cadena de conexión por defecto en `OnConfiguring` (sobreescrita en producción por `AddSqlServer` en `ServiceCollectionExtension`).

> La cadena de conexión embebida en `OnConfiguring` es solo fallback de scaffolding. En producción se inyecta desde `appsettings.json` o variable de entorno.

#### `Database/SqlServer/IUnitOfWork.cs`

Interfaz que define el patrón Unit of Work:

```csharp
public interface IUnitOfWork
{
    IUserRepository userRepository { get; }
    Task SaveChangesAsync();
}
```

Centraliza el acceso a repositorios y la confirmación de cambios.

#### `Interfaces/Repositories/`

Interfaces de repositorios de dominio.

- **`IGenericRepository<T>`**: operaciones básicas CRUD (`GetById`, `Create`, `Update`, `Queryable`).
- **`IUserRepository`**: extiende el genérico con `GetAll(email)` y `HasCreated()`.
- **`IChannelRepository`**: extiende el genérico con `GetByUserId`.
- **`IVideoRepository`**: repositorio base para videos (pendiente de implementación extendida).

#### `Exceptions/`

Excepciones de dominio propias para comunicar errores de negocio al middleware de forma semántica.

| Excepción | HTTP equivalente | Cuándo se usa |
|---|---|---|
| `NotFoundException` | 404 | Entidad no encontrada (usuario, canal, etc.) |
| `BadRequestException` | 400 | Petición inválida de negocio (credenciales incorrectas, etc.) |
| `UnauthorizedException` | 401 | Intento de acceso sin token válido |

---

### `YoutubeClone.Infraestructure`

Capa de infraestructura. Implementa los repositorios del dominio usando EF Core.

**Depende de:** `YoutubeClone.Domain`.

#### `Persistence/SqlServer/Repositories/GenericRepository.cs`

Implementación genérica de `IGenericRepository<T>`. Usa `DbSet<T>` directamente.

- `GetById(Guid id)`: busca por clave primaria con `FindAsync`.
- `Create(T entity)`: añade la entidad y la retorna.
- `Update(T entity)`: marca la entidad como modificada.
- `Queryable()`: retorna un `IQueryable<T>` para consultas ad hoc (permite filtros y paginación desde los servicios).

#### `Persistence/SqlServer/Repositories/UserRepository.cs`

Extiende `GenericRepository<UserAccount>` e implementa `IUserRepository`.

- `GetAll(string email)`: busca usuario por email (usado en login).
- `HasCreated()`: retorna `true` si ya existe al menos un usuario en la base de datos (para el seed del primer usuario).

#### `Persistence/SqlServer/Repositories/ChannelRepository.cs`

Extiende `GenericRepository<Channel>` e implementa `IChannelRepository`.

- `GetByUserId(Guid userId)`: obtiene todos los canales de un usuario dado.
- `Create(Channel channel)`: crea un canal.

#### `UnitOfWork.cs`

Implementación de `IUnitOfWork`. Recibe el `YoutubeCloneContext` por inyección de dependencias e instancia los repositorios.

```csharp
public class UnitOfWork(YoutubeCloneContext context) : IUnitOfWork
{
    public IUserRepository userRepository { get; } = new UserRepository(context);
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
```

> Centraliza el `SaveChanges` para que todos los repositorios participen en la misma transacción de EF Core.

---

### `YoutubeClone.Application`

Capa de aplicación. Contiene la lógica de negocio, las interfaces de servicios, los modelos de entrada/salida (DTOs, Requests, Responses) y helpers de aplicación.

**Depende de:** `YoutubeClone.Domain`, `YoutubeClone.Shared`.

#### `Interfaces/Services/`

Contratos de los servicios de aplicación.

| Interfaz | Descripción |
|---|---|
| `IUserService` | CRUD de usuarios + creación del primer usuario |
| `IAuthService` | Login y renovación de token |
| `ICacheService` | Abstracción sobre `IMemoryCache` |
| `IChannelService` | Creación y consulta de canales |
| `IAppService` | Información general de la API (versión, etc.) |

#### `Services/UserService.cs`

Implementa `IUserService`. Recibe `IUnitOfWork` e `IConfiguration` por constructor (primary constructor de C# 12).

| Método | Descripción |
|---|---|
| `Create(CreateUserRequest)` | Crea un usuario normalizando username (lowercase) y displayName (TitleCase). Persiste con `SaveChangesAsync`. |
| `Delete(Guid id)` | Soft delete: asigna `DeletedAt = UtcNow()` y persiste. |
| `GetAll(FilterUserRequest)` | Lista usuarios con paginación (Offset + Limit) usando `IQueryable`. |
| `GetById(Guid id)` | Busca usuario por ID. Lanza `NotFoundException` si no existe. |
| `Update(Guid id, UpdateUserRequest)` | Actualización parcial (campos nulos no se sobreescriben). |
| `CreateFirstUser()` | Crea el usuario administrador inicial si la BD está vacía. Lee credenciales desde `IConfiguration`. |

El método privado `Map(UserAccount) → UserDTO` convierte la entidad de dominio al DTO de salida. La contraseña **no** se incluye en el DTO en versiones futuras (actualmente se mapea como campo del DTO, pendiente de ocultar).

#### `Services/AuthService.cs`

Implementa `IAuthService`. Recibe `IUserRepository`, `IConfiguration` e `ICacheService`.

| Método | Descripción |
|---|---|
| `Login(LoginAuthRequest)` | Busca usuario por email, valida contraseña con `Hasher.ComparePassword`, genera access token + refresh token. Retorna ambos en `LoginAuthResponse`. |
| `Renew(RenewAuthRequest)` | Busca el refresh token en caché, genera nuevos tokens y elimina el refresh token anterior (rotación de tokens). |

#### `Services/CacheService.cs`

Implementa `ICacheService` sobre `IMemoryCache` de ASP.NET Core.

- `Create(key, expiration, value)`: guarda un objeto en caché con tiempo de expiración.
- `Get<T>(key)`: recupera y deserializa un objeto de caché.
- `Delete(key)`: elimina una entrada de caché.

#### `Services/ChannelService.cs`

Implementa `IChannelService`. CRUD básico de canales usando `IUnitOfWork`.

#### `Services/AppService.cs`

Retorna información básica de la aplicación (versión desde `appsettings.json`).

#### `Helpers/TokenHelper.cs`

Utilidad estática para creación de tokens JWT y refresh tokens.

- `Create(userId, configuration, cache)`: genera un JWT firmado con HMAC-SHA256, con claims (`USERACCOUNT_ID`), expiración aleatoria entre `Min` y `Max` minutos, y lo guarda en caché.
- `CreateRefresh(userId, configuration, cacheService)`: genera un string aleatorio (100 chars) como refresh token opaco, lo guarda en caché junto con el `UserId` y la expiración (en días).
- `Configuration(configuration)`: lee las claves JWT de configuración/variables de entorno y retorna un objeto `TokenConfiguration` con el `SymmetricSecurityKey` ya construido.

> La expiración del JWT es **aleatoria** entre `Min` y `Max` minutos (por defecto 1–5 minutos según `appsettings.json`). Esto añade entropía para mitigar ataques de predicción.

#### `Helpers/CacheHelper.cs`

Genera las claves de caché para tokens.

- `AuthTokenCreationKey(token, expiration)`: clave y expiración para el access token.
- `AuthRefreshTokenKey(token)`: clave para buscar un refresh token.
- `AuthRefreshTokenCreationKey(token, configuration)`: clave y expiración para guardar un refresh token nuevo.

#### `Helpers/ResponseHelper.cs`

Factory de respuestas uniformes `GenericResponse<T>`.

- `Create<T>(data, errors, message)`: construye la respuesta estándar de la API.

#### `Models/DTOs/UserDTO.cs`

DTO de salida para usuario: `UserId`, `UserName`, `DisplayName`, `Email`, `Birthday`, `Location`, `Password`, `CreatedAt`.

#### `Models/DTOs/ChannelDTO.cs`

DTO de salida para canal: `ChannelId`, `UserId`, `Handle`, `DisplayName`, `Verification`, `Description`, `AvatarUrl`, `BannerUrl`, `CreatedAt`.

#### `Models/Requests/User/`

Modelos de entrada con validaciones mediante Data Annotations:

| Request | Campos principales |
|---|---|
| `CreateUserRequest` | `UserName` (req, max 20), `Email` (req, email, max 255), `DisplayName` (req, max 50), `Birthday` (req), `Location` (max 30), `Password` (req) |
| `UpdateUserRequest` | `UserName`, `DisplayName`, `Email`, `Location` (todos opcionales) |
| `FilterUserRequest` | `Offset` (default 0), `Limit` (default 10) para paginación |
| `GetAllUserRequest` | Alias/extensión de `FilterUserRequest` |

#### `Models/Requests/Auth/`

| Request | Campos |
|---|---|
| `LoginAuthRequest` | `Email` (req, email), `Password` (req) |
| `RenewAuthRequest` | `RefreshToken` (req) |

#### `Models/Requests/Channel/CreateChannelRequest.cs`

Campos: `UserId` (req), `Handle` (req, max 20), `DisplayName` (req, max 50), `Description` (max 255), `AvatarUrl`, `BannerUrl`.

#### `Models/Responses/GenericResponse.cs`

Respuesta estándar de todos los endpoints:

```json
{
  "data": { ... },
  "errors": [],
  "message": "..."
}
```

#### `Models/Responses/Auth/LoginAuthResponse.cs`

```json
{
  "token": "eyJ...",
  "refreshToken": "abc123..."
}
```

#### `Models/Helpers/TokenConfiguration.cs`

Objeto de valor con: `Issuer`, `Audience`, `SecurityKey` (SymmetricSecurityKey), `Expiration` (DateTime), `ExpirationTimeSpan`.

#### `Models/Helpers/RefreshToken.cs`

Objeto almacenado en caché: `UserId` (Guid), `ExpirationInDays` (TimeSpan).

#### `Models/Helpers/CacheKey.cs`

Par `Key` (string) + `Expiration` (TimeSpan) para operaciones de caché.

#### `Models/DTOs/AppInfoDTO.cs`

DTO con información de la versión de la API: `Version`.

#### `Queries/UserFilterQuery.cs`

Clase con extensiones para construir consultas filtradas de `IQueryable<UserAccount>` (búsqueda por nombre, paginación).

---

### `YoutubeClone.WebApp`

Capa de presentación. Contiene los controllers, extensiones de registro de servicios, middlewares y el punto de entrada `Program.cs`.

**Depende de:** `YoutubeClone.Application`, `YoutubeClone.Domain`, `YoutubeClone.Infraestructure`, `YoutubeClone.Shared`.

#### `Program.cs`

Punto de entrada de la aplicación. Configura la pipeline HTTP mínima:

```csharp
builder.Host.UseSerilog();
builder.Services.AddCore(builder.Configuration); // registra todo

var app = builder.Build();
app.MapScalarApiReference();   // documentación API (solo desarrollo)
app.UseMiddleware<ErrorHandleMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

#### `Extensions/ServiceCollectionExtension.cs`

Clase estática que agrupa todos los registros de DI en métodos de extensión limpios:

| Método | Qué registra |
|---|---|
| `AddCore(configuration)` | Orquesta todos los demás. También llama a `Initialize` para crear el primer usuario. |
| `AddServices()` | `IUserService`, `IAuthService`, `ICacheService` como Scoped |
| `AddRepositories()` | `IUnitOfWork` como Scoped, `IUserRepository` como Transient |
| `AddMiddlewares()` | `ErrorHandleMiddleware` como Scoped |
| `AddLogging()` | Serilog con sink a archivo (rolling diario) y consola |
| `AddAuth(configuration)` | JWT Bearer con validación completa (issuer, audience, lifetime, clave). El evento `OnChallenge` lanza `UnauthorizedException`. |
| `AddCache()` | `IMemoryCache` |
| `Initialize()` | Construye un `ServiceProvider` temporal y llama a `CreateFirstUser()` |

> `AddCore` es `async` porque `Initialize` es asíncrono (crea el primer usuario en BD al arrancar).

#### `Middlewares/ErrorHandleMiddleware.cs`

Middleware global de manejo de errores. Intercepta excepciones y las convierte en respuestas HTTP estructuradas (`GenericResponse<string>`):

| Excepción capturada | Status HTTP | Comportamiento |
|---|---|---|
| `NotFoundException` | 404 | Retorna el mensaje de la excepción |
| `BadRequestException` | 400 | Retorna el mensaje de la excepción |
| `UnauthorizedException` | 401 | Retorna el mensaje de la excepción |
| `Exception` (cualquier otra) | 500 | Genera un `traceId` único (GUID), loguea con Serilog y retorna mensaje genérico con el traceId |

#### `Controllers/UserController.cs`

Base route: `api/users`. Requiere `[Authorize]` por defecto.

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/` | ✅ | Lista usuarios con paginación (query: `offset`, `limit`) |
| GET | `/{id}` | ✅ | Obtiene usuario por ID |
| POST | `/` | ❌ (público) | Crea un nuevo usuario |
| PUT | `/{id}` | ✅ | Actualiza usuario por ID |
| DELETE | `/{id}` | ✅ | Soft delete de usuario |

#### `Controllers/AuthController.cs`

Base route: `api/auth`.

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/login` | ❌ | Login con email y contraseña. Retorna access token + refresh token |
| POST | `/renew` | ❌ | Renueva el access token usando el refresh token |

#### `Controllers/AppController.cs`

Base route: `api/app`. Retorna información de la versión de la API.

#### `appsettings.json` — Configuración de referencia

```json
{
  "Version": "0.0.0",
  "Jwt": {
    "Audience": "http://localhost:4200",
    "Issuer": "http://localhost:5063",
    "ExpirationInMinutesMin": 1,
    "ExpirationInMinutesMax": 5
  },
  "Auth": {
    "RefreshToken": {
      "ExpirationInDays": 30
    }
  },
  "FirstAppTime": {
    "User": {
      "UserName": "User Root",
      "DisplayName": "Administrador",
      "Email": "changeme@userroot.com",
      "Password": "changeme"
    }
  }
}
```

> La `ConnectionString` y la `PrivateKey` del JWT **deben** configurarse como variables de entorno en producción para no exponerse en código fuente.

#### `logs/`

Directorio donde Serilog escribe los archivos de log con rotación diaria (`log{fecha}.txt`).

---

## Flujo de autenticación

```
Cliente                     API
  │                          │
  │── POST /api/auth/login ──►│
  │   { email, password }    │── Busca usuario por email
  │                          │── Valida contraseña (PBKDF2)
  │                          │── Genera JWT (exp aleatoria 1–5 min)
  │                          │── Genera Refresh Token (100 chars random)
  │                          │── Guarda ambos en IMemoryCache
  │◄── { token, refresh } ───│
  │                          │
  │── GET /api/users [Bearer token] ─►│
  │                          │── Valida JWT (issuer, audience, firma, expiración)
  │◄── [200 OK | 401 Unauthorized] ───│
  │                          │
  │── POST /api/auth/renew ──►│
  │   { refreshToken }       │── Busca refresh token en caché
  │                          │── Genera nuevo JWT + nuevo refresh token
  │                          │── Elimina el refresh token anterior (rotación)
  │◄── { token, refresh } ───│
```

---

## Orden recomendado para leer/explorar el código

Para entender el proyecto de cero, se recomienda este orden:

1. **`YoutubeClone.Shared`** — Entender las utilidades base (Hasher, Generate, constantes).
2. **`YoutubeClone.Domain/Entities`** — Conocer las entidades del modelo de datos.
3. **`YoutubeClone.Domain/Interfaces`** — Entender los contratos de repositorio y Unit of Work.
4. **`YoutubeClone.Domain/Context`** — Ver cómo EF Core mapea las entidades.
5. **`YoutubeClone.Infraestructure`** — Ver las implementaciones concretas de repositorios.
6. **`YoutubeClone.Application/Models`** — Entender los DTOs, Requests y Responses.
7. **`YoutubeClone.Application/Helpers`** — TokenHelper, CacheHelper, ResponseHelper.
8. **`YoutubeClone.Application/Services`** — La lógica de negocio (UserService, AuthService).
9. **`YoutubeClone.WebApp/Extensions`** — Cómo se registra todo en DI.
10. **`YoutubeClone.WebApp/Middlewares`** — El manejo global de errores.
11. **`YoutubeClone.WebApp/Controllers`** — Los endpoints expuestos.
12. **`YoutubeClone.WebApp/Program.cs`** — El punto de entrada y la pipeline.

---

## Notas de desarrollo

- El proyecto usa **primary constructors** de C# 12 en los servicios y repositorios (e.g., `public class UserService(IUnitOfWork uow, IConfiguration configuration)`).
- Las validaciones de unicidad de email y username **no están implementadas en el servicio** (el código está comentado); actualmente la validación la hace la base de datos (índice `UNIQUE`) y el error se propaga como excepción no controlada → status 500. Está pendiente implementarlo correctamente.
- La contraseña se incluye en el `UserDTO` actualmente; en una versión productiva debería omitirse.
- El `ChannelService` y repositorios de canales están parcialmente implementados (creación y consulta básica).
- El `IVideoRepository` está definido pero sin métodos adicionales más allá del genérico.
