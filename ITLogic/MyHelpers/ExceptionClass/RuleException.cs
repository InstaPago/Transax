using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace MyHelpers.ExceptionClass
{
    public class RuleException : Exception
    {
        public NameValueCollection Errors { get; private set; }
        public RuleException(string key, string value)
        {
            Errors = new NameValueCollection { { key, value } };
        }
        public RuleException(NameValueCollection errors)
        {
            Errors = errors;
        }
        // Populates a ModelStateDictionary for generating UI feedback
        public void CopyToModelState(ModelStateDictionary modelState)
        {
            foreach (string key in Errors)
                foreach (string value in Errors.GetValues(key))
                    modelState.AddModelError(key, value);
        }
    }

    public class InvalidPointException : Exception
    {
        public InvalidPointException()
        {  }
    }
}
