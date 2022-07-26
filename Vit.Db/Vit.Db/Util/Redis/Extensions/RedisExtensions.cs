using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vit.Extensions.Redis
{
    /// <summary>
    /// 
    ///         //demo:
    ///         
    ///         string connection = Appsettings.json.GetStringByPath("ConnectionStrings.Redis");
    ///         using (var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(connection))
    ///         {
    ///             var db = redis.GetDatabase(1);
    /// 
    ///             db.KeyDeleteAsync("a:b");
    ///         }
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public static partial class RedisExtensions
    {

       
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Redis_BuildKey(this IEnumerable<string> keys)
        {
            return String.Join(":", keys);
        }





        #region 对象获取


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(this IDatabase db,object value, TimeSpan? expiry, params string[] keys)
        {
            return db.StringSet(keys.Redis_BuildKey(),value.Serialize(), expiry);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(this IDatabase db, object value, params string[] keys)
        {
            return db.Set(value, (TimeSpan?)null, keys);
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(this IDatabase db, object value, DateTime? expiry = null, params string[] keys)
        {
            return db.Set(value, null == expiry ? null : (expiry - DateTime.Now), keys);
        }





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(this IDatabase db, params string[] keys)
        {
            string str = db.StringGet(keys.Redis_BuildKey());
            return str.Deserialize<T>();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(this IDatabase db, TimeSpan? expiry, params string[] keys)
        {
            RedisKey redisKey = keys.Redis_BuildKey();
            string str = db.StringGet(redisKey);

            if (str == null)
            {
                return default;
            }

            db.KeyExpireAsync(redisKey, expiry);
            return str.Deserialize<T>();
        }

        #endregion








    }
}
