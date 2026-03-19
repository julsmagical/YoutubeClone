USE YoutubeClone;
GO

-- Videos de un canal ordenados por visualizaciones

-- Canales con más suscriptores
SELECT 
	c.DisplayName As ChannelName,
	COUNT(*) AS SubscriptionCount
FROM Subscription s
INNER JOIN Channel c
	ON s.ChannelID = c.ChannelID
GROUP BY
	c.DisplayName
ORDER BY
	SubscriptionCount DESC
GO


-- Lista de todos los usuarios
SELECT
	DisplayName,
	UserID
FROM UserAccount
GO

-- Lista de todos las visualizaciones con porcentaje de visto
SELECT
	v.Title,
	ua.DisplayName,
	CompletionRate
FROM ViewHistory vh
INNER JOIN Video v
	ON vh.VideoID = v.VideoID
INNER JOIN UserAccount ua
	ON vh.UserID = ua.UserID
ORDER BY ua.DisplayName
GO

-- Videos completados por un usuario (porcentaje >= 90%)
DECLARE @User UNIQUEIDENTIFIER = 'C9237A7B-9A1D-4515-B4F7-A4B5B707DA91';

SELECT
	v.Title,
	ua.DisplayName,
	CompletionRate
FROM ViewHistory vh
INNER JOIN Video v
	ON vh.VideoID = v.VideoID
INNER JOIN UserAccount ua
	ON vh.UserID = ua.UserID
WHERE vh.CompletionRate>=90.0 AND vh.UserID = @USER
ORDER BY vh.CompletionRate
GO

-- Todos los comentarios con nombre del autor, fecha, y video donde se realizaron
SELECT
	v.Title,
	v.VideoID,
	c.Content,
	ua.DisplayName,
	c.CreatedAt
FROM Comment c
INNER JOIN Video v
	ON c.VideoID = v.VideoID
INNER JOIN UserAccount ua
	ON c.UserID = ua.UserID
GO

-- Comentarios de un video con el nombre del autor y su fecha
DECLARE @Video UNIQUEIDENTIFIER = '06C251F7-C35D-4351-ABF4-5BA5EA5A2553';
DECLARE @User NVARCHAR(30) = 'Neider Vélez';
DECLARE @Date DATETIME2 = '2026-03-18';

SELECT
	v.Title,
	c.Content,
	ua.DisplayName,
	c.CreatedAt
FROM Comment c
INNER JOIN Video v
	ON c.VideoID = v.VideoID
INNER JOIN UserAccount ua
	ON c.UserID = ua.UserID
WHERE
	v.VideoID = @Video AND ua.DisplayName = @User AND CAST(c.CreatedAt AS DATE) = @Date
GO

-- Videos con más likes que dislikes
SELECT
	v.Title,
	SUM(CASE WHEN ReactionTypeID = 1 THEN 1 ELSE 0 END) AS Likes,
    SUM(CASE WHEN ReactionTypeID = 2 THEN 1 ELSE 0 END) AS Dislikes
FROM VideoReaction vr
INNER JOIN Video v
	ON vr.VideoID = v.VideoID
GROUP BY v.Title
HAVING
	-- No pude usar los alias porque el SELECT se ejecuta después :(
	SUM(CASE WHEN ReactionTypeID = 1 THEN 1 ELSE 0 END) > SUM(CASE WHEN ReactionTypeID = 2 THEN 1 ELSE 0 END)
GO

-- Suscripciones de un usuario con el último video publicado de cada canal

-- Top 3 etiquetas con más videos

-- Usuarios que no han visto ningún video en los últimos 30 días

-- Duración total en horas del contenido de un canal

-- Videos de canales suscritos ordenados por fecha para el feed de un usuario

