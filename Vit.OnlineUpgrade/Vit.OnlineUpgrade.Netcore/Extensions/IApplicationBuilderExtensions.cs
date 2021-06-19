using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using Vit.Core.Module.Log;
using Vit.Core.Util.ConfigurationManager;
using Vit.OnlineUpgrade;

namespace Vit.Extensions
{

    public static partial class IApplicationBuilderExtensions
    {

        #region Use_OnlineUpgrade

        /// <summary>
        /// 加载在线升级功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="apiPrefix">在线升级api的地址前缀，如"/sys/onlineupgrade"(对应的在线升级首页为 /sys/onlineupgrade/index.html)</param>
        /// <param name="password">升级密码，若不指定则不进行权限校验</param>
        /// <param name="appVersion">首页显示的版本号，可不指定</param>

        public static void Use_OnlineUpgrade(this IApplicationBuilder data, string apiPrefix, string password = null, string appVersion=null)
        {
            if (data == null)
            {
                return;
            }


            if (string.IsNullOrWhiteSpace(apiPrefix))
            {
                apiPrefix = "/sys/onlineupgrade";
            }


            #region load upgrade
            {

                #region HandleMap
                Action<IApplicationBuilder> HandleMap = (app) =>
                {

                    Action StopApp = () => {
                        IApplicationLifetime lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
                        lifetime?.StopApplication();
                    };

                    app.Run(async context =>
                    {
                        var ContentType = context.Request.ContentType;
                        string result = UpgradeApi.Upgrate(context.Request.Body, ContentType, StopApp, apiPrefix, password, appVersion);
                        context.Response.ContentType = "text/json; charset=utf-8";
                        await context.Response.WriteAsync(result, Encoding.UTF8);

                    });
                };
                #endregion

                data.Map(apiPrefix + "/upgrade", HandleMap);
                 
            }
            #endregion



            #region load index.html
            {

                #region HandleMap
                Action<IApplicationBuilder> HandleMap = (app) =>
                {
                    app.Run(async context =>
                    {
                        var html = UpgradeApi.GetHtml(apiPrefix, password, appVersion);
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(html, Encoding.UTF8);

                    });
                };
                #endregion

                data.Map(apiPrefix + "/index.html", HandleMap);

            }
            #endregion
        }
        #endregion


        #region Use_OnlineUpgrade

        /// <summary>
        /// 加载在线升级功能（从appsettings.json中读取配置）
        /// </summary>
        public static void Use_OnlineUpgrade(this IApplicationBuilder data)
        {
            var apiPrefix = ConfigurationManager.Instance.GetStringByPath("OnlineUpgrade.apiPrefix");
            if (string.IsNullOrEmpty(apiPrefix)) return;

            Use_OnlineUpgrade(data, apiPrefix, ConfigurationManager.Instance.GetStringByPath("OnlineUpgrade.password"), ConfigurationManager.Instance.GetStringByPath("OnlineUpgrade.appVersion"));

        }
        #endregion

        #region StopApp

        /// <summary>
        /// 升级时会调用
        /// </summary>
        public static Action OnStop;

        static void StopApp()
        {
            try
            {
                OnStop?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }



            try
            {
                //退出当前进程
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            try
            {
                //退出当前进程已经当前进程开启的所有进程
                //System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        #endregion







    }
}
