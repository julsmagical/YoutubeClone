namespace YoutubeClone.WebApp.Middlewares
{
    public class ErrorHandleMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
