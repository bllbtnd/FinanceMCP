using MongoDB.Bson.Serialization.Attributes;

namespace FinanceMCP.DB.Models;

public class Account
{
    [BsonId]
    public string Id { get; set; } = "";          // account name (Revolut, Wise, Crypto, etc.)
    public string Currency { get; set; } = "";
    public decimal Balance { get; set; }
    public DateTime UpdatedAt { get; set; }
}