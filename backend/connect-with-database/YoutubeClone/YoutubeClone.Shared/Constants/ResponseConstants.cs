namespace YoutubeClone.Shared.Constants
{
    public static class ResponseConstants
    {
        public const string USER_NOT_EXIST = "El usuario no existe";

        public const string CHANNEL_NOT_EXIST = "El canal no existe";
        public static string ERROR_UNEXPECTED(string traceId)
        {
            return $"Ha ocurrido un error inesperado. Contacte con soporte, mencionando el siguiente codigo de error> {traceId}";
        }
    }
}
