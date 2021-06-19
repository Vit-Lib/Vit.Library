using System;
using System.Diagnostics;
using System.Threading;
using Vit.Core.Module.Log;

namespace Vit.OnlineUpgrade
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                    
                //(x.1)
                Logger.Info("  OnlineUpgrade  -Vit.AspNetCore.OnlineUpgrade start");
                string zipPath = args[0];
                Logger.Info("  OnlineUpgrade  -arg: " + zipPath);


                #region (x.2)解压，失败重试，最多10次
                {
                    int tryCount = 0;
                    while (true)
                    {
                        Thread.Sleep(2000);
                        tryCount++;
                        try
                        {
                            UpgradeHelp.ExtractZipFile(zipPath);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("  OnlineUpgrade  ExtractZipFile(" + tryCount + ") error", ex);
                            if (tryCount >= 10)
                            {
                                Logger.Error("  OnlineUpgrade  failed！！！ ExtractZipFile faile,tried "+ tryCount + " times");
                                return;
                            }                          
                        }
                    }
                }
                #endregion

                //(x.3)操作成功
                Logger.Info("  OnlineUpgrade  -Vit.AspNetCore.OnlineUpgrade succeed!");
                Console.WriteLine("  OnlineUpgrade  -Vit.AspNetCore.OnlineUpgrade succeed!");


                #region (x.4)执行脚本（若存在）
                for (int i = 1; i < args.Length; i++)
                { 
                    var cmd = args[i];
                    if (string.IsNullOrWhiteSpace(cmd)) continue;

                    try
                    {

                        Logger.Info("  OnlineUpgrade  - shell :" + cmd);
                        string fileName;
                        string arguments;
                        
                        var argIndex= cmd.IndexOf(' ');
                        if (argIndex <= 0)
                        {
                            fileName = cmd;
                            arguments = null;
                        }
                        else
                        {
                            fileName = cmd.Substring(0, argIndex);
                            arguments = cmd.Substring(argIndex + 1);
                        }

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo(fileName, arguments)
                            {
                                UseShellExecute = true
                            }
                        };
                        process.Start();

                    }
                    catch (Exception ex)
                    {
                        Logger.Error("  OnlineUpgrade - shell error  ", ex);
                    }
                }
                #endregion



            }
            catch (Exception ex)
            {
                Logger.Error("  OnlineUpgrade  ", ex);        
            }
          
        }
    }
}
