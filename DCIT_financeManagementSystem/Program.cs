using System;
using System.Collections.Generic;

// Record: positional parameters (Guid, DateTime, decimal, string)
public record Transaction(Guid Id, DateTime Date, decimal Amount, string Category);

// Interface for processors
public interface ITransactionProcessor
{
    void Process(Transaction transaction);
}

// Each processor must IMPLEMENT the interface and the method must be PUBLIC
public class BankTransferProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[BankTransfer] Amount: {transaction.Amount}, Category: {transaction.Category}");
    }
}

public class MobileMoneyProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[MoMo] Amount: {transaction.Amount}, Category: {transaction.Category}");
    }
}

public class CryptoWalletProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[Crypto] Amount: {transaction.Amount}, Category: {transaction.Category}");
    }
}

// Base account
public class Account
{
    public string AccountNumber { get; }
    public decimal Balance { get; protected set; }

    public Account(string accountNumber, decimal initialBalance)
    {
        AccountNumber = accountNumber;
        Balance = initialBalance;
    }

    public virtual void ApplyTransaction(Transaction transaction)
    {
        Balance -= transaction.Amount;
        Console.WriteLine($"[Account] New balance: {Balance}");
    }
}

// Sealed savings account with overdraft check
public sealed class SavingsAccount : Account
{
    public SavingsAccount(string accountNumber, decimal initialBalance)
        : base(accountNumber, initialBalance) { }

    public override void ApplyTransaction(Transaction transaction)
    {
        if (transaction.Amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
            return;
        }

        Balance -= transaction.Amount;
        Console.WriteLine($"[Savings] Deducted {transaction.Amount}. Updated balance: {Balance}");
    }
}

public class FinanceApp
{
    // field must be at class scope and needs a semicolon at the end
    private readonly List<Transaction> _transactions = new List<Transaction>();

    public void Run()
    {
        var account = new SavingsAccount("Cal121233", 1000m);

        // Correct order matches the record: (Guid, DateTime, decimal, string)
        var transaction1 = new Transaction(Guid.NewGuid(), DateTime.Now, 100m, "Groceries");
        var transaction2 = new Transaction(Guid.NewGuid(), DateTime.Now, 250m, "Utilities");
        var transaction3 = new Transaction(Guid.NewGuid(), DateTime.Now, 900m, "Entertainment");

        // Processors
        ITransactionProcessor momo = new MobileMoneyProcessor();
        ITransactionProcessor bank = new BankTransferProcessor();
        ITransactionProcessor crypto = new CryptoWalletProcessor();

        // Process + apply each transaction
        momo.Process(transaction1);
        account.ApplyTransaction(transaction1);

        bank.Process(transaction2);
        account.ApplyTransaction(transaction2);

        crypto.Process(transaction3);
        account.ApplyTransaction(transaction3); // may print "Insufficient funds" if balance too low

        // Track transactions
        _transactions.AddRange(new[] { transaction1, transaction2, transaction3 });

        Console.WriteLine($"[App] Stored transactions: {_transactions.Count}");
    }

    public static void Main(string[] args)
    {
        var app = new FinanceApp();
        app.Run(); // must CALL the method (parentheses)
    }
}




