﻿using System;
using System.Collections.Generic;
using System.Text;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bangazon.Data {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base (options) { }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductType> ProductType { get; set; }
        public DbSet<PaymentType> PaymentType { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderProduct> OrderProduct { get; set; }

        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            base.OnModelCreating (modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            modelBuilder.Entity<Order> ()
                .Property (b => b.DateCreated)
                .HasDefaultValueSql ("GETDATE()");

            // Restrict deletion of related order when OrderProducts entry is removed
            modelBuilder.Entity<Order> ()
                .HasMany (o => o.OrderProducts)
                .WithOne (l => l.Order)
                .OnDelete (DeleteBehavior.Restrict);

            modelBuilder.Entity<Product> ()
                .Property (b => b.DateCreated)
                .HasDefaultValueSql ("GETDATE()");

            // Restrict deletion of related product when OrderProducts entry is removed
            modelBuilder.Entity<Product> ()
                .HasMany (o => o.OrderProducts)
                .WithOne (l => l.Product)
                .OnDelete (DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentType> ()
                .Property (b => b.DateCreated)
                .HasDefaultValueSql ("GETDATE()");

            ApplicationUser user = new ApplicationUser {
                FirstName = "admin",
                LastName = "admin",
                StreetAddress = "123 Infinity Way",
                UserName = "admin@admin.com",
                NormalizedUserName = "ADMIN@ADMIN.COM",
                Email = "admin@admin.com",
                NormalizedEmail = "ADMIN@ADMIN.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid ().ToString ("D")
            };
            var passwordHash = new PasswordHasher<ApplicationUser> ();
            user.PasswordHash = passwordHash.HashPassword (user, "Admin8*");
            modelBuilder.Entity<ApplicationUser> ().HasData (user);

            modelBuilder.Entity<PaymentType> ().HasData (
                new PaymentType () {
                    PaymentTypeId = 1,
                        UserId = user.Id,
                        Description = "American Express",
                        AccountNumber = "86753095551212"
                },
                new PaymentType () {
                    PaymentTypeId = 2,
                        UserId = user.Id,
                        Description = "Discover",
                        AccountNumber = "4102948572991"
                }
            );

            modelBuilder.Entity<ProductType> ().HasData (
                new ProductType () 
                {
                    ProductTypeId = 1,
                        Label = "Sporting Goods"
                },
                new ProductType () 
                {
                    ProductTypeId = 2,
                        Label = "Appliances"
                },
                new ProductType()
                {
                    ProductTypeId = 3,
                    Label = "Tools"
                },
                new ProductType()
                {
                    ProductTypeId = 4,
                    Label = "Books"
                },
                new ProductType()
                {
                    ProductTypeId = 5,
                    Label = "Movies and TV"
                },
                new ProductType()
                {
                    ProductTypeId = 6,
                    Label = "Video Games"
                },
                new ProductType()
                {
                    ProductTypeId = 7,
                    Label = "Clothing"
                }
            );

            modelBuilder.Entity<Product> ().HasData (
                new Product () {
                    ProductId = 1,
                        ProductTypeId = 1,
                        UserId = user.Id,
                        Description = "It flies high",
                        Title = "Kite",
                        City = "Nashville",
                        ImagePath = "https://i.imgur.com/c7u218K.gif",
                        Quantity = 100,
                        Price = 7.99
                },
                new Product () {
                    ProductId = 2,
                        ProductTypeId = 2,
                        UserId = user.Id,
                        Description = "We need it, NSS needs it. Get it!",
                        Title = "Microwave 4",
                        City = "Nashville",
                        ImagePath = "https://i.imgur.com/c7u218K.gif",
                        Quantity = 5,
                        Price = 189.99
                },
                new Product()
                {
                    ProductId = 3,
                    ProductTypeId = 3,
                    UserId = user.Id,
                    Description = "When you only have a hammer, everything starts to look like a nail..",
                    Title = "Hammer",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 5,
                    Price = 29.99
                },
                new Product()
                {
                    ProductId = 4,
                    ProductTypeId = 4,
                    UserId = user.Id,
                    Description = "You know there's a lot more magic going on at that school...",
                    Title = "50 Shades of Potter Hardcover",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 5,
                    Price = 24.99
                },
                new Product()
                {
                    ProductId = 5,
                    ProductTypeId = 4,
                    UserId = user.Id,
                    Description = "In a hole in the ground there lived a Hobbit...",
                    Title = "The Hobbit",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 9,
                    Price = 19.99
                },
                new Product()
                {
                    ProductId = 6,
                    ProductTypeId = 5,
                    UserId = user.Id,
                    Description = "Join the Parr family on their continuing adventure!",
                    Title = "The Incredibles 2",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 15,
                    Price = 29.99
                },
                new Product()
                {
                    ProductId = 7,
                    ProductTypeId = 5,
                    UserId = user.Id,
                    Description = "Like a turd, rolling in the wind...",
                    Title = "Venom 4k Blu-ray Combo",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 35,
                    Price = 29.99
                },
                new Product()
                {
                    ProductId = 8,
                    ProductTypeId = 5,
                    UserId = user.Id,
                    Description = "Find out what happned to Ant-Man during the events of Infinity War",
                    Title = "Ant-Man 2 4k Blu-ray Combo",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 22,
                    Price = 34.99
                },
                new Product()
                {
                    ProductId = 9,
                    ProductTypeId = 6,
                    UserId = user.Id,
                    Description = "The wait is over! Check out this chilling peek of the future!",
                    Title = "Red Dead Redemption 2",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 45,
                    Price = 59.99
                },
                new Product()
                {
                    ProductId = 10,
                    ProductTypeId = 6,
                    UserId = user.Id,
                    Description = "Insomniac saves the superhero genre with this stellar entry!",
                    Title = "Spider-Man",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 65,
                    Price = 59.99
                },
                new Product()
                {
                    ProductId = 11,
                    ProductTypeId = 6,
                    UserId = user.Id,
                    Description = "Yes, another one. Pick a war, any war!",
                    Title = "Battlefield 5",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 125,
                    Price = 49.99
                },
                new Product()
                {
                    ProductId = 12,
                    ProductTypeId = 7,
                    UserId = user.Id,
                    Description = "Everyone loves a gender neutral sweater!",
                    Title = "Hoodie",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 74,
                    Price = 69.99
                },
                new Product()
                {
                    ProductId = 13,
                    ProductTypeId = 7,
                    UserId = user.Id,
                    Description = "Like a regular hat, but seen on hobos and hipsters alike!",
                    Title = "Hipster Hat",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 128,
                    Price = 19.99
                },
                new Product()
                {
                    ProductId = 14,
                    ProductTypeId = 2,
                    UserId = user.Id,
                    Description = "Yet another expansion for a dying game...",
                    Title = "Destiny 2: Forsaken",
                    City = "Nashville",
                    ImagePath = "https://i.imgur.com/c7u218K.gif",
                    Quantity = 154,
                    Price = 39.99
                }
            );

            modelBuilder.Entity<Order> ().HasData (
                new Order () {
                    OrderId = 1,
                    UserId = user.Id,
                    PaymentTypeId = null
                }
            );

            modelBuilder.Entity<OrderProduct> ().HasData (
                new OrderProduct () {
                    OrderProductId = 1,
                    OrderId = 1,
                    ProductId = 1
                }
            );

            modelBuilder.Entity<OrderProduct> ().HasData (
                new OrderProduct () {
                    OrderProductId = 2,
                    OrderId = 1,
                    ProductId = 2
                }
            );

        }
    }
}