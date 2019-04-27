using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Senparc.CO2NET.Helpers;
using Senparc.NeuChar.Context;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Entities.Request;
using Senparc.NeuChar.Helpers;
using Senparc.NeuChar.MessageHandlers;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Agent;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using Service.TemplateData;
using Service.WriteLog;

namespace Service
{
    /// <summary>
    /// 消息处理进行了封装（消息相关）
    /// 另：CustomMessageHandler_Event （事件相关）
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        string agentUrl = "http://localhost:12222/App/Weixin/4";
        string agentToken = "27C455F496044A87";
        string wiweihiKey = "CNadjJuWzyX5bz5Gn+/XoyqiqMa5DjXQ";

        private string appId = Config.SenparcWeixinSetting.WeixinAppId;
        private string appSecret = Config.SenparcWeixinSetting.WeixinAppSecret;

        /// <summary>
        /// 模板消息集合（Key：checkCode，Value：OpenId）
        /// </summary>
        public static Dictionary<string, string> TemplateMessageCollection = new Dictionary<string, string>();

        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {

        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            //说明：实际项目中这里的逻辑可以交给Service处理具体信息，参考OnLocationRequest方法或/Service/LocationSercice.cs

            #region 书中例子 
            //if (requestMessage.Content == "你好")
            //{
            //    var responseMessage = base.CreateResponseMessage<ResponseMessageNews>();
            //    var title = "Title";
            //    var description = "Description";
            //    var picUrl = "PicUrl";
            //    var url = "Url";
            //    responseMessage.Articles.Add(new Article()
            //    {
            //        Title = title,
            //        Description = description,
            //        PicUrl = picUrl,
            //        Url = url
            //    });
            //    return responseMessage;
            //}
            //else if (requestMessage.Content == "Senparc")
            //{
            //    //相似处理逻辑
            //}
            //else
            //{
            //    //...
            //}

            #endregion

            #region 历史方法

            //方法一（v0.1），此方法调用太过繁琐，已过时（但仍是所有方法的核心基础），建议使用方法二到四
            //var responseMessage =
            //    ResponseMessageBase.CreateFromRequestMessage(RequestMessage, ResponseMsgType.Text) as
            //    ResponseMessageText;

            //方法二（v0.4）
            //var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(RequestMessage);

            //方法三（v0.4），扩展方法，需要using Senparc.Weixin.MP.Helpers;
            //var responseMessage = RequestMessage.CreateResponseMessage<ResponseMessageText>();

            //方法四（v0.6+），仅适合在HandlerMessage内部使用，本质上是对方法三的封装
            //注意：下面泛型 ResponseMessageText 即返回给客户端的类型，可以根据自己的需要填写ResponseMessageNews等不同类型。

            #endregion

            var defaultResponseMessage = base.CreateResponseMessage<ResponseMessageText>();

            var requestHandler =
                requestMessage.StartHandler()
                //关键字不区分大小写，按照顺序匹配成功后将不再运行下面的逻辑
                .Keyword("约束", () =>
                {
                    defaultResponseMessage.Content =
                    @"您正在进行微信内置浏览器约束判断测试。您可以：
<a href=""http://sdk.weixin.senparc.com/FilterTest/"">点击这里</a>进行客户端约束测试（地址：http://sdk.weixin.senparc.com/FilterTest/），如果在微信外打开将直接返回文字。
或：
<a href=""http://sdk.weixin.senparc.com/FilterTest/Redirect"">点击这里</a>进行客户端约束测试（地址：http://sdk.weixin.senparc.com/FilterTest/Redirect），如果在微信外打开将重定向一次URL。";
                    return defaultResponseMessage;
                })
                .Keyword("绑定", () =>
                {
                    defaultResponseMessage.Content = "<a href=\"http://www.qq.com\">点击跳转绑定地址</a>";
                    return defaultResponseMessage;
                }).
                //匹配任一关键字
                Keywords(new[] { "托管", "代理" }, () =>
                {
                    //开始用代理托管，把请求转到其他服务器上去，然后拿回结果
                    //甚至也可以将所有请求在DefaultResponseMessage()中托管到外部。

                    DateTime dt1 = DateTime.Now; //计时开始

                    var agentXml = RequestDocument.ToString();

                    #region 暂时转发到SDK线上Demo

                    agentUrl = "http://sdk.weixin.senparc.com/weixin";
                    //agentToken = WebConfigurationManager.AppSettings["WeixinToken"];//Token

                    //修改内容，防止死循环
                    var agentDoc = XDocument.Parse(agentXml);
                    agentDoc.Root.Element("Content").SetValue("代理转发文字：" + requestMessage.Content);
                    agentDoc.Root.Element("CreateTime").SetValue(DateTimeHelper.GetWeixinDateTime(DateTime.Now));//修改时间，防止去重
                    agentDoc.Root.Element("MsgId").SetValue("123");//防止去重
                    agentXml = agentDoc.ToString();

                    #endregion

                    var responseXml = MessageAgent.RequestXml(this, agentUrl, agentToken, agentXml);
                    //获取返回的XML
                    //上面的方法也可以使用扩展方法：this.RequestResponseMessage(this,agentUrl, agentToken, RequestDocument.ToString());

                    /* 如果有WeiweihiKey，可以直接使用下面的这个MessageAgent.RequestWeiweihiXml()方法。
                    * WeiweihiKey专门用于对接www.weiweihi.com平台，获取方式见：https://www.weiweihi.com/ApiDocuments/Item/25#51
                    */
                    //var responseXml = MessageAgent.RequestWeiweihiXml(weiweihiKey, RequestDocument.ToString());//获取Weiweihi返回的XML

                    DateTime dt2 = DateTime.Now; //计时结束

                    //转成实体。
                    /* 如果要写成一行，可以直接用：
                    * responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
                    * 或
                    *
                    */
                    var msg = string.Format("\r\n\r\n代理过程总耗时：{0}毫秒", (dt2 - dt1).Milliseconds);
                    var agentResponseMessage = responseXml.CreateResponseMessage(this.MessageEntityEnlightener);
                    if (agentResponseMessage is ResponseMessageText)
                    {
                        (agentResponseMessage as ResponseMessageText).Content += msg;
                    }
                    else if (agentResponseMessage is ResponseMessageNews)
                    {
                        (agentResponseMessage as ResponseMessageNews).Articles[0].Description += msg;
                    }
                    return agentResponseMessage;//可能出现多种类型，直接在这里返回
                })
                .Keywords(new[] { "测试", "退出" }, () =>
                {
                    /*
                     * 这是一个特殊的过程，此请求通常来自于微微嗨（http://www.weiweihi.com）的“盛派网络小助手”应用请求（https://www.weiweihi.com/User/App/Detail/1），
                     * 用于演示微微嗨应用商店的处理过程，由于微微嗨的应用内部可以单独设置对话过期时间，所以这里通常不需要考虑对话状态，只要做最简单的响应。
                     */
                    if (defaultResponseMessage.Content == "测试")
                    {
                        //进入APP测试
                        defaultResponseMessage.Content = "您已经进入【盛派网络小助手】的测试程序，请发送任意信息进行测试。发送文字【退出】退出测试对话。10分钟内无任何交互将自动退出应用对话状态。";
                    }
                    else
                    {
                        //退出APP测试
                        defaultResponseMessage.Content = "您已经退出【盛派网络小助手】的测试程序。";
                    }
                    return defaultResponseMessage;
                })
                .Keyword("AsyncTest", () =>
                {
                    //异步并发测试（提供给单元测试使用）
#if NET45
                    DateTime begin = DateTime.Now;
                    int t1, t2, t3;
                    System.Threading.ThreadPool.GetAvailableThreads(out t1, out t3);
                    System.Threading.ThreadPool.GetMaxThreads(out t2, out t3);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(4));
                    DateTime end = DateTime.Now;
                    var thread = System.Threading.Thread.CurrentThread;
                    defaultResponseMessage.Content = string.Format("TId:{0}\tApp:{1}\tBegin:{2:mm:ss,ffff}\tEnd:{3:mm:ss,ffff}\tTPool：{4}",
                            thread.ManagedThreadId,
                            HttpContext.Current != null ? HttpContext.Current.ApplicationInstance.GetHashCode() : -1,
                            begin,
                            end,
                            t2 - t1
                            );
#endif

                    return defaultResponseMessage;
                })
                .Keyword("OPEN", () =>
                {
                    var openResponseMessage = requestMessage.CreateResponseMessage<ResponseMessageNews>();
                    openResponseMessage.Articles.Add(new Article()
                    {
                        Title = "开放平台微信授权测试",
                        Description = @"点击进入Open授权页面。

授权之后，您的微信所收到的消息将转发到第三方（盛派网络小助手）的服务器上，并获得对应的回复。

测试完成后，您可以登陆公众号后台取消授权。",
                        Url = "http://sdk.weixin.senparc.com/OpenOAuth/JumpToMpOAuth"
                    });
                    return openResponseMessage;
                })
                .Keyword("错误", () =>
                {
                    var errorResponseMessage = requestMessage.CreateResponseMessage<ResponseMessageText>();
                    //因为没有设置errorResponseMessage.Content，所以这小消息将无法正确返回。
                    return errorResponseMessage;
                })
                .Keyword("容错", () =>
                {
                    Thread.Sleep(4900);//故意延时1.5秒，让微信多次发送消息过来，观察返回结果
                    var faultTolerantResponseMessage = requestMessage.CreateResponseMessage<ResponseMessageText>();
                    faultTolerantResponseMessage.Content = string.Format("测试容错，MsgId：{0}，Ticks：{1}", requestMessage.MsgId,
                        DateTime.Now.Ticks);
                    return faultTolerantResponseMessage;
                })
                .Keyword("TM", () =>
                {
                    var openId = requestMessage.FromUserName;
                    var checkCode = Guid.NewGuid().ToString("n").Substring(0, 3);//为了防止openId泄露造成骚扰，这里启用验证码
                    TemplateMessageCollection[checkCode] = openId;
                    defaultResponseMessage.Content = string.Format(@"新的验证码为：{0}，请在网页上输入。网址：https://sdk.weixin.senparc.com/AsyncMethods", checkCode);
                    return defaultResponseMessage;
                })
                .Keyword("OPENID", () =>
                {
                    var openId = requestMessage.FromUserName;//获取OpenId
                    var userInfo = Senparc.Weixin.MP.AdvancedAPIs.UserApi.Info(appId, openId, Language.zh_CN);

                    defaultResponseMessage.Content = string.Format(
                        "您的OpenID为：{0}\r\n昵称：{1}\r\n性别：{2}\r\n地区（国家/省/市）：{3}/{4}/{5}\r\n关注时间：{6}\r\n关注状态：{7}",
                        requestMessage.FromUserName, userInfo.nickname, (WeixinSex)userInfo.sex, userInfo.country, userInfo.province, userInfo.city, DateTimeHelper.GetDateTimeFromXml(userInfo.subscribe_time), userInfo.subscribe);
                    return defaultResponseMessage;
                })
                .Keyword("EX", () =>
                {
                    var ex = new WeixinException("openid:" + requestMessage.FromUserName + ":这是一条测试异常信息");//回调过程在global的ConfigWeixinTraceLog()方法中
                    defaultResponseMessage.Content = "请等待异步模板消息发送到此界面上（自动延时数秒）。\r\n当前时间：" + DateTime.Now.ToString();
                    return defaultResponseMessage;
                })
                .Keyword("MUTE", () => //不回复任何消息
                {
                    //方案一：
                    return new SuccessResponseMessage();

                    //方案二：
                    var muteResponseMessage = base.CreateResponseMessage<ResponseMessageNoResponse>();
                    return muteResponseMessage;

                    //方案三：
                    base.TextResponseMessage = "success";
                    return null;

                    //方案四：
                    return null;//在 Action 中结合使用 return new FixWeixinBugWeixinResult(messageHandler);
                })
                .Keyword("JSSDK", () =>
                {
                    defaultResponseMessage.Content = "点击打开：http://sdk.weixin.senparc.com/WeixinJsSdk";
                    return defaultResponseMessage;
                })
                .Keyword("Oauth网页授权", () =>
                {
                    string redirectUrl = AgentUrl + "/WeiXin/OAuth";
                    string oAuthUrl = OAuthApi.GetAuthorizeUrl(AppId,
                             redirectUrl,
                              "123", OAuthScope.snsapi_base);

                    Log.Info("1.oAuthUrl地址", oAuthUrl, null);

                    defaultResponseMessage.Content = oAuthUrl.ToString();
                    return defaultResponseMessage;
                })
                .Keyword("消费", () =>
                {
                    //模版消息
                    string access_token = AccessTokenContainer.TryGetAccessToken(AppId,AppSecret); //AccessToken
                    string openId = requestMessage.FromUserName;   //用户openId
                    string templateId = "YdCnLUZGJATUv5POmdg4cTBpEWe8CGm8nkuoa9hJnjc";   //模版id
                    string linkUrl = "http://www.baidu.com";    //点击详情后跳转后的链接地址，为空则不跳转


                    var templateData = new ProductTemplateData()
                    {
                        first = new TemplateDataItem("您好，您的订单已支付成功！", "#000000"),
                        product = new TemplateDataItem("旺旺大礼包", "#000000"),
                        price = new TemplateDataItem("99.8元", "#000000"),
                        time = new TemplateDataItem("2016-11-09 16:50:38", "#000000"),
                        remark = new TemplateDataItem("感谢您的光临~", "#000000")
                    };

                    SendTemplateMessageResult sendResult = TemplateApi.SendTemplateMessage(
                        access_token, openId, templateId, linkUrl, templateData);

                    if (sendResult.errcode.ToString() == "请求成功")
                    {
                        //这里操作数据库，不返回数据。
                        //defaultResponseMessage.Content = sendResult.errmsg;
                    }
                    else
                    {
                        defaultResponseMessage.Content = "模版消息出现错误" + sendResult.errmsg;
                        Log.Info("模版消息出现错误", sendResult.errmsg, null);
                    }

                    return defaultResponseMessage;
                })
                //Default不一定要在最后一个
                .Default(() =>
                    {
                        var result = new StringBuilder();
                        result.AppendFormat("您刚才发送了文字信息：{0}\r\n\r\n", requestMessage.Content);

                        if (CurrentMessageContext.RequestMessages.Count > 1)
                        {
                            result.AppendFormat("您刚才还发送了如下消息（{0}/{1}）：\r\n", CurrentMessageContext.RequestMessages.Count,
                                CurrentMessageContext.StorageData);
                            for (int i = CurrentMessageContext.RequestMessages.Count - 2; i >= 0; i--)
                            {
                                var historyMessage = CurrentMessageContext.RequestMessages[i];
                                result.AppendFormat("{0} 【{1}】{2}\r\n",
                                    historyMessage.CreateTime.ToString("HH:mm:ss"),
                                    historyMessage.MsgType.ToString(),
                                    (historyMessage is RequestMessageText)
                                        ? (historyMessage as RequestMessageText).Content
                                        : "[非文字类型]"
                                    );
                            }
                            result.AppendLine("\r\n");
                        }

                        result.AppendFormat("如果您在{0}分钟内连续发送消息，记录将被自动保留（当前设置：最多记录{1}条）。过期后记录将会自动清除。\r\n",
                            GlobalMessageContext.ExpireMinutes, GlobalMessageContext.MaxRecordCount);
                        result.AppendLine("\r\n");
                        result.AppendLine(
                            "您还可以发送【位置】【图片】【语音】【视频】等类型的信息（注意是这几种类型，不是这几个文字），查看不同格式的回复。\r\nSDK官方地址：http://sdk.weixin.senparc.com");

                        defaultResponseMessage.Content = result.ToString();
                        return defaultResponseMessage;
                    })
                //“一次订阅消息”接口测试
                .Keyword("订阅", () =>
                {
                    defaultResponseMessage.Content = "点击打开：https://sdk.weixin.senparc.com/SubscribeMsg";
                    return defaultResponseMessage;
                })
                //正则表达式
                .Regex(@"^\d+#\d+$", () =>
                {
                    defaultResponseMessage.Content = string.Format("您输入了：{0}，符合正则表达式：^\\d+#\\d+$", requestMessage.Content);
                    return defaultResponseMessage;
                });

            return requestHandler.GetResponseMessage() as IResponseMessageBase;
        }
    }

}