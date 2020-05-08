using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic;
using InstaTransfer.ITLogic.Factory;
using ITApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITResources.Api;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.BLL.Models;
using System.Web.Http.Description;

namespace ITApi.Controllers
{
    /// <summary>
    /// Controlador de bancos y cuentas bancarias del sistema
    /// </summary>
    [Authorize(Roles = "CommerceApiUser")]
    [RoutePrefix("api/banks")]
    public class BankController : ApiController
    {
        BaseSuccessResponse baseSuccessResponse;

        // GET api/banks/accounts
        /// <summary>
        /// Obtiene todas las cuentas bancarias disponibles
        /// </summary>
        /// <returns>Respuesta con el estado, mensaje, y lista de bancos</returns>
        [Route("accounts")]
        [ResponseType(typeof(BankAccountGetResponse))]
        [HttpGet]
        public HttpResponseMessage GetAllBankAccounts()
        {

            var jwtSecurityToken = ApiHelper.ReadTokenFromHeader(Request);

            string rif = jwtSecurityToken.Claims.Where(c => c.Type == ApiResources.RifClaim).FirstOrDefault().Value;

            Command commandGetBankAccounts;
            List<UBankAccount> bankAccounts;
            List<BankAccountModel> bankAccountModels = new List<BankAccountModel>();

            commandGetBankAccounts = CommandFactory.GetCommandGetBankAccounts();
            commandGetBankAccounts.Execute();

            bankAccounts = (List<UBankAccount>)commandGetBankAccounts.Receiver;

            foreach (var bankAccount in bankAccounts)
            {
                // Creamos la lista ordenada de instrucciones
                var instructions = bankAccount.UBankInstructions.OrderBy(x => x.Order).Select(x => x.Description);

                // Creamos cada elemento del modelo
                var bankAccountModel = new BankAccountModel()
                {
                    accountnumber = bankAccount.AccountNumber,
                    bankreceiver = bankAccount.UBank.Name,
                    rif = bankAccount.IdUSocialReason,
                    bankinstructions = instructions.ToArray()
                };
                // Agregamos el elemento a la lista de modelos
                bankAccountModels.Add(bankAccountModel);
            }
            // Construimos la respuesta base
            baseSuccessResponse = new BaseSuccessResponse(ApiResources.OperationSuccessMessage, bankAccountModels);
            // Construimos el formato del resultado
            var result = new
            {
                success = baseSuccessResponse.Success,
                message = baseSuccessResponse.Message,
                bankAccounts = baseSuccessResponse.ResponseObject
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.OK, result);

        }

        // GET api/banks/get
        /// <summary>
        /// Obtiene todos los bancos disponibles
        /// </summary>
        /// <returns>Respuesta con el estado, mensaje, y lista de bancos</returns>
        [Route("get")]
        [ResponseType(typeof(BankGetResponse))]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            URepository<UBank> UBRepo = new URepository<UBank>();

            List<IssuingBankModel> issuingBankModels = new List<IssuingBankModel>();

            var issuingBanks = UBRepo.GetAllRecords();

            foreach (var issuingBank in issuingBanks)
            {
                // Creamos cada elemento del modelo
                var issuingBankModel = new IssuingBankModel()
                {
                    id = issuingBank.Id,
                    bankname = issuingBank.Name
                };
                // Agregamos el elemento a la lista de modelos
                issuingBankModels.Add(issuingBankModel);
            }
            // Construimos la respuesta base
            baseSuccessResponse = new BaseSuccessResponse(ApiResources.OperationSuccessMessage, issuingBankModels);
            // Construimos el formato del resultado
            var result = new
            {
                success = baseSuccessResponse.Success,
                message = baseSuccessResponse.Message,
                issuingbanks = baseSuccessResponse.ResponseObject
            };
            // Retornamos el resultado y codigo de status
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

    }
}

