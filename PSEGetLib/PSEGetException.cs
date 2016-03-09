using System;

namespace PSEGetLib.Types
{
    public class PSEGetException : Exception
    {
        public PSEGetException()
            : base()
        {
        }

        public PSEGetException(string message) : base(message)
        {

        }
    }
}
