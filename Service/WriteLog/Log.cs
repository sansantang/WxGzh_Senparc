using System;
using System.IO;
using System.Text;

namespace Service.WriteLog
{
    public class Log
    {
        //在网站根目录下创建日志目录
        //public static string path = HttpContext.Current.Request.PhysicalApplicationPath + "logs";
        public static string path = System.AppDomain.CurrentDomain.BaseDirectory + "logs";


        /**
         * 向日志文件写入调试信息
         * @param className 类名
         * @param content 写入内容
         * @param remark 备注
         */
        public static void MostDebug(string className, string content, string remark)
        {
            if (LogLevel.LOG_LEVENL >= 4)
            {
                WriteLog("MostDebug", className, content, remark);
            }
        }

        /**
         * 向日志文件写入调试信息
         * @param className 类名
         * @param content 写入内容
         * @param remark 备注
         */
        public static void Debug(string className, string content, string remark)
        {
            if (LogLevel.LOG_LEVENL >= 3)
            {
                WriteLog("DEBUG", className, content, remark);
            }
        }

        /**
        * 向日志文件写入运行时信息
        * @param className 类名
        * @param content 写入内容
        * @param remark 备注
        */
        public static void Info(string className, string content, string remark)
        {
            if (LogLevel.LOG_LEVENL >= 2)
            {
                WriteLog("INFO", className, content, remark);
            }
        }

        /**
        * 向日志文件写入出错信息
        * @param className 类名
        * @param content 写入内容
        * @param remark 备注
        */
        public static void Error(string className, string content, string remark)
        {
            if (LogLevel.LOG_LEVENL >= 1)
            {
                WriteLog("ERROR", className, content, remark);
            }
        }

        /**
        * 实际的写日志操作
        * @param type 日志记录类型
        * @param className 类名
        * @param content 写入内容
        * @param remark 备注
        */
        protected static void WriteLog(string type, string className, string content, string remark)
        {
            //用户浏览器标识
            //string agent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] == null ? "后端调用" : HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();

            if (!Directory.Exists(path))//如果日志目录不存在就创建
            {
                Directory.CreateDirectory(path);
            }


            string time = DateTime.Now.ToString("HH:mm:ss.fff");//获取当前系统时间
            string filename = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";//用日期对日志文件命名

            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
            }

            //解决【正由另一进程使用，因此该进程无法访问该文件】
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {

                DateTime now = DateTime.Now;

                string write_content = "[" + time + "]【" + type + "】" + remark + " (" + className + "): " + content;//+ "(" + agent + ")";
                byte[] bytes = null;
                if (now.Hour >= 0 && now.Hour < 8)
                {
                    //向日志文件写入内容
                    write_content = "「凌晨」" + write_content;
                    bytes = Encoding.Default.GetBytes(write_content);
                }
                else if (now.Hour >= 8 && now.Hour < 12)
                {
                    //向日志文件写入内容
                    write_content = "【上午】" + write_content;
                    bytes = Encoding.Default.GetBytes(write_content);
                }
                else if (now.Hour >= 12 && now.Hour < 18)
                {
                    //向日志文件写入内容
                    write_content = "『下午』" + write_content;
                    bytes = Encoding.Default.GetBytes(write_content);
                }
                else if (now.Hour >= 18 && now.Hour < 24)
                {
                    //向日志文件写入内容
                    write_content = "〖晚上〗" + write_content;
                    bytes = Encoding.Default.GetBytes(write_content);
                }
                //2、写操作
                fs.Position = fs.Length;
                fs.Write(bytes, 0, bytes.Length);
                //byte(13) byte(10)等效于 \r\n，直接输入\r\n不起作用
                fs.WriteByte(13);
                fs.WriteByte(10);
                fs.Flush();//清空流
            }

        }
    }
}