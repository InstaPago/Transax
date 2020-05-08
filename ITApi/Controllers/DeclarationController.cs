using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using InstaTransfer.BLL.Models.Declaration;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Api;
using InstaTransfer.ITLogic;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.Api;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using ITApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;

namespace ITApi.Controllers
{
    /// <summary>
    /// Controlador de las declaraciones
    /// </summary>
    [Authorize(Roles = "CommerceApiUser")]
    [RoutePrefix("api/declarations")]
    public class DeclarationController : ApiController
    {
        BaseErrorResponse _baseErrorResponse;
        BaseSuccessResponse _baseSuccessResponse;

        // POST api/declarations/create
        /// <summary>
        /// Crea una declaracion asociada a una orden de compra
        /// </summary>
        /// <param name="declaration">Modelo de la declaracion a crear</param>
        /// <returns>Mensaje de respuesta de la operacion</returns>
        [Route("create")]
        [ResponseType(typeof(DeclarationPostResponse))]
        [HttpPost]
        public HttpResponseMessage Create(DeclarationModel declaration)
        {
            // Variables
            UDeclaration _uDeclaration;
            CPurchaseOrder _cPurchaseOrder;
            var DBLL = new DeclarationBLL();
            var CPOBLL = new CPurchaseOrderBLL();
            var errorList = new List<string>();
            bool isRequestReconciled;



            if (!ModelState.IsValid)
            {
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
                _baseErrorResponse = new BaseErrorResponse(errorList, null);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.Message,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }

            try
            {
                // Leemos los datos necesarios desde el token
                var jwtSecurityToken = ApiHelper.ReadTokenFromHeader(Request);
                string rif = jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.RifClaim).FirstOrDefault().Value;
                Guid cuserid = Guid.Parse(jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.CUserClaim).FirstOrDefault().Value);

                // Obtenemos la orden de compra asociada a la declaracion
                _cPurchaseOrder = CPOBLL.GetPurchaseOrder(rif, declaration.idpurchaseorder);

                #region Validations
                // Verificamos que el numero de referencia no este duplicado
                if (DBLL.GetDeclarations(declaration.referencenumber, declaration.idissuingbank).ToList().Count > 0)
                {
                    throw new DuplicateReferenceException(ApiErrors.DuplicateReferenceExceptionMessage, ApiErrors.DuplicateReferenceExceptionCode);
                }
                // Verificamos que la orden de compra exista
                if (_cPurchaseOrder == null)
                {
                    throw new PurchaseOrderNotFoundException(ApiErrors.PurchaseOrderNotFoundExceptionMessage, ApiErrors.PurchaseOrderNotFoundExceptionCode);
                }
                // Verificamos si la orden de compra ya tiene una declaracion asociada no anulada
                if (_cPurchaseOrder.UDeclarations.Count > 0 && _cPurchaseOrder.UDeclarations.Any(c => c.IdUDeclarationStatus != (int)DeclarationStatus.Annulled))
                {
                    throw new PurchaseOrderDeclaredException(ApiErrors.PurchaseOrderDeclaredExceptionMessage, ApiErrors.PurchaseOrderDeclaredExceptionCode);
                }
                // Verificamos si los montos coinciden
                if (_cPurchaseOrder.Amount != declaration.amount)
                {
                    throw new DeclarationAmountException(ApiErrors.DeclarationAmountExceptionMessage, ApiErrors.DeclarationAmountExceptionCode);
                }
                // Verificamos que las credenciales del pagador coinciden
                if (_cPurchaseOrder.EndUserCI != declaration.paymentuser.userci || _cPurchaseOrder.EndUserEmail != declaration.paymentuser.useremail)
                {
                    throw new DeclarationPaymentUserException(ApiErrors.DeclarationPaymentUserExceptionMessage, ApiErrors.DeclarationPaymentUserExceptionCode);
                }
                // Verificamos el estado de la orden de compra
                if (_cPurchaseOrder.IdCPurchaseOrderStatus.Equals((int)PurchaseOrderStatus.Annulled))
                {
                    throw new PurchaseOrderAnnulledException(ApiErrors.PurchaseOrderAnnulledExceptionMessage, ApiErrors.PurchaseOrderAnnulledExceptionCode);
                }
                #endregion

