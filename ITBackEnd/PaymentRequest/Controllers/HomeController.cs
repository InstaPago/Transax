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
using PaymentRequest.Models;
using static InstaTransfer.ITLogic.Helpers.SessionHelper;
using InstaTransfer.BLL.Models.Declaration;
using InstaTransfer.BLL.Models.PaymentUser;

namespace PaymentRequest.Controllers
{
    [Authorize(Roles =
    UserRoleConstant.TransaXAdmin + "," +
    UserRoleConstant.TransaXUser + "," +
    UserRoleConstant.CommerceAdmin + "," +
    UserRoleConstant.CommerceUser
    )]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}