using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.IO;

using Vit.Core.Util.ComponentModel.Data;
using Vit.Extensions;
using Vit.Http.ChunkUpload;

using static Vit.Http.ChunkUpload.UploadChunk;

namespace Vit.OnlineUpgrade.Netcore.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseStaticFiles();


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




            app.UseMvc();
        }
    }
}
