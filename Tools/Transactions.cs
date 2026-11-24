using ModelContextProtocol.Server;
using System.ComponentModel;
using FinanceMCP.DB;
using FinanceMCP.DB.Models;

namespace FinanceMCP.Tools;

[McpServerToolType]
internal class Transactions
{
    private readonly FinanceDatabase _db;

    public Transactions(FinanceDatabase db)
    {
        _db = db;
    }

    [McpServerTool, Description("Add a transaction to an account. Positive amount = income, negative = expense. Creates the account if it does not exist. Returns the created transaction including its Uid.")]
    public Task<Transaction> AddTransaction(
        [Description("Account name (e.g. Revolut, Wise, Crypto)")] string accountName,
        [Description("Currency code (e.g. EUR, USD, BTC)")] string currency,
        [Description("Amount: positive for income, negative for expense")] decimal amount,
        [Description("Transaction type (e.g. groceries, salary, transfer, buy)")] string type,
        [Description("Optional description")] string? description = null,
        [Description("Optional date (UTC). Defaults to now.")] DateTime? date = null)
    {
        return _db.AddTransactionAsync(accountName, currency, amount, type, description, date);
    }

    [McpServerTool, Description("Get transactions for an account. Optional filters: from date, to date, and type.")]
    public Task<List<Transaction>> GetTransactions(
        [Description("Account name")] string accountName,
        [Description("Optional start date (inclusive)")] DateTime? from = null,
        [Description("Optional end date (inclusive)")] DateTime? to = null,
        [Description("Optional transaction type filter")] string? type = null)
    {
        return _db.GetTransactionsAsync(accountName, from, to, type);
    }

    [McpServerTool, Description("Get a single transaction by its unique Uid.")]
    public Task<Transaction?> GetTransactionByUid(
        [Description("The unique Guid of the transaction")] Guid uid)
    {
        return _db.GetTransactionByUidAsync(uid);
    }

    [McpServerTool, Description("Get the current balance of an account.")]
    public Task<decimal> GetBalance(
        [Description("Account name")] string accountName)
    {
        return _db.GetBalanceAsync(accountName);
    }

    [McpServerTool, Description("Get full account details including currency and balance.")]
    public Task<Account?> GetAccount(
        [Description("Account name")] string accountName)
    {
        return _db.GetAccountAsync(accountName);
    }

    [McpServerTool, Description("Delete a single transaction by its Uid and adjust the account balance.")]
    public Task DeleteTransactionByUid(
        [Description("The unique Guid of the transaction to delete")] Guid uid)
    {
        return _db.DeleteTransactionByUidAsync(uid);
    }

    [McpServerTool, Description("Delete an account and all of its transactions.")]
    public Task DeleteAccount(
        [Description("Account name to delete")] string accountName)
    {
        return _db.DeleteAccountAsync(accountName);
    }
    
    [McpServerTool, Description("Adjust the balance of an account without creating a transaction. Positive increases the balance, negative decreases it.")]
    public Task AdjustBalance(
        [Description("Account name")] string accountName,
        [Description("Amount to add (positive) or subtract (negative)")] decimal amount)
    {
        return _db.AdjustBalanceAsync(accountName, amount);
    }
}