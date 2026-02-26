using EntrevistiaWEB.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EntrevistiaWEB.Controllers
{

    public class AccountController : Controller
    {
        private readonly IMongoCollection<Administrador> _admins;
        private readonly IMongoCollection<Cliente> _clientes;

        public AccountController()
        {
            // Cadena de conexion MongoBD Atlas
            var client = new MongoClient("mongodb+srv://arthurcoley:634990@entrevistia.jstrsql.mongodb.net/?retryWrites=true&w=majority&appName=entrevistia");
            var database = client.GetDatabase("EntrevistIA");

            _admins = database.GetCollection<Administrador>("Administrador");
            _clientes = database.GetCollection<Cliente>("Cliente");
        }

        [HttpPost]
        public ActionResult Login(string usuario, string password)
        {
            // 1. Buscar como Administrador
            var admin = _admins.Find(a => (a.correoAdmin == usuario || a.idAdmin == usuario)
                                     && a.contraseñaAdmin == password).FirstOrDefault();

            if (admin != null)
            {
                Session["Perfil"] = "Admin";
                Session["Nombre"] = admin.nombreAdmin;
                return RedirectToAction("InicioAdmin", "Home");
            }

            // 2. Buscar como cliente
            var cliente = _clientes.Find(c => (c.correoCliente == usuario || c.idCliente == usuario)
                                         && c.contraseñaCliente == password).FirstOrDefault();

            if (cliente != null)
            {
                Session["Perfil"] = "Cliente";
                Session["Nombre"] = cliente.nombresCliente;
                return RedirectToAction("InicioCliente", "Home");
            }

            ViewBag.Error = "Credenciales incorrectas";
            return View("~/Views/Home/Login.cshtml");
        }


        [HttpPost]
        public ActionResult RegisterCliente(Cliente nuevo)
        {
            try
            {
                // 1. Validaciones de Formato (Servidor)
                if (string.IsNullOrEmpty(nuevo.nombresCliente) || !System.Text.RegularExpressions.Regex.IsMatch(nuevo.nombresCliente, @"^[a-zA-Z\s]+$"))
                {
                    ViewBag.ErrorRegistro = "El nombre solo debe contener letras.";
                    return View("~/Views/Home/Login.cshtml");
                }

                if (!nuevo.correoCliente.Contains("@"))
                {
                    ViewBag.ErrorRegistro = "El correo electrónico debe ser válido (incluir @).";
                    return View("~/Views/Home/Login.cshtml");
                }

                if (nuevo.edadCliente <= 0 || nuevo.edadCliente > 120)
                {
                    ViewBag.ErrorRegistro = "Por favor, ingrese una edad válida.";
                    return View("~/Views/Home/Login.cshtml");
                }

                // 2. Verificación de Duplicados en MongoDB
                var correoExiste = _clientes.Find(c => c.correoCliente == nuevo.correoCliente).FirstOrDefault();
                if (correoExiste != null)
                {
                    ViewBag.ErrorRegistro = "Este correo ya está registrado.";
                    return View("~/Views/Home/Login.cshtml");
                }

                var idExiste = _clientes.Find(c => c.idCliente == nuevo.idCliente).FirstOrDefault();
                if (idExiste != null)
                {
                    ViewBag.ErrorRegistro = "Esta identificación ya está registrada.";
                    return View("~/Views/Home/Login.cshtml");
                }

                // 3. Inserción Final
                _clientes.InsertOne(nuevo);
                TempData["Success"] = "¡Registro exitoso! Ya puedes ingresar.";
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorRegistro = "Error crítico: " + ex.Message;
                return View("~/Views/Home/Login.cshtml");
            }
        }

        public ActionResult Logout()
        {
            // Limpia todas las variables de sesión
            Session.Clear();
            Session.Abandon();

            // Opcional: Elimina la cookie de autenticación si usas FormsAuthentication
            // System.Web.Security.FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
    }
}