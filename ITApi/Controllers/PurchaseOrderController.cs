using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.PaymentUser;
using InstaTransfer.BLL.Models.PurchaseOrder;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Api;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.Api;
using ITApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;

namespace ITApi.Controllers
{
    /// <summary>
    /// Controlador de las órdendes de compra
    /// </summary>
    [Authorize(Roles = "CommerceApiUser")]
    [RoutePrefix("api/purchaseOrders")]
    public class PurchaseOrderController : ApiController
    {
        BaseErrorResponse _baseErrorResponse;
        BaseSuccessResponse _baseSuccessResponse;

        // POST api/purchaseOrders/create
        /// <summary>
        /// Crea una orden de compra con los datos obtenidos del modelo
        /// </summary>
        /// <param name="purchaseorder">Modelo de la orden recibido por el api</param>
        /// <returns>Mensaje de respuesta de la operacion</returns>
        [Route("create")]
        [ResponseType(typeof(PurchaseOrderPostResponse))]
        [HttpPost]
        public HttpResponseMessage Create(PurchaseOrderModel purchaseorder)
        {
            CPurchaseOrder _cPurchaseOrder;
            URepository<CPurchaseOrder> UPORepo = new URepository<CPurchaseOrder>();
            var errorList = new List<string>();

            // Verificamos que el ModelState sea valido
            if (!ModelState.IsValid)
            {
                // Recorremos los errores del modelo y lo añadimos a una lista de errores
                foreach (ModelState modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        if (error.ErrorMessage != string.Empty)
                        {
                            errorList.Add(error.ErrorMessage);
                        }
                    }
                }
                // Creamos la respuesta de error base con la lista de errores generada
                _baseErrorResponse = new BaseErrorResponse(errorList, null);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    idpurchaseorder = _baseErrorResponse.ResponseObject
                };
                // Devolvemos la respuesta
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }

