using System.Net;
using System.Web;

namespace Ogdi.DataServices
{
    public static class IHttpHandlerExtensionMethods
    {
        public static bool IsHttpGet(this IHttpHandler handler, HttpContext context)
        {
            return context.Request.HttpMethod == "GET";
        }

        public static void RespondMethodNotAllowed(this IHttpHandler handler, HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            context.Response.End();
        }

        public static void RespondNotFound(this IHttpHandler handler, HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.End();
        }

        public static void RespondForbidden(this IHttpHandler handler, HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.End();
        }
    }
}
