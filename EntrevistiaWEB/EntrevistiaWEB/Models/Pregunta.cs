using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EntrevistiaWEB.Models
{
    public class Pregunta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdInternoMongo { get; set; }

        
        [BsonElement("idPregunta")]
        public string idPregunta { get; set; }

        
        [BsonElement("textoPregunta")]
        public string textoPregunta { get; set; }

        [BsonElement("categoria")]
        public string categoria { get; set; } 

        [BsonElement("tipo")]
        public string tipo { get; set; } 

        [BsonElement("tiempoEstimado")]
        public int tiempoEstimado { get; set; } 

        
        [BsonElement("criteriosEvaluacion")]
        public string criteriosEvaluacion { get; set; }
    }
}