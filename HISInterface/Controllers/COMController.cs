using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using HISInterface.DBContext;
using HISInterface.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;

namespace HISInterface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class COMController : ControllerBase
    {
        public COMController(DB db,IConfiguration configuration)
        {
            this.db = db; this.configuration = configuration;
        }
       
      
        public DB db { get; set; }
        public IConfiguration configuration { get; }

        [HttpGet,Route("Get")]
        public IActionResult Get()
        {
            return Content("测试");
        }
        #region 获取科室的医生排班情况
        /// <summary>
        /// 获取科室的医生排班情况
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost, Route("getDoctorScheduling")]
        public IActionResult getDoctorScheduling(dynamic Obj)//直接点参数
        {
            this.UpdateSql("his");
            JObject job = Methods.dynamicToJObject(Obj);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n获取科室的医生排班情况的入参" + job.ToString());
            //输入参数
            List<OracleParameter> parems = new List<OracleParameter>();
            try
            {
                parems.Add(Logic.Methods.GetInput("deptId", job.GetValue("deptId").ToString()));
                parems.Add(Logic.Methods.GetInput("Docdate", job.GetValue("date").ToString()));
                parems.Add(Logic.Methods.GetInput("hospitalId", job.GetValue("hospitalId").ToString()));
            }
            catch (Exception)
            {
                return new ObjectResult(new { msg = "请求失败", data = "请检查您所传递的参数是否有误！", code = "500" });
            }
            //输出参数
            parems.Add(Logic.Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            parems.Add(Logic.Methods.GetOutput("ReturnCode", OracleDbType.Int32, 200));
            parems.Add(Logic.Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 200));

            var ds = Logic.Methods.SqlQuery(db, @"zjhis.PKG_ZHYY_MZ.PRC_OutpDoctorQuery", parems.ToArray());
            ObjectResult obj = Methods.GetResult(parems, ds);
        //    Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(obj));
            return obj;
        } 
        #endregion


        #region 获得医生某天排班序号
        /// <summary>
        /// 获得医生某天排班序号
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost, Route("getDoctorAppointmentOrders")]
        public IActionResult getDoctorAppointmentOrders(dynamic Obj)//直接点参数
        {
            this.UpdateSql("HIS");
            JObject job = Methods.dynamicToJObject(Obj);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n获得医生某天排班序号的入参" + job.ToString());
            //输入参数
            List<OracleParameter> parems = new List<OracleParameter>();
            try
            {
                parems.Add(Logic.Methods.GetInput("shemaId", job.GetValue("shemaId").ToString()));
            }
            catch (Exception)
            {
                return new ObjectResult(new { msg = "请求失败", data = "请检查您所传递的参数是否有误！", code = "500" });
            }
            //输出参数
            parems.Add(Logic.Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            parems.Add(Logic.Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            parems.Add(Logic.Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 200));

            var ds = Logic.Methods.SqlQuery(db, @"zjhis.PKG_ZHYY_MZ.PRC_OutpDoctorQueryBySortId", parems.ToArray());
            ObjectResult obj = Methods.GetResult(parems, ds);
          //  Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(obj));
            return obj;
        } 
        #endregion


        #region 获取医院科目列表以及科室
        /// <summary>
        /// 获取医院科目列表以及科室
        /// PRC_OutpRegisterDeptQuery
        /// </summary>
        [HttpPost, Route("getAllDeptRoom")]
        public IActionResult getAllDeptRoom()
        {
            this.UpdateSql("HIS");
            //JObject job = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n获取医院科目列表以及科室参数为空");
            //输入参数
            List<OracleParameter> parems = new List<OracleParameter>();

            //输出参数
            parems.Add(Logic.Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            parems.Add(Logic.Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            parems.Add(Logic.Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 200));
            var ds = Logic.Methods.SqlQuery(db, @"zjhis.PKG_ZHYY_MZ.PRC_OutpRegisterDeptQuery", parems.ToArray());
            ObjectResult obj = Methods.GetResultAndHaveSon(parems, ds);
          //  Console.WriteLine("返回参数："+JsonConvert.SerializeObject(obj));
            return obj;
        }
        #endregion


        #region 缴费查询
        [HttpPost,Route("getHospitalItemList")]
        public IActionResult getHospitalItemList([FromBody] dynamic dynamic)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n缴费查询的入参" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("appointmentId", j.GetValue("appointmentId").ToString()));
                oralist.Add(Methods.GetInput("bizType", j.GetValue("bizType").ToString()));
                oralist.Add(Methods.GetInput("status", j.GetValue("status").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            var ds = Methods.SqlQuery(db, "zjhis.PKG_ZHYY_MZ.getHospitalItemList", oralist.ToArray());
           // Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 国家绩效考核指标查询接口
        [HttpPost,Route("QueryZBInfoByDate")]
        public IActionResult QueryZBInfoByDate([FromBody]dynamic dynamic)
        {
            this.UpdateSql("his");
            JObject jobj = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n国家绩效考核指标查询接口的入参" + jobj.ToString());
            string date = Convert.ToDateTime(jobj.GetValue("date").ToString()).ToString("yyyy-MM-01");
            string sql = $"select p.target_code,p.target_name,p.target_value from zjhis.per_for_mance p where p.assessment_time=to_date('{date}','yyyy-mm-dd')";
            var dt = Methods.QuerySql(this.db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到如何指标的信息！", data = "查询结果为空", code = 404 });
            }

            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        }
        #endregion

        #region 住院人数统计
        [HttpPost, Route("QueryInMainNum")]
        public IActionResult QueryInMainNum()
        {
            this.UpdateSql("his");
            //JObject jobj = Methods.dynamicToJObject(dynamic);
            //string date = Convert.ToDateTime(jobj.GetValue("date").ToString()).ToString("yyyy-MM-01");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n住院人数统计无入参");
            string sql = $"select   count(*) \"InMainCount\"   from zjhis.fin_ipr_inmaininfo a where a.in_state = 'I' and a.patient_no not like '%B%'  and a.dept_code not in  ('0120','0124')";
            var dt = Methods.QuerySql(this.db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到住院信息！", data = "查询结果为空", code = 404 });
            }
           // Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(new JsonResult(new { msg = "查询成功！", data = arr, code = 200 })));
            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        }
        #endregion

        #region 门诊挂号人数统计
        [HttpPost, Route("QueryRegisterNum")]
        public IActionResult QueryRegisterNum()
        {
            this.UpdateSql("his");
            //JObject jobj = Methods.dynamicToJObject(dynamic);
            //string date = Convert.ToDateTime(jobj.GetValue("date").ToString()).ToString("yyyy-MM-01");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n挂号人数统计无入参");
            string sql = $"select count(*) \"RegCount\" from zjhis.fin_opr_register a where a.reg_date > trunc(sysdate) and a.reg_date < trunc(sysdate) + 1  and a.trans_type = '1'  and not exists(select 1  from zjhis.fin_opr_register b  where b.clinic_code = a.clinic_code  and b.trans_type = '2')";
            var dt = Methods.QuerySql(this.db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到挂号信息！", data = "查询结果为空", code = 404 });
            }

            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        }
        #endregion


        #region 清单查询
        [HttpPost, Route("getHospitalListing")]
        public IActionResult getHospitalListing(dynamic Obj)//直接点参数
        {
            this.UpdateSql("his");
            JObject job = Methods.dynamicToJObject(Obj);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n清单查询的入参" + job.ToString());
            //输入参数
            List<OracleParameter> parems = new List<OracleParameter>();
            try
            {
                parems.Add(Logic.Methods.GetInput("appointmentId", job.GetValue("hospitalNo").ToString()));
                parems.Add(Logic.Methods.GetInput("bizType", job.GetValue("type").ToString()));
                parems.Add(Logic.Methods.GetInput("patientName", job.GetValue("patientName").ToString()));
                parems.Add(Logic.Methods.GetInput("QueryDate", job.GetValue("date").ToString()));
            }
            catch (Exception)
            {
                return new ObjectResult(new { msg = "请求失败", data = "请检查您所传递的参数是否有误！", code = "500" });
            }
            //输出参数
            parems.Add(Logic.Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            parems.Add(Logic.Methods.GetOutput("ReturnCode", OracleDbType.Int32, 200));
            parems.Add(Logic.Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 200));

            var ds = Logic.Methods.SqlQuery(db, @"zjhis.PKG_ZHYY_MZ.GetQDForDate", parems.ToArray());
            if (job.GetValue("type").ToString()=="2")
            {
                ArrayList arr = Methods.getJObject(ds);
                if (arr.Count == 0 || arr == null)
                {
                    return new JsonResult(new { msg = "没有找到患者详细的信息！", data = "查询结果为空", code = 404 });
                }
                IEnumerable<JObject> j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(arr));
                string[] ids = j.Select(s => s.GetValue("hospitalNo").ToString()).Distinct().ToArray();
                List<object> objects = new List<object>();
                foreach (string item in ids)
                {
                    objects.Add(new JsonResult(new { hospitalName = j.FirstOrDefault(u => u.GetValue("hospitalNo").ToString() == item).GetValue("hospitalName").ToString(), patientName = j.FirstOrDefault(u => u.GetValue("hospitalNo").ToString() == item).GetValue("patientName").ToString(), hospitalNo = item.ToString(), inTime = j.FirstOrDefault(u => u.GetValue("hospitalNo").ToString() == item).GetValue("inTime").ToString(), outTime = j.FirstOrDefault(u => u.GetValue("hospitalNo").ToString() == item).GetValue("outTime").ToString(), detail = (from JObject x in j where x.GetValue("hospitalNo").ToString() == item select x).Select(s => new { titleName = s.GetValue("titleName").ToString(), unit = s.GetValue("unit").ToString(), num = s.GetValue("num").ToString(), price = s.GetValue("price").ToString() }) }).Value);
                }
              //  Console.WriteLine("返回参数：\n" + JsonConvert.SerializeObject(new JsonResult(new { msg = "查询成功！", data = objects, code = 200 })));
                return new JsonResult(new { msg = "查询成功！", data = objects, code = 200 });
            }
            else
            {
                //Console.WriteLine("返回参数：\n" + JsonConvert.SerializeObject(Methods.GetResult(parems, ds)));
                return Methods.GetResult(parems, ds);
            }
         }
        #endregion

        #region 挂号看诊状态查询接口
        [HttpPost, Route("QueryRegStatus")]
        public IActionResult QueryRegStatus([FromBody] dynamic dynamic)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n挂号看诊状态查询接口的入参" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("CardNo", j.GetValue("CardNo").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            var ds = Methods.SqlQuery(db, "zjhis.PKG_ZHYY_MZ.QueryRegStatus", oralist.ToArray());
            //Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 住院患者基本信息查询接口
        [HttpPost, Route("queryPatientInfoByHospitalNo")]
        public IActionResult queryPatientInfoByHospitalNo([FromBody] dynamic dynamic)
        {
            this.UpdateSql("his");
            JObject jobj = Methods.dynamicToJObject(dynamic);
            //打印入参日志
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n住院患者基本信息查询接口的入参" + jobj.ToString());
            string PatientNo = jobj.GetValue("hospitalNo").ToString();
            string type = jobj.GetValue("type").ToString();
            //医院的判断
            string HosName = type == "1" ? "枝江市人民医院" : (type == "2" ? "枝江市中医医院" : (type == "3" ? "枝江市妇幼保健院" : "其他医院"));
            string sql = $"select  '{HosName}' as \"hospitalName\", i.name as \"patientName\",i.patient_no as \"patientId\",i.linkman_tel as \"phone\", i.card_no as \"cardNo\" from fin_ipr_inmaininfo i where i.patient_no = '{PatientNo}'";
            var dt = Methods.QuerySql(this.db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到住院患者的信息！", data = "查询结果为空", code = 404 });
            }
            JsonResult js = new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
            //Console.WriteLine("出参："+JsonConvert.SerializeObject(js.Value));
            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        }
        #endregion


        #region 获取科室未来15天的医生排班情况
        /// <summary>
        /// 获取科室未来15天的医生排班情况
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        [HttpPost, Route("getDoctorByWL15")]
        public IActionResult getDoctorByWL15(dynamic Obj)//直接点参数
        {
            this.UpdateSql("his");
            JObject job = Methods.dynamicToJObject(Obj);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n获取科室未来15天的医生排班情况的入参" + job.ToString());
            //输入参数
            List<OracleParameter> parems = new List<OracleParameter>();
            try
            {
                parems.Add(Logic.Methods.GetInput("deptId", job.GetValue("deptId").ToString()));
                parems.Add(Logic.Methods.GetInput("hospitalId", job.GetValue("hospitalId").ToString()));
            }
            catch (Exception)
            {
                return new ObjectResult(new { msg = "请求失败", data = "请检查您所传递的参数是否有误！", code = "500" });
            }
            //输出参数
            parems.Add(Logic.Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            parems.Add(Logic.Methods.GetOutput("ReturnCode", OracleDbType.Int32, 200));
            parems.Add(Logic.Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 200));

            var ds = Logic.Methods.SqlQuery(db, @"zjhis.PKG_ZHYY_MZ.PRC_DoctorQueryToWL15", parems.ToArray());
            ObjectResult obj = Methods.GetResult(parems, ds);
            //Console.WriteLine("返回参数：\n" + JsonConvert.SerializeObject(obj));
            return obj;
        }
        #endregion

        #region 电子发票查询接口
        [HttpPost, Route("QueryEinvoiceBill")]
        public IActionResult QueryEinvoiceBill([FromBody] dynamic dy)
        {
            this.UpdateSql("PJWHIS");
            JObject j = Methods.dynamicToJObject(dy);
            Console.WriteLine(" 枝江市人民医院电子发票接口请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "电子发票请求入参" + j.ToString());

            if (!j.ContainsKey("patientId"))
            {
                return new ObjectResult(new { msg = "请求失败", data = "入参传入错误，没有找到参数名为patientId的参数！", code = "500" });
            }

            if (!j.ContainsKey("begin"))
            {
                return new ObjectResult(new { msg = "请求失败", data = "入参传入错误，没有找到参数名为begin的参数！", code = "500" });
            }
            if (!j.ContainsKey("end"))
            {
                return new ObjectResult(new { msg = "请求失败", data = "入参传入错误，没有找到参数名为end的参数！", code = "500" });
            }
            string start = j.GetValue("begin", StringComparison.OrdinalIgnoreCase).ToString();
            string End = j.GetValue("end", StringComparison.OrdinalIgnoreCase).ToString();
            string PatientId = j.GetValue("patientId", StringComparison.OrdinalIgnoreCase).ToString();
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(End))
            {
                start = System.DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss");
                End = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            string sql = "select  e.inpatient_no as \"inpatientno\", e.patient_no as \"patientno\", e.card_no as  \"cardno\",  e.hisinvoice_no as \"hisinvoiceno\",  e.his_name as \"hisname\",  replace(to_char(e.pictureurl),'http://172.22.156.72:7001','https://api.lizhi.co') as \"pictureurl\",  e.tot_cost as \"totcost\", e.oper_date as \"operdate\" from zjhis.fin_com_einvoicebill e where e.vaild_flag = '1' and e.cancel_flag = '1' and e.exe_flag = '1'  and e.patient_no is not null   and e.patient_no = '{0}'  and e.oper_date >= to_date('{1}', 'yyyy-mm-dd hh24:mi:ss') and e.oper_date < to_date('{2}', 'yyyy-mm-dd hh24:mi:ss') union all  select e.inpatient_no as \"inpatientno\", e.patient_no as \"patientno\", e.card_no as  \"cardno\",  e.hisinvoice_no as \"hisinvoiceno\", e.his_name as \"hisname\", replace(to_char(e.pictureurl),'http://172.22.156.72:7001','https://api.lizhi.co') as \"pictureurl\",  e.tot_cost as \"totcost\",  e.oper_date as \"operdate\" from zjhis.fin_com_einvoicebill e where e.vaild_flag = '1' and e.cancel_flag = '1'  and e.exe_flag = '1'   and e.card_no is not null  and e.card_no = '{0}' and e.oper_date >= to_date('{1}', 'yyyy-mm-dd hh24:mi:ss')   and e.oper_date < to_date('{2}', 'yyyy-mm-dd hh24:mi:ss')";
            sql = string.Format(sql, PatientId, start, End);
            var dt = Methods.QuerySql(this.db, sql);
            ArrayList arr = Methods.getJObject(dt);
            if (arr.Count == 0 || dt == null)
            {
                return new JsonResult(new { msg = "没有找到任何电子发票的信息！", data = "查询结果为空", code = 404 });
            }

            return new JsonResult(new { msg = "查询成功！", data = dt, code = 200 });
        }
        #endregion

        #region 手机短信接口
        [HttpPost,Route("GetSmsInfo")]
        public IActionResult GetSmsInfo([FromBody ] dynamic dynamic)
        {
            JObject j = Methods.dynamicToJObject(dynamic);
            string phone = j.GetValue("phone").ToString();
            string content = j.GetValue("content").ToString();
            string result = Methods.getSMSPost(phone, content);
            JObject jObject = JsonConvert.DeserializeObject<JObject>(result).GetValue("SendSmsResponse").ToObject<JObject>();
            return new ObjectResult(new {success=jObject.GetValue("success").ToString(),rspcod=jObject.GetValue("rspcod").ToString(),msgGroup=jObject.GetValue("msgGroup").ToString() });
        }
        #endregion

        

        #region 获取扫码支付二维码
        [HttpGet,Route("GetQRImage")]
        public IActionResult GetQRImage(string ClinicNo)
        {
           Stream imgarr = Functions.GetPictureurl(ClinicNo); 
          
            return  File(imgarr, "image/jpeg");
        }
        #endregion


        /// <summary>
        /// 切换数据库的方法
        /// </summary>
        private void UpdateSql(string SqlType)
        {
            switch(SqlType.ToUpper())
            {
                case "CSHIS":
                    this.db = Logic.SqlMethods.GetHISDBCSK(db);
                    break;
                case "HIS":
                    this.db = Logic.SqlMethods.GetHISDBZSK(db);
                    break;
                case "PJWHIS":
                    this.db = Logic.SqlMethods.GetHISDBCSK2(db);
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