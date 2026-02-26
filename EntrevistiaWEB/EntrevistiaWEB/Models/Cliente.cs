using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntrevistiaWEB.Models
{
    public class Cliente
    {
        [BsonId]
        public ObjectId InternalId { get; set; }

        public string idCliente { get; set; }
        public string nombresCliente { get; set; }
        public string contraseñaCliente { get; set; }
        public int edadCliente { get; set; }
        public string correoCliente { get; set; }
        public string telefonoCliente { get; set; }
    }
}