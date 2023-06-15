using System.Net.Http;

namespace WebDav
{
    internal static class WebDavMethod
    {
        public static readonly HttpMethod Propfind = new HttpMethod("PROPFIND");

        public static readonly HttpMethod Proppatch = new HttpMethod("PROPPATCH");

        public static readonly HttpMethod Mkcalendar = new HttpMethod("MKCALENDAR");

        public static readonly HttpMethod Mkcol = new HttpMethod("MKCOL");

        public static readonly HttpMethod Report = new HttpMethod("REPORT");

        public static readonly HttpMethod Copy = new HttpMethod("COPY");

        public static readonly HttpMethod Move = new HttpMethod("MOVE");

        public static readonly HttpMethod Lock = new HttpMethod("LOCK");

        public static readonly HttpMethod Unlock = new HttpMethod("UNLOCK");
    }
}
