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
        private readonly IMongoCollection<Entrevista> _entrevistas;
        private readonly IMongoCollection<Pregunta> _preguntas;
        public HomeController()
        {
            var client = new MongoClient("mongodb+srv://arthurcoley:634990@entrevistia.jstrsql.mongodb.net/?retryWrites=true&w=majority&appName=entrevistia");
            var database = client.GetDatabase("EntrevistIA");

            _clientes = database.GetCollection<Cliente>("Cliente");
            
            _admins = database.GetCollection<Administrador>("Administrador");
            _entrevistas = database.GetCollection<Entrevista>("Entrevista");
            _preguntas = database.GetCollection<Pregunta>("Preguntas");
        }

        [HttpPost]
        public ActionResult EditarCliente(Cliente modificado, string nuevaContraseña)
        {
            try
            {
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

                // TRUCO: Le decimos a la vista que active la pestaña clientes
                TempData["ActiveTab"] = "clientes";

                return RedirectToAction("InicioAdmin");
            }
            catch (Exception) { return RedirectToAction("InicioAdmin"); }
        }

        [HttpPost]
        public ActionResult CrearEntrevista(Entrevista nuevaEntrevista, List<string> idPreguntasSeleccionadas)
        {
            try
            {
                // Seguridad
                if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin") return RedirectToAction("Login", "Home");

                // Generamos ID único para la entrevista
                if (string.IsNullOrEmpty(nuevaEntrevista.idEntrevista))
                {
                    nuevaEntrevista.idEntrevista = "ENT_" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                }

                // Asignamos la lista de IDs de las preguntas que el administrador seleccionó (los checkboxes)
                if (idPreguntasSeleccionadas != null)
                {
                    nuevaEntrevista.idPreguntas = idPreguntasSeleccionadas;
                }
                else
                {
                    nuevaEntrevista.idPreguntas = new List<string>(); // Evitamos que sea nulo si no seleccionó ninguna
                }

                // Guardamos en la Base de Datos
                _entrevistas.InsertOne(nuevaEntrevista);

                // Forzamos a que se abra la pestaña de entrevistas al recargar
                TempData["ActiveTab"] = "entrevistas";

                return RedirectToAction("InicioAdmin");
            }
            catch (Exception ex)
            {
                TempData["ActiveTab"] = "entrevistas";
                return RedirectToAction("InicioAdmin");
            }
        }

        [HttpPost]
        public ActionResult EliminarCliente(string idCliente)
        {
            try
            {
                if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin") return RedirectToAction("Login", "Home");

                if (!string.IsNullOrEmpty(idCliente))
                {
                    _clientes.DeleteOne(c => c.idCliente == idCliente);
                }

                // TRUCO: Le decimos a la vista que active la pestaña clientes
                TempData["ActiveTab"] = "clientes";

                return RedirectToAction("InicioAdmin");
            }
            catch (Exception) { return RedirectToAction("InicioAdmin"); }
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
            if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Cliente")
            {
                return RedirectToAction("Login", "Home");
            }

            // Traemos las entrevistas
            List<Entrevista> entrevistasDisponibles = _entrevistas.Find(e => true).ToList();

            // Si por alguna razón la BD devuelve null, creamos una lista vacía para que no de error
            if (entrevistasDisponibles == null)
            {
                entrevistasDisponibles = new List<Entrevista>();
            }

            // Pasamos la lista a la vista
            return View(entrevistasDisponibles);
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

            ViewBag.Entrevistas = _entrevistas.Find(e => true).ToList();

            ViewBag.Preguntas = _preguntas.Find(p => true).ToList();

            // PASAMOS LA LISTA DE CLIENTES AL MODELO PRINCIPAL
            return View(listaClientes);
        }

        [HttpPost]
        public ActionResult CrearPregunta(Pregunta nuevaPregunta)
        {
            try
            {
                // Seguridad: Validamos que sea un Admin
                if (Session["Perfil"] == null || Session["Perfil"].ToString() != "Admin") return RedirectToAction("Login", "Home");

                // Le generamos un código único a la pregunta (Ej: PRG_8A4B92)
                if (string.IsNullOrEmpty(nuevaPregunta.idPregunta))
                {
                    nuevaPregunta.idPregunta = "PRG_" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                }

                // Guardamos la pregunta en MongoDB
                _preguntas.InsertOne(nuevaPregunta);

                // TRUCO: Le decimos a la página que al recargar, abra directamente la pestaña de Preguntas
                TempData["ActiveTab"] = "preguntas";

                return RedirectToAction("InicioAdmin");
            }
            catch (Exception ex)
            {
                TempData["ActiveTab"] = "preguntas";
                return RedirectToAction("InicioAdmin");
            }
        }

        [HttpPost]
        public JsonResult CrearPreguntaAjax(Pregunta nuevaPregunta)
        {
            try
            {
                // Generamos ID
                if (string.IsNullOrEmpty(nuevaPregunta.idPregunta))
                {
                    nuevaPregunta.idPregunta = "PRG_" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                }

                // Guardamos en Mongo
                _preguntas.InsertOne(nuevaPregunta);

                // Devolvemos un JSON con los datos para que JavaScript los pinte en la pantalla de inmediato
                return Json(new
                {
                    success = true,
                    idPregunta = nuevaPregunta.idPregunta,
                    texto = nuevaPregunta.textoPregunta,
                    categoria = nuevaPregunta.categoria
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}