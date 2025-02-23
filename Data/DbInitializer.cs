using server.Models;

namespace server.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if we already have any products
        if (context.Products.Any())
        {
            return; // DB has been seeded
        }

        // Add sample products
        var products = new Product[]
        {
            new Product
            {
                Nom = "Product 1",
                Description = "Description for product 1",
                Prix = 19.99M,
                Stock = 100,
                ImageURL = "http://localhost:5000/uploads/products/default.jpg",
                CategorieID = 1
            },
            new Product
            {
                Nom = "Product 2",
                Description = "Description for product 2",
                Prix = 29.99M,
                Stock = 50,
                ImageURL = "http://localhost:5000/uploads/products/default.jpg",
                CategorieID = 1
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}