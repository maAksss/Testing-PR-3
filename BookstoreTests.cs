using NUnit.Framework;

[TestFixture]
public class BookstoreTests
{
    private Bookstore _store = null!;
    private readonly Customer _anna = new("Anna", "anna@mail.com");
    private readonly Customer _ivan = new("Ivan", "ivan@mail.com");

    [SetUp]
    public void SetUp()
    {
        _store = new Bookstore();
        _store.AddBook("Kobzar", "Shevchenko", "111", 100, 5);
    }

    // ===== Функція 1: інвентар =====
    [Test]
    public void AddBook_StoresAllData()
    {
        var book = _store.GetBook("111");
        Assert.That(book.Title, Is.EqualTo("Kobzar"));
        Assert.That(book.Author, Is.EqualTo("Shevchenko"));
        Assert.That(book.Price, Is.EqualTo(100));
        Assert.That(book.Quantity, Is.EqualTo(5));
    }

    [Test]
    public void AddBook_DuplicateIsbn_Throws() =>
        Assert.Throws<InvalidOperationException>(() => _store.AddBook("Other", "Author", "111", 50, 1));

    [Test]
    public void AddBook_EmptyIsbn_Throws() =>
        Assert.Throws<ArgumentException>(() => _store.AddBook("A", "B", "", 50, 1));

    [Test]
    public void AddBook_NegativePrice_Throws() =>
        Assert.Throws<ArgumentException>(() => _store.AddBook("A", "B", "222", -1, 1));

    // ===== Функція 2: покупки =====
    [Test]
    public void Purchase_DecreasesQuantity()
    {
        _store.Purchase(_anna, "111", 2);
        Assert.That(_store.GetBook("111").Quantity, Is.EqualTo(3));
    }

    [Test]
    public void Purchase_NotEnoughStock_Throws() =>
        Assert.Throws<InvalidOperationException>(() => _store.Purchase(_anna, "111", 6));

