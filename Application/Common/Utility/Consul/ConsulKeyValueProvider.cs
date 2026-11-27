// using System.Text;
// using Consul;
// using Newtonsoft.Json;
//
// namespace Application.Common.Utility.Consul;
//
// public class ConsulTool
// {
//     public static async Task<T?> GetValueAsync<T>(string key, IConsulClient client)
//     {
//         var getPair = await client.KV.Get(key);
//
//         if (getPair?.Response == null) return default;
//
//         var value = Encoding.UTF8.GetString(getPair.Response.Value, 0, getPair.Response.Value.Length);
//
//         return JsonConvert.DeserializeObject<T>(value);
//     }
//
//     public static async Task<bool> SetValueAsync<T>(string key, T entity, IConsulClient client)
//     {
//         var json = JsonConvert.SerializeObject(entity);
//
//         var kvPair = new KVPair(key)
//         {
//             Value = Encoding.UTF8.GetBytes(json)
//         };
//
//
//         var result = client.KV.Put(kvPair).Result;
//
//         return true;
//     }
// }