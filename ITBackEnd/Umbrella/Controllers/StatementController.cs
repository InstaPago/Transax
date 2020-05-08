using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITLogic;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITLogic.Factory;
using InstaTransfer.ITLogic.Helpers;
using InstaTransfer.ITLogic.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using InstaTransfer.ITResources.Constants;
using Newtonsoft;
using Newtonsoft.Json;
using Umbrella.App_Code;

namespace Umbrella.Controllers
{
    [Authorize(Roles = UserRoleConstant.TransaXAdmin + "," + UserRoleConstant.TransaXUser)]
    public class StatementController : Controller
    {
        // GET: Statement
        [SessionExpireFilter]
        public ActionResult Index()
        {
            List<UBankStatementEntry> entries = GetPositiveEntries();
            ViewBag.SocialReasons = GetSocialReasons();

            List<Models.SelectTypehead> SelectedListRoomTypePeriod = new List<Models.SelectTypehead>();
            foreach (var Item in GetSocialReasons())
            {
                SelectedListRoomTypePeriod.Add(new Models.SelectTypehead
                {
                    id = Item.Name.Trim(),
                    name = Item.Id.ToString()
                });
            }
            ViewBag.SocialReasonsJson = JsonConvert.SerializeObject(SelectedListRoomTypePeriod, Newtonsoft.Json.Formatting.None, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });

            return View(entries);
        }

        /// <summary>
        /// Obtiene todos los <see cref="UBankStatementEntry"/> de la base de datos
        /// </summary>
        /// <returns>Lista de <see cref="UBankStatementEntry"/></returns>
        public List<UBankStatementEntry> GetEntries()
        {
            List<UBankStatementEntry> entryList = new List<UBankStatementEntry>();
            Command commandGetAllEntries;
            try
            {
                //Obtiene la instancia del comando
                commandGetAllEntries = CommandFactory.GetCommandGetAllEntries();
                //ejecuta el comando deseado
                commandGetAllEntries.Execute();
                //asigno el resultado a la lista de movimientos
                entryList = (List<UBankStatementEntry>)commandGetAllEntries.Receiver;
                //reviso si la lista de movimientos no esta vacia
                if (entryList == null)
                {
                    //throw new NullEntryListException();
                }

            }
            catch (Exception)
            {
                // No hacer nada
            }
            return entryList;
        }

        /// <summary>
        /// Obtiene todos los <see cref="UBankStatementEntry"/> de la base de datos
        /// </summary>
        /// <returns>Lista de <see cref="UBankStatementEntry"/></returns>
        public List<UBankStatementEntry> GetPositiveEntries()
        {
            List<UBankStatementEntry> entryList = new List<UBankStatementEntry>();
            Command commandGetAllEntries;
            try
            {
                //Obtiene la instancia del comando
                commandGetAllEntries = CommandFactory.GetCommandGetAllEntries();
                commandGetAllEntries.Parameter = BankStatementEntryType.Credit;
                //ejecuta el comando deseado
                commandGetAllEntries.Execute();
                //asigno el resultado a la lista de movimientos
                entryList = (List<UBankStatementEntry>)commandGetAllEntries.Receiver;
                //reviso si la lista de movimientos no esta vacia
                if (entryList == null)
                {
                    //throw new NullEntryListException();
                }

            }
            catch (Exception)
            {
                // No hacer nada
            }
            return entryList;
        }

        /// <summary>
        /// Obtiene todos los <see cref="USocialReason"/> de la base de datos
        /// </summary>
        /// <returns>Lista de <see cref="USocialReason"/></returns>
        public List<USocialReason> GetSocialReasons()
        {
            List<USocialReason> socialReasonList = new List<USocialReason>();
            Command commandGetAllSocialReasons;
            try
            {
                //Obtiene la instancia del comando
                commandGetAllSocialReasons = CommandFactory.GetCommandGetAllSocialReasons();
                //ejecuta el comando deseado
                commandGetAllSocialReasons.Execute();
                //asigno el resultado a la lista de razon social
                socialReasonList = (List<USocialReason>)commandGetAllSocialReasons.Receiver;
                //reviso si la lista de razon social no esta vacia
                if (socialReasonList == null)
                {
                    //throw new NullEntryListException();
                }

            }
            catch (Exception)
            {
                // No hacer nada
            }
            return socialReasonList;
        }

        /// <summary>
        /// Obtiene toda la data a mostrar en la vista
        /// </summary>
        /// <returns>La data a mostrar</returns>
        [SessionExpireFilter]
        public ActionResult GetData()
        {
            List<UBankStatementEntry> entries = GetPositiveEntries();
            List<USocialReason> socialReasons = GetSocialReasons();

            int __counttotal = entries.Count;
            var result = from c in entries
                         select new[] {
                           c.Date.ToString("yyyy/MM/dd", new System.Globalization.CultureInfo("es-VE")),
                           c.Ref,
                           c.Description,
                           c.Amount.ToString("#,##0.00", new System.Globalization.CultureInfo("es-VE")),
                           c.UBank != null ? c.UBank.Name.ToString() : string.Empty,
                           c.TimeStamp.Value != null ? c.TimeStamp.Value.ToString("yyyy/MM/dd h:mm tt",  System.Globalization.CultureInfo.InvariantCulture) : string.Empty,
                           socialReasons.Find(sr => sr.Id == c.UBankStatement.IdSocialReason).Name.Trim()
                        };
            return Json(new
            {
                data = result
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
