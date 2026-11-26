using System.ComponentModel;
using FinanceMCP.DB;
using FinanceMCP.DB.Models;
using ModelContextProtocol.Server;

namespace FinanceMCP.Tools;

internal class Accounts
{
    private readonly FinanceDatabase _db;

    public Accounts(FinanceDatabase db)
    {
        _db = db;
    }
    [McpServerTool, Description("Get full account details including currency and balance.")]
    public Task<Account?> GetAccount(
        [Description("Account name")] string accountName)
    {
        return _db.GetAccountAsync(accountName);
    }

    [McpServerTool, Description("Delete an account and all of its transactions.")]
    public Task DeleteAccount(
        [Description("Account name to delete")] string accountName)
    {
        return _db.DeleteAccountAsync(accountName);
    }
    
    [McpServerTool, Description("List all accounts with full details (name, currency, balance, updatedAt).")]
    public Task<List<Account>> GetAllAccounts()
    {
        return _db.GetAllAccountsAsync();
    }
}