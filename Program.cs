using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class MenuItem
{
    public string Name { get; }
    public decimal Price { get; }
    public bool IsShared { get; }

    public MenuItem(string name, decimal price, bool isShared)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative");

        Name = name;
        Price = price;
        IsShared = isShared;
    }
}

class Diner
{
    public string Name { get; }
    public List<MenuItem> PersonalItems { get; } = new List<MenuItem>();
    public decimal TipPercentage { get; }

    public Diner(string name, decimal tipPercentage)
    {
        Name = name;
        TipPercentage = tipPercentage;
    }

    public decimal CalculateTotal(decimal sharedItemsCost, decimal serviceChargePercentage, int totalDiners)
    {
        decimal personalItemsTotal = PersonalItems.Sum(item => item.Price);
        decimal sharedItemsAllocation = sharedItemsCost / totalDiners;

        decimal subtotal = personalItemsTotal + sharedItemsAllocation;
        decimal serviceCharge = subtotal * serviceChargePercentage / 100m;
        decimal tip = subtotal * TipPercentage / 100m;

        return subtotal + serviceCharge + tip;
    }
}

class BillSplitter
{
    public List<MenuItem> MenuItems { get; } = new List<MenuItem>();
    public List<Diner> Diners { get; } = new List<Diner>();
    public decimal ServiceChargePercentage { get; }

    public BillSplitter(decimal serviceChargePercentage)
    {
        ServiceChargePercentage = serviceChargePercentage;
    }

    public void AssignItemToDiner(string itemName, string dinerName)
    {
        var item = MenuItems.FirstOrDefault(i => i.Name == itemName);
        var diner = Diners.FirstOrDefault(d => d.Name == dinerName);

        if (item == null || diner == null)
            throw new ArgumentException("Item or diner not found");

        if (!item.IsShared)
            diner.PersonalItems.Add(item);
    }

    public Dictionary<string, decimal> CalculateTotals()
    {
        decimal sharedItemsTotal = MenuItems
            .Where(item => item.IsShared)
            .Sum(item => item.Price);

        var results = new Dictionary<string, decimal>();
        foreach (var diner in Diners)
        {
            results[diner.Name] = diner.CalculateTotal(
                sharedItemsTotal,
                ServiceChargePercentage,
                Diners.Count);
        }

        return results;
    }
}


namespace _8__Parcel_Shipping_Estimator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("BILL SPLITTER");
                Console.WriteLine("=============");

                // Get service charge
                Console.Write("Enter service charge percentage (e.g., 12.5): ");
                decimal serviceCharge = GetPositiveDecimal();

                var splitter = new BillSplitter(serviceCharge);

                // Add menu items
                Console.WriteLine("\nADD MENU ITEMS (leave name blank when done)");
                while (true)
                {
                    Console.Write("Item name: ");
                    string name = Console.ReadLine().Trim();
                    if (string.IsNullOrEmpty(name)) break;

                    Console.Write("Price: ");
                    decimal price = GetPositiveDecimal();

                    Console.Write("Is this a shared item? (y/n): ");
                    bool isShared = Console.ReadLine().Trim().ToLower() == "y";

                    splitter.MenuItems.Add(new MenuItem(name, price, isShared));
                }

                // Add diners
                Console.WriteLine("\nADD DINERS (leave name blank when done)");
                while (true)
                {
                    Console.Write("Diner name: ");
                    string name = Console.ReadLine().Trim();
                    if (string.IsNullOrEmpty(name)) break;

                    Console.Write("Tip percentage (e.g., 15): ");
                    decimal tip = GetPositiveDecimal();

                    splitter.Diners.Add(new Diner(name, tip));
                }

                // Assign items
                Console.WriteLine("\nASSIGN PERSONAL ITEMS");
                foreach (var item in splitter.MenuItems.Where(i => !i.IsShared))
                {
                    Console.WriteLine($"\nAssigning: {item.Name} (${item.Price})");

                    var validDiners = splitter.Diners
                        .Select(d => d.Name)
                        .ToList();

                    Console.WriteLine("Available diners: " + string.Join(", ", validDiners));

                    while (true)
                    {
                        Console.Write("Enter diner name: ");
                        string dinerName = Console.ReadLine().Trim();

                        try
                        {
                            splitter.AssignItemToDiner(item.Name, dinerName);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}. Try again.");
                        }
                    }
                }

                // Calculate and display results
                Console.WriteLine("\nBILL SUMMARY");
                Console.WriteLine("============");

                Console.WriteLine($"\nService Charge: {serviceCharge}%");
                Console.WriteLine("\nDiners:");
                foreach (var diner in splitter.Diners)
                {
                    Console.WriteLine($"- {diner.Name} (Tip: {diner.TipPercentage}%)");
                    if (diner.PersonalItems.Any())
                    {
                        Console.WriteLine("  Personal items:");
                        foreach (var item in diner.PersonalItems)
                            Console.WriteLine($"    {item.Name}: ${item.Price}");
                    }
                }

                var sharedItems = splitter.MenuItems.Where(i => i.IsShared).ToList();
                if (sharedItems.Any())
                {
                    Console.WriteLine("\nShared items (split equally):");
                    foreach (var item in sharedItems)
                        Console.WriteLine($"- {item.Name}: ${item.Price}");
                }

                Console.WriteLine("\nTOTALS");
                var totals = splitter.CalculateTotals();
                foreach (var kvp in totals.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"{kvp.Key}: ${kvp.Value:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static decimal GetPositiveDecimal()
        {
            while (true)
            {
                if (decimal.TryParse(Console.ReadLine(), out decimal value) && value >= 0)
                    return value;
                Console.Write("Invalid input. Please enter a positive number: ");
            }
        }
    }
}




