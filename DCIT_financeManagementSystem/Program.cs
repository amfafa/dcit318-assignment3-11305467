public record Transaction
{
    int Id;
    DateTime Date;
    decimal Amount;
    string Category;
}
public interface ITransactionProcessor
{
    public void Process(Transaction transaction);
}

public class BankTransferProcessor
{
    void Process(Transaction transaction)
    {
        Console.WriteLine("Your Bank Transfer Amount  {Amount}  and Category is {Category}");
    }
}

public class MobileMoneyProcessor
{
    void Process(Transaction transaction)
    {
        Console.WriteLine("Your Mobile money amount  {Amount}  and Category is {Category}");
    }
}
public class CryptoWalletProcessor
{

}
