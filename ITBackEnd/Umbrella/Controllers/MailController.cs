using BLL.Concrete;
using InstaTransfer.BLL.Concrete;
using InstaTransfer.DataAccess;
using InstaTransfer.ITResources.Constants;
using InstaTransfer.ITResources.Enums;
using InstaTransfer.ITResources.Global;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Umbrella.App_Code;
using Umbrella.Models;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;

namespace Umbrella.Controllers
{
    public class MailController : Controller
    {
        URepository<AE_ValorAccionTR> AE_ValorAccionTRREPO = new URepository<AE_ValorAccionTR>();
        // GET: Mail
        public ActionResult Mail()
        {
            var model = AE_ValorAccionTRREPO.GetAllRecords().OrderByDescending(u => u.FechaOperacion).Take(2).ToList();
            return View(model);
        }
    }
}