using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ConsumoModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = null; // Permite valores nulos para que o MongoDB gere automaticamente o Id

    [BsonElement("dataRegistro")]
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

    [BsonElement("consumo")]
    public int Consumo { get; set; }
}
