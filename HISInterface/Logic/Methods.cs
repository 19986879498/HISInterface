using HISInterface.DBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HISInterface.Logic
{
    /// <summary>
    /// ado.net 方法
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// 调用存储过程获取方法
        /// </summary>
        /// <param name="db"></param>
        /// <param name="spName"></param>
        /// <param name="paramsters"></param>
        /// <returns></returns>
        public static DataSet SqlQuery(DB db, string spName, params OracleParameter[] paramsters)
        {
            OracleConnection connection = db.Database.GetDbConnection() as OracleConnection;
            OracleDataAdapter adapter = null;
            DataSet set = null;
            using (OracleCommand command = new OracleCommand(spName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = spName;
                command.Parameters.AddRange(paramsters);
                adapter = new OracleDataAdapter(command);

                set = new DataSet();
                adapter.Fill(set);
                adapter.SelectCommand.Parameters.Clear();
                adapter.Dispose();
                command.Parameters.Clear();
                command.Dispose();
                connection.Close();
                connection.Dispose();
                return set;
            }
        }
        /// <summary>
        /// 获取查询
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataSet SqlQuery(DB db, string sql)
        {
            //创建Oracle连接对象
            OracleConnection conn =db.Database.GetDbConnection() as OracleConnection;
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            //创建操作对象
            OracleCommand command = conn.CreateCommand();
            DataSet dataTable = new DataSet();
            command.CommandText = string.Format(sql);
            OracleDataAdapter oradata = new OracleDataAdapter();
            oradata.SelectCommand = command;
            oradata.Fill(dataTable);

            command.Parameters.Clear();
            conn.Close();
            int count = dataTable.Tables[0].Rows.Count;

            return dataTable;
        }
        public static object ReturnOracleResult(List<OracleParameter> parems, string Name)
        {
            return parems.FirstOrDefault(u => u.ParameterName == Name).Value.ToString();
        }
        /// <summary>
        /// dynamic转jobject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JObject dynamicToJObject(dynamic obj)
        {
            return JsonConvert.DeserializeObject<JObject>(obj.ToString());
        }
        
        /// <summary>
        /// string转T
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T getStringToT<T>(string res)
        {
            return JsonConvert.DeserializeObject<T>(res);
        }
     
        public static DataTable QuerySql(DB db, string sql)
        {
            OracleConnection conn = db.Database.GetDbConnection() as OracleConnection; DataTable ds = new DataTable();
            try
            {
                conn.Open();
                OracleDataAdapter oda = new OracleDataAdapter(sql, conn);
                OracleCommand cmd = new OracleCommand(sql, conn);
                DataTable dt = new DataTable();
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 999;
                var reader = cmd.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                return dt;

                // ds.Load(reader);
            }
            catch
            {
                return null;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        /// <summary>
        /// 获得入参
        /// </summary>
        /// <returns></returns>
        public static OracleParameter GetInput(string name, object value)
        {
            var input = new OracleParameter(name, value);
            input.OracleDbType = OracleDbType.Varchar2;
            input.Direction = System.Data.ParameterDirection.Input;
            return input;
        }
          /// <summary>
        /// 获得入参
        /// </summary>
        /// <returns></returns>
        public static OracleParameter GetInput(string name,OracleDbType dbType, object value)
        {
            var input = new OracleParameter(name, value);
            input.OracleDbType = dbType;
            input.Direction = System.Data.ParameterDirection.Input;
            return input;
        }
        /// <summary>
        /// 获得出参
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static OracleParameter GetOutput(string name, OracleDbType dbType, int dataLength)
        {
            var output = new OracleParameter(name, dbType, dataLength);
            output.Direction = System.Data.ParameterDirection.Output;
            return output;
        }
        /// <summary>
        /// 将dataset数据集转换成json对象
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static ArrayList getJObject(DataSet ds)
        {
            ArrayList arr = new ArrayList();
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    Dictionary<string, string> j = new Dictionary<string, string>();
                    for (int i = 0; i < item.ItemArray.Length; i++)
                    {
                        j.Add(ds.Tables[0].Columns[i].ColumnName.ToString(), item.ItemArray[i].ToString());
                    }
                    arr.Add(j);
                }
            }
            return arr;
        }
        public static ArrayList getJObject(DataTable ds)
        {
            ArrayList arr = new ArrayList();
            if (ds==null)
            {
                return null;
            }
            if ( ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    Dictionary<string, string> j = new Dictionary<string, string>();
                    for (int i = 0; i < item.ItemArray.Length; i++)
                    {
                        j.Add(ds.Columns[i].ColumnName.ToString(), item.ItemArray[i].ToString());
                    }
                    arr.Add(j);
                }
            }
            return arr;
        }
        /// <summary>
        /// 获取json返回得到的json数据
        /// </summary>
        /// <param name="parems"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static ObjectResult GetResult(List<OracleParameter> parems, DataSet ds,string ErrName=null)
        {
            ErrName = string.IsNullOrEmpty(ErrName) ? "ErrorMsg" : ErrName;
            int code = 0;
            try
            {
                code = Convert.ToInt32(parems.Where(i => i.ParameterName == "ReturnCode").FirstOrDefault().Value.ToString());
            }
            catch (Exception)
            {
                code = 404;
            }
            var data = getJObject(ds);
            IEnumerable<JObject> j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(data));
            string jsonstrData = string.Empty;
            if (data != null && data.Count > 0)
            {
                jsonstrData = JsonConvert.SerializeObject(data);
            }
            else
            {
                goto Error;
            }
            if (code == 1)
            {
                string Msg = parems.Where(i => i.ParameterName == ErrName).FirstOrDefault().Value.ToString();
                var res = new { msg = Msg, data = data, code = 200 };
                return new ObjectResult(res);

            }
            else
            {
                goto Error;

            }

        Error:
            var result = new { msg = parems.Where(i => i.ParameterName == ErrName).FirstOrDefault().Value.ToString(), data ="查询无数据", code = 404 };
            return new ObjectResult(result);
        }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="parems">参数</param>
      /// <param name="ds">数据集</param>
      /// <param name="Page">第几行</param>
      /// <param name="PageNum">页行数</param>
      /// <param name="ErrName"></param>
      /// <returns></returns>
        public static ObjectResult GetResult(List<OracleParameter> parems, DataSet ds,int Page,int PageNum, string ErrName = null)
        {
            ErrName = string.IsNullOrEmpty(ErrName) ? "ErrorMsg" : ErrName;
            int code = 0;
            try
            {
                code = Convert.ToInt32(parems.Where(i => i.ParameterName == "ReturnCode").FirstOrDefault().Value.ToString());
            }
            catch (Exception)
            {
                code = 404;
            }
            var data = getJObject(ds);
            string jsonstrData = string.Empty;
            if (data != null && data.Count > 0)
            {
                jsonstrData = JsonConvert.SerializeObject(data);
            }
            else
            {
                goto Error;
            }
            //分页 
            IEnumerable < JObject > j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(data));
            j=j.Skip((Page - 1) * PageNum).Take(PageNum).ToList();
           
            if (code == 1)
            {
                string Msg = parems.Where(i => i.ParameterName == ErrName).FirstOrDefault().Value.ToString();
                var res = new { msg = Msg, data = data, code = 200 };
                return new ObjectResult(res);

            }
            else
            {
                goto Error;

            }

        Error:
            var result = new { msg = parems.Where(i => i.ParameterName == ErrName).FirstOrDefault().Value.ToString(), data = "查询无数据", code = 404 };
            return new ObjectResult(result);
        }
        /// <summary>
        /// 获取json返回得到的json数据
        /// </summary>
        /// <param name="parems"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static ObjectResult GetResultAndHaveSon(List<OracleParameter> parems, DataSet ds)
        {
            int code = Convert.ToInt32(parems.Where(i => i.ParameterName == "ReturnCode").FirstOrDefault().Value.ToString());
            var data = getJObject(ds);
            string jsonstrData = string.Empty;
            if (data != null && data.Count > 0)
            {
                jsonstrData = JsonConvert.SerializeObject(data);
            }
            else
            {
                goto Error;
            }
            if (code == 1)
            {
                IEnumerable<JObject> j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(data));
                string Msg = parems.Where(i => i.ParameterName == "ErrorMsg").FirstOrDefault().Value.ToString();
                string[] ids = j.Select(s => s.GetValue("subjectId").ToString()).Distinct().ToArray();
                List<object> objects = new List<object>();
                foreach (string item in ids)
                {//, subjectName=j.FirstOrDefault(u=>u.GetValue("subjectId").ToString()==item).GetValue("subjectName").ToString() 
                    objects.Add(new JsonResult(new { subjectId = item, subjectName = j.FirstOrDefault(u => u.GetValue("subjectId").ToString() == item).GetValue("subjectName").ToString(), details = (from JObject x in j where x.GetValue("subjectId").ToString() == item select  x).Select(s=>new { deptName = s.GetValue("deptName").ToString(), deptId = s.GetValue("DeptId").ToString() }) }).Value);
                }
                var res = new { msg = Msg, data = objects, code = 200 };
                return new ObjectResult(res);

            }
            else
            {
                goto Error;

            }

        Error:
            var result = new { msg = parems.Where(i => i.ParameterName == "ErrorMsg").FirstOrDefault().Value.ToString(), data = new ArrayList(), code = 404 };
            return new ObjectResult(result);
        }


        #region 短信接口
        public static string getSMSPost(string phone,string content)
        {
            string data = $"<request><id>1</id><phone>{phone}</phone><content>{content}</content></request>";
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://192.168.1.110:8081/SmsServer.ashx");
            myHttpWebRequest.Method = "POST";
            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            byte[] encodedBytes = Encoding.UTF8.GetBytes(data);
            myHttpWebRequest.ContentLength = encodedBytes.Length;

            // Write encoded data into request stream
            Stream requestStream = myHttpWebRequest.GetRequestStream();
            requestStream.Write(encodedBytes, 0, encodedBytes.Length);
            requestStream.Close();
            HttpWebResponse result;
            try
            {
                result = (HttpWebResponse)myHttpWebRequest.GetResponse();
            }
            catch
            {
                return string.Empty;
            }

            if (result.StatusCode == HttpStatusCode.OK)
            {
                Stream mystream = result.GetResponseStream();
                StreamReader reader = new StreamReader(mystream);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(reader.ReadToEnd());
                string res = JsonConvert.SerializeXmlNode(doc);
                return res;
            }

            return "";

        }
        #endregion
    }
}
