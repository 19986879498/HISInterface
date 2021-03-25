using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
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
    public class COMtestController : ControllerBase
    {
        public COMtestController(DB db,IConfiguration configuration)
        {
            this.db = db; this.configuration = configuration;
        }
       
      
        public DB db { get; set; }
        public IConfiguration configuration { get; }

        /// <summary>
        /// 测试api
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("Get")]
        public IActionResult Get()
        {
            return Content("测试");
        }
        #region 获取科室的医生排班情况
        /// <summary>
        /// 获取科室的医生排班情况PRC_OutpDoctorQuery
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "deptId":"科室id",
        ///      "date":"时间",
        ///      "hospitalId":""
        ///  }
        /// </remarks>
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

            var ds = Logic.Methods.SqlQuery(db, @"PKG_ZHYY_MZ.PRC_OutpDoctorQuery", parems.ToArray());
            ObjectResult obj = Methods.GetResult(parems, ds);
            //    Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(obj));
            return obj;
        }
        #endregion


        #region 获得医生某天排班序号
        /// <summary>
        /// 获得医生某天排班序号PRC_OutpDoctorQueryBySortId
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "shemaId":"排班id"
        ///  }
        /// </remarks>
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

            var ds = Logic.Methods.SqlQuery(db, @"PKG_ZHYY_MZ.PRC_OutpDoctorQueryBySortId", parems.ToArray());
            ObjectResult obj = Methods.GetResult(parems, ds);
            //  Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(obj));
            return obj;
        }
        #endregion


        #region 获取医院科目列表以及科室
        /// <summary>
        /// 获取医院科目列表以及科室PRC_OutpRegisterDeptQuery
        /// PRC_OutpRegisterDeptQuery
        /// </summary>
        /// <returns></returns>
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
            var ds = Logic.Methods.SqlQuery(db, @"PKG_ZHYY_MZ.PRC_OutpRegisterDeptQuery", parems.ToArray());
            ObjectResult obj = Methods.GetResultAndHaveSon(parems, ds);
            //  Console.WriteLine("返回参数："+JsonConvert.SerializeObject(obj));
            return obj;
        }
        #endregion


        #region 缴费查询
        /// <summary>
        /// 缴费查询getHospitalItemList
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "appointmentId":"门诊流水号",
        ///      "bizType":"",
        ///      "status":"状态"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        [HttpPost, Route("getHospitalItemList")]
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
            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.getHospitalItemList", oralist.ToArray());
            // Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 国家绩效考核指标查询接口
        /// <summary>
        /// 国家绩效考核指标查询接口
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "date":"查询时间"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        [HttpPost, Route("QueryZBInfoByDate")]
        public IActionResult QueryZBInfoByDate([FromBody] dynamic dynamic)
        {
            this.UpdateSql("his");
            JObject jobj = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n国家绩效考核指标查询接口的入参" + jobj.ToString());
            string date = Convert.ToDateTime(jobj.GetValue("date").ToString()).ToString("yyyy-MM-01");
            string sql = $"select p.target_code,p.target_name,p.target_value from per_for_mance p where p.assessment_time=to_date('{date}','yyyy-mm-dd')";
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
        /// <summary>
        /// 住院人数统计
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("QueryInMainNum")]
        public IActionResult QueryInMainNum()
        {
            this.UpdateSql("his");
            //JObject jobj = Methods.dynamicToJObject(dynamic);
            //string date = Convert.ToDateTime(jobj.GetValue("date").ToString()).ToString("yyyy-MM-01");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n住院人数统计无入参");
            string sql = $"select   count(*) \"InMainCount\"   from fin_ipr_inmaininfo a where a.in_state = 'I' and a.patient_no not like '%B%'  and a.dept_code not in  ('0120','0124')";
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
        /// <summary>
        /// 门诊挂号人数统计
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("QueryRegisterNum")]
        public IActionResult QueryRegisterNum()
        {
            this.UpdateSql("his");
            //JObject jobj = Methods.dynamicToJObject(dynamic);
            //string date = Convert.ToDateTime(jobj.GetValue("date").ToString()).ToString("yyyy-MM-01");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n挂号人数统计无入参");
            string sql = $"select count(*) \"RegCount\" from fin_opr_register a where a.reg_date > trunc(sysdate) and a.reg_date < trunc(sysdate) + 1  and a.trans_type = '1'  and not exists(select 1  from fin_opr_register b  where b.clinic_code = a.clinic_code  and b.trans_type = '2')";
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
        /// <summary>
        /// 清单查询GetQDForDate
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "hospitalNo":"门诊流水号",
        ///      "type":"业务类别",
        ///      "patientName":"患者姓名",
        ///      "date":"查询时间"
        ///  }
        /// </remarks>
        /// <param name="Obj"></param>
        /// <returns></returns>
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

            var ds = Logic.Methods.SqlQuery(db, @"PKG_ZHYY_MZ.GetQDForDate", parems.ToArray());
            if (job.GetValue("type").ToString() == "2")
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
        /// <summary>
        /// 挂号看诊状态查询接口QueryRegStatus
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "CardNo":"患者卡号",
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
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
            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.QueryRegStatus", oralist.ToArray());
            //Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 住院患者基本信息查询接口
        /// <summary>
        /// 住院患者基本信息查询接口
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "hospitalNo":"患者卡号",
        ///      "type":"1 省直医院"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
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
        /// 获取科室未来15天的医生排班情况PRC_DoctorQueryToWL15
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "deptId":"科室id",
        ///      "hospitalId":"医院id默认1"
        ///  }
        /// </remarks>
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

            var ds = Logic.Methods.SqlQuery(db, @"PKG_ZHYY_MZ.PRC_DoctorQueryToWL15", parems.ToArray());
            ObjectResult obj = Methods.GetResult(parems, ds);
            //Console.WriteLine("返回参数：\n" + JsonConvert.SerializeObject(obj));
            return obj;
        }
        #endregion

        #region 电子发票查询接口
        /// <summary>
        /// 挂号订单查询
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "patientId":"门诊卡号",
        ///      "begin":"开始时间",
        ///      "end":"结束时间"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
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
            string sql = "select  e.inpatient_no as \"inpatientno\", e.patient_no as \"patientno\", e.card_no as  \"cardno\",  e.hisinvoice_no as \"hisinvoiceno\",  e.his_name as \"hisname\",  replace(to_char(e.pictureurl),'http://172.22.156.72:7001','https://api.lizhi.co') as \"pictureurl\",  e.tot_cost as \"totcost\", e.oper_date as \"operdate\" from fin_com_einvoicebill e where e.vaild_flag = '1' and e.cancel_flag = '1' and e.exe_flag = '1'  and e.patient_no is not null   and e.patient_no = '{0}'  and e.oper_date >= to_date('{1}', 'yyyy-mm-dd hh24:mi:ss') and e.oper_date < to_date('{2}', 'yyyy-mm-dd hh24:mi:ss') union all  select e.inpatient_no as \"inpatientno\", e.patient_no as \"patientno\", e.card_no as  \"cardno\",  e.hisinvoice_no as \"hisinvoiceno\", e.his_name as \"hisname\", replace(to_char(e.pictureurl),'http://172.22.156.72:7001','https://api.lizhi.co') as \"pictureurl\",  e.tot_cost as \"totcost\",  e.oper_date as \"operdate\" from fin_com_einvoicebill e where e.vaild_flag = '1' and e.cancel_flag = '1'  and e.exe_flag = '1'   and e.card_no is not null  and e.card_no = '{0}' and e.oper_date >= to_date('{1}', 'yyyy-mm-dd hh24:mi:ss')   and e.oper_date < to_date('{2}', 'yyyy-mm-dd hh24:mi:ss')";
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
        /// <summary>
        /// 手机短信接口
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "phone":"电话号码",
        ///      "content":"短信内容"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        [HttpPost, Route("GetSmsInfo")]
        public IActionResult GetSmsInfo([FromBody] dynamic dynamic)
        {
            JObject j = Methods.dynamicToJObject(dynamic);
            string phone = j.GetValue("phone").ToString();
            string content = j.GetValue("content").ToString();
            string result = Methods.getSMSPost(phone, content);
            JObject jObject = JsonConvert.DeserializeObject<JObject>(result).GetValue("SendSmsResponse").ToObject<JObject>();
            return new ObjectResult(new { success = jObject.GetValue("success").ToString(), rspcod = jObject.GetValue("rspcod").ToString(), msgGroup = jObject.GetValue("msgGroup").ToString() });
        }
        #endregion



        #region 获取扫码支付二维码
        /// <summary>
        /// 获取扫码支付二维码
        /// </summary>
        /// <param name="ClinicNo">门诊流水号</param>
        /// <returns></returns>
        [HttpGet, Route("GetQRImage")]
        public IActionResult GetQRImage(string ClinicNo)
        {
            Console.WriteLine("获取扫码支付二维码时间：" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "参数：ClinicNo:" + ClinicNo);
            Stream imgarr = Functions.GetPictureurl(ClinicNo);

            return File(imgarr, "image/jpeg");
        }
        #endregion

        #region 挂号订单查询
        /// <summary>
        /// 挂号订单查询PRC_REGORDERQUERY
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "registerNo":"门诊流水号",
        ///     "patientName":"患者姓名",
        ///     "Status":"交易状态 1 正交易 2 负交易",
        ///     "doctorName":"医生名称",
        ///      "startTime":"开始时间",
        ///     "endTime":"结束时间",
        ///     "Page":"页数",
        ///     "pageSize":"每页行数"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        [HttpPost, Route("GetRegOrderInfo")]
        public IActionResult GetRegOrderInfo([FromBody] dynamic dynamic)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n挂号订单查询的入参" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            if (!j.ContainsKey("Page"))
            {
                return new ObjectResult(new { msg = "请求失败", data = "入参错误！没有找到参数名为Page的参数", code = "500" });
            }
            if (!j.ContainsKey("pageSize"))
            {
                return new ObjectResult(new { msg = "请求失败", data = "入参错误！没有找到参数名为pageSize的参数", code = "500" });
            }
            int page = Convert.ToInt32(j.GetValue("Page", StringComparison.OrdinalIgnoreCase).ToString());
            int pageNum = Convert.ToInt32(j.GetValue("pageSize", StringComparison.OrdinalIgnoreCase).ToString());
            try
            {
                oralist.Add(Methods.GetInput("clinic_code", j.GetValue("registerNo").ToString()));
                oralist.Add(Methods.GetInput("patname", j.GetValue("patientName").ToString()));
                oralist.Add(Methods.GetInput("status", j.GetValue("Status").ToString()));
                oralist.Add(Methods.GetInput("doctName", j.GetValue("doctorName").ToString()));
                oralist.Add(Methods.GetInput("startTime", j.GetValue("startTime").ToString()));
                oralist.Add(Methods.GetInput("endTime", j.GetValue("endTime").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_REGORDERQUERY", oralist.ToArray());

            // Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds, page, pageNum);
        }

        #endregion

        #region 根据医生选择缴费查询
        /// <summary>
        /// 根据医生选择缴费查询PRC_GetHosItemListByDoc
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///      "recipeNo":"门诊流水号",
        ///     "patientName":"患者姓名",
        ///     "Status":"缴费状态1 已缴费 0 待缴费 ",
        ///     "doctorName":"医生名称",
        ///      "startTime":"开始时间",
        ///     "endTime":"结束时间"
        ///  }
        /// </remarks>
        /// <param name="dynamic"></param>
        /// <returns></returns>
        [HttpPost, Route("getHospitalItemListByDoctor")]
        public IActionResult getHospitalItemListByDoctor([FromBody] dynamic dynamic)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n根据医生选择缴费查询的入参" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("recipeNo", j.GetValue("recipeNo").ToString()));
                oralist.Add(Methods.GetInput("patientName", j.GetValue("patientName").ToString()));
                oralist.Add(Methods.GetInput("Status", j.GetValue("Status").ToString()));
                oralist.Add(Methods.GetInput("doctorName", j.GetValue("doctorName").ToString()));
                oralist.Add(Methods.GetInput("startTime", j.GetValue("startTime").ToString()));
                oralist.Add(Methods.GetInput("endTime", j.GetValue("endTime").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_GetHosItemListByDoc", oralist.ToArray());
            // Console.WriteLine("返回参数：\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            if (!j.ContainsKey("Page"))
            {
                return new ObjectResult(new { msg = "请求失败,参数出现错误", data = "没有找到参数名为Page的参数", code = "500" });
            }
            if (!j.ContainsKey("pageSize"))
            {
                return new ObjectResult(new { msg = "请求失败,参数出现错误", data = "没有找到参数名为pageSize的参数", code = "500" });
            }
            int page = Convert.ToInt32(j.GetValue("Page", StringComparison.OrdinalIgnoreCase).ToString());
            int pageSize = Convert.ToInt32(j.GetValue("pageSize", StringComparison.OrdinalIgnoreCase).ToString());
            return Methods.GetResult(oralist, ds, page, pageSize);
        }
        #endregion


        #region 缴费明细查询
        /// <summary>
        /// 缴费明细查询PRC_GetHosItemDetail
        /// </summary>
        /// <param name="orderNo">订单号RecipeNo</param>
        /// <returns></returns>
        [HttpGet, Route("getHosItemDetail")]
        public IActionResult getHosItemDetail(string orderNo)
        {
            UpdateSql("his");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n缴费明细查询的入参" + orderNo);
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("orderNO", orderNo));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_GetHosItemDetail", oralist.ToArray());
            return Methods.GetResult(oralist, ds);
        }
        #endregion


        #region 收入金额订单数量接口
        /// <summary>
        /// 收入金额订单数量接口PRC_GetTotal
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        [HttpGet, Route("getHosItemNumList")]
        public IActionResult getHosItemNumList(string start, string end)
        {
            UpdateSql("his");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n缴费明细查询getHosItemNumList的入参start:" + start + "end:" + end);
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("beginDate", start));
                oralist.Add(Methods.GetInput("endDate", end));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_GetTotal", oralist.ToArray());
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 交易金额走势图接口
        /// <summary>
        /// 交易金额走势图接口PRC_GetTotalByDate
        /// </summary>
        /// <param name="Type">查询类别1 查询 本月  2 查询本年</param>
        /// <returns></returns>
        [HttpGet, Route("getHosItemNumListByType")]
        public IActionResult getHosItemNumListByType(int Type)
        {
            UpdateSql("his");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n交易金额走势图接口getHosItemNumListByType的入参Type:" + Type);
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(new OracleParameter() { ParameterName = "SelType", OracleDbType = OracleDbType.Int32, Value = Type });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_GetTotalByDate", oralist.ToArray());
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 各业务交易金额接口
        /// <summary>
        /// 各业务交易金额接口PRC_GetTotalByType
        /// </summary>
        /// <param name="Type">查询类别1 查询 本月  2 查询本年</param>
        /// <returns></returns>
        [HttpGet, Route("getHosItemNumListByYW")]
        public IActionResult getHosItemNumListByYW(int Type)
        {
            UpdateSql("his");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n交易金额走势图接口getHosItemNumListByYW的入参Type:" + Type);
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(new OracleParameter() { ParameterName = "SelType", OracleDbType = OracleDbType.Int32, Value = Type });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_GetTotalByType", oralist.ToArray());
            return Methods.GetResult(oralist, ds);
        }
        #endregion


        #region HIS账单接口(查询当天日期的前一天数据)------  备注:我们这边每天凌晨1点用定时任务刷数据
        /// <summary>
        /// HIS账单接口PRC_GetyesterdayTotal(查询当天日期的前一天数据)------  备注:我们这边每天凌晨1点用定时任务刷数据
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetyesterdayTotal")]
        public IActionResult GetyesterdayTotal()
        {
            UpdateSql("his");
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n\n交易金额走势图接口GetyesterdayTotal");
            List<OracleParameter> oralist = new List<OracleParameter>();
            //try
            //{
            //    oralist.Add(new OracleParameter() { ParameterName = "SelType", OracleDbType = OracleDbType.Int32, Value = Type });
            //}
            //catch (Exception ex)
            //{
            //    return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            //}
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));

            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_GetyesterdayTotal", oralist.ToArray());
            return Methods.GetResult(oralist, ds);
        }
        #endregion


        #region 根据条件获取医生列表 1 是根据医生id查 排班  2是根据排班查号源
        /// <summary>
        /// 根据条件获取医生排班列表 1 是根据医生id查 排班  2是根据排班查号源
        /// </summary>
        /// <remarks>
        /// >参数实例
        /// {
        ///  "doctorId":"医生编号",
        ///  "type":"业务id号 1 是根据医生id查 排班  2是根据排班查号源",
        ///  "date":"默认空就好"
        ///  }
        /// </remarks>
        /// <param name="dy">请求正文</param>
        /// <returns></returns>
        [HttpPost, Route("GetSchemaInfoByDoc")]
        public IActionResult GetSchemaInfoByDoc([FromBody] dynamic dy)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n根据条件获取医生排班列表的入参" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("doctId", j.GetValue("doctorId").ToString()));
                oralist.Add(Methods.GetInput("bizType", j.GetValue("type").ToString()));
                oralist.Add(Methods.GetInput("Docdate", j.GetValue("date").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_OutpDoctorQueryByDoc", oralist.ToArray());
            //Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion

        #region 查询住院预交金余额 1、查询预交金余额2、查询预交金记录
        /// <summary>
        /// 查询住院预交金余额 1、查询预交金余额2、查询预交金记录
        /// </summary>
        /// <remarks>
        /// >请求实例
        /// {
        /// "idCardNo":"身份证号",
        /// "hospitalNum":"住院号",
        /// "phone":"电话",
        /// "type":"业务Id 1、查询预交金余额2、查询预交金记录"
        /// }
        /// </remarks>
        /// <param name="dy">请求正文</param>
        /// <returns></returns>
        [HttpPost, Route("QueryInPrepayInfo")]
        public IActionResult QueryInPrepayInfo([FromBody] dynamic dy)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n查询住院预交金余额" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("PatientNo", j.GetValue("hospitalNum").ToString()));
                oralist.Add(Methods.GetInput("IdCard", j.GetValue("idCardNo").ToString()));
                oralist.Add(Methods.GetInput("Phone", j.GetValue("phone").ToString()));
                oralist.Add(Methods.GetInput("bizType", j.GetValue("type").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_QUERYPREPAY", oralist.ToArray());
            //Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion


        #region 出诊变更信息查询--1 是 出诊变更根据医生查询挂号患者信息dto --2 是 出诊变更根据医生查询挂号患者信息vo 
        /// <summary>
        /// 出诊变更信息查询  --1 是 出诊变更根据医生查询挂号患者信息dto --2 是 出诊变更根据医生查询挂号患者信息vo 
        /// </summary>
        /// <remarks>
        /// >参数实例 
        /// {
        ///  "doctorId":"医生编号",
        ///  "schedulingDate":"排班时间",
        ///  "phone":"电话",可为空
        ///  "timeFrame":"时间段类型",
        ///  "department":"科室id",
        ///  "isRefundRegister":"1 正常 2 退号",
        ///  "type":"业务Id 1、出诊变更根据医生查询挂号患者信息dto 2、出诊变更根据医生查询挂号患者信息vo "
        ///  }
        /// </remarks>
        /// <param name="dy">请求正文</param>
        /// <returns></returns>
        [HttpPost, Route("QueryRegInfo")]
        public IActionResult QueryRegInfo([FromBody] dynamic dy)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n出诊变更信息查询" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("DoctID", j.GetValue("doctorId").ToString()));
                oralist.Add(Methods.GetInput("schemaTime", j.GetValue("schedulingDate").ToString()));
                oralist.Add(Methods.GetInput("Phone", j.GetValue("phone").ToString()));
                oralist.Add(Methods.GetInput("NOONCODE", j.GetValue("timeFrame").ToString()));
                oralist.Add(Methods.GetInput("DEPTID", j.GetValue("department").ToString()));
                oralist.Add(Methods.GetInput("TRANSTYPE", j.GetValue("isRefundRegister").ToString()));
                oralist.Add(Methods.GetInput("bizType", j.GetValue("type").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ResultSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_REGISTERCHANGQUERY", oralist.ToArray());
            //Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
        }
        #endregion


        #region 获取住院信息
        /// <summary>
        /// 获取住院信息 1、检验信息接口 2、获取住院信息 
        /// </summary>
        /// <remarks>
        /// >参数实例 
        /// {
        ///  "idCardNo":"身份证号"，
        ///  "hospitalNum":"住院号"
        ///  "patientName":"病人姓名",
        ///  "phone":"病人电话",
        ///  "type":"业务Id 1、检验信息接口 2、获取住院信息 "
        ///  }
        /// </remarks>
        /// <param name="dy">请求正文</param>
        /// <returns></returns>
        [HttpPost, Route("QueryInMainInfo")]
        public IActionResult QueryInMainInfo([FromBody] dynamic dy)
        {
            UpdateSql("his");
            JObject j = Methods.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n获取住院信息" + j.ToString());
            List<OracleParameter> oralist = new List<OracleParameter>();
            try
            {
                oralist.Add(Methods.GetInput("PatientNo", j.GetValue("hospitalNum").ToString()));
                oralist.Add(Methods.GetInput("patientName", j.GetValue("patientName").ToString()));
                oralist.Add(Methods.GetInput("IdCard", j.GetValue("idCardNo").ToString()));
                oralist.Add(Methods.GetInput("Phone", j.GetValue("phone").ToString()));
                oralist.Add(Methods.GetInput("bizType", j.GetValue("type").ToString()));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { msg = "请求失败", data = ex.Message, code = "500" });
            }
            oralist.Add(Methods.GetOutput("ReturnSet", OracleDbType.RefCursor, 1024));
            oralist.Add(Methods.GetOutput("ReturnCode", OracleDbType.Int32, 20));
            oralist.Add(Methods.GetOutput("ErrorMsg", OracleDbType.Varchar2, 50));
            var ds = Methods.SqlQuery(db, "PKG_ZHYY_MZ.PRC_QUERYINMAININFO", oralist.ToArray());
            //Console.WriteLine("返回参数:\n"+JsonConvert.SerializeObject(Methods.GetResult(oralist, ds)));
            return Methods.GetResult(oralist, ds);
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
                    this.db = Logic.SqlMethods.GetHISDBCSK(db);
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