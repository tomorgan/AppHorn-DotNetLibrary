using System;
using System.Net;

namespace AppHornDotNetLibrary
{
    public class MessageResponse
    {
        public bool success;
        public Exception exception;
        public HttpStatusCode httpResponse;
    }
}
