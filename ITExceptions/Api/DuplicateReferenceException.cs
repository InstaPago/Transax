using System;

namespace InstaTransfer.ITExceptions.Api
{
    public class DuplicateReferenceException : ITException
    {
        #region Constructors

        public DuplicateReferenceException(string message, Exception ex) : base(message, ex)
        {
        }
        public DuplicateReferenceException(string message) : base(message)
        {
        }
        public DuplicateReferenceException(string message, string errorCode) : base(message, errorCode)
        {
        }
        public DuplicateReferenceException(string id, string classname, string method, string message, Exception ex) 
            : base(id, classname, method, message, ex)
        {
        }

        #endregion

    }
}
