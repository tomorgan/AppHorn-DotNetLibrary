using System;

namespace AppHornDotNetLibrary
{
    public class MessageRequest
    {
        public Guid queueID;
        public Guid queueAppID;
        public string userID;
        public Guid secretKey;
        public string messageText;
    }
}
