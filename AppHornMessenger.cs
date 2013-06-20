using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace AppHornDotNetLibrary
{
    public class AppHornMessenger
    {
        private string APPHORN_MSG_URL { get; set; }

        public AppHornMessenger()
        {
            //use default URL value
            APPHORN_MSG_URL =  "http://localhost:60444/AddMessage";
        }

        public AppHornMessenger(string url)
        {
            APPHORN_MSG_URL = url;
        }
        
        public MessageResponse SendMessage(MessageRequest request)
        {
           return Send(request);
        }

        private MessageResponse Send(MessageRequest messageRequest)
        {
            var response = new MessageResponse();
            try
            {
                var jsonRequest = JsonConvert.SerializeObject(messageRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APPHORN_MSG_URL);
                request.Method = "POST";
                request.ContentLength = jsonRequest.Length;
                request.ContentType = @"application/json";
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                Byte[] byteArray = encoding.GetBytes(jsonRequest);

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse())
                {
                    response.httpResponse = httpResponse.StatusCode;
                }

                response.success = true;
            }

            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    System.Diagnostics.Trace.WriteLine("AppHornMessenger Web error in Send: {0}", errorText);
                }
                response.success = false;
                response.exception = ex;                
            }
            catch (Exception ex)
            {
                response.success = false;
                response.exception = ex;                
            }

            return response;
        }
    }
}
