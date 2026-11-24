using FinanceMCP.DB.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace FinanceMCP.DB;

public class FinanceDatabase
{
    private readonly IMongoCollection<Transaction> _transactions;
    private readonly IMongoCollection<Account> _accounts;

    public FinanceDatabase(IMongoClient client)
    {
        var db = client.GetDatabase("finance");
        _transactions = db.GetCollection<Transaction>("transactions");
        _accounts = db.GetCollection<Account>("accounts");

        EnsureCollection(db, "transactions");
        EnsureCollection(db, "accounts");
    }

    // ========== PUBLIC API ==========

    public async Task<Transaction> AddTransactionAsync(
        string accountName,
        string currency,
        decimal amount,
        string type,
        string? description = null,
        DateTime? date = null)
    {
        var now = date ?? DateTime.UtcNow;

        var account = await _accounts.Find(a => a.Id == accountName).FirstOrDefaultAsync();
        if (account == null)
        {
            account = new Account
            {
                Id = accountName,
                Currency = currency,
                Balance = 0,
                UpdatedAt = now
            };
            await _accounts.InsertOneAsync(account);
        }
        else if (account.Currency != currency)
        {
            throw new InvalidOperationException(
                $"Account '{accountName}' uses currency '{account.Currency}', not '{currency}'.");
        }

        var tx = new Transaction
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Uid = Guid.NewGuid(),
            AccountName = accountName,
            Amount = amount,
            Type = type,
            Description = description ?? "",
            Date = now
        };
        await _transactions.InsertOneAsync(tx);

        var newBalance = account.Balance + amount;
        await _accounts.UpdateOneAsync(
            a => a.Id == accountName,
            Builders<Account>.Update
                .Set(a => a.Balance, newBalance)
                .Set(a => a.UpdatedAt, now)
        );

        return tx;   // returns the created transaction (includes Uid)
    }

    public async Task<List<Transaction>> GetTransactionsAsync(
        string accountName,
        DateTime? from = null,
        DateTime? to = null,
        string? type = null)
    {
        var filter = Builders<Transaction>.Filter.Eq(t => t.AccountName, accountName);

        if (from.HasValue)
            filter &= Builders<Transaction>.Filter.Gte(t => t.Date, from.Value);

        if (to.HasValue)
            filter &= Builders<Transaction>.Filter.Lte(t => t.Date, to.Value);

        if (!string.IsNullOrWhiteSpace(type))
            filter &= Builders<Transaction>.Filter.Eq(t => t.Type, type);

        return await _transactions
            .Find(filter)
            .SortByDescending(t => t.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Get a single transaction by its Uid.
    /// </summary>
    public async Task<Transaction?> GetTransactionByUidAsync(Guid uid)
    {
        return await _transactions.Find(t => t.Uid == uid).FirstOrDefaultAsync();
    }

    public async Task<decimal> GetBalanceAsync(string accountName)
    {
        var account = await _accounts.Find(a => a.Id == accountName).FirstOrDefaultAsync();
        return account?.Balance ?? 0;
    }

    public async Task<Account?> GetAccountAsync(string accountName)
    {
        return await _accounts.Find(a => a.Id == accountName).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Delete a single transaction by Uid and adjust the account balance.
    /// </summary>
    public async Task DeleteTransactionByUidAsync(Guid uid)
    {
        var tx = await _transactions.Find(t => t.Uid == uid).FirstOrDefaultAsync();
        if (tx == null) return;

        await _transactions.DeleteOneAsync(t => t.Uid == uid);

        // Reverse the amount on the account balance
        await _accounts.UpdateOneAsync(
            a => a.Id == tx.AccountName,
            Builders<Account>.Update
                .Inc(a => a.Balance, -tx.Amount)
                .Set(a => a.UpdatedAt, DateTime.UtcNow)
        );
    }

    /// <summary>
    /// Delete an account and all of its transactions.
    /// </summary>
    public async Task DeleteAccountAsync(string accountName)
    {
        await _transactions.DeleteManyAsync(t => t.AccountName == accountName);
        await _accounts.DeleteOneAsync(a => a.Id == accountName);
    }
    
    /// <summary>
    /// Adjust the balance of an account without creating a transaction.
    /// Positive increases the balance, negative decreases it.
    /// </summary>
    public async Task AdjustBalanceAsync(string accountName, decimal amount)
    {
        var account = await _accounts.Find(a => a.Id == accountName).FirstOrDefaultAsync();
        if (account == null)
            throw new InvalidOperationException($"Account '{accountName}' does not exist.");

        var newBalance = account.Balance + amount;
        await _accounts.UpdateOneAsync(
            a => a.Id == accountName,
            Builders<Account>.Update
                .Set(a => a.Balance, newBalance)
                .Set(a => a.UpdatedAt, DateTime.UtcNow)
        );
    }

    // ========== HELPERS ==========

    private static void EnsureCollection(IMongoDatabase db, string name)
    {
        var names = db.ListCollectionNames().ToList();
        if (!names.Contains(name))
            db.CreateCollection(name);
    }
}