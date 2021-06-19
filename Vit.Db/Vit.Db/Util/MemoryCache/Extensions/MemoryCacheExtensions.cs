using Microsoft.Extensions.Caching.Memory;
using System;
using System.Runtime.CompilerServices;

namespace Vit.Extensions
{
    public static partial class MemoryCacheExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TItem SetWithSlidingExpiration<TItem>(this Microsoft.Extensions.Caching.Memory.MemoryCache memoryCache,object key, TItem value, TimeSpan offset)
        {
            using (ICacheEntry cacheEntry = memoryCache.CreateEntry(key))
            {
                //cacheEntry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                cacheEntry.SetSlidingExpiration(offset);
                cacheEntry.Value = value;
            }
            return value;
        }


    }
}
