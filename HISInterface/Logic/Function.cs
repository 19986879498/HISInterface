using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace HISInterface.Logic
{
   public class Functions
    {
        /// <summary>
        /// ��ȡaccess_token
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken()
        {
            string token = string.Empty;
            try
            {
                //΢��С����ӿ�
                string appID = "wx2335bbc9cdff2768";
                string appSecret = "a4971ccff81e9c31d13e944602d7e0c1";
                //��ȡ΢��token
                string token_url = "https://api.weixin.qq.com/cgi-bin/token?appid=" + appID + "&secret=" + appSecret + "&grant_type=client_credential";
                Log.Logger.GetLog("Url:" + token_url);
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(token_url);
                //����ʽ
                myRequest.Method = "GET";
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string content = reader.ReadToEnd();
                Log.Logger.GetLog("Content:" + content.ToString());
                myResponse.Close();
                reader.Dispose();
                //var result= JsonConvert.DeserializeObject(content);
                var result = JsonConvert.DeserializeObject<RecordResult>(content);
                token = result.access_token.ToString();
            }
            catch (Exception ex)
            {
                token = "";

            }
           // Log.Logger.GetLog("Token:" + token.ToString());
            return token;
        }
        //��ȡС����ͼƬ
        public static byte[] GetPictureurl(string ClinicNo)
        {
            try
            {
                string token = Functions.GetAccessToken();
                if (string.IsNullOrEmpty(token))
                    return null;
                string jsonParam = "{\"page\":\"pages/payment_record_sao/index\",\"width\":280,\"scene\":\"hosid=1&clino=" + ClinicNo + "\"}";

                string strURL = "https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + token;
                //����һ��HTTP����  
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
                //Post����ʽ  
                request.Method = "POST";
                //��������
                request.ContentType = "application/json;charset=utf-8";

                //���ò�����������URL���� 

                string paraUrlCoded = jsonParam;//System.Web.HttpUtility.UrlEncode(jsonParas);   

                byte[] payload;
                //��Json�ַ���ת��Ϊ�ֽ�  
                payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                //���������ContentLength   
                request.ContentLength = payload.Length;
                //�������󣬻�������� 

                Stream writer;
                try
                {
                    writer = request.GetRequestStream();//��ȡ����д���������ݵ�Stream����
                }
                catch (Exception)
                {
                    writer = null;
                    Console.Write("���ӷ�����ʧ��!");
                }
                //���������д����
                writer.Write(payload, 0, payload.Length);
                writer.Close();//�ر�������
                // String strValue = "";//strValueΪhttp��Ӧ�����ص��ַ���
                HttpWebResponse response;
                try
                {
                    //�����Ӧ��
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                Stream s = response.GetResponseStream();

                //return Image.FromStream(s);
                ////  Stream postData = Request.InputStream;
                StreamReader sRead = new StreamReader(s);
                string postContent = sRead.ReadToEnd();
                byte[] bytearr = Encoding.UTF8.GetBytes(postContent);
                return bytearr;//����Json����
            }

            catch (Exception)
            {

                throw;
            }
            return null;
        } 

        public class RecordResult
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
        }
    }
}