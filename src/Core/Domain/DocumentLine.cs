namespace Core.Domain;

// Рядок документа посилається на товар за ключем, а не за об'єктом:
// документ не залежить від стану Product.
public sealed record DocumentLine(string ProductId, int Quantity);
