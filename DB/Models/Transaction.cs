using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanceMCP.DB.Models;

public class Transaction
{
    [BsonId]
    public string Id { get; set; } = "";

    public string AccountName { get; set; } = "";
    public decimal Amount { get; set; }           // + income, - expense
    public string Type { get; set; } = "";        // e.g. "groceries", "salary", "transfer"
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Uid { get; set; } = Guid.NewGuid();  // unique identifier

}