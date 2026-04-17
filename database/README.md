# Base de datos — YoutubeClone

Este directorio contiene todos los scripts SQL necesarios para crear, poblar y consultar la base de datos del proyecto YoutubeClone. El motor utilizado es **SQL Server**.

---

## Archivos

| Archivo | Propósito |
|---|---|
| `ddl.sql` | DDL (Data Definition Language): crea la base de datos y todas las tablas. |
| `dml.sql` | DML (Data Manipulation Language): inserta datos iniciales y de prueba (seed data). |
| `queries.sql` | Consultas SQL de referencia y ejemplo para explorar los datos. |

---

## Orden de ejecución

Siempre ejecutar en este orden:

```
1. ddl.sql
2. dml.sql
3. queries.sql  (opcional, solo para consultas)
```

---

## Modelo de datos

### Diagrama de entidades

```
UserAccount ──< Channel ──< Video >── VideoTags >── Tag
     │                │
     │                └──< Playlist >── PlaylistVideos >── Video
     │
     ├──< Subscription >── Channel
     ├──< ViewHistory >── Video
     ├──< VideoReaction >── Video >── ReactionType
     └──< Comment (autorreferencia: replies)
                └── Video

VideoAccessibility ──< Video
CreatorType ──< Playlist
```

### Tablas

#### `UserAccount`
Almacena los usuarios del sistema.

| Columna | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `UserID` | `UNIQUEIDENTIFIER` | PK, DEFAULT NEWID() | Identificador único del usuario |
| `UserName` | `NVARCHAR(20)` | NOT NULL, UNIQUE | Nombre de usuario único |
| `Email` | `NVARCHAR(255)` | NOT NULL, UNIQUE | Correo electrónico único |
| `DisplayName` | `NVARCHAR(50)` | NOT NULL | Nombre visible |
| `Birthday` | `DATETIME2` | NOT NULL | Fecha de nacimiento |
| `Location` | `NVARCHAR(30)` | NULL | Ubicación opcional |
| `Password` | `NVARCHAR(255)` | NOT NULL | Contraseña hasheada (PBKDF2) |
| `CreatedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() | Fecha de creación |
| `UpdatedAt` | `DATETIME2` | NULL | Fecha de última actualización |
| `DeletedAt` | `DATETIME2` | NULL | Soft delete: si no es NULL, el usuario está eliminado |

#### `Channel`
Canales de video asociados a un usuario.

| Columna | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `ChannelID` | `UNIQUEIDENTIFIER` | PK, DEFAULT NEWID() | Identificador del canal |
| `UserID` | `UNIQUEIDENTIFIER` | NOT NULL, FK → UserAccount | Propietario del canal |
| `Handle` | `NVARCHAR(20)` | NOT NULL, UNIQUE | @handle único del canal |
| `DisplayName` | `NVARCHAR(50)` | NOT NULL | Nombre visible del canal |
| `Verification` | `BIT` | NOT NULL, DEFAULT 0 | Si el canal está verificado |
| `Description` | `NVARCHAR(255)` | NULL | Descripción del canal |
| `AvatarURL` | `NVARCHAR(255)` | NULL | URL de imagen de perfil |
| `BannerURL` | `NVARCHAR(255)` | NULL | URL de banner |
| `CreatedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() | Fecha de creación |
| `UpdatedAt` | `DATETIME2` | NULL | Última actualización |
| `DeletedAt` | `DATETIME2` | NULL | Soft delete |

#### `VideoAccessibility`
Catálogo de niveles de accesibilidad para videos (e.g., Público, Privado, No listado).

| Columna | Tipo | Restricciones |
|---|---|---|
| `VideoAccessibilityID` | `INT IDENTITY(1,1)` | PK |
| `DisplayName` | `NVARCHAR(30)` | NOT NULL |
| `CreatedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() |

#### `Video`
Videos publicados por canales.

| Columna | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `VideoID` | `UNIQUEIDENTIFIER` | PK, DEFAULT NEWID() | Identificador del video |
| `ChannelID` | `UNIQUEIDENTIFIER` | NOT NULL, FK → Channel | Canal que publicó el video |
| `VideoAccessibilityID` | `INT` | NOT NULL, FK → VideoAccessibility | Nivel de acceso |
| `Title` | `NVARCHAR(255)` | NOT NULL | Título del video |
| `Description` | `NVARCHAR(255)` | NULL | Descripción |
| `DurationSeconds` | `INT` | NOT NULL | Duración en segundos |
| `ThumbnailURL` | `NVARCHAR(255)` | NOT NULL | URL de la miniatura |
| `AgeRestriction` | `BIT` | NOT NULL, DEFAULT 0 | Restricción de edad |
| `PublishedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() | Fecha de publicación |
| `CreatedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() | Fecha de creación del registro |
| `UpdatedAt` | `DATETIME2` | NULL | Última actualización |
| `DeletedAt` | `DATETIME2` | NULL | Soft delete |

#### `Subscription`
Suscripciones de usuarios a canales (opcionalmente a un video específico).

| Columna | Tipo | Restricciones |
|---|---|---|
| `SubscriptionID` | `UNIQUEIDENTIFIER` | PK |
| `UserID` | `UNIQUEIDENTIFIER` | FK → UserAccount |
| `ChannelID` | `UNIQUEIDENTIFIER` | FK → Channel |
| `VideoID` | `UNIQUEIDENTIFIER` | NULL, FK → Video |
| `CreatedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() |
| `DeletedAt` | `DATETIME2` | NULL |

