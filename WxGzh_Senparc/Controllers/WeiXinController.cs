using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.CO2NET.Extensions;
using Senparc.NeuChar;
using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.GroupMessage;
using Senparc.Weixin.MP.AdvancedAPIs.Groups;
using Senparc.Weixin.MP.AdvancedAPIs.Media;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Entities.Request;
using Service;
using Service.WriteLog;

namespace WxGzh_Senparc.Controllers
{
    public  class WeiXinController : Controller
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


        ///<summary>
        /// 验证消息的确来自微信服务器
        /// </summary>
        [HttpGet]
        public ActionResult Index(PostModel pmodel, string echostr)
        {
            Log.Info("服务器", "GET验证", null);

//           int isOK = CustomMessageHandler.SaveUser(null);
//            Log.Debug("订阅事件，保存用户", isOK > 0 ? "保存成功" : "保存失败", null);

            return Content(CheckSignatureToken.CheckToken(pmodel, echostr));
        }

        /// <summary>
        /// 用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML
        /// </summary>
        [HttpPost]
        public ActionResult Index(PostModel pmodel)
        {

            Log.Info("≤============", null, null);
            Log.Info("服务器", "POST握手一次", null);
            if (!CheckSignature.Check(pmodel.Signature, pmodel.Timestamp, pmodel.Nonce, Token))
            {
                Log.Info("服务器", "参数错误！", null);
                return Content("参数错误！");
            }

            pmodel.Token = Token;
            pmodel.EncodingAESKey = EncodingAesKey;//根据自己后台的设置保持一致
            pmodel.AppId = AppId;//根据自己后台的设置保持一致

            var messageHandler = new CustomMessageHandler(Request.InputStream, pmodel);//接收消息（第一步）

            try
            {
                messageHandler.Execute(); //执行微信处理过程（第二步）
                if (messageHandler.ResponseMessage != null)
                {

                    Log.Info("Execute：执行微信处理过程", "\r\n" + messageHandler.ResponseDocument.ToString(), "running...");
                    return Content(messageHandler.ResponseDocument.ToString());
                    return new FixWeixinBugWeixinResult(messageHandler); //返回（第三步）
                }
            }
            catch (Exception e)
            {
                Log.Info("Execute：执行微信处理过程", e.Message, "Exception");
                throw;
            }
            finally
            {
                Log.Info("============≥", null, null);
            }

            return new FixWeixinBugWeixinResult(messageHandler);//返回（第三步）
        }

        /// <summary>
        /// 菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("Menu")]
        public ActionResult MenuGet()
        {
            return View();
        }

        /// <summary>
        /// 菜单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Menu")]
        public ActionResult MenuPost()
        {
            return View();
        }

        /// <summary>
        /// OAuth验证,第三方网页时授权
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult OAuth(string code, string state, string returnUrl)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

//            if (state != HttpContext.Session.GetString("State"))
//            {
//                //这里的state其实是会暴露给客户端的，验证能力很弱，这里只是演示一下，
//                //建议用完之后就清空，将其一次性使用
//                //实际上可以存任何想传递的数据，比如用户ID，并且需要结合例如下面的Session["OAuthAccessToken"]进行验证
//                return Content("验证失败！请从正规途径进入！");
//            }

            OAuthAccessTokenResult result = null;

            //通过，用code换取access_token
            try
            {
                Log.Debug("code值",code,null);
                result = OAuthApi.GetAccessToken(AppId, AppSecret, code);
                Log.Info("openid",result.openid,null);
            }
            catch (Exception ex)
            {
                Log.Debug("1.发生错误啦~~~",ex.Message,null);
                return Content(ex.Message);
            }
            if (result.errcode != ReturnCode.请求成功)
            {
                 Log.Debug("result.errcode",result.errcode.ToString(),null);
                return Content("错误：" + result.errmsg);
            }
            //下面2个数据也可以自己封装成一个类，储存在数据库中（建议结合缓存）
            //如果可以确保安全，可以将access_token存入用户的cookie中，每一个人的access_token是不一样的
//            HttpContext.Session.SetString("OAuthAccessTokenStartTime", DateTime.Now.ToString());
//            HttpContext.Session.SetString("OAuthAccessToken", result.ToJson());

