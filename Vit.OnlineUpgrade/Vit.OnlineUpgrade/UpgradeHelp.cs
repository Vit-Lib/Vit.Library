using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Vit.Core.Module.Log;
using Vit.Core.Util.Common;

namespace Vit.OnlineUpgrade
{
    public class UpgradeHelp
    {


        #region ExtractZipFile       
        internal static void ExtractZipFile(string zipPath)
        {
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, AppContext.BaseDirectory, CodePagesEncodingProvider.Instance.GetEncoding("GB2312")/*, true*/);
        }
        #endregion


        #region Save ZipFile

        public static string SaveZipFile(byte[] zipFileContent)
        {
            // /Logs/UpgradeFile/upgrade(20191203 162635 xxxxxxx).zip

            #region (x.1) make sure directory exist            
            var directoryPath = Path.Combine(AppContext.BaseDirectory, "Logs");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            directoryPath = Path.Combine(directoryPath, "UpgradeFile");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            #endregion


            #region (x.2)build file name
            var fileName = "upgrade(" + DateTime.Now.ToString("yyyyMMdd HHmmss") + " " + CommonHelp.Random(10000, 19999) + ").zip";
            #endregion

            string filePath = Path.Combine(directoryPath, fileName);

            System.IO.File.WriteAllBytes(filePath, zipFileContent);
            return filePath;
        }

        #endregion


        #region Upgrade


        public static void Upgrade(string zipPath, Action actionToStop, string cmdAfterUpgrade = null)
        {

            #region (x.1)启动后台升级程序
            try
            {
                File.WriteAllText("Vit.OnlineUpgrade.runtimeconfig.json", @"
{
  ""runtimeOptions"": {
    ""tfm"": ""netcoreapp2.1"",
    ""framework"": {
                    ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""2.1.0""
    }
    }
}
");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }


            ShellBehind("dotnet", "Vit.OnlineUpgrade.dll \"" + zipPath + "\" \"" + cmdAfterUpgrade + "\"");
            #endregion


            #region (x.2) 退出当前程序
            try
            {
                actionToStop?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            #endregion

        }
        #endregion


        #region ShellBehind
        /// <summary>
        /// 后台执行
        /// </summary>
        /// <param name="fileName"> 如 reboot </param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        static void ShellBehind(string fileName, string arguments)
        {

            var process = new Process
            {

                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    //UseShellExecute = true
                }
            };

            process.Start();

        }
        #endregion



    }
}
