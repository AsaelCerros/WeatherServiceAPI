using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherService.Models
{
    public class WeatherData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("temperature")]
        public double Temperature { get; set; }

        [BsonElement("windDirection")]
        public double WindDirection { get; set; }

        [BsonElement("windSpeed")]
        public double WindSpeed { get; set; }

        [BsonElement("sunrise")]
        public DateTime Sunrise { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}