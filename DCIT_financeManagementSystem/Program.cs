using System;
using System.Collections.Generic;

namespace Transaction
{
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
}


namespace Repository
{


    public class Repository<T>
    {
        // Field
        private readonly List<T> items = new List<T>();

        // Methods
        public void Add(T item) => items.Add(item);

        public List<T> GetAll() => new List<T>(items);

        // Return first match by predicate or null (default)
        public T? GetById(Func<T, bool> predicate)
        {
            foreach (var it in items)
            {
                if (predicate(it)) return it;
            }
            return default;
        }

        // Remove by condition
        public bool Remove(Func<T, bool> predicate)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (predicate(items[i]))
                {
                    items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }


    public class Patient
    {
        // Fields
        public int Id;
        public string Name;
        public int Age;
        public string Gender;

        // Constructor
        public Patient(int id, string name, int age, string gender)
        {
            Id = id;
            Name = name;
            Age = age;
            Gender = gender;
        }

        public override string ToString() => $"{Id}: {Name} ({Gender}), Age {Age}";
    }

    public class Prescription
    {
        // Fields
        public int Id;
        public int PatientId;
        public string MedicationName;
        public DateTime DateIssued;

        // Constructor
        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id;
            PatientId = patientId;
            MedicationName = medicationName;
            DateIssued = dateIssued;
        }

        public override string ToString() => $"{Id}: {MedicationName} (Issued: {DateIssued:d})";
    }


    public class HealthSystemApp
    {
        // Fields
        private readonly Repository<Patient> _patientRepo = new Repository<Patient>();
        private readonly Repository<Prescription> _prescriptionRepo = new Repository<Prescription>();
        private Dictionary<int, List<Prescription>> _prescriptionMap = new Dictionary<int, List<Prescription>>();

        // Seed 2–3 patients and 4–5 prescriptions
        public void SeedData()
        {
            // Patients
            _patientRepo.Add(new Patient(1, "Ama Mensah", 23, "F"));
            _patientRepo.Add(new Patient(2, "Kwame Yeboah", 31, "M"));
            _patientRepo.Add(new Patient(3, "Efua Sarpong", 28, "F"));

            // Prescriptions (valid PatientIds: 1, 2, 3)
            _prescriptionRepo.Add(new Prescription(1, 1, "Amoxicillin", DateTime.Today.AddDays(-2)));
            _prescriptionRepo.Add(new Prescription(2, 1, "Ibuprofen", DateTime.Today));
            _prescriptionRepo.Add(new Prescription(3, 2, "Paracetamol", DateTime.Today.AddDays(-1)));
            _prescriptionRepo.Add(new Prescription(4, 2, "Vitamin C", DateTime.Today));
            _prescriptionRepo.Add(new Prescription(5, 3, "Cetirizine", DateTime.Today.AddDays(-3)));
        }

        // (d, e) Build Dictionary<int, List<Prescription>> grouped by PatientId
        public void BuildPrescriptionMap()
        {
            _prescriptionMap = new Dictionary<int, List<Prescription>>();

            foreach (var p in _prescriptionRepo.GetAll())
            {
                if (!_prescriptionMap.TryGetValue(p.PatientId, out var list))
                {
                    list = new List<Prescription>();
                    _prescriptionMap[p.PatientId] = list;
                }
                list.Add(p);
            }
        }

        // (f) Retrieve by PatientId from the dictionary
        public List<Prescription> GetPrescriptionsByPatientId(int patientId)
        {
            return _prescriptionMap.TryGetValue(patientId, out var list)
                ? new List<Prescription>(list)
                : new List<Prescription>();
        }

        // Print all patients
        public void PrintAllPatients()
        {
            Console.WriteLine("=== Patients ===");
            foreach (var p in _patientRepo.GetAll())
            {
                Console.WriteLine(p);
            }
        }

        // Print all prescriptions for a given patient
        public void PrintPrescriptionsForPatient(int id)
        {
            Console.WriteLine($"\n=== Prescriptions for Patient ID {id} ===");
            var list = GetPrescriptionsByPatientId(id);
            if (list.Count == 0)
            {
                Console.WriteLine("No prescriptions found.");
                return;
            }
            foreach (var rx in list)
            {
                Console.WriteLine(rx);
            }
        }
    }


