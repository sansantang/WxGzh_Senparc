using System;
using System.Text;
using Model;
using Senparc.CO2NET.Extensions;
using Senparc.NeuChar.Entities;
using Senparc.Weixin;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.Agent;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Service.WriteLog;

namespace Service
{
    /// <summary>
    /// 消息处理进行了封装（事件相关）
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {

        private static readonly string CONFIG_AGENTURUL = "agentUrl";
        private static readonly string CONFIG_TOKEN = "agentToken";
        private static readonly string CONFIG_ENCODINGAESKEY = "EncodingAESKey";
        private static readonly string CONFIG_APPID = "appID";
        private static readonly string CONFIG_APPSECRET = "appsecret";
        private static readonly string AgentUrl = System.Configuration.ConfigurationManager.AppSettings[CONFIG_AGENTURUL];//相当于一个秘钥 我们自己秘钥 可以配置的 
        private static readonly string Token = System.Configuration.ConfigurationManager.AppSettings[CONFIG_TOKEN];//相当于一个秘钥 我们自己秘钥 可以配置的 
        private static readonly string EncodingAesKey = System.Configuration.ConfigurationManager.AppSettings[CONFIG_ENCODINGAESKEY];//相当于一个秘钥 我们自己秘钥 可以配置的 
        private static readonly string AppId = System.Configuration.ConfigurationManager.AppSettings[CONFIG_APPID];//相当于一个秘钥 我们自己秘钥 可以配置的 
        private static readonly string AppSecret = System.Configuration.ConfigurationManager.AppSettings[CONFIG_APPSECRET];//相当于一个秘钥 我们自己秘钥 可以配置的 



        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            IResponseMessageBase reponseMessage = null;
            //菜单点击，需要跟创建菜单时的Key匹配

            switch (requestMessage.EventKey)
            {
                //                case "OneClick":
                //                    {
                //                        //这个过程实际已经在OnTextOrEventRequest中完成，这里不会执行到。
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Content = "您点击了底部按钮。\r\n为了测试微信软件换行bug的应对措施，这里做了一个——\r\n换行";
                //                    }
                //                    break;
                //                case "SubClickRoot_Text":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Content = "您点击了子菜单按钮。";
                //                    }
                //                    break;
                //                case "SubClickRoot_News":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageNews>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Articles.Add(new Article()
                //                        {
                //                            Title = "您点击了子菜单图文按钮",
                //                            Description = "您点击了子菜单图文按钮，这是一条图文信息。这个区域是Description内容\r\n可以使用\\r\\n进行换行。",
                //                            PicUrl = "http://sdk.weixin.senparc.com/Images/qrcode.jpg",
                //                            Url = "http://sdk.weixin.senparc.com"
                //                        });
                //
                //                        //随机添加一条图文，或只输出一条图文信息
                //                        if (DateTime.Now.Second % 2 == 0)
                //                        {
                //                            strongResponseMessage.Articles.Add(new Article()
                //                            {
                //                                Title = "这是随机产生的第二条图文信息，用于测试多条图文的样式",
                //                                Description = "这是随机产生的第二条图文信息，用于测试多条图文的样式",
                //                                PicUrl = "http://sdk.weixin.senparc.com/Images/qrcode.jpg",
                //                                Url = "http://sdk.weixin.senparc.com"
                //                            });
                //                        }
                //                    }
                //                    break;
                //                case "SubClickRoot_Music":
                //                    {
                //                        //上传缩略图
                //                        var accessToken = Containers.AccessTokenContainer.TryGetAccessToken(appId, appSecret);
                //                        var uploadResult = AdvancedAPIs.MediaApi.UploadTemporaryMedia(accessToken, UploadMediaFileType.thumb,
                //                                                                     Server.GetMapPath("~/Images/Logo.thumb.jpg"));
                //                        //PS：缩略图官方没有特别提示文件大小限制，实际测试哪怕114K也会返回文件过大的错误，因此尽量控制在小一点（当前图片39K）
                //
                //                        //设置音乐信息
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageMusic>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Music.Title = "天籁之音";
                //                        strongResponseMessage.Music.Description = "真的是天籁之音";
                //                        strongResponseMessage.Music.MusicUrl = "https://sdk.weixin.senparc.com/Content/music1.mp3";
                //                        strongResponseMessage.Music.HQMusicUrl = "https://sdk.weixin.senparc.com/Content/music1.mp3";
                //                        strongResponseMessage.Music.ThumbMediaId = uploadResult.thumb_media_id;
                //                    }
                //                    break;
                //                case "SubClickRoot_Image":
                //                    {
                //                        //上传图片
                //                        var accessToken = Containers.AccessTokenContainer.TryGetAccessToken(appId, appSecret);
                //                        var uploadResult = AdvancedAPIs.MediaApi.UploadTemporaryMedia(accessToken, UploadMediaFileType.image,
                //                                                                     Server.GetMapPath("~/Images/Logo.jpg"));
                //                        //设置图片信息
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageImage>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Image.MediaId = uploadResult.media_id;
                //                    }
                //                    break;
                //                case "SubClickRoot_Agent"://代理消息
                //                    {
                //                        //获取返回的XML
                //                        DateTime dt1 = DateTime.Now;
                //                        reponseMessage = MessageAgent.RequestResponseMessage(this, agentUrl, agentToken, RequestDocument.ToString());
                //                        //上面的方法也可以使用扩展方法：this.RequestResponseMessage(this,agentUrl, agentToken, RequestDocument.ToString());
                //
                //                        DateTime dt2 = DateTime.Now;
                //
                //                        if (reponseMessage is ResponseMessageNews)
                //                        {
                //                            (reponseMessage as ResponseMessageNews)
                //                                .Articles[0]
                //                                .Description += string.Format("\r\n\r\n代理过程总耗时：{0}毫秒", (dt2 - dt1).Milliseconds);
                //                        }
                //                    }
                //                    break;
                //                case "Member"://托管代理会员信息
                //                    {
                //                        //原始方法为：MessageAgent.RequestXml(this,agentUrl, agentToken, RequestDocument.ToString());//获取返回的XML
                //                        reponseMessage = this.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
                //                    }
                //                    break;
                //                case "OAuth"://OAuth授权测试
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageNews>();
                //
                //                        strongResponseMessage.Articles.Add(new Article()
                //                        {
                //                            Title = "OAuth2.0测试",
                //                            Description = "选择下面两种不同的方式进行测试，区别在于授权成功后，最后停留的页面。",
                //                            //Url = "http://sdk.weixin.senparc.com/oauth2",
                //                            //PicUrl = "http://sdk.weixin.senparc.com/Images/qrcode.jpg"
                //                        });
                //
                //                        strongResponseMessage.Articles.Add(new Article()
                //                        {
                //                            Title = "OAuth2.0测试（不带returnUrl），测试环境可使用",
                //                            Description = "OAuth2.0测试（不带returnUrl）",
                //                            Url = "http://sdk.weixin.senparc.com/oauth2",
                //                            PicUrl = "http://sdk.weixin.senparc.com/Images/qrcode.jpg"
                //                        });
                //
                //                        var returnUrl = "/OAuth2/TestReturnUrl";
                //                        strongResponseMessage.Articles.Add(new Article()
                //                        {
                //                            Title = "OAuth2.0测试（带returnUrl），生产环境强烈推荐使用",
                //                            Description = "OAuth2.0测试（带returnUrl）",
                //                            Url = "http://sdk.weixin.senparc.com/oauth2?returnUrl=" + returnUrl.UrlEncode(),
                //                            PicUrl = "http://sdk.weixin.senparc.com/Images/qrcode.jpg"
                //                        });
                //
                //                        reponseMessage = strongResponseMessage;
                //
                //                    }
                //                    break;
                //                case "Description":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        strongResponseMessage.Content = GetWelcomeInfo();
                //                        reponseMessage = strongResponseMessage;
                //                    }
                //                    break;
                //                case "SubClickRoot_PicPhotoOrAlbum":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Content = "您点击了【微信拍照】按钮。系统将会弹出拍照或者相册发图。";
                //                    }
                //                    break;
                //                case "SubClickRoot_ScancodePush":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Content = "您点击了【微信扫码】按钮。";
                //                    }
                //                    break;
                //                case "ConditionalMenu_Male":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Content = "您点击了个性化菜单按钮，您的微信性别设置为：男。";
                //                    }
                //                    break;
                //                case "ConditionalMenu_Femle":
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        reponseMessage = strongResponseMessage;
                //                        strongResponseMessage.Content = "您点击了个性化菜单按钮，您的微信性别设置为：女。";
                //                    }
                //                    break;
                //                case "GetNewMediaId"://获取新的MediaId
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        try
                //                        {
                //                            var result = Senparc.Weixin.MP.AdvancedAPIs.MediaApi.UploadForeverMedia(appId, Server.GetMapPath("~/Images/logo.jpg"));
                //                            strongResponseMessage.Content = result.media_id;
                //                        }
                //                        catch (Exception e)
                //                        {
                //                            strongResponseMessage.Content = "发生错误：" + e.Message;
                //                            WeixinTrace.SendCustomLog("调用UploadForeverMedia()接口发生异常", e.Message);
                //                        }
                //                    }
                //                    break;
                //                default:
                //                    {
                //                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                //                        strongResponseMessage.Content = "您点击了按钮，EventKey：" + requestMessage.EventKey;
                //                        reponseMessage = strongResponseMessage;
                //                    }
                //                    break;
            }

            return reponseMessage;
        }


        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);


            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);

            string result = string.Format("欢迎关注唐门年华公众号！");


            //            string redirectUrl = AgentUrl + "/WeiXin/OAuth";
            //            string oAuthUrl = OAuthApi.GetAuthorizeUrl(AppId,
            //                     redirectUrl,
            //                      "123", OAuthScope.snsapi_base);


            UserInfoJson userInfo = UserApi.Info(accessToken, requestMessage.FromUserName);

            int isOk = SaveUser(userInfo);

            Log.Debug("订阅事件，保存用户", isOk > 0 ? requestMessage.FromUserName + "保存成功" : "保存失败", null);

            responseMessage.Content = result;

            if (!string.IsNullOrEmpty(requestMessage.EventKey))
            {
                responseMessage.Content += "\r\n============\r\n场景值：" + requestMessage.EventKey;
            }

            return responseMessage;
        }


        public static int SaveUser(UserInfoJson userInfoJson)
        {
            using (weixin_gzhEntities db = new weixin_gzhEntities())
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < userInfoJson.tagid_list.Length; i++)
                {
                    sb.Append(userInfoJson.tagid_list[i] + ",");
                }

                UserInfoJson userInfo = userInfoJson;
                weixin_userinfo user = new weixin_userinfo();
                user.subscribe = userInfo.subscribe;
                user.openid = userInfo.openid;
                user.nickname = userInfo.nickname;
                user.sex = userInfo.sex;
                user.language = userInfo.language;
                user.city = userInfo.city;
                user.province = userInfo.province;
                user.country = userInfo.country;
                user.headimgurl = userInfo.headimgurl;
                user.subscribe_time = DateTime.Now;
                user.unionid = userInfo.unionid;
                user.remark = userInfo.remark;
                user.groupid = userInfo.groupid;
                user.tagid_list = sb.ToString();
                user.subscribe_scene = userInfo.subscribe_scene;
                user.qr_scene = userInfo.qr_scene;
                user.qr_scene_str = userInfo.qr_scene_str;
                db.weixin_userinfo.Add(user);
                int isOk = db.SaveChanges();
                return isOk;
            }
        }
    }
}