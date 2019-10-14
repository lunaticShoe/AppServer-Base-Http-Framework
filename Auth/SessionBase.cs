using System;
using System.Collections.Generic;
using System.Text;

namespace AppServerBase.Auth
{
    public class SessionBase
    {
        public DateTime Created;
        public DateTime Lastactive;
        public string Token;

        public SessionBase()
        {
            Created = DateTime.Now;
            Lastactive = DateTime.Now;
        }
    }
}
