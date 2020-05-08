using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Umbrella.Controllers
{
    public class PagesController : Controller
    {

        public ActionResult Email()
        {
            return View();
        }
        public ActionResult NotFoundError()
        {
            return View();
        }

        public ActionResult InternalServerError()
        {
            return View();
        }

        public ActionResult EmptyPage()
        {
            var z = 0;
            var x = 3 / z;
            return View();
        }

    }
}