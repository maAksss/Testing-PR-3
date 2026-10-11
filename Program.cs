Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var store = new Bookstore();
store.AddBook("Kobzar", "Shevchenko", "111", 100, 5);
store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 3);

string Ask(string text)
{
    Console.Write(text + ": ");
    return Console.ReadLine() ?? "";
}

while (true)
{
    Console.WriteLine("\n=== КНИЖКОВИЙ МАГАЗИН ===");
    Console.WriteLine("1 - Каталог");
    Console.WriteLine("2 - Додати книгу");
    Console.WriteLine("3 - Купити книгу");
    Console.WriteLine("4 - Замовлення");
    Console.WriteLine("5 - Змінити статус замовлення");
    Console.WriteLine("6 - Оцінити книгу");
    Console.WriteLine("7 - Встановити знижку");
    Console.WriteLine("0 - Вихід");

    try
    {
        switch (Ask("Оберіть пункт"))
        {
            case "1":
                foreach (var b in store.GetBooks())
                    Console.WriteLine($"{b.Isbn} | {b.Title} | {b.Author} | {b.FinalPrice} грн (знижка {b.Discount}%) | {b.Quantity} шт | рейтинг {b.AverageRating:0.0}");
                break;

            case "2":
                store.AddBook(Ask("Назва"), Ask("Автор"), Ask("ISBN"),
                              double.Parse(Ask("Ціна")), int.Parse(Ask("Кількість")));
                Console.WriteLine("Книгу додано.");
                break;

            case "3":
                var customer = new Customer(Ask("Ім'я клієнта"), Ask("Контакт"));
                var order = store.Purchase(customer, Ask("ISBN"), int.Parse(Ask("Кількість")));
                Console.WriteLine($"Замовлення №{order.Id} оформлено. Сума: {order.Total} грн");
                break;

            case "4":
                foreach (var o in store.GetAllOrders())
                    Console.WriteLine($"№{o.Id} | {o.Customer.Name} ({o.Customer.Contact}) | {o.Title} x{o.Quantity} | {o.Total} грн | {o.Status}");
                break;

            case "5":
                int id = int.Parse(Ask("Номер замовлення"));
                int status = int.Parse(Ask("Статус (0 - В обробці, 1 - Відправлено, 2 - Доставлено)"));
                if (!Enum.IsDefined(typeof(OrderStatus), status)) throw new ArgumentException("Невірний статус");
                store.UpdateStatus(id, (OrderStatus)status);
                Console.WriteLine("Статус оновлено.");
                break;

            case "6":
                store.Rate(Ask("ISBN"), int.Parse(Ask("Оцінка (1-5)")));
                Console.WriteLine("Оцінку додано.");
                break;

            case "7":
                store.SetDiscount(Ask("ISBN"), double.Parse(Ask("Знижка у % (0-100)")));
                Console.WriteLine("Знижку встановлено.");
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Невірний пункт меню.");
                break;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("Помилка: " + e.Message);
    }
}
