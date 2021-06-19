#region << 版 本 注 释 - v1 >>
/*
 * ========================================================================
 *  
 * 作者：lith
 * 时间：2020-03-05
 * 邮箱：serset@yeah.net
 * 
 * ========================================================================
*/
#endregion

using Newtonsoft.Json;

namespace Vit.Mail
{
    public class MailAccount
    {
        /// <summary>
        /// 发件人邮件地址,例：123456789@qq.com 
        /// </summary>
        [JsonIgnore]
        private string _address;
        /// <summary>
        /// 发件人邮件地址,例：123456789@qq.com 
        /// </summary>
        [JsonProperty]
        public string address
        {
            set { _address = value; try { _userName = value.Remove(value.LastIndexOf('@')); } catch { _userName = value; } }
            get { return _address; }
        }

        /// <summary>
        ///  用户名，注意如果发件人地址是abc@def.com ，则用户名是abc 而不是abc@def.com 
        /// </summary>
        private string _userName;

        /// <summary>
        /// 用户名，注意如果发件人地址是abc@def.com ，则用户名是abc 而不是abc@def.com 
        /// </summary>
        [JsonIgnore]
        public string userName { get { return _userName; } }

        /// <summary>
        /// 密码(发件人的邮箱登陆密码或授权码)
        /// </summary>
        [JsonProperty]
        public string password { get; set; }


        /// <summary>
        /// 发送邮件的服务器地址或IP,例："smtp.qq.com"
        /// </summary>
        [JsonProperty]
        public string host { get; set; }


        public MailAccount(string address, string password, string host)
        {
            this.address = address; this.password = password; this.host = host;
        }
        public MailAccount() { }

    }
}
