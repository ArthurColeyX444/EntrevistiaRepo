using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EntrevistiaWEB.Models; // Para que reconozca la clase Cliente
using MongoDB.Driver;      // Para que reconozca MongoClient

namespace EntrevistiaWEB.Controllers
{
    public class HomeController : Controller
    {
        // 1. Instanciamos la colección de la base de datos
        private readonly IMongoCollection<Cliente> _clientes;

        public HomeController()
        {
            // 2. Configuramos la conexión igual que en tu AccountController
            var client = new MongoClient("mongodb+srv://arthurcoley:634990@entrevistia.jstrsql.mongodb.net/?retryWrites=true&w=majority&appName=entrevistia");
            var database = client.GetDatabase("EntrevistIA");

            _clientes = database.GetCollection<Cliente>("Cliente");
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
            // CORREGIDO: Cambié la redirección a "Login". 
            // Antes decía "PerfilCliente" y eso causa un bucle infinito en el navegador.
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

            // 3. OBTENEMOS TODOS LOS CLIENTES DE LA BASE DE DATOS
            List<Cliente> listaClientes = _clientes.Find(c => true).ToList();

            // 4. PASAMOS LA LISTA A LA VISTA
            return View(listaClientes);
        }
    }
}