using System;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Vit.Http.ChunkUpload
{
    public class MsCache : Microsoft.Extensions.Caching.Memory.MemoryCache
    {
        public MsCache() : this(new MemoryCacheOptions())
        {
        }

        public MsCache(IOptions<MemoryCacheOptions> optionsAccessor) : base(optionsAccessor)
        {

        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public TItem SetWithSlidingExpiration<TItem>(object key, TItem value, TimeSpan offset)
        {

            using (ICacheEntry cacheEntry = CreateEntry(key))
            {
                //cacheEntry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                cacheEntry.SetSlidingExpiration(offset);
                cacheEntry.Value = value;
            }
            return value;
        }


        public static MsCache Instance = new MsCache();

    }
}