#### `ViewHistory`
Historial de visualización de videos por usuario.

| Columna | Tipo | Descripción |
|---|---|---|
| `ViewHistoryID` | `UNIQUEIDENTIFIER` | PK |
| `UserID` | `UNIQUEIDENTIFIER` | FK → UserAccount |
| `VideoID` | `UNIQUEIDENTIFIER` | FK → Video |
| `CompletionRate` | `DECIMAL` | Porcentaje de video visto (0.0 – 1.0 o porcentaje) |
| `CreatedAt` | `DATETIME2` | DEFAULT SYSUTCDATETIME() |

#### `Tag`
Etiquetas asociables a videos.

| Columna | Tipo | Restricciones |
|---|---|---|
| `TagID` | `UNIQUEIDENTIFIER` | PK |
| `DisplayName` | `NVARCHAR(20)` | NOT NULL |

#### `VideoTags` _(tabla de unión)_
Relación muchos a muchos entre `Video` y `Tag`.

| Columna | Tipo |
|---|---|
| `VideoID` | `UNIQUEIDENTIFIER` (PK compuesta) |
| `TagID` | `UNIQUEIDENTIFIER` (PK compuesta) |

#### `ReactionType`
Catálogo de tipos de reacción (e.g., Like, Dislike).

| Columna | Tipo |
|---|---|
| `ReactionTypeID` | `INT IDENTITY(1,1)` PK |
| `DisplayName` | `NVARCHAR(20)` NOT NULL |
| `CreatedAt` | `DATETIME2` |

#### `VideoReaction`
Reacciones de usuarios a videos.

| Columna | Tipo |
|---|---|
| `VideoReactionID` | `UNIQUEIDENTIFIER` PK |
| `VideoID` | `UNIQUEIDENTIFIER` FK → Video |
| `UserID` | `UNIQUEIDENTIFIER` FK → UserAccount |
| `ReactionTypeID` | `INT` FK → ReactionType |
| `CreatedAt` | `DATETIME2` |

#### `Comment`
Comentarios en videos, con soporte de respuestas anidadas (autorreferencia).

| Columna | Tipo | Descripción |
|---|---|---|
| `CommentID` | `UNIQUEIDENTIFIER` | PK |
| `VideoID` | `UNIQUEIDENTIFIER` | FK → Video |
| `UserID` | `UNIQUEIDENTIFIER` | FK → UserAccount |
| `Content` | `NVARCHAR(255)` | Contenido del comentario |
| `IsPinned` | `BIT` | Si el comentario está fijado |
| `ParentCommentID` | `UNIQUEIDENTIFIER` | NULL: comentario raíz. Con valor: es respuesta al padre |
| `CreatedAt` | `DATETIME2` | |
| `UpdatedAt` | `DATETIME2` | NULL |
| `DeletedAt` | `DATETIME2` | NULL (soft delete) |

#### `CreatorType`
Catálogo de tipos de creador de playlist (e.g., Usuario, Canal).

| Columna | Tipo |
|---|---|
| `CreatorTypeID` | `INT IDENTITY(1,1)` PK |
| `DisplayName` | `NVARCHAR(30)` NOT NULL |

#### `Playlist`
Listas de reproducción, pueden ser creadas por un usuario o por un canal.

| Columna | Tipo | Descripción |
|---|---|---|
| `PlaylistID` | `UNIQUEIDENTIFIER` | PK |
| `CreatorTypeID` | `INT` | FK → CreatorType |
| `UserID` | `UNIQUEIDENTIFIER` | FK → UserAccount |
| `ChannelID` | `UNIQUEIDENTIFIER` | NULL, FK → Channel |
| `CreatedAt` | `DATETIME2` | |
| `UpdatedAt` | `DATETIME2` | NULL |
| `DeletedAt` | `DATETIME2` | NULL (soft delete) |

#### `PlaylistVideos` _(tabla de unión)_
Relación muchos a muchos entre `Playlist` y `Video`.

| Columna | Tipo |
|---|---|
| `PlaylistID` | `UNIQUEIDENTIFIER` (PK compuesta) |
| `VideoID` | `UNIQUEIDENTIFIER` (PK compuesta) |

---

## Convenciones

- **Claves primarias:** `UNIQUEIDENTIFIER` con `DEFAULT NEWID()` para la mayoría de entidades. Las tablas de catálogo usan `INT IDENTITY(1,1)`.
- **Soft delete:** Las entidades principales (usuarios, canales, videos, comentarios, playlists) no se eliminan físicamente; se marca la columna `DeletedAt` con la fecha de eliminación.
- **Timestamps UTC:** Se usa `SYSUTCDATETIME()` como valor por defecto en `CreatedAt` para garantizar consistencia horaria.
- **Unicidad:** `UserName` y `Email` en `UserAccount` tienen índice `UNIQUE`. `Handle` en `Channel` también es único.
