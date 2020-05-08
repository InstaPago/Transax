using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaymentRequest.Controllers
{
    public class PagesController : Controller
    {

        public ActionResult Email()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View();
        }

        public ViewResult ServerError()
        {
            return View();
        }

        public ActionResult Blank()
        {
            return View();
        }

        public ActionResult RequestNotFound()
        {
            return View();
        }

        public ActionResult RequestProcessed()
        {
            return View();
        }
        public ActionResult RequestNotAssociated()
        {
            return View();
        }
    }
}