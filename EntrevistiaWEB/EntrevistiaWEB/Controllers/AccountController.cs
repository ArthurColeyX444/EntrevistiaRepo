using EntrevistiaWEB.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security; // 🔥 AGREGADO
using System.Security.Claims; // 🔥 AGREGADO

namespace EntrevistiaWEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMongoCollection<Administrador> _admins;
        private readonly IMongoCollection<Cliente> _clientes;

        public AccountController()
        {
            var client = new MongoClient("mongodb+srv://arthurcoley:634990@entrevistia.jstrsql.mongodb.net/?retryWrites=true&w=majority&appName=entrevistia");
            var database = client.GetDatabase("EntrevistIA");

            _admins = database.GetCollection<Administrador>("Administrador");
            _clientes = database.GetCollection<Cliente>("Cliente");
        }

        [HttpPost]
        public ActionResult Login(string usuario, string password)
        {
            var admin = _admins.Find(a => (a.correoAdmin == usuario || a.idAdmin == usuario)
                                     && a.contraseñaAdmin == password).FirstOrDefault();

            if (admin != null)
            {
                Session["Perfil"] = "Admin";
                Session["Nombre"] = admin.nombreAdmin;
                return RedirectToAction("InicioAdmin", "Home");
            }

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
                try
                {
                    if (nuevo.InternalId == ObjectId.Empty)
                        nuevo.InternalId = ObjectId.GenerateNewId();
                    _clientes.InsertOne(nuevo);
                    // Log the user in immediately after successful registration
                    Session["Perfil"] = "Cliente";
                    Session["Nombre"] = nuevo.nombresCliente;

                    var auth = HttpContext.GetOwinContext().Authentication;
                    var appIdentity = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.NameIdentifier, nuevo.idCliente ?? string.Empty),
                        new Claim(ClaimTypes.Name, nuevo.nombresCliente ?? string.Empty),
                        new Claim(ClaimTypes.Email, nuevo.correoCliente ?? string.Empty),
                        new Claim(ClaimTypes.Role, "Cliente")
                    }, "ApplicationCookie");

                    auth.SignOut("ExternalCookie");
                    auth.SignIn(new AuthenticationProperties { IsPersistent = false }, appIdentity);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorRegistro = "Error al insertar en la base de datos: " + ex.Message;
                    return View("~/Views/Home/Login.cshtml");
                }
                TempData["Success"] = "¡Registro exitoso! Ya puedes ingresar.";
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorRegistro = "Error crítico: " + ex.Message;
                return View("~/Views/Home/Login.cshtml");
            }
        }

        // 🔥🔥🔥 GOOGLE LOGIN AGREGADO (NO MODIFICA NADA DE LO TUYO)

        public void ExternalLogin(string provider)
        {
            HttpContext.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("ExternalLoginCallback", "Account")
                },
                provider);
        }

        public ActionResult ExternalLoginCallback()
        {
            var auth = HttpContext.GetOwinContext().Authentication;

            // Authenticate the external cookie to obtain the external identity
            var authResult = auth.AuthenticateAsync("ExternalCookie").Result;

            if (authResult == null || authResult.Identity == null)
                return RedirectToAction("Login", "Home");

            var externalIdentity = (ClaimsIdentity)authResult.Identity;

            // Try to extract email and name from available claim types
            var email = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email" || c.Type == "urn:google:email")?.Value;
            var name = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name" || c.Type == "urn:google:name" || c.Type == "given_name" || c.Type == "urn:google:given_name")?.Value;

            // If we have an email, ensure there's a Cliente in MongoDB (no duplicates)
            if (!string.IsNullOrEmpty(email))
            {
                var existing = _clientes.Find(c => c.correoCliente == email).FirstOrDefault();
                if (existing == null)
                {
                    var nuevo = new Cliente
                    {
                        idCliente = Guid.NewGuid().ToString(),
                        correoCliente = email,
                        nombresCliente = string.IsNullOrEmpty(name) ? email : name
                    };

                    try
                    {
                        if (nuevo.InternalId == ObjectId.Empty)
                            nuevo.InternalId = ObjectId.GenerateNewId();

                        _clientes.InsertOne(nuevo);
                    }
                    catch (Exception ex)
                    {
                        // Surface DB insertion errors so it's easier to debug
                        TempData["ErrorDB"] = "No se pudo guardar el cliente en la base de datos: " + ex.Message;
                    }
                }

                // Set session as Cliente
                Session["Perfil"] = "Cliente";
                Session["Nombre"] = name ?? email;
            }

            // Create an application identity and sign in so the user is authenticated in the app
            var appIdentity = new ClaimsIdentity(externalIdentity.Claims, "ApplicationCookie");
            // Ensure the app identity includes useful claims
            if (!appIdentity.HasClaim(c => c.Type == ClaimTypes.Name))
                appIdentity.AddClaim(new Claim(ClaimTypes.Name, name ?? email));
            if (!appIdentity.HasClaim(c => c.Type == ClaimTypes.Email))
                appIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
            if (!appIdentity.HasClaim(c => c.Type == ClaimTypes.Role))
                appIdentity.AddClaim(new Claim(ClaimTypes.Role, "Cliente"));

            // Sign out external cookie and sign in with application cookie
            auth.SignOut("ExternalCookie");
            auth.SignIn(new AuthenticationProperties { IsPersistent = false }, appIdentity);

            // Redirect client to their inicio
            return RedirectToAction("InicioCliente", "Home");
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
    }
}