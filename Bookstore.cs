public record Customer(string Name, string Contact);

public enum OrderStatus { Processing, Shipped, Delivered }

public class Book
{
    public string Title = "";
    public string Author = "";
    public string Isbn = "";
    public double Price;
    public int Quantity;
    public double Discount;                 // знижка у відсотках (одна на книгу)
    public List<int> Ratings = new();

    public double FinalPrice => Price * (100 - Discount) / 100;
    public double AverageRating => Ratings.Count == 0 ? 0 : Ratings.Average();
}

public class Order
{
    public int Id;
    public Customer Customer = null!;
    public string Isbn = "";
    public string Title = "";
    public int Quantity;
    public double Total;
    public OrderStatus Status = OrderStatus.Processing;
}

public class Bookstore
{
    private readonly Dictionary<string, Book> _books = new();
    private readonly List<Order> _orders = new();

    // ---------- Функція 1: інвентар ----------
    public void AddBook(string title, string author, string isbn, double price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("Назва, автор та ISBN обов'язкові");
        if (price < 0 || quantity < 0)
            throw new ArgumentException("Від'ємна ціна або кількість");
        if (_books.ContainsKey(isbn))
            throw new InvalidOperationException("Книга з таким ISBN вже існує");

        _books[isbn] = new Book { Title = title, Author = author, Isbn = isbn, Price = price, Quantity = quantity };
    }

    public Book GetBook(string isbn) =>
        _books.TryGetValue(isbn, out var book) ? book : throw new KeyNotFoundException("Книгу не знайдено");

    public List<Book> GetBooks() => _books.Values.ToList();

    // ---------- Функція 2: покупки ----------
    // ---------- Функція 3: замовлення (покупка створює замовлення клієнта) ----------
    public Order Purchase(Customer customer, string isbn, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Кількість має бути > 0");
        var book = GetBook(isbn);
        if (quantity > book.Quantity) throw new InvalidOperationException("Недостатньо книг на складі");

        book.Quantity -= quantity;
        var order = new Order
        {
            Id = _orders.Count + 1,
            Customer = customer,
            Isbn = isbn,
            Title = book.Title,
            Quantity = quantity,
            Total = book.FinalPrice * quantity
        };
        _orders.Add(order);
        return order;
    }

    public List<Order> GetOrders(Customer customer) =>
        _orders.Where(o => o.Customer == customer).ToList();

    public List<Order> GetAllOrders() => _orders.ToList();

    public void UpdateStatus(int orderId, OrderStatus status) =>
        (_orders.FirstOrDefault(o => o.Id == orderId) ?? throw new KeyNotFoundException("Замовлення не знайдено"))
            .Status = status;

    // ---------- Додаткова функція 1: рейтинг ----------
    public void Rate(string isbn, int stars)
    {
        if (stars < 1 || stars > 5) throw new ArgumentException("Оцінка має бути від 1 до 5");
        GetBook(isbn).Ratings.Add(stars);
    }

    public double AverageRating(string isbn) => GetBook(isbn).AverageRating;

    // ---------- Додаткова функція 2: знижки ----------
    // Нова знижка замінює попередню, тому на книгу діє не більше однієї знижки.
    public void SetDiscount(string isbn, double percent)
    {
        if (percent < 0 || percent > 100) throw new ArgumentException("Знижка має бути від 0 до 100");
        GetBook(isbn).Discount = percent;
    }
}
