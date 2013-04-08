using System.Net;
using System.Web;
using System.Xml.Linq;

namespace Ogdi.DataServices
{
    public static class IHttpHandlerExtensionMethods
    {
        public static bool IsHttpGet(this IHttpHandler handler, HttpContext context)
        {
            return context.Request.HttpMethod.Equals("GET");
        }

        public static void RespondBadRequest(this IHttpHandler handler, HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.End();
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
