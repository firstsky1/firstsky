using System;
using System.Collections.Generic;
using System.Text;

namespace ClayTablet.CT3.Sample.Net
{
    public class HandleResult
    {
        private bool _Success = false;
        private String _Message;
        private String _ErrorMessage;
        private Boolean _CanDeleteMessage = false;

        public bool Success
        {
            get { return _Success; }
            set { _Success = value; }
        }

        public bool CanDeleteMessage
        {
            get { return _CanDeleteMessage; }
            set { _CanDeleteMessage = value; }
        }

        public String ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }

        public String Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
    }
}
