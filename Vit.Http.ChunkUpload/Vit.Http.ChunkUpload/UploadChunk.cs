using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Vit.Core.Module.Serialization;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.SsError;
using Vit.Extensions;

namespace Vit.Http.ChunkUpload
{
    public abstract class UploadChunk
    {  


        /// <summary>
        /// 
        /// </summary>
        public string apiRoute;


        /// <summary>
        /// 文件块间距过期时间（默认10分钟）
        /// </summary>
        public TimeSpan expireTime = TimeSpan.FromMinutes(10);




        public class ChunkData
        {
            public byte[] data;
            public string fileGuid;
            public long startIndex;
            public long fileSize;
            public string FileName;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task<ChunkData> GetChunkDataAsync(HttpContext context)
        {
            var chunkData = new ChunkData();

            chunkData.fileGuid = context.Request.Form["_ChunkUpload_fileGuid"];
            if (string.IsNullOrEmpty(chunkData.fileGuid)) return null;


            IList<IFormFile> files = context.Request.Form.Files.ToList();

            chunkData.startIndex = Int64.Parse(context.Request.Form["_ChunkUpload_startIndex"].ToString());
            chunkData.fileSize = Int64.Parse(context.Request.Form["_ChunkUpload_fileSize"].ToString());


            if (files.Count != 1) return chunkData;


            #region get file content
            using (var ms = files[0].OpenReadStream())
            {
                chunkData.data = await ms.ToBytesAsync();
            }

            chunkData.FileName = files[0].FileName;

            #endregion

            return chunkData;

        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task RunAsync(HttpContext context)
        {
            object apiRet;
            try
            {
                var chunkData = await GetChunkDataAsync(context);

                if (chunkData != null)
                {
                    apiRet = await OnUploadChunkAsync(context, chunkData);
                }
                else 
                {
                    apiRet = (ApiReturn)SsError.Err_InvalidParam;
                }
            }
            catch (Exception ex)
            {
                ApiReturn ret = (SsError)ex;
                apiRet = ret;
            }
            if (apiRet != null)
            {
                string result = Json.Serialize(apiRet);
                context.Response.ContentType = "text/json; charset=utf-8";
                await context.Response.WriteAsync(result, Encoding.UTF8);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Task<object> OnUploadChunkAsync(HttpContext httpContext, ChunkData chunkData);        

    }
}
