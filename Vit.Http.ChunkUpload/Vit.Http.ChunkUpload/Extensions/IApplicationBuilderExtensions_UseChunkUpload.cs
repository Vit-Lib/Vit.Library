using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Http.ChunkUpload;

namespace Vit.Extensions
{

    public static partial class IApplicationBuilderExtensions_UseChunkUpload
    {

        #region UseChunkUpload

        public static IApplicationBuilder UseChunkUpload(this IApplicationBuilder data, string apiRoute,
            Func<List<byte[]>, string, HttpContext, ApiReturn> onUploadedFile)
        {
            return UseChunkUpload(data, new UploadChunk_MsCache { apiRoute = apiRoute, onUploadedFile = onUploadedFile });
        }



        public static IApplicationBuilder UseChunkUpload(this IApplicationBuilder data, UploadChunk uploadChunk)
        {
            if (data == null)
            {
                return data;
            }

            data.Map(uploadChunk.apiRoute, (app) =>
            {
                app.Run(uploadChunk.RunAsync);
            });

            return data;
        }
        #endregion










    }
}
