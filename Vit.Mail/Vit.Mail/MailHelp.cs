#region << 版 本 注 释 - v1 >>
/*
 * ========================================================================
 *  
 * 作者：lith
 * 时间：2019-05-10
 * 邮箱：serset@yeah.net
 * 
 * ========================================================================
*/
#endregion

using MailKit.Security;
using MimeKit;
using Vit.Core.Util.ConfigurationManager;

namespace Vit.Mail
{

    /// <summary>
    /// 
    /// </summary>
    public class MailHelp
    {
        // 参考 https://www.lagou.com/lgeduarticle/50081.html
        // https://www.cnblogs.com/sunnytrudeau/p/10822470.html

        #region 默认邮箱账号
        private static readonly MailAccount _MailSenderAccount = Appsettings.json.GetByPath<MailAccount>("MailAccount");
        #endregion


        #region Send 指定发件人账户

        /// <summary>
        /// 发送邮件,失败则抛异常
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tos">收件人地址数组，例：123456789@qq.com</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="bodyIsHtml">indicating whether the mail message body is in Html.</param> 
        public static void Send(MailAccount account, string[] tos, string subject, string body, bool bodyIsHtml = true)
        {
            var message = new MimeMessage();

            //收件人
            foreach (var item in tos)
            {
                message.To.Add(new MailboxAddress(item, item));
            }

            //发件人
            message.From.Add(new MailboxAddress(account.userName, account.address));

            message.Subject = subject;

            //body
            var builder = new BodyBuilder();
            if (bodyIsHtml)
            {
                builder.HtmlBody = body;
            }
            else
            {
                builder.TextBody = body;
            }
            message.Body = builder.ToMessageBody();



            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.CheckCertificateRevocation = false;

                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;


                //client.Connect(account.host);
                client.Connect(account.host, 465, SecureSocketOptions.Auto);         

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(account.userName, account.password);

                client.Send(message);
                client.Disconnect(true);
            }


            //MailMessage message = new MailMessage();

            //#region 附件
            //if (null != attachmentsPath && attachmentsPath.Length > 0)
            //{
            //    foreach (string attachmentPath in attachmentsPath)
            //    {
            //        //attachmentPath="d://test.txt"
            //        message.Attachments.Add(new Attachment(attachmentPath));
            //    }
            //}
            //#endregion

            //message.From = new MailAddress(account.address);

            ////收件人邮箱地址是多个
            //foreach (string to in tos)
            //    message.To.Add(to);

            //message.Subject = subject;
            //message.Body = body;

            ////是否为html格式 
            //message.IsBodyHtml = bodyIsHtml;

            ////发送邮件的优先等级 
            //message.Priority = MailPriority.Normal;

            //SmtpClient client = new SmtpClient(account.host);

            //client.Credentials = new NetworkCredential(account.userName, account.password);

            ////发送邮件
            //client.Send(message);
        }


        #endregion


        #region Send

        /// <summary>
        /// 使用系统账户发送邮件,失败则抛异常
        /// </summary>
        /// <param name="tos">收件人地址数组，例：123456789@qq.com</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="bodyIsHtml">indicating whether the mail message body is in Html.</param> 
        public static void Send(string[] tos, string subject, string body, bool bodyIsHtml = true)
        {
            Send(_MailSenderAccount, tos, subject, body, bodyIsHtml);
        }


        /// <summary>
        /// 使用系统账户发送邮件,发送邮件,失败则抛异常
        /// </summary>   
        /// <param name="to">收件人地址数组，例：123456789@qq.com</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="bodyIsHtml">indicating whether the mail message body is in Html.</param>
        public static void Send(string to, string subject, string body, bool bodyIsHtml = true)
        {
            Send(_MailSenderAccount, new[] { to }, subject, body, bodyIsHtml);
        }
        #endregion


    }
}