            try
            {
                // Leemos los datos necesarios desde el token
                var jwtSecurityToken = ApiHelper.ReadTokenFromHeader(Request);
                string rif = jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.RifClaim).FirstOrDefault().Value;
                Guid cuserid = Guid.Parse(jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.CUserClaim).FirstOrDefault().Value);

                // Creamos la orden de compra en la base de datos a partir del modelo de la api
                _cPurchaseOrder = new CPurchaseOrder(purchaseorder);
                // Asociamos el comercio y el usuario a la orden de compra creada
                _cPurchaseOrder.RifCommerce = rif;
                _cPurchaseOrder.IdCUser = cuserid;

                // Guardamos la entidad en la base de datos
                UPORepo.AddEntity(_cPurchaseOrder);
                UPORepo.SaveChanges();
            }
            catch (Exception)
            {
                // Capturamos la exception y construimos la respuesta
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.PurchaseOrderErrorMessage, ApiErrors.PurchaseOrderCreateErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    idpurchaseorder = _baseErrorResponse.ResponseObject
                };
                // Devolvemos la respuesta 
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            // Construimos la respuesta base
            _baseSuccessResponse = new BaseSuccessResponse(ApiResources.PurchaseOrderSuccessMessage, purchaseorder);
            // Construimos el formato del resultado
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message,
                idpurchaseorder = purchaseorder.id
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        // POST api/purchaseOrders/get
        /// <summary>
        /// Obtiene la orden de compra a partir de un id especifico
        /// </summary>
        /// <param name="baseModel">Modelo que contiene el id de la orden</param>
        /// <returns><see cref="CPurchaseOrder"/> buscada y mensaje de respuesta</returns>
        [Route("get")]
        [ResponseType(typeof(PurchaseOrderGetResponse))]
        [HttpPost]
        public HttpResponseMessage Get(BaseModel baseModel)
        {
            CPurchaseOrder _cPurchaseOrder = null;
            PurchaseOrderViewModel purchaseorder = null; /*new PurchaseOrderViewModels();*/
            var CPOBLL = new CPurchaseOrderBLL();

            // Obtenemos los datos necesarios desde el token
            var jwtSecurityToken = ApiHelper.ReadTokenFromHeader(Request);
            string rif = jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.RifClaim).FirstOrDefault().Value;


            if (baseModel == null)
            {
                // Construimos la respuesta base
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.ParametersRequiredErrorMessage, ApiErrors.ParametersRequiredErrorCode);
                // Construimos el formato del resultado
                var badResult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    purchaseorder
                };
                // Retornamos el resultado y codigo de status
                return Request.CreateResponse(HttpStatusCode.BadRequest, badResult);
            }
            // Obtenemos la orden de compra desde la base de datos a partir del id recibido por el api
            _cPurchaseOrder = CPOBLL.GetPurchaseOrder(rif, baseModel.id);
            // Verificamos que exista en la base de datos
            if (_cPurchaseOrder != null)
            {
                // Construimos el modelo de la orden de compra
                purchaseorder = new PurchaseOrderViewModel
                {
                    amount = _cPurchaseOrder.Amount,
                    ordernumber = _cPurchaseOrder.OrderNumber,
                    creationdate = _cPurchaseOrder.CreateDate,
                    id = _cPurchaseOrder.Id,
                    paymentuser = new PaymentUserModel
                    {
                        userci = _cPurchaseOrder.EndUserCI,
                        useremail = _cPurchaseOrder.EndUserEmail
                    },
                };
            }
            else if (_cPurchaseOrder == null)
            {
                // Construimos la respuesta base
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.PurchaseOrderNotFoundExceptionMessage, ApiErrors.PurchaseOrderNotFoundExceptionCode);
                // Construimos el formato del resultado
                var badResult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    purchaseorder
                };
                // Retornamos el resultado y codigo de status
                return Request.CreateResponse(HttpStatusCode.NotFound, badResult);
            }
            // Construimos la respuesta base
            _baseSuccessResponse = new BaseSuccessResponse(ApiResources.OperationSuccessMessage);
            // Construimos el formato del resultado
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message,
                purchaseorder
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/purchaseOrders/getorder
        /// <summary>
        /// Obtiene la orden de compra a partir de un numero de orden especifico
        /// </summary>
        /// <param name="model">Modelo que contiene el numero de orden</param>
        /// <returns><see cref="CPurchaseOrder"/> buscada y mensaje de respuesta</returns>
        [Route("getOrder")]
        [ResponseType(typeof(PurchaseOrderGetResponse))]
        [HttpPost]
        public HttpResponseMessage GetOrder(OrderNumberModel model)
        {
            CPurchaseOrder _cPurchaseOrder = null;
            PurchaseOrderViewModel purchaseorder = null;
            CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL();

            // Obtenemos los datos necesarios desde el token
            var jwtSecurityToken = ApiHelper.ReadTokenFromHeader(Request);
            string rif = jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.RifClaim).FirstOrDefault().Value;


            if (model == null)
            {
                // Construimos la respuesta base
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.ParametersRequiredErrorMessage, ApiErrors.ParametersRequiredErrorCode);
                // Construimos el formato del resultado
                var badResult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    purchaseorder
                };
                // Retornamos el resultado y codigo de status
                return Request.CreateResponse(HttpStatusCode.BadRequest, badResult);
            }
            // Obtenemos la orden de compra desde la base de datos a partir del id recibido por el api
            _cPurchaseOrder = CPOBLL.GetPurchaseOrder(rif, model.ordernumber);
            // Verificamos que exista en la base de datos
            if (_cPurchaseOrder != null)
            {
                // Construimos el modelo de la orden de compra
                purchaseorder = new PurchaseOrderViewModel
                {
                    amount = _cPurchaseOrder.Amount,
                    ordernumber = _cPurchaseOrder.OrderNumber,
                    creationdate = _cPurchaseOrder.CreateDate,
                    id = _cPurchaseOrder.Id,
                    paymentuser = new PaymentUserModel
                    {
                        userci = _cPurchaseOrder.EndUserCI,
                        useremail = _cPurchaseOrder.EndUserEmail
                    },
                };
            }
            else if (_cPurchaseOrder == null)
            {
                // Construimos la respuesta base
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.PurchaseOrderNotFoundExceptionMessage, ApiErrors.PurchaseOrderNotFoundExceptionCode, purchaseorder);
                // Construimos el formato del resultado
                var badResult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    purchaseorder
                };
                // Retornamos el resultado y codigo de status
                return Request.CreateResponse(HttpStatusCode.NotFound, badResult);
            }
            // Construimos la respuesta base
            _baseSuccessResponse = new BaseSuccessResponse(ApiResources.OperationSuccessMessage, purchaseorder);
            // Construimos el formato del resultado
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message,
                purchaseorder
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
