using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("видача нульової кількості", () => product.Issue(0));
TryDo("прихід від'ємної кількості", () => product.RegisterArrival(-5));
TryDo("порожній SKU", () => Product.Create("P-002", "   ", "Пісок", "т", 10));
TryDo("порожня назва", () => Product.Create("P-002", "SKU-002", "", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));
Console.WriteLine($"Залишок після усіх відмов: {product.Quantity} {product.Unit}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 3: ToDto / FromDto ===");
ProductDto dto = product.ToDto();
Console.WriteLine(dto);
Product restored = Product.FromDto(dto);
Console.WriteLine($"Відновлено: {restored}");
TryDo("FromDto з порожнім SKU", () => Product.FromDto(dto with { Sku = "" }));
TryDo("FromDto з від'ємним залишком", () => Product.FromDto(dto with { Quantity = -1 }));

Console.WriteLine();
Console.WriteLine("=== Додатково: імпорт файлу в сутності ===");
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample_domain.csv");
if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

DomainImportResult imported = DomainImportResult.FromImport(ProductCsvImporter.Load(path));
Console.WriteLine($"Створено сутностей: {imported.Products.Count}");
foreach (Product p in imported.Products)
    Console.WriteLine($"  {p}");
Console.WriteLine($"Відхилено: {imported.Errors.Count}");
foreach (string e in imported.Errors)
    Console.WriteLine($"  ! {e}");

Console.WriteLine();
Console.WriteLine("=== Додатково 3: статуси документа (enum + switch expression) ===");
IssueDocument doc = IssueDocument.Create("D-001", "R-10");
Console.WriteLine(doc);
TryDo("підтвердити порожній документ", () => doc.Confirm());
doc.AddLine("P-001", 20);
doc.Confirm();
Console.WriteLine(doc);
TryDo("додати рядок до підтвердженого", () => doc.AddLine("P-001", 5));
TryDo("підтвердити вдруге", () => doc.Confirm());
doc.Cancel();
Console.WriteLine(doc);
TryDo("відновити скасований (Cancelled → Confirmed)", () => doc.Confirm());

Console.WriteLine();
Console.WriteLine("=== Додатково 2: правила на двох сутностях (сервіс) ===");
Product stock = Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 50);
var service = new IssuingService([stock]);

IssueDocument big = service.OpenDocument("D-100", "R-20");
big.AddLine("P-001", 30);
big.AddLine("P-001", 30);
TryDo("видача 60 при залишку 50 (два рядки по 30)", () => service.ConfirmIssue(big));
Console.WriteLine($"  залишок після відмови: {stock.Quantity} {stock.Unit}, статус {big.Status}");

big.Cancel();
IssueDocument ok = service.OpenDocument("D-101", "R-20");
ok.AddLine("P-001", 30);
service.ConfirmIssue(ok);
Console.WriteLine($"  документ {ok.Id} підтверджено, залишок {stock.Quantity} {stock.Unit}");

for (int i = 1; i <= IssuingService.MaxOpenDocuments; i++)
    service.OpenDocument($"D-2{i:00}", "R-30");
TryDo("шостий відкритий документ одного отримувача", () => service.OpenDocument("D-206", "R-30"));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}
