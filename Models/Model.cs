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

        public WebshopContext(DbContextOptions<WebshopContext> options): base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Remove On Cascade for all relationships
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())){
                relationship.DeleteBehavior = DeleteBehavior.Restrict;}

            modelBuilder.ForNpgsqlUseIdentityColumns();


            // Address
            modelBuilder.Entity<Address>()
            .Property(a => a.Street)
            .IsRequired();
            modelBuilder.Entity<Address>()
            .Property(a => a.City)
            .IsRequired();
            modelBuilder.Entity<Address>()
            .Property(a => a.ZipCode)
            .IsRequired();
            modelBuilder.Entity<Address>()
            .Property(a => a.HouseNumber)
            .IsRequired();

            // Brand
            modelBuilder.Entity<Brand>()
            .Property(a => a.BrandName)
            .IsRequired();
            modelBuilder.Entity<Brand>()
            .HasMany(p => p.Collections)
            .WithOne(p => p.Brand)
            .OnDelete(DeleteBehavior.Cascade);

            // Cart
            modelBuilder.Entity<Cart>()
            .HasOne(s => s.User)
            .WithOne(b => b.Cart)
            .HasForeignKey<Cart>(b => b.UserId)
            .IsRequired();
            modelBuilder.Entity<Cart>()
            .HasMany(p => p.Products)
            .WithOne(p => p.Cart)
            .OnDelete(DeleteBehavior.Cascade);

            // CartProduct
            modelBuilder.Entity<CartProduct>()
            .HasKey(cp => new {cp.CartId, cp.ProductId});
            modelBuilder.Entity<CartProduct>()
            .HasOne(cp => cp.Cart)
            .WithMany(c => c.Products)
            .HasForeignKey(cp => cp.CartId)
            .IsRequired();
            modelBuilder.Entity<CartProduct>()
            .HasOne(cp => cp.Product)
            .WithMany(p => p.Carts)
            .HasForeignKey(cp => cp.ProductId)
            .IsRequired();

            // Category
            modelBuilder.Entity<Category>()
            .Property(a => a.CategoryName)
            .IsRequired();

            // Category_Type
            modelBuilder.Entity<Category_Type>()
            .HasKey(ct => new {ct.CategoryId, ct._TypeId});
            modelBuilder.Entity<Category_Type>()
            .HasOne(ct => ct.Category)
            .WithMany(c => c._Types)
            .HasForeignKey(ct => ct.CategoryId)
            .IsRequired();
            modelBuilder.Entity<Category_Type>()
            .HasOne(ct => ct._Type)
            .WithMany(t => t.Categories)
            .HasForeignKey(ct => ct._TypeId)
            .IsRequired();

            // Collection
            modelBuilder.Entity<Collection>()
            .Property(a => a.CollectionName)
            .IsRequired();
            modelBuilder.Entity<Collection>()
            .HasOne(s => s.Brand)
            .WithMany(b => b.Collections)
            .HasForeignKey(b => b.BrandId)
            .IsRequired();

            // Order
            modelBuilder.Entity<Order>()
            .HasOne(s => s.Address)
            .WithMany(b => b.Orders)
            .HasForeignKey(b => b.AddressId)
            .IsRequired();
            modelBuilder.Entity<Order>()
            .HasOne(s => s.OrderStatus)
            .WithMany(b => b.Orders)
            .HasForeignKey(b => b.OrderStatusId)
            .IsRequired();
            modelBuilder.Entity<Order>()
            .HasOne(s => s.User)
            .WithMany(b => b.Orders)
            .HasForeignKey(b => b.UserId)
            .IsRequired(false);
            modelBuilder.Entity<Order>()
            .HasMany(p => p.Products)
            .WithOne(p => p.Order)
            .OnDelete(DeleteBehavior.Cascade);

            // OrderProduct
            modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new {op.OrderId, op.ProductId});
            modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.Orders)
            .HasForeignKey(op => op.ProductId)
            .IsRequired();
            modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.Products)
            .HasForeignKey(op => op.OrderId)
            .IsRequired();

            // OrderStatus
            modelBuilder.Entity<OrderStatus>()
            .Property(a => a.OrderDescription)
            .IsRequired();

            // Product
            modelBuilder.Entity<Product>()
            .Property(a => a.ProductNumber)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .Property(a => a.ProductName)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .Property(a => a.ProductEAN)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .Property(a => a.ProductPrice)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .Property(a => a.ProductColor)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .HasIndex(u => u.ProductNumber)
            .IsUnique(true);
            modelBuilder.Entity<Product>()
            .HasOne(s => s._Type)
            .WithMany(b => b.Products)
            .HasForeignKey(b => b._TypeId)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .HasOne(s => s.Category)
            .WithMany(b => b.Products)
            .HasForeignKey(b => b.CategoryId)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .HasOne(s => s.Collection)
            .WithMany(b => b.Products)
            .HasForeignKey(b => b.CollectionId)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .HasOne(s => s.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(b => b.BrandId)
            .IsRequired();
            modelBuilder.Entity<Product>()
            .HasOne(s => s.Stock)
            .WithOne(b => b.Product)
            .HasForeignKey<Product>(b => b.StockId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>()
            .HasMany(p => p.Carts)
            .WithOne(p => p.Product)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>()
            .HasMany(p => p.Wishlists)
            .WithOne(p => p.Product)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>()
            .HasMany(p => p.ProductImages)
            .WithOne(p => p.Product)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>()
            .HasOne(p => p.Sale)
            .WithOne(p => p.Product)
            .OnDelete(DeleteBehavior.Cascade);

            // ProductImage
            modelBuilder.Entity<ProductImage>()
            .Property(a => a.ImageURL)
            .IsRequired();
            modelBuilder.Entity<ProductImage>()
            .HasOne(s => s.Product)
            .WithMany(b => b.ProductImages)
            .HasForeignKey(b => b.ProductId)
            .IsRequired();

            // Sale
            modelBuilder.Entity<Sale>()
            .HasOne(s => s.Product)
            .WithOne(s => s.Sale)
            .HasForeignKey<Sale>(b => b.ProductId)
            .IsRequired();

            // Stock


            // _Type
            modelBuilder.Entity<_Type>()
            .Property(a => a._TypeName)
            .IsRequired();

            // User
            modelBuilder.Entity<User>()
            .Property(a => a.UserPassword)
            .IsRequired();
            modelBuilder.Entity<User>()
            .Property(a => a.FirstName)
            .IsRequired();
            modelBuilder.Entity<User>()
            .Property(a => a.LastName)
            .IsRequired();
            modelBuilder.Entity<User>()
            .Property(a => a.EmailAddress)
            .IsRequired();
            modelBuilder.Entity<User>()
            .Property(a => a.Role)
            .HasDefaultValue<string>("User")
            .IsRequired();
            modelBuilder.Entity<User>()
            .Property(a => a.PhoneNumber)
            .IsRequired(false);
            modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailAddress)
            .IsUnique(true);
            modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique(true);
            modelBuilder.Entity<User>()
            .HasOne(p => p.Cart)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
            .HasOne(p => p.Wishlist)
            .WithOne(p => p.User)
            .OnDelete(DeleteBehavior.Cascade);

            // UserAddress
            modelBuilder.Entity<UserAddress>()
            .HasKey(ua => new {ua.UserId, ua.AddressId});
            modelBuilder.Entity<UserAddress>()
            .HasOne(ua => ua.Addresses)
            .WithMany(a => a.Users)
            .HasForeignKey(ua => ua.AddressId)
            .IsRequired();
            modelBuilder.Entity<UserAddress>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(ua => ua.UserId)
            .IsRequired();

            // Wishlist
            modelBuilder.Entity<Wishlist>()
            .HasOne(s => s.User)
            .WithOne(b => b.Wishlist)
            .HasForeignKey<Wishlist>(b => b.UserId)
            .IsRequired();
            modelBuilder.Entity<Wishlist>()
            .HasMany(p => p.Products)
            .WithOne(p => p.Wishlist)
            .OnDelete(DeleteBehavior.Cascade);
            

            // WishlistProduct
            modelBuilder.Entity<WishlistProduct>()
            .HasKey(wp => new {wp.WishlistId, wp.ProductId});
            modelBuilder.Entity<WishlistProduct>()
            .HasOne(wp => wp.Product)
            .WithMany(p => p.Wishlists)
            .HasForeignKey(wp => wp.ProductId)
            .IsRequired();
            modelBuilder.Entity<WishlistProduct>()
            .HasOne(wp => wp.Wishlist)
            .WithMany(w => w.Products)
            .HasForeignKey(wp => wp.WishlistId)
            .IsRequired();
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
        public int UserId {get; set;}
        public User User {get; set;}
        public double? CartTotalPrice {get; set;}
        public List<CartProduct> Products {get; set;}
    }
 
    public class CartProduct
    {
        public int CartId {get; set;}
        public Cart Cart {get; set;}
        public int ProductId {get; set;}
        public Product Product {get; set;}
        public int CartQuantity {get; set;}
        public DateTime CartDateAdded {get; set;}
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
        public Brand Brand {get; set;}
        public string CollectionName {get; set;}
        public List<Product> Products {get; set;}
    }


    public class Order
    {
        public int Id {get; set;}
        public int? UserId {get; set;}
        public User User {get; set;}
        public int AddressId {get; set;}
        public Address Address {get; set;}
        public int OrderStatusId {get; set;}
        public OrderStatus OrderStatus {get; set;}
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
        public _Type _Type {get; set;}
        public int CategoryId {get; set;}
        public Category Category {get; set;}
        public int CollectionId {get; set;}
        public Collection Collection {get; set;}
        public int BrandId {get; set;}
        public Brand Brand {get; set;}
        public int StockId {get; set;}
        public Stock Stock {get; set;}
        public Sale Sale {get; set;}
        public List<ProductImage> ProductImages {get; set;}
        public List<OrderProduct> Orders {get; set;}
        public List<WishlistProduct> Wishlists {get; set;}
        public List<CartProduct> Carts {get; set;}
    }

    public class ProductImage
    {
        public int Id {get; set;}
        public int ProductId {get; set;}
        public Product Product {get; set;}
        public string ImageURL {get; set;}
    }

    public class Sale
    {
        public int Id {get; set;}
        public int ProductId {get; set;}
        public Product Product {get; set;}
        public double ProductNewPrice {get; set;}
        public DateTime StartDate {get; set;}
        public DateTime ExpiryDate {get; set;}
    }

    public class Stock
    {
        public int Id {get; set;}
        public int ProductQuantity {get; set;}
        public Product Product {get; set;}
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
        public string UserPassword {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public DateTime BirthDate {get; set;}
        public string Gender {get; set;}
        public string EmailAddress {get; set;}
        public int? PhoneNumber {get; set;}
        public string Role {get; set;}
        public Cart Cart {get; set;}
        public Wishlist Wishlist {get; set;}
        public List<UserAddress> Addresses {get; set;}
        public List<Order> Orders {get; set;}
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
        public User User {get; set;}
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