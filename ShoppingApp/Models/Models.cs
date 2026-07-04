using System;
using System.Collections.Generic;

namespace ShoppingApp.Models
{
    public class User
    {
        public int UserID       { get; set; }
        public string Username  { get; set; }
        public string FullName  { get; set; }
        public string Email     { get; set; }
        public bool IsAdmin     { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Product
    {
        public int ProductID        { get; set; }
        public string Name          { get; set; }
        public string Description   { get; set; }
        public decimal Price        { get; set; }
        public int Stock            { get; set; }
        public int CategoryID       { get; set; }
        public string CategoryName  { get; set; }
        public string CategoryIcon  { get; set; }

        public string PriceFormatted => $"Rs. {Price:N0}";
    }

    public class CartItem
    {
        public Product Product  { get; set; }
        public int Quantity     { get; set; }
        public decimal Subtotal => Product.Price * Quantity;
        public string SubtotalFormatted => $"Rs. {Subtotal:N0}";
    }

    public class Order
    {
        public int OrderID          { get; set; }
        public int UserID           { get; set; }
        public decimal TotalAmount  { get; set; }
        public string Status        { get; set; }
        public string Address       { get; set; }
        public DateTime OrderDate   { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public string TotalFormatted => $"Rs. {TotalAmount:N0}";
    }

    public class Category
    {
        public int CategoryID   { get; set; }
        public string Name      { get; set; }
        public string Icon      { get; set; }
        public string Display   => $"{Icon} {Name}";
    }

    // Session singleton
    public static class Session
    {
        public static User CurrentUser         { get; set; }
        public static List<CartItem> Cart      { get; set; } = new List<CartItem>();

        public static decimal CartTotal =>
            Cart.Count == 0 ? 0 : Cart.Sum(c => c.Subtotal);

        public static int CartCount =>
            Cart.Count == 0 ? 0 : Cart.Sum(c => c.Quantity);

        public static void AddToCart(Product product, int qty = 1)
        {
            var existing = Cart.Find(c => c.Product.ProductID == product.ProductID);
            if (existing != null)
                existing.Quantity += qty;
            else
                Cart.Add(new CartItem { Product = product, Quantity = qty });
        }

        public static void RemoveFromCart(int productID)
        {
            Cart.RemoveAll(c => c.Product.ProductID == productID);
        }

        public static void ClearCart() => Cart.Clear();

        public static void Logout()
        {
            CurrentUser = null;
            Cart.Clear();
        }
    }
}
