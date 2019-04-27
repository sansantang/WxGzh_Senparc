using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;

namespace WxGzh_Senparc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            var isGLobalDebug = true;//设置全局 Debug 状态
            var senparcSetting = SenparcSetting.BuildFromWebConfig(isGLobalDebug);
            var register = RegisterService.Start(senparcSetting).UseSenparcGlobal();//CO2NET全局注册，必须！

            var isWeixinDebug = true;//设置微信 Debug 状态
            var senparcWeixinSetting = SenparcWeixinSetting.BuildFromWebConfig(isWeixinDebug);
            register.UseSenparcWeixin(senparcWeixinSetting, senparcSetting);////微信全局注册，必须！

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
