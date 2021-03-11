using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HISInterface.DBContext;
using HISInterface.Filters;
using HISInterface.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;

namespace HISInterface.Controllers
{
    [ServiceFilter(typeof(CustomExceptionFilterAttribute))]
    [TypeFilter(typeof(CustomExceptionFilterAttribute))]
    [Route("api/[controller]")]
    [ApiController]
    public class IPBtestController : ControllerBase
    {
        public IPBtestController(DB dB, IConfiguration configuration)
        {
            this.db = dB;
        }

        public DB db { get; set; }

        [HttpGet, Route("Get")]
        public IActionResult Get()
        {
            this.UpdateSql(SqlType: "lis");
            string sql = "select  r.lab_apply_no \"id\", r.paritemname \"title\",r.micro_flag \"iswsw\",r.patient_id \"patientId\",\'' \"patientName\",r.report_date_time \"sendTime\",\'枝江市人民医院' \"hospitalName\" from v_jhmk_lis_report r where r.patient_id = '0000200745' and r.is_valid = '1'";
            var dt = Methods.SqlQuery(db, sql);
            ArrayList arr = Methods.getJObject(dt);


            return new ObjectResult(arr);

        }
        [HttpPost, Route("queryInspectionReport")]
        public IActionResult queryInspectionReport([FromBody] dynamic dy)
        {
            this.UpdateSql("lis");
            JObject res = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nqueryInspectionReport的入参" + res.ToString());
            string patientno = string.Empty;
            try
            {
                patientno = res.GetValue("cardNo").ToString();
            }
            catch
            {
                return new JsonResult(new { msg = "你输入的参数有误！", data = "参数错误", code = 403 });
            }
            string sql = $"select  r.lab_apply_no \"id\", r.paritemname \"title\",r.micro_flag \"iswsw\",r.patient_id \"patientId\",\'' \"patientName\",r.report_date_time \"sendTime\",\'枝江市人民医院' \"hospitalName\" from v_jhmk_lis_report r where   (r.patient_id='{patientno}' and r.file_visit_type='2') or (r.patient_id = '{patientno}' and r.file_visit_type = '0') and r.is_valid = '1'";
            var dt = Methods.SqlQuery(db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告的信息！", data = "查询结果为空", code = 404 });
            }
            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        }
        [HttpPost, Route("queryInspectionReportDetails")]
        public IActionResult queryInspectionReportDetails([FromBody] dynamic dy)
        {
            this.UpdateSql("lis");
            JObject jObject = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nLIS接口的入参" + jObject.ToString());
            string id = string.Empty;
            string iswsw = string.Empty;
            try
            {
                id = jObject.GetValue("id").ToString();
                iswsw = jObject.GetValue("iswsw").ToString();
            }
            catch
            {
                return new JsonResult(new { msg = "你输入的参数有误！", data = "参数错误", code = 403 });
            }
            string sql = string.Empty;
            if (iswsw == "0")
            {
                sql = $"select p.lab_item_name \"item\",p.lab_item_name \"itemName\", p.lab_item_sname \"name\", p.result  \"value\",p.result_range \"reference\", p.units \"unit\", p.status \"status\", ' '   \"remark\"  from v_jhmk_report_item p where p.lis_apply_no = '{id}'";
            }
            else if (iswsw == "1")
            {
                sql = $"select  m.lab_item_name \"item\",m.micro_name \"itemName\",m.anti_name \"name\",m.susquan  \"value\",m.ref_rang \"reference\",'' \"unit\",m.suscept \"status\",m.desc_name \"remark\"  from v_jhmk_report_micro m where m.lab_apply_no = '{id}'";
            }
            else
            {
                return new JsonResult(new { msg = "iswsw参数找不到对应值！", data = "参数错误", code = 403 });
            }
            var dt = Methods.SqlQuery(db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告详情的信息！", data = "查询结果为空", code = 404 });
            }
            if (iswsw == "1")
            {
                IEnumerable<JObject> j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(arr));
                string[] ids = j.Select(s => s.GetValue("itemName").ToString()).Distinct().ToArray();
                List<object> objects = new List<object>();
                foreach (string item in ids)
                {
                    objects.Add(new JsonResult(new { item = j.FirstOrDefault(u => u.GetValue("itemName").ToString() == item).GetValue("item").ToString(), itemName = item.ToString(), details = (from JObject x in j where x.GetValue("itemName").ToString() == item select x).Select(s => new { name = s.GetValue("name").ToString(), value = s.GetValue("value").ToString(), reference = s.GetValue("reference").ToString(), unit = s.GetValue("unit").ToString(), status = s.GetValue("status").ToString(), remark = s.GetValue("remark").ToString() }) }).Value);
                }
                return new JsonResult(new { msg = "查询成功！", data = objects, code = 200 });
            }
            else
            {
                return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
            }

        }
        [HttpPost, Route("queryMedicalReport")]
        public IActionResult queryMedicalReport([FromBody] dynamic dy)
        {
            this.UpdateSql("pacs");
            JObject jobj = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nPACS的入参" + jobj.ToString());
            string CardNo = string.Empty;
            try
            {
                CardNo = jobj.GetValue("cardNo").ToString();
            }
            catch
            {
                return new JsonResult(new { msg = "你输入的参数有误！", data = "参数错误", code = 403 });
            }
            string sql = $"select e.EXAM_APPLY_NO \"id\", e.EXAM_ITEM_CODE \"title\",e.PATIENT_ID \"patientId\",e.REPORT_PHYSICIAN \"patientName\",e.EXAM_DATE_TIME \"sendTime\",'枝江市人民医院' \"hospitalName\",e.REPORT_TEXT \"describe\",e.REPORT_TEXT_1 \"result\"  from v_jh_exam_report e where e.PATIENT_ID='{CardNo}'";
            var dt = Methods.SqlQuery(db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告详情的信息！", data = "查询结果为空", code = 404 });
            }

            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });

        }
        #region 住院预交金收取接口
        [HttpPost, Route("PayInPrepayCost")]
        public IActionResult PayInPrepayCost([FromBody] dynamic dy)
        {
            this.UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n住院预交金收取接口的入参" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            #region 绑定存储过程的参数
            try
            {
                oralist.Add(Methods.GetInput("InpatientNo", j.GetValue("InpatientNo").ToString()));
                oralist.Add(Methods.GetInput("IdCard", j.GetValue("IdCard").ToString()));
                oralist.Add(Methods.GetInput("TransNo", j.GetValue("TransNo").ToString()));
                oralist.Add(new OracleParameter() { ParameterName = "YJCost", OracleDbType = OracleDbType.Decimal, Value = Convert.ToDecimal(j.GetValue("YJCost").ToString()) });
                oralist.Add(new OracleParameter() { ParameterName = "YJTime", OracleDbType = OracleDbType.Date, Value =Convert.ToDateTime(j.GetValue("YJTime").ToString()) });
                oralist.Add(Methods.GetInput("PayMode", j.GetValue("PayMode").ToString()));
            }
            catch (Exception)
            {
                return new ObjectResult(new { msg = "操作失败", data = "请检查你的入参是否不一致", code = 404 });
            }
            oralist.Add(Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 200));
            #endregion
            var ds = Methods.SqlQuery(db, "zjhis.PKG_ZHYY_MZ.PRC_InPrepayPayedConfirm", oralist.ToArray());
            ObjectResult resobj = Methods.GetResult(oralist, ds);
            Console.WriteLine("返回数据：\n" + JsonConvert.SerializeObject(resobj.Value));
            return resobj;
        }
        #endregion 
        /// <summary>
        /// 切换数据库的方法
        /// </summary>
        /// <param name="SqlType"></param>
        private void UpdateSql(string SqlType)
        {
            switch (SqlType.ToUpper())
            {
                case "HIS":
                    this.db = Logic.SqlMethods.GetHISDBCSK(db);
                    break;
                case "LIS":
                    this.db = Logic.SqlMethods.GetLISDB(db);
                    break;
                case "PACS":
                    this.db = Logic.SqlMethods.GetPacsDB(db);
                    break;
                default:
                    this.db = db;
                    break;
            }
        }
    }
}