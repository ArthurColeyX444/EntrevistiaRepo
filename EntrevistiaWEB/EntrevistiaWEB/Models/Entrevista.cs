using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntrevistiaWEB.Models
{
    // ESTA LÍNEA ES LA MAGIA: Le dice a C# "Si ves un campo en la BD que no está aquí, ignóralo, no crashees"
    [BsonIgnoreExtraElements]
    public class Entrevista
    {
        // 1. Este es el identificador real interno de MongoDB (_id)
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // 2. Este es el campo idEntrevista que tú creaste manualmente en la base de datos
        public string idEntrevista { get; set; }

        public string tituloEntrevista { get; set; }
        public string descripcion { get; set; }
        public string categoria { get; set; }
        public string tipo { get; set; }
        public string nivel { get; set; }
        public string modalidad { get; set; }
        public int duracion { get; set; }
    }
}