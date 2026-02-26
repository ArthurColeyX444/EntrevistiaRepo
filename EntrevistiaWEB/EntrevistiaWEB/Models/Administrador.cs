using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntrevistiaWEB.Models
{
    public class Administrador
    {
        [BsonId]
        public ObjectId InternalId { get; set; }

        public string idAdmin { get; set; }
        public string nombreAdmin { get; set; }
        public string correoAdmin { get; set; }

        public string contraseñaAdmin { get; set; }
    }
}