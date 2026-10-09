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

        var taxon = new Taxon
        {
            DwcTaxonId = "MSTSNM:Arter:c28811f4",
            DanishVernacularName = "Fiskehejre"
        };

        context.Taxons.Add(taxon);
       

        var observation = new Observation
        {
            Text = "Observed a fiskehejre near the lake.",
            TimeStamp = DateTime.UtcNow,
            Author = author,
            Taxon = taxon
        };



        context.Observations.Add(observation);

        

        await context.SaveChangesAsync();
    }
}