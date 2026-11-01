using Cli;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Composition root: конкретні класи знає лише Cli.
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());
var service = new CatalogService(store);
Console.WriteLine($"Сховище: {store.GetType().Name}");

// ---------- Журнал: шлях будується тут, Core про файл нічого не знає ----------
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
File.WriteAllText(logPath, string.Empty);   // кожен запуск — чистий журнал

// Крок 3: підписник — локальний метод (на відміну від лямбди, його можна зняти через -=)
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = LogFormat.Line(e);
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

// Крок 4: другий підписник — лямбда із замиканням над локальною змінною
int changes = 0;
service.Changed += OnCatalogChanged;      // 1-й за порядком підписки
service.Changed += (_, _) => changes++;   // 2-й

// ---------- Сценарій 1: події (кроки 5 і 6) ----------
Console.WriteLine("\n=== Сценарій 1: додати, змінити, порахувати події ===");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");

// Крок 6: відмови, поки підписка ще активна — події не має бути
Console.WriteLine("\n=== Сценарій 2: відмови (виняток є, події немає) ===");
TryDo("прихід -5", () => service.Receive(created.Id, -5));
TryDo("прихід на неіснуючий id", () => service.Receive("no-such-id", 5));
TryDo("видача більша за залишок", () => service.Issue(created.Id, 1000));
Console.WriteLine($"Подій отримано: {changes} (без змін), залишок: {service.Find(created.Id)!.Quantity}");

// Крок 5: відписка — операція виконується, повідомлення немає
Console.WriteLine("\n=== Після відписки -= OnCatalogChanged ===");
service.Changed -= OnCatalogChanged;
service.Receive(created.Id, 5);   // тиха операція: підписка керована
Console.WriteLine($"Подій отримано: {changes}");   // +1 лише від лямбди-лічильника
Console.WriteLine($"Залишок змінився: {service.Find(created.Id)!.Quantity} {created.Unit}");

// Крок 7: Func замість події — потрібне значення у відповідь
Console.WriteLine("\n=== Func: FindAll(p => p.Quantity < 10) ===");
foreach (var p in service.FindAll(p => p.Quantity < 10))
    Console.WriteLine($"{p.Id} [{p.Sku}] {p.Name} — {p.Quantity} {p.Unit}");
int threshold = 20;   // замикання і тут: лямбда бачить локальну змінну
Console.WriteLine($"Записів із залишком < {threshold}: {service.FindAll(p => p.Quantity < threshold).Count}");

// ---------- Сценарій 3: порядок підписників, Issued і Removed ----------
Console.WriteLine("\n=== Сценарій 3: порядок підписників, Issued, Removed ===");
var service3 = new CatalogService(new InMemoryCatalogStore());
service3.Changed += (_, e) => Console.WriteLine($"  підписник A: {e.Kind,-8} {e.Delta,4:+0;-0} → {e.QuantityAfter}");
service3.Changed += (_, e) => Console.WriteLine($"  підписник B: {e.Kind,-8} {e.Delta,4:+0;-0} → {e.QuantityAfter}");
var tmp = service3.Add("SKU-300", "Лампа LED", "шт", 40);
service3.Issue(tmp.Id, 15);
service3.Remove(tmp.Id);

// ---------- Сценарій 4: виняток у підписнику ----------
Console.WriteLine("\n=== Сценарій 4: виняток в одному з підписників ===");
var service4 = new CatalogService(new InMemoryCatalogStore());
service4.Changed += (_, _) => throw new InvalidOperationException("збій першого підписника");
service4.Changed += (_, _) => Console.WriteLine("  другий підписник викликано");
try { service4.Add("SKU-400", "Скотч", "шт", 10); }
catch (Exception ex) { Console.WriteLine($"  {ex.GetType().Name} — {ex.Message}"); }
Console.WriteLine($"  Запис у сховищі є: {service4.All().Count == 1} (зміна стану відбулася, подія обірвалася)");

// ---------- Сценарій 5: відписка новою лямбдою не працює ----------
Console.WriteLine("\n=== Сценарій 5: -= новою лямбдою не знімає підписку ===");
var service6 = new CatalogService(new InMemoryCatalogStore());
int c6 = 0;
service6.Changed += (_, _) => c6++;
service6.Changed -= (_, _) => c6++;   // інший екземпляр делегата — нічого не знімає
service6.Add("SKU-701", "Ізолента", "шт", 5);
Console.WriteLine($"  Після -= новою лямбдою: {c6} (підписка лишилась)");
EventHandler<CatalogChangedEventArgs> counter = (_, _) => c6++;
service6.Changed += counter;
service6.Changed -= counter;          // той самий екземпляр, збережений у змінній
c6 = 0;
service6.Add("SKU-702", "Стрічка", "шт", 5);
Console.WriteLine($"  Після -= збереженим делегатом: {c6} (знято лише counter; перша лямбда ще працює)");

// ---------- Додаткове 2: Action<string> ----------
Console.WriteLine("\n=== Додатково 2: Import з Action<string> ===");
var service5 = new CatalogService(new InMemoryCatalogStore());
int imported5 = 0;
service5.Changed += (_, _) => imported5++;
string[] csv =
[
    "SKU-501;Болт М8;шт;200",
    "SKU-502;Гайка М8;шт",
    "SKU-503;Шайба;шт;багато",
    "SKU-504;Дюбель;шт;-5",
    "SKU-505;Шуруп;шт;80",
];
var skipped = new List<string>();
int ok = service5.Import(csv, msg => { skipped.Add(msg); Console.WriteLine($"  пропущено: {msg}"); });
Console.WriteLine($"  Імпортовано: {ok}, пропущено: {skipped.Count}, подій: {imported5}");

// ---------- Додаткове 1: INotificationSink замість події ----------
Console.WriteLine("\n=== Додатково 1: INotificationSink через конструктор ===");
var sinkLog = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
File.WriteAllText(sinkLog, string.Empty);
var sinkService = new SinkCatalogService(new InMemoryCatalogStore(),
    new CompositeSink(new ConsoleSink(), new FileSink(sinkLog)));
var s = sinkService.Add("SKU-601", "Шнур", "м", 30);
sinkService.Receive(s.Id, 10);
Console.WriteLine($"  Рядків у sink.log: {File.ReadAllLines(sinkLog).Length}");

Console.WriteLine($"\nЖурнал: {logPath}");

static void TryDo(string title, Action action)
{
    try { action(); Console.WriteLine($"{title}: OK"); }
    catch (Exception ex) { Console.WriteLine($"{title}: {ex.GetType().Name} — {ex.Message}"); }
}
