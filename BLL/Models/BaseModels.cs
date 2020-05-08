using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InstaTransfer.BLL.Models
{
    /// <summary>
    /// Modelo base
    /// </summary>
    public class BaseModel
    {
        private Guid _id;
        /// <summary>
        /// Identificador del modelo
        /// </summary>
        [Display(Name = "id")]
        public Guid id
        {
            get
            {
                if (_id == Guid.Empty)
                {
                    _id = Guid.NewGuid();
                }
                return _id;
            }

            set { _id = value; }
        }
    }

    public class BaseResponse
    {
        #region Variables

        private bool _success;
        public bool Success
        {
            get { return _success; }

            set { _success = value; }
        }

        private object _responseobject;
        public object ResponseObject
        {
            get { return _responseobject; }

            set { _responseobject = value; }
        }

        private List<string> _message;
        public List<string> Message
        {
            get { return _message; }

            set { _message = value; }
        }

        public BaseResponse()
        {
            this.Success = false;

            this.Message = new List<string>();

            this.ResponseObject = new object();
        }

        #endregion
    }

    public class BaseSuccessResponse : BaseResponse
    {
        #region Constructors
        /// <summary>
        /// Constructor de la respuesta base del api para resultado, mensaje y objeto
        /// </summary>
        /// <param name="success">Resultado de la respuesta</param>
        /// <param name="message">Mensaje de la respuesta</param>
        /// <param name="responseObject">Objeto a devolver</param>
        public BaseSuccessResponse(string message, object responseObject)
        {
            Success = true;

            Message = new List<string> { message };

            ResponseObject = responseObject;
        }

        /// <summary>
        /// Constructor de la respuesta base del api para resultado y mensaje
        /// </summary>
        /// <param name="success">Resultado de la respuesta</param>
        /// <param name="message">Mensaje de la respuesta</param>
        public BaseSuccessResponse(string message)
        {
            Success = true;

            Message = new List<string> { message };
        }
        #endregion
    }

    public class BaseErrorResponse : BaseResponse
    {
        #region Variables

        private string _errorCode;
        public string ErrorCode
        {
            get { return _errorCode; }

            set { _errorCode = value; }
        }

        public List<string> ResponseMessage
        {
            get { return new List<string> { string.Format("{0} ({1})", Message.First(), ErrorCode) }; }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor de la respuesta base del api para mensaje y objeto
        /// </summary>
        /// <param name="message">Mensaje de la respuesta</param>
        /// <param name="responseObject">Objeto a devolver</param>
        public BaseErrorResponse(string message, object responseObject)
        {
            Success = false;

            Message = new List<string> { message };

            ResponseObject = responseObject;
        }

        /// <summary>
        /// Constructor de la respuesta base del api para mensaje y objeto
        /// </summary>
        /// <param name="message">Lista de mesajes de errores</param>
        /// <param name="errors">Objeto a devolver</param>
        public BaseErrorResponse(List<string> messageList, object responseObject)
        {
            Success = false;

            Message = messageList;

            ResponseObject = responseObject;
        }

        /// <summary>
        /// Constructor de la respuesta base para lista de string y codigo de error
        /// </summary>
        /// <param name="messageList">Lista de mensajes tipo string</param>
        /// <param name="errorCode">Codigo de error</param>
        public BaseErrorResponse(List<string> messageList, string errorCode)
        {
            Success = false;

            Message = messageList;

            ErrorCode = errorCode;
        }

        /// <summary>
        /// Constructor de la respuesta base del api para mensaje, codigo y objeto
        /// </summary>
        /// <param name="message">Mensaje de la respuesta</param>
        /// <param name="errorCode">Codigo de error</param>
        /// <param name="responseObject">Objeto a devolver</param>
        public BaseErrorResponse(string message, string errorCode, object responseObject)
        {
            Success = false;

            Message = new List<string> { message };

            ErrorCode = errorCode;
    
            ResponseObject = responseObject;
        }

        /// <summary>
        /// Constructor de la respuesta base del api para mensaje codigo de error
        /// </summary>
        /// <param name="message">Mensaje de la respuesta</param>
        /// <param name="errorCode">Codigo de error</param>
        public BaseErrorResponse(string message, string errorCode)
        {
            Success = false;

            Message = new List<string> { message };

            ErrorCode = errorCode;
        }
        #endregion
    }

    /// <summary>
    /// Modelo base para las transacciones (Ordenes y Declaraciones)
    /// </summary>
    public class BaseTransactionModel
    {
        /// <summary>
        /// Numero de transacciones (Ordenes y Declaraciones)
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Resultado de la operacion para obtener el numero de transacciones
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// Monto total de la suma de las transacciones
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}