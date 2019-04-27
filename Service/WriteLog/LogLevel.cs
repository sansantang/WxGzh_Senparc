using System;
using System.Configuration;

namespace Service.WriteLog
{
    public class LogLevel
    {
        public static string AppKey(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息 
        /// </summary>

        public static int LOG_LEVENL
        {
            get
            {
                string log_levenl = "0";
                if (AppKey("log_leven") != "")
                {
                    log_levenl = AppKey("log_leven");
                }
                return Convert.ToInt32(log_levenl);
            }
        }
    }
}