using Microsoft.AspNetCore.Mvc;

namespace YoutubeClone.WebApp.Helpers
{
    public class ResponseStatus
    {
        public static T Ok<T>(HttpContext context, T data)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return data;
        }

        public static T Created<T>(HttpContext context, T data)
        {
            context.Response.StatusCode = StatusCodes.Status201Created;
            return data;
        }

        public static IActionResult Updated<T>(HttpContext context, T data) //Cambio por IActionResult
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return new JsonResult(data);
        }
    }
}
