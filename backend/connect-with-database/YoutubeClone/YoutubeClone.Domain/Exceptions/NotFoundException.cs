namespace YoutubeClone.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        //puede haber un constructor por defecto
        public NotFoundException(string message) : base(message) { }
    }
}