                // Creamos la declaracion en la base de datos
                _uDeclaration = new UDeclaration(declaration);
                // Asociamos la declaracion al comercio, usuario, y orden de compra correspondiente
                _uDeclaration.RifCommerce = rif;
                _uDeclaration.IdCUser = cuserid;
                _uDeclaration.IdCPurchaseOrder = declaration.idpurchaseorder;

                // Cambiamos el estado de la orden de compra
                _cPurchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.Declared;

                // Cambiamos el estado de la solicitud si existe
                if (_cPurchaseOrder.PaymentRequests.Count > 0)
                {
                    _cPurchaseOrder.PaymentRequests.FirstOrDefault().IdPaymentRequestStatus = (int)InstaTransfer.ITResources.Enums.PaymentRequestStatus.Declared;
                }

                // Guardamos los cambios en la orden de compra en la base de datos
                CPOBLL.SaveChanges();

                // Guardamos la declaracion en la base de datos
                DBLL.AddEntity(_uDeclaration);
                DBLL.SaveChanges();

                //Creamos la declaracion en la base de datos
                using (DeclarationBLL UDBLL = new DeclarationBLL())
                {
                    // Intentamos conciliar la declaración
                    isRequestReconciled = UDBLL.TryReconcileDeclaration(_uDeclaration.Id, ReconciliationType.Api);
                }
            }

            catch (DuplicateReferenceException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationValidationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            catch (PurchaseOrderNotFoundException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationValidationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            catch (PurchaseOrderDeclaredException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationValidationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            catch (DeclarationAmountException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationValidationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            catch (DeclarationPaymentUserException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationValidationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            catch (PurchaseOrderAnnulledException e)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationValidationErrorMessage, e.ErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            catch (Exception)
            {
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationErrorMessage, ApiErrors.DeclarationErrorCode);
                var badresult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.ResponseMessage,
                    iddeclaration = _baseErrorResponse.ResponseObject
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, badresult);
            }
            // Construimos la respuesta base
             var _message = isRequestReconciled ? ApiResources.DeclarationReconciledSuccessMessage : ApiResources.DeclarationSuccessMessage;
            _baseSuccessResponse = new BaseSuccessResponse(_message, declaration);
            // Construimos el formato del resultado
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message,
                iddeclaration = declaration.id
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        // POST api/declarations/get
        /// <summary>
        /// Obtiene una <see cref="UDeclaration"/> a partir de un id especifoco
        /// </summary>
        /// <param name="baseModel">Modelo con el id de la declaracion</param>
        /// <returns>Respuesta de la operacion</returns>
        [Route("get")]
        [ResponseType(typeof(DeclarationGetResponse))]
        [HttpPost]
        public HttpResponseMessage Get(BaseModel baseModel)
        {
            UDeclaration declaration = null;
            var DBLL = new DeclarationBLL();
            object idstatus = null;
            object status = null;
            // Obtenemos los datos necesarios desde el token
            var jwtSecurityToken = ApiHelper.ReadTokenFromHeader(Request);
            string rif = jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.RifClaim).FirstOrDefault().Value;

            if (baseModel == null)
            {
                // Construimos la respuesta base
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.ParametersRequiredErrorMessage, null);
                // Construimos el formato del resultado
                var badResult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.Message,
                    idstatus,
                    status
                };
                // Retornamos el resultado y codigo de status
                return Request.CreateResponse(HttpStatusCode.BadRequest, badResult);
            }
            declaration = DBLL.GetDeclaration(rif, baseModel.id);

            if (declaration == null)
            {
                // Construimos la respuesta base
                _baseErrorResponse = new BaseErrorResponse(ApiErrors.DeclarationNotFoundErrorCode, declaration);
                // Construimos el formato del resultado
                var badResult = new
                {
                    success = _baseErrorResponse.Success,
                    message = _baseErrorResponse.Message,
                    idstatus,
                    status
                };
                // Retornamos el resultado y codigo de status
                return Request.CreateResponse(HttpStatusCode.NotFound, badResult);
            }
            // Construimos la respuesta base
            _baseSuccessResponse = new BaseSuccessResponse(ApiResources.OperationSuccessMessage, declaration);
            // Construimos el formato del resultado
            var result = new
            {
                success = _baseSuccessResponse.Success,
                message = _baseSuccessResponse.Message,
                idstatus = declaration.IdUDeclarationStatus,
                status = declaration.UDeclarationStatus.Description
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

    }
}
