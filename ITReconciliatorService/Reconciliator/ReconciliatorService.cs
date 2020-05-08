using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITExceptions.Service.Reconciliator;
using InstaTransfer.ITLogic.Log;
using InstaTransfer.ITResources.BackEnd;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.General;
using InstaTransfer.ITResources.Service.Reconciliator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace InstaTransfer.ITReconciliatorService.Reconciliator
{
    public partial class ReconciliatorService : ServiceBase
    {
        #region Variables
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        #endregion

        #region Constructor
        public ReconciliatorService()
        {
            InitializeComponent();

            // Ties the EventLog member of the ServiceBase base class to the
            // ServiceExample event log created when the service was installed.
            EventLog.Log = "Umbrella Log";
        }
        #endregion

        #region Callbacks
        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Set up a timer to trigger n minutes.  
            System.Timers.Timer timer = new System.Timers.Timer();
            double time;
            //Tomo el tiempo desde el app.config. Por defecto en 1m.
            try
            {
                time = int.Parse(ConfigurationManager.AppSettings[GeneralResources.AppSettingsKeyReconciliatorTime]);
            }
            catch (Exception)
            {
                time = 60000;
            }

            timer.Interval = time;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            Logger.WriteSuccessLog(this.ServiceName + " inició correctamente.", GeneralResources.SourceNameUmbrellaReconciliator);

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            Logger.WriteSuccessLog(this.ServiceName + " termino correctamente.", GeneralResources.SourceNameUmbrellaReconciliator);
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            int successReconciliationsCount;
            int failedReconciliationsCount;
            int pendingReconciliationsCount;
            int requestsCount;
            try
            {
                RunReconciliator(out successReconciliationsCount, out failedReconciliationsCount, out pendingReconciliationsCount, out requestsCount);
                // Success
                Logger.WriteSuccessLog(string.Format("{0} movimiento(s) conciliado(s). {1} conciliaciones fallidas. {2} conciliaciones pendientes. {3} solicitudes procesadas", successReconciliationsCount, failedReconciliationsCount, pendingReconciliationsCount, requestsCount), GeneralResources.SourceNameUmbrellaReconciliator);
            }
            catch (Exception e)
            {
                // Error
                Logger.WriteErrorLog(e, GeneralResources.SourceNameUmbrellaReconciliator);
            }
        }

        protected override void OnContinue()
        {
            Logger.WriteSuccessLog(this.ServiceName + " ha sido resumido.", GeneralResources.SourceNameUmbrellaReconciliator);
        }


        #endregion

        #region Methods
        /// <summary>
        /// Concilia las declaraciones del sistema
        /// </summary>
        /// <param name="successReconciliationsCount">Conciliaciones exitosas</param>
        /// <param name="failedReconciliationsCount">Conciliaciones fallidas</param>
        /// <param name="pendingReconciliationsCount">Conciliaciones pendientes</param>
        /// <param name="requestsCount">Solicitudes de pago</param>
        /// <returns></returns>
        public bool RunReconciliator(out int successReconciliationsCount, out int failedReconciliationsCount, out int pendingReconciliationsCount, out int requestsCount)
        {
            // Declaramos las variables
            var DBLL = new DeclarationBLL();
            URepository<UBankStatementEntry> UBSERepo = new URepository<UBankStatementEntry>();
            CPurchaseOrderBLL CPOBLL = new CPurchaseOrderBLL();
            List<UBankStatementEntry> entryList = new List<UBankStatementEntry>();
            UBankStatementEntry entry = new UBankStatementEntry();
            CPurchaseOrder cPurchaseOrder;
            successReconciliationsCount = 0;
            failedReconciliationsCount = 0;
            pendingReconciliationsCount = 0;
            requestsCount = 0;

            // Obtenemos todas las declaraciones pendientes
            var pendingDeclarations = DBLL.GetDeclarationsByStatus(DeclarationStatus.ReconciliationPending).ToList();
            // Guardamos el numero de conciliaciones pendientes
            pendingReconciliationsCount = pendingDeclarations.Count;
            // Recorremos la lista de declaraciones pendientes
            foreach (var pendingDeclaration in pendingDeclarations)
            {
                try
                {
                    // Obtengo todos los movimientos que coincidan con el numero de referencia y banco declarado
                    entryList = UBSERepo.GetAllRecords(ubse => (ubse.Description.Contains(pendingDeclaration.Reference) || ubse.Ref.Contains(pendingDeclaration.Reference)) &&
                                                                ubse.UBankStatement.IdUBank_Receiver == pendingDeclaration.IdUBank).ToList();

                    // Verifico que existan movimientos
                    if (entryList.Count > 0)
                    {
                        // Obtengo los movimientos que coincidan con el monto declarado
                        entryList = entryList.Where(ubse => ubse.Amount == pendingDeclaration.Amount).ToList();
                        // Verifico si existe mas de un movimiento
                        if (entryList.Count > 1)
                        {
                            throw new MultiplePossibleEntriesException(ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionMessage, ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionCode);
                        }
                        // Si no coincide el monto declarado con el monto en cuenta entonces se notificara al comercio que debe modificar la orden de compra
                        else if (entryList.Count == 0)
                        {
                            throw new StatementAmountException(ReconciliatorServiceErrors.StatementAmountExceptionMessage, ReconciliatorServiceErrors.StatementAmountExceptionCode);
                        }
                    }
                    // Si el numero de referencia no coincide procedo a hacer otras validaciones
                    else
                    {
                        continue;
                    }
                    // Validamos el numero de movimientos posibles
                    if (entryList.Count == 1)
                    {
                        // Agarramos el unico elemento de la lista
                        entry = entryList.FirstOrDefault();

                        // Verifico que el movimiento no tenga declaraciones asociadas previamente
                        if (entry.UDeclarations.Count == 0)
                        {
                            // Le asignamos el entry a la declaracion
                            pendingDeclaration.IdUBankStatementEntry = entry.Id;
                            // Cambiamos el estado de la declaracion a concilaida
                            pendingDeclaration.IdUDeclarationStatus = (int)DeclarationStatus.Reconciled;
                            // Guardamos los cambios de la base de datos
                            DBLL.SaveChanges();

                            // Obtenemos la orden de compra asociada a la declaracion
                            cPurchaseOrder = CPOBLL.GetPurchaseOrder(pendingDeclaration.Id);
                            // Cambiamos el estado de la orden de compra
                            cPurchaseOrder.IdCPurchaseOrderStatus = (int)PurchaseOrderStatus.DeclaredReconciled;
                            // Verificamos si tiene solicitudes de pago
                            if (cPurchaseOrder.PaymentRequests.Count > 0)
                            {
                                // Obtenemos el request
                                var request = cPurchaseOrder.PaymentRequests.FirstOrDefault();
                                // Modificamos el estado de la solicitud
                                request.IdPaymentRequestStatus = (int)ITResources.Enums.PaymentRequestStatus.DeclaredReconciled;
                                requestsCount++;

                                // Enviamos el correo de confirmacion
                                var result = SendMail(request);

                                // Verificamos si se envio el correo
                                if (result)
                                {
                                    request.TimesSent++;
                                }
                            }

                            // Guardamos los cambios en la orden de compra en la base de datos
                            CPOBLL.SaveChanges();

                            // Registramos la transaccion en el balance del comercio
                            using (var CBLL = new CommerceBLL())
                            {
                                // Obtenemos el comercio asociado a la declaracion
                                var commerce = CBLL.GetCommerce(pendingDeclaration.Id);
                                // Agregamos el registro de la conciliacion al balance del comercio
                                using (var CBBLL = new CommerceBalanceBLL())
                                {
                                    CBBLL.AddBalance("Conciliación Automática ID: " + pendingDeclaration.Id + "Declara: " + pendingDeclaration.EndUserCI, commerce.Rif, pendingDeclaration.Amount, pendingDeclaration.Id, CommerceBalanceType.Declaration);
                                }
                            }
                            // Success
                            successReconciliationsCount++;
                            pendingReconciliationsCount--;
                            continue;
                            //return true;
                        }
                        else
                        {
                            throw new StatementDeclaredException(ReconciliatorServiceErrors.StatementDeclaredExceptionMessage, ReconciliatorServiceErrors.StatementDeclaredExceptionCode);
                        }

                    }
                    // Error
                    else if (entryList.Count > 1)
                    {
                        throw new MultiplePossibleEntriesException(ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionMessage, ReconciliatorServiceErrors.MultiplePossibleEntriesExceptionCode);
                    }
                    else if (entryList.Count == 0 || entryList.Equals(null))
                    {
                        continue;
                        //return false;
                    }
                }
                // Error
                catch (StatementDeclaredException e)
                {
                    failedReconciliationsCount++;
                    pendingReconciliationsCount--;
                    Logger.WriteErrorLog("ID: " + pendingDeclaration.Id + " - " + e.MessageException, GeneralResources.SourceNameUmbrellaReconciliator);
                    continue;
                }
                catch (MultiplePossibleEntriesException e)
                {
                    failedReconciliationsCount++;
                    pendingReconciliationsCount--;
                    Logger.WriteErrorLog("ID: " + pendingDeclaration.Id + " - " + e.MessageException, GeneralResources.SourceNameUmbrellaReconciliator);
                    continue;
                }
                catch (StatementAmountException e)
                {
                    failedReconciliationsCount++;
                    pendingReconciliationsCount--;
                    Logger.WriteErrorLog("ID: " + pendingDeclaration.Id + " - " + e.MessageException, GeneralResources.SourceNameUmbrellaReconciliator);
                    continue;
                }
                catch (Exception e)
                {
                    failedReconciliationsCount++;
                    pendingReconciliationsCount--;
                    Logger.WriteErrorLog("ID: " + pendingDeclaration.Id + " - " + e, GeneralResources.SourceNameUmbrellaReconciliator);
                    continue;
                }
            }
            // Success
            if (successReconciliationsCount > 0)
            {
                return true;
            }
            else
            {
                // Nothing reconciled
                return false;
            }


        }

        public bool SendMail(PaymentRequest request)
        {
            try
            {
                // Verificamos que la solicitud no este vacía
                if (request == null)
                {
                    throw new Exception();
                }
                // Construimos el body
                StringBuilder requestBody = new StringBuilder("");
                requestBody.Append("type=" + InstaTransfer.ITResources.Enums.EmailType.ReconciliationSuccess + "&");
                requestBody.Append("pass=" + "MmS(#FG844c6J" + "&");
                requestBody.Append("model=" + request.Id);
                string query = requestBody.ToString();
                string url = ConfigurationManager.AppSettings[BackEndResources.AppSettingsKeyPaymentRequestUrl].ToString();
                byte[] queryStream = Encoding.UTF8.GetBytes(query);

                //Llamo al API con el url que contiene los parámetros
                WebRequest req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = queryStream.Length;
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(queryStream, 0, queryStream.Length);
                reqStream.Close();

                //response
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                SendMailModelResponse _sendMailModelResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(SendMailModelResponse)) as SendMailModelResponse;

                // Verificamos la respuesta
                if (_sendMailModelResponse.success)
                {
                    // Success
                    return true;
                }
                else
                {
                    // Fallida
                    return false;
                }
            }
            catch (Exception)
            {
                // Error
                return false;
            }
            // Error - Llegamos hasta aqui
            return false;
        }

        public class SendMailModelResponse
        {
            public bool success { get; set; }
        }
        #endregion

    }
}
