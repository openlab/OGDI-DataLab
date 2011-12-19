using System;
using System.Text;

namespace Ogdi.Data.DataLoader
{
    public static class ExceptionHelper
    {
        public static string GetMessageStack(Exception exception)
        {
            var builder = new StringBuilder();

            for (var ex = exception; ex != null; ex = ex.InnerException)
            {
                if (builder.Length > 0)
                    builder.AppendLine(">>>");

                builder.AppendLine(ex.Message);
            }
            return builder.ToString();
        }
    }
}
