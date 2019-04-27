using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace Service.TemplateData
{
    /// <summary>
    /// 定义模版中的字段属性（需与微信模版中的一致）
    /// </summary>
    public class ProductTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem product { get; set; }
        public TemplateDataItem price { get; set; }
        public TemplateDataItem time { get; set; }
        public TemplateDataItem remark { get; set; }
    }
}