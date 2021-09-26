using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Vit.Core.Util.ComponentModel.Data;

namespace Vit.Http.ChunkUpload
{
    public class UploadChunk_TempFile: UploadChunk
    {

        public Func<string, ChunkData, string> BuildTempFilePath = (string tempDirPath, ChunkData chunkData) => Path.Combine(tempDirPath, chunkData.fileGuid + ".tmp");

        public string tempDirPath;

        public class UploadedFile
        {
            public string fileGuid;

            public long fileSize;
            public long uploadedSize = 0;

            public List<byte[]> content;
        }


        /// <summary>
        /// async(tempFilePath, fileName, httpContext)=>{  return (ApiReturn)true;   }
        /// </summary>
        public Func<string, string, HttpContext, Task<ApiReturn>> OnUploadedFile;



        void Destroy(ChunkData chunkData) 
        {
            var tempFile = Path.Combine(tempDirPath, chunkData.fileGuid + ".tmp");
            var file = new FileInfo(tempFile);
            if (file.Exists) 
            {
                file.Delete();
            }
        }


     
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public override async Task<object> OnUploadChunkAsync(HttpContext httpContext, ChunkData chunkData)
        {
            var tempFilePath = BuildTempFilePath(tempDirPath, chunkData);
            var file = new FileInfo(tempFilePath);

            var fileLength = file.Exists ? file.Length : 0;

            if (chunkData.startIndex != fileLength)
            {
                Destroy(chunkData);
                return (ApiReturn)false;
            }   

            using (var stream = file.OpenWrite())
            { 
                stream.Seek(0, SeekOrigin.End);
                //await stream.WriteAsync(chunkData.data, 0, chunkData.data.Length);
                stream.Write(chunkData.data,0, chunkData.data.Length);
            }

            #region (x.3) 文件上传完成           
            if (new FileInfo(tempFilePath).Length >= chunkData.fileSize)
            {
                var apiRet = await OnUploadedFile?.Invoke(tempFilePath, chunkData.FileName, httpContext);
                return apiRet;
            }
            #endregion


            return (ApiReturn)true;
        }

    }
}
