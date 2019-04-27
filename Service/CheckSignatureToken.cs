using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;

namespace Service
{
    public class CheckSignatureToken
    {
        private static readonly string TOKEN = "agentToken";
        private static readonly string ENCODINGAESKEY = "EncodingAESKey";
        private static readonly string APPID = "appID";
        public static readonly string Token = System.Configuration.ConfigurationManager.AppSettings[TOKEN];//相当于一个秘钥 我们自己秘钥 可以配置的 
        private static readonly string EncodingAesKey = System.Configuration.ConfigurationManager.AppSettings[ENCODINGAESKEY];//相当于一个秘钥 我们自己秘钥 可以配置的 
        public static readonly string AppId = System.Configuration.ConfigurationManager.AppSettings[APPID];//相当于一个秘钥 我们自己秘钥 可以配置的 

        // GET: WeiXin
        /// <summary>
        /// 验证消息的确来自微信服务器
        /// </summary>
        /// <param name="pModel">封装post传过来的加密参数
        /// @param signature:微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数。
        /// @param timestamp:时间戳
        /// @param nonce:随机数
        /// </param>
        /// <param name="echostr">随机字符串</param>
        public static string CheckToken(PostModel pModel, string echostr)
        {
            if (CheckSignature.Check(pModel.Signature, pModel.Timestamp, pModel.Nonce, Token))
            {
                return echostr; //返回随机字符串则表示验证通过
            }
            else
            {
                return "failed:" + pModel.Signature + "," +
                    CheckSignature.GetSignature(pModel.Timestamp, pModel.Nonce, Token) +
                    "。如果您在浏览器中看到这条信息，表明此Url可以填入微信后台。";
            }
        }

    }
}
