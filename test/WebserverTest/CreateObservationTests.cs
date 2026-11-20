using Bison.Core;
using Bison.Infrastructure.Data;
using Bison.Repositories;
using Bison.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class CreateObservationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BisonDBContext _context;
    private readonly PostRepository _repository;
    private readonly PostService _service;
    private readonly int _taxonId;

    public CreateObservationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<BisonDBContext>()
            .UseSqlite(_connection).Options;
        _context = new BisonDBContext(options);
        _context.Database.EnsureCreated();

        _context.Taxons.Add(new Taxon { VernacularName = "Bison" });
        _context.SaveChanges();
        _taxonId = _context.Taxons.First().TaxonId;

        _repository = new PostRepository(_context);
        _service = new PostService(_repository);
    }

    [Fact]
    public void CreatedObservation_AppearsInPublicList()
    {
        _service.CreateObservation("Eduard", "edka@itu.dk", "A bison!", _taxonId);

        var created = Assert.Single(_service.GetObservations());
        Assert.Equal("A bison!", created.Message);
        Assert.Equal("Eduard", created.Author);
        Assert.False(string.IsNullOrEmpty(created.Timestamp));
    }

    [Fact]
    public void CreatedObservation_AppearsInAuthorsOwnList()
    {
        _service.CreateObservation("Eduard", "edka@itu.dk", "A bison!", _taxonId);

        Assert.Single(_repository.GetObservationsByAuthor("Eduard"));
        Assert.Empty(_repository.GetObservationsByAuthor("Debasmita"));
    }

    [Fact]
    public void CreatedObservation_CanBeFoundByAuthorEmail()
    {
        _service.CreateObservation("Eduard", "edka@itu.dk", "A bison!", _taxonId);

        Assert.Single(_repository.GetObservationsByAuthor("edka@itu.dk"));
    }

    [Fact]
    public void SecondObservation_ReusesExistingAuthor()
    {
        _service.CreateObservation("Eduard", "edka@itu.dk", "One", _taxonId);
        _service.CreateObservation("Eduard", "edka@itu.dk", "Two", _taxonId);

        Assert.Equal(1, _context.Authors.Count());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyText_IsRejected(string text)
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateObservation("Eduard", "edka@itu.dk", text, _taxonId));
        Assert.Empty(_service.GetObservations());
    }

    [Fact]
    public void UnknownTaxon_IsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.CreateObservation("Eduard", "edka@itu.dk", "Hello", 99999));
    }

    [Fact]
    public void CreatedObservation_WithoutPicture_HasNoPicturePath()
    {
    var created = _service.CreateObservation("Eduard", "edka@itu.dk", "No picture", _taxonId);

    Assert.Null(created.PicturePath);
    } 

    [Fact]
    public void CreatedObservation_WithPicture_KeepsPicturePath()
    {
    _service.CreateObservation("Eduard", "edka@itu.dk", "With picture", _taxonId, "/uploads/heron.jpg");

    var listed = Assert.Single(_service.GetObservations());
    Assert.Equal("/uploads/heron.jpg", listed.PicturePath);
    }  

    [Fact]
    public void CreatedObservation_WithPicture_KeepsPictureLicense()
    {
    _service.CreateObservation("Eduard", "edka@itu.dk", "Licensed", _taxonId, "/uploads/heron.jpg", "CC BY-SA");

    var listed = Assert.Single(_service.GetObservations());
    Assert.Equal("/uploads/heron.jpg", listed.PicturePath);
    Assert.Equal("CC BY-SA", listed.PictureLicense);
    }

    [Fact]
    public void CreatedObservation_WithoutLicense_HasNoPictureLicense()
    {
    var created = _service.CreateObservation("Eduard", "edka@itu.dk", "No license", _taxonId, "/uploads/heron.jpg");

    Assert.Null(created.PictureLicense);
    }

    [Fact]
    public void PictureLicenses_OnlyAcceptsKnownCodes()
    {
    Assert.True(PictureLicenses.IsValid(null));
    Assert.True(PictureLicenses.IsValid(""));
    foreach (var (code, _) in PictureLicenses.Options)
        Assert.True(PictureLicenses.IsValid(code));
    Assert.False(PictureLicenses.IsValid("CC GPL-3.0"));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}