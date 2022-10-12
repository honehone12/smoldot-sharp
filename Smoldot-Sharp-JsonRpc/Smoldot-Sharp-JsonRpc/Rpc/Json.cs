using Newtonsoft.Json;

namespace SmoldotSharp.JsonRpc
{
    public static class Json
    {
        public static T? Deserialize<T>(this string json) where T : JsonRpcFormat
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize<T>(this T format) where T : JsonRpcFormat
        {
            return JsonConvert.SerializeObject(format);
        }

        public static string GetJson(string methodName, uint id)
        {
            var fmt = new RequestFormat(methodName, id);
            return fmt.Serialize();
        }

        public static string GetJsonWithParams<T>(string methodName, uint id, params T[] param)
        {
            var fmt = new RequestFormatWithParams<T>(methodName, id, param);
            return fmt.Serialize();
        }
    }
}
