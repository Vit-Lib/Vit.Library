using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Vit.Core.Util.ComponentModel.Data;
using Vit.Extensions;
using Vit.Http.ChunkUpload;

using static Vit.Http.ChunkUpload.UploadChunk;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.AllowAnyOrigin().UseUrlsFromConfig();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseStaticFiles();

            #region UseChunkUpload
            {
                app.UseChunkUpload("/upload/uploadToMemory", (fileContent, fileName, content) =>
                {
                    string filePath = @"\file" + fileName;
                    ApiReturn apiRet = (ApiReturn<string>)("/file/" + fileName);
                    return apiRet;
                });

                var tempDirPath = Vit.Core.Util.Common.CommonHelp.GetAbsPath("temp");
                Directory.CreateDirectory(tempDirPath);
                app.UseChunkUpload(new UploadChunk_TempFile
                {
                    apiRoute = "/upload/uploadToTempDir",
                    tempDirPath = tempDirPath,
                    BuildTempFilePath = (string tempDirPath_, ChunkData chunkData) => Path.Combine(tempDirPath_, chunkData.FileName),
                    OnUploadedFile = async (tempFilePath, fileName, httpContext) =>
                    {
                        //File.Delete(tempFilePath);
                        ApiReturn apiRet = (ApiReturn<string>)(tempFilePath);
                        return apiRet;
                    }

                });
            }
            #endregion

            //app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

    }
}
