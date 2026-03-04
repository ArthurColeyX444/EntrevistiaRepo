using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EntrevistiaWEB.Models;
using MongoDB.Driver;

namespace EntrevistiaWEB.Controllers
{
    public class HomeController : Controller
    {
        // 1. Instanciamos AMBAS colecciones
        private readonly IMongoCollection<Cliente> _clientes;
        private readonly IMongoCollection<Administrador> _admins;

        public HomeController()
        {
            var client = new MongoClient("mongodb+srv://arthurcoley:634990@entrevistia.jstrsql.mongodb.net/?retryWrites=true&w=majority&appName=entrevistia");
            var database = client.GetDatabase("EntrevistIA");

            _clientes = database.GetCollection<Cliente>("Cliente");
            // Conectamos la colección de Administradores
            _admins = database.GetCollection<Administrador>("Administrador");
        }

        [HttpPost]
        
        public ActionResult EditarCliente(Cliente modificado, string nuevaContraseña)
        {
            try
            {
                // Seguridad
                if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin") return RedirectToAction("Login", "Home");

                var actualizacion = Builders<Cliente>.Update
                    .Set(c => c.nombresCliente, modificado.nombresCliente)
                    .Set(c => c.correoCliente, modificado.correoCliente)
                    .Set(c => c.telefonoCliente, modificado.telefonoCliente)
                    .Set(c => c.edadCliente, modificado.edadCliente);

                if (!string.IsNullOrEmpty(nuevaContraseña))
                {
                    actualizacion = actualizacion.Set(c => c.contraseñaCliente, nuevaContraseña);
                }

                _clientes.UpdateOne(c => c.idCliente == modificado.idCliente, actualizacion);

                return RedirectToAction("InicioAdmin");
            }
            catch (Exception)
            {
                // Si la base de datos falla, no cerramos la sesión, solo recargamos la página
                return RedirectToAction("InicioAdmin");
            }
        }

        [HttpPost]
        public ActionResult EliminarCliente(string idCliente)
        {
            try
            {
                // Seguridad
                if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin") return RedirectToAction("Login", "Home");

                // Validamos que el ID no esté vacío antes de enviarlo a MongoDB
                if (!string.IsNullOrEmpty(idCliente))
                {
                    _clientes.DeleteOne(c => c.idCliente == idCliente);
                }

                return RedirectToAction("InicioAdmin");
            }
            catch (Exception)
            {
                // Evita que un error de conexión a Mongo destruya tu sesión
                return RedirectToAction("InicioAdmin");
            }
        }
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        // Este es el método que carga la página la primera vez (GET)
        [HttpGet]
        public ActionResult Login()
        {
            // Limpiamos cualquier rastro de sesión anterior por seguridad
            Session.Clear();
            Session.Abandon();

            return View();
        }

        public ActionResult InicioCliente()
        {
            // Validación de seguridad básica: mandarlo al login
            if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Cliente")
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        public ActionResult PerfilCliente()
        {
            if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Cliente")
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        public ActionResult InicioAdmin()
        {
            // Validación de seguridad básica: mandarlo al login
            if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Home");
            }

            // OBTENEMOS TODOS LOS CLIENTES Y ADMINISTRADORES DE LA BASE DE DATOS
            List<Cliente> listaClientes = _clientes.Find(c => true).ToList();

            // Pasamos los administradores usando ViewBag para que la vista los pueda leer
            ViewBag.Admins = _admins.Find(a => true).ToList();

            // PASAMOS LA LISTA DE CLIENTES AL MODELO PRINCIPAL
            return View(listaClientes);
        }
    }
}