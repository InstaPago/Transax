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



        // GET api/banks/Charge
        /// <summary>
        /// Obtiene todos los bancos disponibles
        /// </summary>
        /// <returns>Respuesta con el estado, mensaje, y lista de bancos</returns>
        [Route("GetInfoCharge")]
        //[ResponseType(typeof(BankGetResponse))]
        [HttpPost]
        public HttpResponseMessage GetInfoCharge([FromBody]GetInfo model)
        {
            Guid Key = model.Key;
            Guid Data = model.Id;

            URepository<CP_ItemArchivo> CP_ItemArchivoREPO = new URepository<CP_ItemArchivo>();
            URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();

            CP_Archivo archivo = CP_ArchivoREPO.GetAllRecords().Where(u => u.IdReferencia == Data).FirstOrDefault();

            List<CP_ItemArchivo> items = CP_ItemArchivoREPO.GetAllRecords().Where(u => u.IdReferencia == Data).ToList();

            var json = new JavaScriptSerializer().Serialize(items);

            var result = new
            {
                status = "1",
                id = archivo.Id.ToString(),
                timecreated = archivo.Nombre.ToString(),
                responsebanktime = archivo.FechaLectura.ToString(),
                items = json,
                fulldata = archivo.Contenido.ToString()               

            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/banks/Charge
        /// <summary>
        /// Obtiene todos los bancos disponibles
        /// </summary>
        /// <returns>Respuesta con el estado, mensaje, y lista de bancos</returns>
        [Route("Charge")]
        //[ResponseType(typeof(BankGetResponse))]
        [HttpPost]
        public HttpResponseMessage Charge([FromBody]Create model)
        {
            Guid Key = model.Key;
            String Data = model.DataPayment;

            JavaScriptSerializer __jsonSerializer = new JavaScriptSerializer();
            Cobro __data = (Cobro)__jsonSerializer.Deserialize(Data, typeof(Cobro));
            CP_Archivo _archivo = _GenerarArchivoBANPOL(__data);

            var result = new
            {
                success = true,
                message = "Archivo guardado de forma correcta",
                referencia = _archivo.IdReferencia.ToString(),
                name = _archivo.Nombre.ToString()

            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        public CP_Archivo  _GenerarArchivoBANPOL(Cobro Cobros)
        {          
            Guid referencia = Guid.NewGuid();
            int cantidadmovimientos = Cobros.Cliente.Count();
            string rif = Cobros.Empresa.Documento;
            string fechaarchivo = DateTime.Now.AddDays(0).ToString("ddMMyy.hhmm");
            string id = Cobros.Empresa.IdCobro.ToString();
            if (id.Length > 4)
            {
                id = id.Substring((id.Length - 4), 4);
            }
            else if (id.Length < 4)
            {
                id = id.PadLeft(4, '0');
            }
            string numeroorden = DateTime.Now.AddDays(0).ToString("yyMMdd");
            string _fecha = DateTime.Now.AddDays(0).ToString("yyyyMMdd");
            numeroorden = numeroorden + id;
            string registro = "00";
            string asociado = Cobros.Empresa.CodBancario;
            string ordencobroreferencia = numeroorden;
            string documento = "DIRDEB";
            string banco = "01";
            string fecha = DateTime.Now.AddDays(0).ToString("yyyyMMddhhmmss");
            string registrodecontrol = registro + asociado.PadRight(35) + ordencobroreferencia.PadRight(30) + documento + fecha.PadRight(14) + banco;
            string tiporegistro = "01";
            string transaccion = "DMI";
            string condicion = "9";

            //string fecha = DateTime.Now.ToString("yyyyMMddhhmmss");
            string encabezado = tiporegistro + transaccion.PadRight(35) + condicion.PadRight(3) + ordencobroreferencia.PadRight(35) + _fecha;
            URepository<CP_ItemArchivo> CP_ItemArchivoREPO = new URepository<CP_ItemArchivo>();
            decimal total = 0;
            //debitos
            //decimal total = 0;
            List<string> _cobros = new List<string>();
            foreach (var cobro in Cobros.Cliente)
            {
                string tipo = "03";
                string recibo = cobro.Idtransaccion.ToString().PadLeft(8, '0');
                decimal _monto = decimal.Parse(cobro.Monto) / 100;
                string montoacobrar = cobro.Monto;
                total = total + _monto;
                string moneda = "VES";
                string numerocuenta = cobro.CuentaBancaria;
                string swift = "UNIOVECA";
                string nombre = cobro.RazonSocial.Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
                string libre = "423";
                string contrato = cobro.Documento;
                string fechavencimiento = "      ";

                string debito = tipo + recibo.PadRight(30)
                    + montoacobrar.PadLeft(15, '0') + moneda + numerocuenta.PadRight(30)
                    + swift.PadRight(11) + cobro.Documento.PadRight(17) + nombre.PadRight(35)
                    + libre + contrato.PadRight(35) + fechavencimiento;
                _cobros.Add(debito);

                CP_ItemArchivo item = new CP_ItemArchivo();
                item.Cuenta = cobro.CuentaBancaria;
                item.Detalle = debito;
                item.Estatus = 1;
                item.FechaRegistro = DateTime.Now;
                item.FechaRespuesta = null;
                item.IdReferencia = referencia;
                item.Monto = _monto;
                item.RazonSocial = cobro.RazonSocial;
                item.RespuestaBanco = "";
                CP_ItemArchivoREPO.AddEntity(item);

            }
    
            decimal _total = total * 100;
            //registro credito
            string _tipo2 = "02";
            string _recibo = Cobros.Cliente.First().Idtransaccion.ToString().PadLeft(8, '0');
            string _rif = Cobros.Empresa.Documento;
            string ordenante = Cobros.Empresa.RazonSocial.ToString().Replace(".", " ").Replace(",", " ").ToUpper().TrimEnd();
            string _montoabono = _total.ToString().Split(',')[0];
            string _moneda = "VES";
            string _numerocuenta = Cobros.Empresa.CuentaBancaria;
            string _swift = "UNIOVECA";
            //string _fecha = DateTime.Now.ToString("yyyyMMdd");
            string formadepago = "423";
            string instruordenante = " ";
            string credito = _tipo2 + _recibo.PadRight(30) + _rif.PadRight(17) + ordenante.PadRight(35)
                + _montoabono.PadLeft(15, '0') + _moneda + instruordenante + _numerocuenta.PadRight(35)
                + _swift.PadRight(11) + _fecha + formadepago;


            //_cobros
            string[] lines = { registrodecontrol, encabezado, credito };
            foreach (var _item in _cobros)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = _item;
            }

            //totalizador
            string _tipo = "04";
            string totalcreditos = "1";
            string debitos = Cobros.Cliente.Count().ToString();
            string montototal = total.ToString().Split(',')[0];
            string totales = _tipo + totalcreditos.PadLeft(15, '0') + debitos.PadLeft(15, '0') + montototal.PadLeft(15, '0');
            Array.Resize(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = totales;

            string ruta = @"E:\Apps\archivos\" + "I0005." + asociado + "." + fechaarchivo + ".txt";
            //System.IO.File.WriteAllLines(ruta, lines);

            URepository<CP_Archivo> CP_ArchivoREPO = new URepository<CP_Archivo>();
            CP_Archivo archivo = new CP_Archivo();
            archivo.IdEmpresa = 1;
            archivo.Nombre = "I0005." + asociado +"."+ fechaarchivo + ".txt";
            archivo.Ruta = ruta;
            archivo.Tipo = 1;
            string contenido = "";
            foreach (var item in lines)
            {
                contenido = contenido + item + "</br>";
            }
            archivo.Contenido = contenido;
            archivo.FechaLectura = DateTime.Now;
            archivo.FechaCreacion = DateTime.Now;
            archivo.Descripcion = "["+ Cobros.Empresa.RazonSocial.ToUpper() + "] Cargo cuenta masivo.";
            archivo.IdCP_Archivo = null;
            archivo.ReferenciaOrigen = "Estado de cuenta operaciones de prestamos";
            archivo.IdReferencia = referencia;
            archivo.EsRespuesta = false;
            archivo.ContenidoRespuesta = "Esperando";
            archivo.ReferenciaArchivoBanco = numeroorden;
            CP_ArchivoREPO.AddEntity(archivo);
            CP_ArchivoREPO.SaveChanges();

            CP_ItemArchivoREPO.SaveChanges();

            return archivo;
        }


        public class Create
        {
            public Guid Key { get; set; }
            public String DataPayment { get; set; }
        }



        public class GetInfo
        {
            public Guid Key { get; set; }
            public Guid Id { get; set; }

        }

        public class Cobro
        { 
            public Empresa Empresa { get; set; }

            public List<Cliente> Cliente { get; set; }
        
        }



        public class Empresa
        { 
            public string Documento { get; set; }

            public string CodBancario { get; set; }

            public string RazonSocial { get; set; }

            public string CuentaBancaria { get; set; }
            public string IdCobro { get; set; }

        }

        public class Cliente
        {
            public string RazonSocial { get; set; }

            public string CuentaBancaria { get; set; }

            public string Monto { get; set; }

            public string Documento { get; set; }

            public string Idtransaccion { get; set; }

        }


    }
}

