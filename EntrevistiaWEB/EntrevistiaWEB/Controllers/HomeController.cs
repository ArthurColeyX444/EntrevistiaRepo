using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntrevistiaWEB.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult InicioCliente()
        {
            // Validación de seguridad básica: Cerrar sesión y mandarlo al login
            if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Cliente")
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        public ActionResult InicioAdmin()
        {
            // Validación de seguridad básica: Cerrar sesión y mandarlo al login
            if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

    }
}
