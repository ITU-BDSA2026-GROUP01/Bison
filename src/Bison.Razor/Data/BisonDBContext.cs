using Bison.Razor.Models;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Data;

public class BisonDBContext : DbContext
{
    public BisonDBContext(DbContextOptions<BisonDBContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Taxon> Taxons => Set<Taxon>();
}