using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// Composition root: єдине місце, де створюються конкретні класи.
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());
var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
if (useFile) Console.WriteLine($"Файл: {dataPath}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 1: додати, змінити, знайти ===");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
Console.WriteLine($"Додано: {created}");
service.Receive(created.Id, 25);
service.Issue(created.Id, 15);
Console.WriteLine($"Після приходу 25 і видачі 15: {service.Find(created.Id)}");
Console.WriteLine($"Пошук за неіснуючим id: {(service.Find("nope") is null ? "null" : "знайдено")}");

Console.WriteLine();
Console.WriteLine("=== Список ===");
foreach (var p in service.All())
    Console.WriteLine($" {p.Id,-9} {p.Sku,-8} {p.Name,-32} {p.Quantity,5} {p.Unit}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: відмови ===");
TryDo("дубль id", () => store.Add(Product.Create(created.Id, "SKU-102", "Дублікат", "шт", 1)));
TryDo("прихід на неіснуючий id", () => service.Receive("no-such-id", 10));
TryDo("видача більша за залишок", () => service.Issue(created.Id, 1000));
TryDo("від'ємний прихід", () => service.Receive(created.Id, -5));
Console.WriteLine($"Залишок {created.Sku} після відмов: {service.Find(created.Id)!.Quantity} {created.Unit}");

Console.WriteLine();
Console.WriteLine("=== Додатково 2: пошук з Func<Product, bool> ===");
PrintSearch("залишок < 50", service.Search(p => p.Quantity < 50));
PrintSearch("одиниця «м» і залишок > 500", service.Search(p => p.Unit == "м" && p.Quantity > 500));
PrintSearch("назва містить «кабель»", service.Search(p => p.Name.Contains("кабель", StringComparison.OrdinalIgnoreCase)));
PrintSearch("залишок > 1_000_000", service.Search(p => p.Quantity > 1_000_000));

Console.WriteLine();
Console.WriteLine("=== Додатково 1 і 3: фабрика + декоратор кешу (--cache) ===");
var cached = StoreFactory.Create(["--cache"]);
Console.WriteLine($"Фабрика створила: {cached.GetType().Name}");
var cachedService = new CatalogService(cached);
for (int i = 0; i < 3; i++) cachedService.All();
for (int i = 0; i < 3; i++) cachedService.Find("P-001");
var counters = (CachingCatalogStore)cached;
Console.WriteLine($"3×All + 3×Find: звернень до внутрішнього сховища {counters.InnerReads}, з кешу {counters.CacheHits}");
cachedService.Receive("P-001", 10);
Console.WriteLine($"Після Receive кеш скинуто, читаємо знову: {cachedService.Find("P-001")}");
Console.WriteLine($"Звернень до внутрішнього сховища {counters.InnerReads}, з кешу {counters.CacheHits}");

static void PrintSearch(string title, IReadOnlyList<Product> found)
{
    Console.WriteLine($" {title}: знайдено {found.Count}");
    foreach (var p in found.Take(4)) Console.WriteLine($"   {p}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} — {ex.Message.Split('\n')[0]}");
    }
}
