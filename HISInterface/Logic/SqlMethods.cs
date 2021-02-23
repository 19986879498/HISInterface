using HISInterface.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HISInterface.Logic
{
    public   class SqlMethods
    {
        public static IConfigurationRoot root = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        /// <summary>
        /// 连接HIS数据库测试库
        /// </summary>
        /// <param name="dB"></param>
        /// <returns></returns>
        public static DB GetHISDBCSK(DB dB)
        {

            dB.Database.GetDbConnection().ConnectionString = root["OrclDBStrCSK"].ToString();
            return dB;
        }
        /// <summary>
        /// 连接HIS数据库测试库
        /// </summary>
        /// <param name="dB"></param>
        /// <returns></returns>
        public static DB GetHISDBCSK2(DB dB)
        {

            dB.Database.GetDbConnection().ConnectionString = root["OrclDBStrCSK2"].ToString();
            return dB;
        }
        /// <summary>
        /// 连接HIS数据库正式库
        /// </summary>
        /// <param name="dB"></param>
        /// <returns></returns>
        public static DB GetHISDBZSK(DB dB)
        {

            dB.Database.GetDbConnection().ConnectionString = root["OrclDBStrZSK"].ToString();
            return dB;
        }
        /// <summary>
        /// 连接LIS数据库
        /// </summary>
        /// <param name="dB"></param>
        /// <returns></returns>
        public static DB GetLISDB(DB dB)
        {

           dB.Database.GetDbConnection().ConnectionString = root["OrclDBLis"].ToString();
            return dB;
        }
        /// <summary>
        /// 连接Pacs数据库
        /// </summary>
        /// <param name="dB"></param>
        /// <returns></returns>
        public static DB GetPacsDB(DB dB)
        {

            dB.Database.GetDbConnection().ConnectionString = root["OrclDBPacs"].ToString();
            return dB;
        }
    }
}
