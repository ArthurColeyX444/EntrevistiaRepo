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
            // Reemplaza con tu cadena de Atlas
            var client = new MongoClient("mongodb+srv://arthurcoley:634990@entrevistia.jstrsql.mongodb.net/?retryWrites=true&w=majority&appName=entrevistia");
            var database = client.GetDatabase("EntrevistIA");

            _admins = database.GetCollection<Administrador>("Administrador");
            _clientes = database.GetCollection<Cliente>("Cliente");
        }

        [HttpPost]
        public ActionResult Login(string usuario, string password)
        {
            // 1. Intentar buscar como Administrador
            var admin = _admins.Find(a => (a.correoAdmin == usuario || a.idAdmin == usuario)
                                     && a.contraseñaAdmin == password).FirstOrDefault();

            if (admin != null)
            {
                Session["Perfil"] = "Admin";
                Session["Nombre"] = admin.nombreAdmin;
                return RedirectToAction("InicioAdmin", "Home");
            }

            // 2. Si no es admin, intentar buscar como Cliente
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
                // 1. Verificar si el correo ya existe
                var correoExiste = _clientes.Find(c => c.correoCliente == nuevo.correoCliente).FirstOrDefault();
                if (correoExiste != null)
                {
                    ViewBag.ErrorRegistro = "El correo electrónico ya se encuentra registrado.";
                    return View("~/Views/Home/Login.cshtml");
                }

                // 2. Verificar si la identificación ya existe
                var idExiste = _clientes.Find(c => c.idCliente == nuevo.idCliente).FirstOrDefault();
                if (idExiste != null)
                {
                    ViewBag.ErrorRegistro = "La identificación ya se encuentra registrada.";
                    return View("~/Views/Home/Login.cshtml");
                }

                // 3. Si todo está bien, insertar
                _clientes.InsertOne(nuevo);
                TempData["Success"] = "¡Registro exitoso! Ya puedes ingresar.";
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorRegistro = "Ocurrió un error en el servidor: " + ex.Message;
                return View("~/Views/Home/Login.cshtml");
            }
        }
        

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
    }
}