    public class Program
    {
        public static void Main(string[] args)
        {
            // i. Instantiate
            var app = new HealthSystemApp();

            // ii. Seed
            app.SeedData();

            // iii. Build map
            app.BuildPrescriptionMap();

            // iv. Print all patients
            app.PrintAllPatients();

            // v. Select one PatientId and display prescriptions (choose any valid Id)
            app.PrintPrescriptionsForPatient(1);
        }
    }



}
namespace InventoryItem
{


    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }


    public class ElectronicItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public string Brand { get; }
        public int WarrantyMonths { get; }

        public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            Brand = brand;
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString() => $"{Id} | {Name} x{Quantity} | {Brand} | {WarrantyMonths} mo warranty";
    }


    public class GroceryItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            ExpiryDate = expiryDate;
        }

        public override string ToString() => $"{Id} | {Name} x{Quantity} | Exp: {ExpiryDate:yyyy-MM-dd}";
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // (e) Custom exceptions
    // ─────────────────────────────────────────────────────────────────────────────
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message) { }
    }
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }
    public class InvalidQuantityException : Exception
    {
        public InvalidQuantityException(string message) : base(message) { }
    }


    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new(); // key = item.Id

        public void AddItem(T item)
        {
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"Item with ID {item.Id} already exists.");
            _items[item.Id] = item;
        }

        public T GetItemById(int id)
        {
            if (!_items.TryGetValue(id, out var item))
                throw new ItemNotFoundException($"Item with ID {id} not found.");
            return item;
        }

        public void RemoveItem(int id)
        {
            if (!_items.Remove(id))
                throw new ItemNotFoundException($"Item with ID {id} not found.");
        }

        public List<T> GetAllItems() => new(_items.Values);

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative.");
            var item = GetItemById(id); // will throw ItemNotFoundException if missing
            item.Quantity = newQuantity;
        }
    }


    public class WareHouseManager
    {
        private readonly InventoryRepository<ElectronicItem> _electronics = new();
        private readonly InventoryRepository<GroceryItem> _groceries = new();

        // Seed 2–3 items of each type
        public void SeedData()
        {
            _electronics.AddItem(new ElectronicItem(1, "LED TV 43\"", 5, "Hisense", 24));
            _electronics.AddItem(new ElectronicItem(2, "Smartphone", 10, "Tecno", 12));
            _electronics.AddItem(new ElectronicItem(3, "Router", 8, "TP-Link", 18));

            _groceries.AddItem(new GroceryItem(1, "Rice (5kg)", 50, DateTime.Today.AddMonths(6)));
            _groceries.AddItem(new GroceryItem(2, "Milk (1L)", 20, DateTime.Today.AddMonths(2)));
            _groceries.AddItem(new GroceryItem(3, "Sugar (1kg)", 40, DateTime.Today.AddMonths(12)));
        }

        // Print all from any repo
        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            foreach (var item in repo.GetAllItems())
                Console.WriteLine(item);
        }

        // Increase stock with try/catch
        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            try
            {
                var current = repo.GetItemById(id); // may throw ItemNotFoundException
                repo.UpdateQuantity(id, current.Quantity + quantity); // may throw InvalidQuantityException
                Console.WriteLine($"[Stock] {current.Name} new qty: {current.Quantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IncreaseStock] {ex.Message}");
            }
        }

        // Remove item with try/catch
        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            try
            {
                repo.RemoveItem(id); // may throw ItemNotFoundException
                Console.WriteLine($"[Remove] Removed item with ID {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Remove] {ex.Message}");
            }
        }

        // Convenience methods for demo clarity
        public void PrintGroceries() { Console.WriteLine("\n=== Groceries ==="); PrintAllItems(_groceries); }
        public void PrintElectronics() { Console.WriteLine("\n=== Electronics ==="); PrintAllItems(_electronics); }

        // Quick accessors (optional)
        public InventoryRepository<ElectronicItem> ElectronicsRepo => _electronics;
        public InventoryRepository<GroceryItem> GroceriesRepo => _groceries;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var manager = new WareHouseManager();

            // i. Seed
            manager.SeedData();

            // ii. Print all grocery items
            manager.PrintGroceries();

            // iii. Print all electronic items
            manager.PrintElectronics();

            // iv. Try exception scenarios
            Console.WriteLine("\n=== Exception Scenarios ===");

            // Add a duplicate item (duplicate ID 1 in groceries)
            try
            {
                manager.GroceriesRepo.AddItem(new GroceryItem(1, "Oil (1L)", 15, DateTime.Today.AddMonths(8)));
            }
            catch (DuplicateItemException ex)
            {
                Console.WriteLine($"[Duplicate] {ex.Message}");
            }

            // Remove a non-existent item
            manager.RemoveItemById(manager.ElectronicsRepo, id: 999);

            // Update with invalid quantity
            try
            {
                manager.ElectronicsRepo.UpdateQuantity(id: 1, newQuantity: -10);
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine($"[InvalidQty] {ex.Message}");
            }
            catch (ItemNotFoundException ex)
            {
                Console.WriteLine($"[InvalidQty-NotFound] {ex.Message}");
            }

            // Bonus: increase stock safely (happy path)
            manager.IncreaseStock(manager.GroceriesRepo, id: 2, quantity: 5);
        }
    }

}


