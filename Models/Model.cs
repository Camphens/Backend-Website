using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend_Website.Models
{
    public class WebshopContext : DbContext
    {
        public DbSet<Address> Addresses                 {get; set;}
        public DbSet<Brand> Brands                      {get; set;}
        public DbSet<Cart> Carts                        {get; set;}
        public DbSet<CartProduct> CartProducts          {get; set;}
        public DbSet<Category> Categories               {get; set;}
        public DbSet<Category_Type> CategoryType        {get; set;}
        public DbSet<Collection> Collections            {get; set;}
        public DbSet<Order> Orders                      {get; set;}
        public DbSet<OrderProduct> OrderProduct         {get; set;}
        public DbSet<OrderStatus> OrderStatus           {get; set;}
        public DbSet<Product> Products                  {get; set;}
        public DbSet<ProductImage> ProductImages        {get; set;}
        public DbSet<Sale> Sales                        {get; set;}
        public DbSet<Stock> Stock                       {get; set;}
        public DbSet<_Type> Types                       {get; set;}
        public DbSet<User> Users                        {get; set;}
        public DbSet<UserAddress> UserAddress           {get; set;}
        public DbSet<Wishlist> Wishlists                {get; set;}
        public DbSet<WishlistProduct> WishlistProduct   {get; set;}
        


        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseNpgsql("User ID=postgres;Password=Postgresqlpasword;Host=localhost;Port=5432;Database=WebshopData;Pooling=true;");
        // }

        public WebshopContext(DbContextOptions<WebshopContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
{
    relationship.DeleteBehavior = DeleteBehavior.Restrict;
}
            modelBuilder.Entity<Category_Type>()
            .HasKey(ct => new {ct.CategoryId, ct._TypeId});
            modelBuilder.Entity<Category_Type>()
            .HasOne(ct => ct.Category)
            .WithMany(c => c._Types)
            .HasForeignKey(ct => ct.CategoryId);
            modelBuilder.Entity<Category_Type>()
            .HasOne(ct => ct._Type)
            .WithMany(t => t.Categories)
            .HasForeignKey(ct => ct._TypeId);

            modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new {op.OrderId, op.ProductId});
            modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.Orders)
            .HasForeignKey(op => op.ProductId);
            modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.Products)
            .HasForeignKey(op => op.OrderId);

            modelBuilder.Entity<UserAddress>()
            .HasKey(ua => new {ua.UserId, ua.AddressId});
            modelBuilder.Entity<UserAddress>()
            .HasOne(ua => ua.Addresses)
            .WithMany(a => a.Users)
            .HasForeignKey(ua => ua.AddressId);
            modelBuilder.Entity<UserAddress>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<WishlistProduct>()
            .HasKey(wp => new {wp.WishlistId, wp.ProductId});
            modelBuilder.Entity<WishlistProduct>()
            .HasOne(wp => wp.Product)
            .WithMany(p => p.Wishlists)
            .HasForeignKey(wp => wp.ProductId);
            modelBuilder.Entity<WishlistProduct>()
            .HasOne(wp => wp.Wishlist)
            .WithMany(w => w.Products)
            .HasForeignKey(wp => wp.WishlistId);

            modelBuilder.Entity<CartProduct>()
            .HasKey(cp => new {cp.CartId, cp.ProductId});
            modelBuilder.Entity<CartProduct>()
            .HasOne(cp => cp.Cart)
            .WithMany(c => c.Products)
            .HasForeignKey(cp => cp.CartId);
            modelBuilder.Entity<CartProduct>()
            .HasOne(cp => cp.Product)
            .WithMany(p => p.Carts)
            .HasForeignKey(cp => cp.CartId);
        }
    }

    public class Address
    {
        public int Id {get; set;}
        public string Street {get; set;}
        public string City {get; set;}
        public string ZipCode {get; set;}
        public string HouseNumber {get; set;}
        public List<UserAddress> Users {get; set;}
        public List<Order> Orders {get; set;}
    }

    public class Brand
    {
        public int Id {get; set;}
        public string BrandName {get; set;}
        public List<Product> Products {get; set;}
        public List<Collection> Collections {get; set;}
    }

    public class Cart
    {
        public int Id {get; set;}
        public int? UserId {get; set;}
        public double CartTotalPrice {get; set;}
        public List<CartProduct> Products {get; set;}
    }
 
    public class CartProduct
    {
        public int CartId {get; set;}
        public Cart Cart {get; set;}
        public int ProductId {get; set;}
        public Product Product {get; set;}
    }

    public class Category
    {
        public int Id {get; set;}
        public string CategoryName {get; set;}
        public List<Product> Products {get; set;}
        public List<Category_Type> _Types {get; set;}
    }

    public class Category_Type
    {
        public int CategoryId {get; set;}
        public Category Category {get; set;}
        public int _TypeId {get; set;}
        public _Type _Type {get; set;}
    }

    public class Collection
    {
        public int Id {get; set;}
        public int BrandId {get; set;}
        public string CollectionName {get; set;}
        public List<Product> Products {get; set;}
    }


    public class Order
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public int AddressId {get; set;}
        public int OrderStatusId {get; set;}
        public double OrderTotalPrice {get; set;}
        public DateTime OrderDate {get; set;}
        public List<OrderProduct> Products {get; set;}
    }

    public class OrderProduct
    {
        public int OrderId {get; set;}
        public Order Order {get; set;}
        public int ProductId {get; set;}
        public Product Product {get; set;}
        public int OrderQuantity {get; set;}
    }

    public class OrderStatus
    {
        public int Id {get; set;}
        public string OrderDescription {get; set;}
        public List<Order> Orders {get; set;}
    }

    public class Product
    {
        public int Id {get; set;}
        public string ProductNumber {get; set;}
        public string ProductName {get; set;}
        public string ProductEAN {get; set;}
        public string ProductInfo {get; set;}
        public string ProductDescription {get; set;}
        public string ProductSpecification {get; set;}
        public double ProductPrice {get; set;}
        public string ProductColor {get; set;}
        public int _TypeId {get; set;}
        public int CategoryId {get; set;}
        public int CollectionId {get; set;}
        public int BrandId {get; set;}
        public int StockId {get; set;}
        public List<ProductImage> ProductImages {get; set;}
        public List<OrderProduct> Orders {get; set;}
        public List<WishlistProduct> Wishlists {get; set;}
        public List<CartProduct> Carts {get; set;}
    }

    public class ProductImage
    {
        public int Id {get; set;}
        public int ProductId {get; set;}
        public string ImageURL {get; set;}
    }

    public class Sale
    {
        public int Id {get; set;}
        public int ProductId {get; set;}
        public double ProductNewPrice {get; set;}
        public DateTime StartDate {get; set;}
        public DateTime ExpiryDate {get; set;}
    }

    public class Stock
    {
        public int Id {get; set;}
        public int ProductQuantity {get; set;}
        public List<Product> Products {get; set;}
    }

    public class _Type
    {
        public int Id {get; set;}
        public string _TypeName {get; set;}
        public List<Category_Type> Categories {get; set;}
        public List<Product> Products {get; set;}
    }

    public class User
    {
        public int Id {get; set;}
        public string UserName {get; set;}
        public string UserPassword {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public DateTime BirthDate {get; set;}
        public string Gender {get; set;}
        public string EmailAddress {get; set;}
        public int PhoneNumber {get; set;}
        public List<UserAddress> Addresses {get; set;}
    }

    public class UserAddress
    {
        public int UserId {get; set;}
        public User User {get; set;}
        public int AddressId {get; set;}
        public Address Addresses {get; set;}
    }

    public class Wishlist
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public List<WishlistProduct> Products {get; set;}
    }

    public class WishlistProduct
    {
        public int WishlistId {get; set;}
        public Wishlist Wishlist {get; set;}
        public int ProductId {get; set;}
        public Product Product {get; set;}
    }

}