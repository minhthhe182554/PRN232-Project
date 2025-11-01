using System.Text.Json;

namespace HRM_Client.Utils
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