    [Test]
    public void Purchase_UnknownBook_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => _store.Purchase(_anna, "999", 1));

    [Test]
    public void Purchase_ZeroQuantity_Throws() =>
        Assert.Throws<ArgumentException>(() => _store.Purchase(_anna, "111", 0));

    // ===== Функція 3: замовлення =====
    [Test]
    public void Purchase_CreatesOrderWithDetails()
    {
        var order = _store.Purchase(_anna, "111", 2);
        Assert.That(order.Customer, Is.EqualTo(_anna));
        Assert.That(order.Title, Is.EqualTo("Kobzar"));
        Assert.That(order.Quantity, Is.EqualTo(2));
        Assert.That(order.Total, Is.EqualTo(200));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Processing));
    }

    [Test]
    public void GetOrders_ReturnsOnlyCustomerOrders()
    {
        _store.Purchase(_anna, "111", 1);
        _store.Purchase(_ivan, "111", 1);
        _store.Purchase(_anna, "111", 1);
        Assert.That(_store.GetOrders(_anna).Count, Is.EqualTo(2));
        Assert.That(_store.GetOrders(_ivan).Count, Is.EqualTo(1));
    }

    [Test]
    public void UpdateStatus_ChangesOrderStatus()
    {
        var order = _store.Purchase(_anna, "111", 1);
        _store.UpdateStatus(order.Id, OrderStatus.Shipped);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Shipped));
    }

    [Test]
    public void UpdateStatus_UnknownOrder_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => _store.UpdateStatus(42, OrderStatus.Delivered));

    // ===== Додаткова функція 1: рейтинг =====
    [Test]
    public void Rate_CalculatesAverage()
    {
        _store.Rate("111", 4);
        _store.Rate("111", 5);
        Assert.That(_store.AverageRating("111"), Is.EqualTo(4.5));
    }

    [Test]
    public void AverageRating_NoRatings_IsZero() =>
        Assert.That(_store.AverageRating("111"), Is.EqualTo(0));

    [TestCase(0)]
    [TestCase(6)]
    public void Rate_OutOfRange_Throws(int stars) =>
        Assert.Throws<ArgumentException>(() => _store.Rate("111", stars));

    // ===== Додаткова функція 2: знижки =====
    [Test]
    public void Purchase_WithDiscount_UsesDiscountedPrice()
    {
        _store.SetDiscount("111", 10);
        var order = _store.Purchase(_anna, "111", 2);
        Assert.That(order.Total, Is.EqualTo(180));
    }

    [Test]
    public void SetDiscount_ReplacesPreviousDiscount()
    {
        _store.SetDiscount("111", 10);
        _store.SetDiscount("111", 20);
        Assert.That(_store.GetBook("111").FinalPrice, Is.EqualTo(80));
    }

    [TestCase(-1)]
    [TestCase(101)]
    public void SetDiscount_OutOfRange_Throws(double percent) =>
        Assert.Throws<ArgumentException>(() => _store.SetDiscount("111", percent));

    // =====================================================
    //                 ДОДАТКОВІ ТЕСТИ
    // =====================================================

    // ----- Функція 1: інвентар (додатково) -----
    [Test]
    public void GetBooks_ReturnsAllBooks()
    {
        _store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 3);
        Assert.That(_store.GetBooks().Count, Is.EqualTo(2));
    }

    [Test]
    public void GetBook_UnknownIsbn_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => _store.GetBook("999"));

    [Test]
    public void AddBook_NegativeQuantity_Throws() =>
        Assert.Throws<ArgumentException>(() => _store.AddBook("A", "B", "222", 10, -1));

    [TestCase("", "Author")]
    [TestCase("Title", "")]
    [TestCase(null, "Author")]
    public void AddBook_EmptyTitleOrAuthor_Throws(string? title, string author) =>
        Assert.Throws<ArgumentException>(() => _store.AddBook(title!, author, "222", 10, 1));

    [Test]
    public void AddBook_DuplicateIsbn_DoesNotChangeExistingBook()
    {
        Assert.Throws<InvalidOperationException>(() => _store.AddBook("Other", "Author", "111", 1, 99));
        var book = _store.GetBook("111");
        Assert.That(book.Title, Is.EqualTo("Kobzar"));
        Assert.That(book.Quantity, Is.EqualTo(5));
    }

    // ----- Функція 2: покупки (додатково) -----
    [Test]
    public void Purchase_AllStock_LeavesZero_ThenNextPurchaseThrows()
    {
        _store.Purchase(_anna, "111", 5);
        Assert.That(_store.GetBook("111").Quantity, Is.EqualTo(0));
        Assert.Throws<InvalidOperationException>(() => _store.Purchase(_anna, "111", 1));
    }

    [Test]
    public void Purchase_NotEnoughStock_DoesNotChangeStockOrCreateOrder()
    {
        Assert.Throws<InvalidOperationException>(() => _store.Purchase(_anna, "111", 6));
        Assert.That(_store.GetBook("111").Quantity, Is.EqualTo(5));
        Assert.That(_store.GetAllOrders(), Is.Empty);
    }

    [Test]
    public void Purchase_NegativeQuantity_Throws() =>
        Assert.Throws<ArgumentException>(() => _store.Purchase(_anna, "111", -1));

    [Test]
    public void Purchase_DoesNotAffectOtherBooks()
    {
        _store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 3);
        _store.Purchase(_anna, "111", 2);
        Assert.That(_store.GetBook("222").Quantity, Is.EqualTo(3));
    }

    // ----- Функція 3: замовлення (додатково) -----
    [Test]
    public void Purchase_OrderIdsAreSequential()
    {
        var first = _store.Purchase(_anna, "111", 1);
        var second = _store.Purchase(_ivan, "111", 1);
        Assert.That(first.Id, Is.EqualTo(1));
        Assert.That(second.Id, Is.EqualTo(2));
    }

    [Test]
    public void Purchase_OrderStoresIsbn()
    {
        var order = _store.Purchase(_anna, "111", 1);
        Assert.That(order.Isbn, Is.EqualTo("111"));
    }

    [Test]
    public void GetOrders_CustomerWithoutOrders_ReturnsEmpty() =>
        Assert.That(_store.GetOrders(_ivan), Is.Empty);

    [Test]
    public void GetAllOrders_ReturnsOrdersOfAllCustomers()
    {
        _store.Purchase(_anna, "111", 1);
        _store.Purchase(_ivan, "111", 1);
        Assert.That(_store.GetAllOrders().Count, Is.EqualTo(2));
    }

    [Test]
    public void UpdateStatus_FullCycle()
    {
        var order = _store.Purchase(_anna, "111", 1);
        _store.UpdateStatus(order.Id, OrderStatus.Shipped);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Shipped));
        _store.UpdateStatus(order.Id, OrderStatus.Delivered);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Delivered));
    }

    [Test]
    public void UpdateStatus_DoesNotAffectOtherOrders()
    {
        var first = _store.Purchase(_anna, "111", 1);
        var second = _store.Purchase(_ivan, "111", 1);
        _store.UpdateStatus(first.Id, OrderStatus.Delivered);
        Assert.That(second.Status, Is.EqualTo(OrderStatus.Processing));
    }

    // ----- Рейтинг (додатково) -----
    [Test]
    public void Rate_UnknownBook_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => _store.Rate("999", 3));

    [Test]
    public void Rate_BoundaryValues_AreAccepted()
    {
        _store.Rate("111", 1);
        _store.Rate("111", 5);
        Assert.That(_store.AverageRating("111"), Is.EqualTo(3));
    }

    [Test]
    public void AverageRating_IsCalculatedPerBook()
    {
        _store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 3);
        _store.Rate("111", 5);
        _store.Rate("222", 1);
        Assert.That(_store.AverageRating("111"), Is.EqualTo(5));
        Assert.That(_store.AverageRating("222"), Is.EqualTo(1));
    }

    [Test]
    public void AverageRating_UnknownBook_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => _store.AverageRating("999"));

    [Test]
    public void Rate_InvalidStars_DoesNotChangeAverage()
    {
        _store.Rate("111", 4);
        Assert.Throws<ArgumentException>(() => _store.Rate("111", 6));
        Assert.That(_store.AverageRating("111"), Is.EqualTo(4));
    }

    // ----- Знижки (додатково) -----
    [Test]
    public void SetDiscount_Zero_RemovesDiscount()
    {
        _store.SetDiscount("111", 10);
        _store.SetDiscount("111", 0);
        Assert.That(_store.GetBook("111").FinalPrice, Is.EqualTo(100));
    }

    [Test]
    public void SetDiscount_Hundred_MakesBookFree()
    {
        _store.SetDiscount("111", 100);
        var order = _store.Purchase(_anna, "111", 2);
        Assert.That(order.Total, Is.EqualTo(0));
    }

    [Test]
    public void SetDiscount_UnknownBook_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => _store.SetDiscount("999", 10));

    [Test]
    public void SetDiscount_DoesNotAffectOtherBooks()
    {
        _store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 3);
        _store.SetDiscount("111", 50);
        Assert.That(_store.GetBook("222").FinalPrice, Is.EqualTo(60));
    }

    [Test]
    public void SetDiscount_DoesNotChangeExistingOrders()
    {
        var order = _store.Purchase(_anna, "111", 1);
        _store.SetDiscount("111", 50);
        Assert.That(order.Total, Is.EqualTo(100));
    }

    [Test]
    public void SetDiscount_InvalidValue_DoesNotChangeCurrentDiscount()
    {
        _store.SetDiscount("111", 10);
        Assert.Throws<ArgumentException>(() => _store.SetDiscount("111", 150));
        Assert.That(_store.GetBook("111").Discount, Is.EqualTo(10));
    }

    // =====================================================
    //        ТЕСТИ ЗА ТЗ (доповнення до вимог 1-3)
    // =====================================================

    // ----- Функція 3: замовлення мають містити дані клієнта, книги, кількість, суму -----
    [Test]
    public void Purchase_OrderContainsCustomerNameAndContact()
    {
        var order = _store.Purchase(_anna, "111", 1);
        Assert.That(order.Customer.Name, Is.EqualTo("Anna"));
        Assert.That(order.Customer.Contact, Is.EqualTo("anna@mail.com"));
    }

    [Test]
    public void Purchase_IsRecordedInOrdersList()
    {
        var order = _store.Purchase(_anna, "111", 1);
        Assert.That(_store.GetAllOrders().Count, Is.EqualTo(1));
        Assert.That(_store.GetAllOrders()[0], Is.SameAs(order));
    }

    [Test]
    public void Purchase_OrderTotalIsPriceTimesQuantity()
    {
        _store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 10);
        var order = _store.Purchase(_anna, "222", 3);
        Assert.That(order.Title, Is.EqualTo("Tiger Hunters"));
        Assert.That(order.Total, Is.EqualTo(180));
    }

    [Test]
    public void GetOrders_FindsOrdersByCustomerData()
    {
        _store.Purchase(_anna, "111", 1);
        var sameCustomer = new Customer("Anna", "anna@mail.com");
        Assert.That(_store.GetOrders(sameCustomer).Count, Is.EqualTo(1));
    }

    [Test]
    public void GetOrders_SameNameDifferentContact_IsDifferentCustomer()
    {
        _store.Purchase(_anna, "111", 1);
        var otherAnna = new Customer("Anna", "other@mail.com");
        Assert.That(_store.GetOrders(otherAnna), Is.Empty);
    }

    [Test]
    public void Purchase_SeveralPurchasesBySameCustomer_AreAllLinkedToCustomer()
    {
        _store.Purchase(_anna, "111", 1);
        _store.Purchase(_anna, "111", 2);
        var orders = _store.GetOrders(_anna);
        Assert.That(orders.Count, Is.EqualTo(2));
        Assert.That(orders.Sum(o => o.Quantity), Is.EqualTo(3));
    }

    // ----- Рейтинг: межі оцінок і обчислення середнього -----
    [Test]
    public void AverageRating_WithSeveralRatings_IsCorrect()
    {
        _store.Rate("111", 3);
        _store.Rate("111", 4);
        _store.Rate("111", 4);
        Assert.That(_store.AverageRating("111"), Is.EqualTo(11.0 / 3).Within(0.0001));
    }

    [Test]
    public void AverageRating_UpdatesAfterNewRating()
    {
        _store.Rate("111", 5);
        Assert.That(_store.AverageRating("111"), Is.EqualTo(5));
        _store.Rate("111", 3);
        Assert.That(_store.AverageRating("111"), Is.EqualTo(4));
    }

    [Test]
    public void Rate_NegativeStars_Throws() =>
        Assert.Throws<ArgumentException>(() => _store.Rate("111", -1));

    // ----- Знижки: правила обчислення, застосування при покупці, фінальна ціна -----
    [Test]
    public void FinalPrice_WithoutDiscount_EqualsPrice() =>
        Assert.That(_store.GetBook("111").FinalPrice, Is.EqualTo(100));

    [Test]
    public void FinalPrice_WithFractionalDiscount_IsCorrect()
    {
        _store.SetDiscount("111", 12.5);
        Assert.That(_store.GetBook("111").FinalPrice, Is.EqualTo(87.5));
    }

    [Test]
    public void Purchase_SeveralBooksWithDiscount_TotalIsCorrect()
    {
        _store.SetDiscount("111", 25);
        var order = _store.Purchase(_anna, "111", 3);   // 100 * 0.75 * 3
        Assert.That(order.Total, Is.EqualTo(225));
    }

    [Test]
    public void Purchase_AfterSecondDiscount_DiscountsDoNotStack()
    {
        _store.SetDiscount("111", 10);
        _store.SetDiscount("111", 20);
        var order = _store.Purchase(_anna, "111", 1);
        Assert.That(order.Total, Is.EqualTo(80));       // а не 72
    }

    [Test]
    public void Purchase_OtherBookWithoutDiscount_PaysFullPrice()
    {
        _store.AddBook("Tiger Hunters", "Bagryanyi", "222", 60, 3);
        _store.SetDiscount("111", 50);
        var order = _store.Purchase(_anna, "222", 1);
        Assert.That(order.Total, Is.EqualTo(60));
    }

    [Test]
    public void Purchase_AfterDiscountRemoved_PaysFullPrice()
    {
        _store.SetDiscount("111", 30);
        _store.SetDiscount("111", 0);
        var order = _store.Purchase(_anna, "111", 1);
        Assert.That(order.Total, Is.EqualTo(100));
    }

    [TestCase(0)]
    [TestCase(100)]
    public void SetDiscount_BoundaryValues_AreAccepted(double percent) =>
        Assert.DoesNotThrow(() => _store.SetDiscount("111", percent));
}
