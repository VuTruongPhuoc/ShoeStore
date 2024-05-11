using ShoeStore.Models;
using ShoeStore.ViewModels;
using System.Text.Json;

namespace ShoeStore.Helpers
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

		public static List<ShoppingCartItem> GetObjFromSession(ISession session, string key)
{

			var value = session.GetString(key);
			if (value != null)
			{
				var listObj = JsonSerializer.Deserialize<List<ShoppingCartItem>>(value);
				return listObj;
			}
			else return new List<ShoppingCartItem>();
		}
	}
}