            //因为第一步选择的是OAuthScope.snsapi_userinfo，这里可以进一步获取用户详细信息
            try
            {
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                //这里获取openid就结束了。OAuthScope.snsapi_base 模式就结束了。
                //保存到数据库中吧。

                return View();
            }
            catch (ErrorJsonResultException ex)
            {
                return Content(ex.Message);
            }
        }


        #region 取得用户组

        //        /// <summary>
        //        /// 取得用户组
        //        /// </summary>
        //        /// <returns></returns>
        //        public JsonResult GetGroupList()
        //        {
        //            Log.Info("≤============", null, null);
        //            string accessToken = string.Empty;
        //            try
        //            {
        //                accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
        //                Log.Info("取得用户组", accessToken, null);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Info("Execute：执行微信处理过程", ex.Message, "Exception");
        //            }
        //            finally
        //            {
        //                Log.Info("============≥", null, null);
        //            }
        //            GroupsJson groupJson = GroupsApi.Get(accessToken);
        //            return Json(groupJson.groups, JsonRequestBehavior.AllowGet);
        //        } 
        #endregion

        #region MyRegion 取得所有微信关注的粉丝
        //        /// <summary>
        //        /// 取得所有微信关注的粉丝
        //        /// </summary>
        //        /// <returns></returns>
        //                public JsonResult GetUserList()
        //                {
        //        
        //                    var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
        //                    OpenIdResultJson openIdResultJson = null;
        //                    string nextOpenid = string.Empty;
        //                    List<UserInfoJson> list = new List<UserInfoJson>();
        //        
        //                    do
        //                    {
        //                        openIdResultJson = UserApi.Get(accessToken, nextOpenid);
        //        
        //                        nextOpenid = openIdResultJson.next_openid;
        //        
        //                        if (openIdResultJson.data != null)
        //                        {
        //                            foreach (string openid in openIdResultJson.data.openid)
        //                            {
        //                                UserInfoJson userJson = UserApi.Info(accessToken, openid);
        //                                list.Add(userJson);
        //                            }
        //                        }
        //        
        //                    } while (openIdResultJson != null && !string.IsNullOrEmpty(openIdResultJson.next_openid));
        //        
        //                    return Json(list, JsonRequestBehavior.AllowGet);
        //                } 
        #endregion

        #region 上传素材
        /// <summary>
        /// 上传临时素材-上传一张图片为例
        /// </summary>
        /// <returns></returns>
        public UploadTemporaryMediaResult UploadTemporaryMedia()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            string imgUrl = Server.MapPath("~/logo.png");
            UploadTemporaryMediaResult mediaResult = MediaApi.UploadTemporaryMedia(accessToken, UploadMediaFileType.image, imgUrl);
            return mediaResult;

        }


        /// <summary>
        /// 上传永久素材 主要用到MediaApi这个接口
        /// </summary>
        /// <returns></returns>
        public UploadImgResult UploadImg()
        {
            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            string imgFilePath = Server.MapPath("~/logo.png");
            return MediaApi.UploadImg(accessToken, imgFilePath);
        }
        #endregion


        /// <summary>
        /// 群发消息
        /// </summary>
        /// <param name="type">
        /// 群发类型Text = 0,
        /// News = 1,
        /// Music = 2,
        /// Image = 3,
        /// Voice = 4,
        /// Video = 5
        /// <param name="groupId">要发送的用户分组ID 发给所有的时候可以不输入</param>
        /// <returns>返回消息ID</returns>
        public string SendGroupMessageAll(int type, int groupId = -1)
        {

            var accessToken = AccessTokenContainer.TryGetAccessToken(AppId, AppSecret);
            SendResult result = null;
            bool isToAll = (groupId == -1);
            string imgFilePath = Server.MapPath("~/logo.png");
            switch ((ResponseMsgType) type)
            {
                case ResponseMsgType.Text:
                    result = GroupMessageApi.SendGroupMessageByGroupId(accessToken, groupId.ToString(),
                        "测试的群发文本内容---来自北盟网校", GroupMessageType.text, isToAll);
                    break;
                case ResponseMsgType.Image:
                    UploadTemporaryMediaResult mediaImage =
                        MediaApi.UploadTemporaryMedia(accessToken, UploadMediaFileType.image, imgFilePath);
                    result = GroupMessageApi.SendGroupMessageByGroupId(accessToken, groupId.ToString(),
                        mediaImage.media_id, GroupMessageType.image, isToAll);
                    break;
                case ResponseMsgType.News:
                    UploadTemporaryMediaResult mediaResultImage =
                        MediaApi.UploadTemporaryMedia(accessToken, UploadMediaFileType.image, imgFilePath);
                    NewsModel[] newsModels = new NewsModel[5];
                    for (int i = 0; i < 5; i++)
                    {
                        newsModels[i] = new NewsModel();
                        newsModels[i].title = "标题" + i;
                        newsModels[i].author = "作者";
                        newsModels[i].thumb_media_id = mediaResultImage.media_id;
                        newsModels[i].show_cover_pic = "http://weixin.bamn.cn/logo.png";
                        newsModels[i].content = "内容";
                        newsModels[i].content_source_url = "http://www.bamn.cn";
                        newsModels[i].digest = "描述";
                    }
                    var mediaResultNews = MediaApi.UploadTemporaryNews(accessToken, 10000, newsModels);
                    result = GroupMessageApi.SendGroupMessageByGroupId(accessToken, groupId.ToString(),
                        mediaResultNews.media_id, GroupMessageType.mpnews, isToAll);
                    break;
            }

            return result.errcode + "-" + result.errmsg;
        }
    }
}