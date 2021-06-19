using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

namespace Vit.Http.ChunkUpload
{
    class MsCache: Microsoft.Extensions.Caching.Memory.MemoryCache
    {
        public MsCache():this(new MemoryCacheOptions())
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


        public static readonly MsCache Instance = new MsCache();
 
    }
}
