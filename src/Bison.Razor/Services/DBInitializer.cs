using Bison.Razor.Data;
using Bison.Razor.Models;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Services;

public static class DBInitializer
{
    public static async Task SeedAsync(BisonDBContext context)
    {
        if (await context.Authors.AnyAsync())
        {
            return;
        }

        var author = new Author
        {
            Name = "Tim",
            Email = "tim@example.com"
        };

        context.Authors.Add(author);

        await context.SaveChangesAsync();
    }
}