using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vit.Core.Util.ComponentModel.Data;

namespace Vit.Http.ChunkUpload
{
    public class UploadChunk_MsCache: UploadChunk
    {
        public class UploadedFile
        {
            public string fileGuid;

            public long fileSize;
            public long uploadedSize = 0;

            public List<byte[]> content;
        }

        public Func<List<byte[]>, string, HttpContext, ApiReturn> onUploadedFile;



        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override async Task<object> OnUploadChunkAsync(HttpContext httpContext, ChunkData chunkData)
        {           

            #region (x.1)get file cache in server

            UploadedFile uploadedFile = null;
            if (MsCache.Instance.TryGetValue(chunkData.fileGuid, out var temp))
            {
                uploadedFile = temp as UploadedFile;
            }

            if (uploadedFile == null)
            {
                if (chunkData.startIndex != 0)
                {
                    MsCache.Instance.Remove(chunkData.fileGuid);
                    return (ApiReturn)false;
                }

                uploadedFile = new UploadedFile();

                uploadedFile.fileGuid = chunkData.fileGuid;
                uploadedFile.fileSize = chunkData.fileSize;
                uploadedFile.content = new List<byte[]>();
                MsCache.Instance.SetWithSlidingExpiration(chunkData.fileGuid, uploadedFile, expireTime);
            }
            #endregion


            #region (x.2)save chunk
            if (uploadedFile.uploadedSize != chunkData.startIndex)
            {
                MsCache.Instance.Remove(chunkData.fileGuid);
                return (ApiReturn)false;
            }

            uploadedFile.content.Add(chunkData.data);

            uploadedFile.uploadedSize += chunkData.data.Length;
            #endregion


            #region (x.3) 文件上传完成           
            if (uploadedFile.fileSize >= uploadedFile.uploadedSize)
            {
                MsCache.Instance.Remove(chunkData.fileGuid);

                var apiRet = onUploadedFile?.Invoke(uploadedFile.content, chunkData.FileName, httpContext);
                return apiRet;
            }
            #endregion


            return (ApiReturn)true;
        }

    }
}
