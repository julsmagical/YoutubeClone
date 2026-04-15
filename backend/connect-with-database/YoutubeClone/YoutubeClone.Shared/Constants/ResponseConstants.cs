namespace YoutubeClone.Shared.Constants
{
    public static class ResponseConstants
    {
        public const string USER_NOT_EXIST = "El usuario no existe";

        public const string CHANNEL_NOT_EXIST = "El canal no existe";

        // Auth
        public const string AUTH_TOKEN_NOT_FOUND = "El token no es correcto, expiró o no se argumentó";
        public const string AUTH_USER_OR_PASSWORD_NOT_FOUND = "Usuario o contraseña incorrectos";
        public const string AUTH_REFRESH_TOKEN_NOT_FOUND = "El token para refrescar la sesión expiró, no existe o es incorrecto";

        public static string ERROR_UNEXPECTED(string traceId)
        {
            return $"Ha ocurrido un error inesperado. Contacte con soporte, mencionando el siguiente código de error: {traceId}";
        }

        public static string ConfigurationPropertyNotFound(string property)
        {
            return $"Falta la propiedad '{property}' por establecer en la configuración del aplicativo.";
        }
    }
}
