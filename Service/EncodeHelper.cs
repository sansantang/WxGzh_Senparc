using System.Web;

namespace Service
{
    public static class EncodeHelper
    {
        public static string EncodeUrl(string url)
        {
            return HttpContext.Current.Server.UrlEncode(url);
        }
    }
}