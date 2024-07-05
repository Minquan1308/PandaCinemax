using System;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DoAnCoSoTL.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
            // Đặt thời gian tồn tại cho key trong Session là 30 phút
            session.SetInt32(key + "_Expiry", (int)DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds());
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            var expiryTime = session.GetInt32(key + "_Expiry");

            if (value == null || expiryTime == null || DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiryTime)
            {
                // Xóa key và key_Expiry nếu đã hết hạn hoặc không tồn tại
                session.Remove(key);
                session.Remove(key + "_Expiry");
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }
    }
}
