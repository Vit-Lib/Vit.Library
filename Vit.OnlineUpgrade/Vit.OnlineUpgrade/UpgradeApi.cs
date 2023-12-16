using System;
using System.IO;
using Vit.Net.Http.FormFile;
using Vit.Extensions;
using Vit.Core.Module.Log;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.SsError;
using Vit.Extensions.Json_Extensions;
using Vit.Extensions.Object_Serialize_Extensions;

namespace Vit.OnlineUpgrade
{
    public class UpgradeApi
    {

        #region GetHtml

        /// <summary>
        /// charset=utf-8
        /// </summary>
        /// <param name="apiPrefix"></param>
        /// <param name="password"></param>
        /// <param name="appVersion"></param>
        /// <returns></returns>
        public static string GetHtml(string apiPrefix, string password, string appVersion)
        {
            var html = $@"
<html lang=""zh"">
<head>
    <meta charset=""utf-8""/>
    <title>upgrade</title>
</head>
<body>  
    <p>在线升级</p>   <br/>    
    appVersion: {appVersion}
    <form method='post' enctype='multipart/form-data' action='{apiPrefix}/upgrade' target='_blank'>
        <div>          
            password:<input type='password' name='password' />   <br/>  
            upgrade file(zip):<input type='file' name='files' />  <br/>  
            升级前停止App:<input type='checkbox' name='stopBeforeUpgrade' checked='checked' />   <br/> 
            升级后执行的脚本:<input type='text' name='cmdAfterUpgrade' />   <br/>   <br/>
        </div>       
        <div>
            <input type='submit' value='升级' />
        </div>
    </form>
    <div style=''>
            注：请在发布后的目录内选择要升级的文件进行压缩，升级前会强制关闭软件。
    </div>
</body>
</html>

";
            return html;
        }
        #endregion


        #region Upgrate

        public static string Upgrate(Stream Body, string ContentType, Action actionToStop,string apiPrefix, string password, string appVersion)
        {
            try
            {
                MultipartForm result = new MultipartForm(Body, ContentType);

                #region (x.1)密码验证                
                if (!string.IsNullOrEmpty(password))
                {
                    if (!result.form.TryGetValue("password", out var pwd) || password != pwd)
                    {                  
                        ApiReturn apiRet = new SsError { errorMessage = "密码不正确。" };
                        return apiRet.Serialize();
                    }
                }
                #endregion


                //(x.2)
                if (result.files.Count != 1)
                {
                    ApiReturn apiRet = new SsError { errorMessage = "获取不到升级文件。" };
                    return apiRet.Serialize();
                }

                //(x.3)
                if (!result.form.TryGetValue("stopBeforeUpgrade", out var stopBeforeUpgrade) || stopBeforeUpgrade != "on")
                {
                    actionToStop = null;
                }

                //(x.4)
                result.form.TryGetValue("cmdAfterUpgrade", out var cmdAfterUpgrade);

                //(x.5)
                var filePath = UpgradeHelp.SaveZipFile(result.files[0].content);
                UpgradeHelp.Upgrade(filePath, actionToStop, cmdAfterUpgrade);
                return new ApiReturn().Serialize();

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ApiReturn apiRet = ex.GetBaseException();
                return apiRet.Serialize();
            }
          

        }
        #endregion

    }
}
