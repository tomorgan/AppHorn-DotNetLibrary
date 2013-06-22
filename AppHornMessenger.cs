using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace AppHornDotNetLibrary
{

    internal class AsyncResult<TResult> : AsyncResultNoResult {
        // Field set when operation completes 
        private TResult m_result = default(TResult);
        public AsyncResult(AsyncCallback asyncCallback, Object state) : base(asyncCallback, state) { } 
        public void SetAsCompleted(TResult result, Boolean completedSynchronously) { 
            // Save the asynchronous operation's result 
            m_result = result; 
            // Tell the base class that the operation completed 
            // sucessfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        } 
        
        new public TResult EndInvoke() { 
            base.EndInvoke();
            // Wait until operation has completed 
            return m_result; 
            // Return the result (if above didn't throw) 
        }
    }

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

        public IAsyncResult BeginSendMessage(MessageRequest request, AsyncCallback callback, object state)
        {
            AsyncResult<MessageResponse> ar = new AsyncResult<MessageResponse>(callback, state);
            
            object payload = new object[] { ar, request };
            ThreadPool.QueueUserWorkItem(DoWorkAsync, payload);
            return ar;
        }
        

        private void DoWorkAsync(object asyncResult)
        {
            object[] payload = (object[])asyncResult;
             AsyncResult<MessageResponse> ar = (AsyncResult<MessageResponse>)payload[0];
             MessageRequest request = (MessageRequest)payload[1];

             var response = Send(request);
             if (response.success)
             {
                 ar.SetAsCompleted(response, false);
             }
             else
             {
                 ar.SetAsCompleted(response.exception, false); 
             }
        }

        public MessageResponse EndSendMessage(IAsyncResult asyncResult)
        {
            AsyncResult<MessageResponse> ar = (AsyncResult<MessageResponse>)asyncResult;
            return ar.EndInvoke(); 
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
