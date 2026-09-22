using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Bison.CLI;
using BisonTest;
using SimpleDB;
using Xunit;

public class UnitTests
{
    [Fact]
    public void StoringInvalidRecord()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"bison_test_{Guid.NewGuid():N}.csv");

        try
        {
            // Given: a database file that already has its header row.
            // Header must match the Cheep record's column names.
            File.WriteAllText(
                path,
                "Id,Author,Message,Timestamp,Location\n");

            var database =
                CSVDatabase<Bison.CLI.Cheep>.GetInstance(path);

            // When: an empty record and an invalid (null-bearing) record are stored.
            database.Store(new Bison.CLI.Cheep());
            database.Store(
                new Bison.CLI.Cheep(
                    0,
                    null!,
                    null!,
                    0,
                    null!));

            // Then: both are persisted and read back.
            var actual = database.Read().ToList();

            Console.WriteLine(actual);

            Assert.Equal(2, actual.Count);

            var empty = actual[0];

            Assert.Equal(string.Empty, empty.Author);
            Assert.Equal(string.Empty, empty.Message);
            Assert.Equal(0, empty.Timestamp);

            var invalid = actual[1];

            Assert.Equal(string.Empty, invalid.Author);
            Assert.Equal(string.Empty, invalid.Message);
            Assert.Equal(0, invalid.Timestamp);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void UnixTimestampTest()
    {
        // Given: a cheep with a fixed, known unix timestamp.
        // 1690891760 == 2023-08-01 12:09:20 UTC.
        var ts = 1690891760L;

        var cheep = new Bison.CLI.Cheep(
            1,
            "alice",
            "Hello",
            ts,
            "DR Byen");

        var expectedLocal = DateTimeOffset
            .FromUnixTimeSeconds(ts)
            .ToLocalTime();

        var originalOut = Console.Out;

        using var captured = new StringWriter();

        Console.SetOut(captured);

        try
        {
            UserInterface.PrintObservations(new[] { cheep });
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = captured.ToString();
        Assert.Contains(expectedLocal.ToString(), output);
    }

    [Fact]
    public async Task CommentInvalidIdTest()
    {
        var fake = new FakeHttpService();

        await Comments.Comment(
            0,
            "test message",
            fake);

        Assert.Empty(fake.Comments);
    }

    [Fact]
    public void Loads_taxa_and_links_parents_by_taxon_id()
    {
        var csv = """
        dwc:taxonID,dwc:parentNameUsageID,dwc:acceptedNameUsageID,dwc:taxonomicStatus,dwc:taxonRank,dwc:scientificName,dwc:scientificNameAuthorship,dcterms:language,dwc:vernacularName,clb:merged
        MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea,,,accepted,order,Pelecaniformes,,dan,Ã…refodede,false
        MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea,MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea,,accepted,family,Ardeidae,,dan,Hejrer,false
        MSTSNM:Arter:7f9ef9f3-f785-ea11-aa77-501ac539d1ea,MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea,,accepted,genus,Ardea,,,,
        MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea,MSTSNM:Arter:7f9ef9f3-f785-ea11-aa77-501ac539d1ea,,accepted,species,Ardea cinerea,"Linnaeus, 1758",dan,Fiskehejre,false
        """;

        var path = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path, csv);

            var taxa = TaxonLoader.Load(path);

            Assert.True(
                taxa.ContainsKey(
                    "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea"));

            Assert.True(
                taxa.ContainsKey(
                    "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea"));

            var species =
                taxa["MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea"];

            Assert.Equal(
                "Ardea",
                species.Parent?.ScientificName);

            Assert.Equal(
                "Ardeidae",
                species.Parent?.Parent?.ScientificName);

            Assert.Equal(
                "Pelecaniformes",
                species.Parent?.Parent?.Parent?.ScientificName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Observe_Creates_Next_Id()
    {
        var fake = new FakeHttpService();

        fake.Cheeps.Add(
            new Cheep(
                5,
                "alice",
                "Hello",
                123,
                "DR Byen"));

        await Observations.Observe(
            "New message",
            "DR Byen",
            fake);

        Assert.Equal(2, fake.Cheeps.Count);
        Assert.Equal(6, fake.Cheeps.Last().Id);
    }

    [Fact]
    public async Task Comment_Creates_Comment()
    {
        var fake = new FakeHttpService();

        fake.Cheeps.Add(
            new Cheep(
                1,
                "alice",
                "Hello",
                123,
                "DR Byen"));

        await Comments.Comment(
            1,
            "This is a comment",
            fake);
        Assert.Single(fake.Comments);
        Assert.Equal(1, fake.Comments[0].ObservationId);
        Assert.Equal(
            "This is a comment",
            fake.Comments[0].Message);
    }

    [Fact]
    public async Task Discussion_Uses_Correct_Observation()
    {
        var fake = new FakeHttpService();

        fake.Cheeps.Add(
            new Cheep(
                1,
                "alice",
                "Hello",
                123,
                "DR Byen"));

        fake.Cheeps.Add(
            new Cheep(
                2,
                "bob",
                "Another observation",
                456,
                "Copenhagen"));

        fake.Comments.Add(
            new Comment(
                1,
                "bob",
                "Nice observation!",
                789));

        await Comments.Discussion(
            1,
            fake);

        var comments =
            await fake.GetCommentsAsync(1);

        Assert.Single(comments);

        Assert.Equal(
            1,
            comments[0].ObservationId);

        Assert.Equal(
            "Nice observation!",
            comments[0].Message);
    }


    // ---------------------------
    //  PROPOSAL TESTS (clean)
    // ---------------------------


    private const string ValidTaxon =
    "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea"; // Ardea cinerea - valid taxon ID for testing, copy from the taxonomy CSV used in the test above.

    private static void SeedTaxonomy()
    {
        Taxonomy.Reset();
        Taxonomy.Lookup[ValidTaxon] = new Taxon { TaxonId = ValidTaxon };
    }

    [Fact]
    public async Task AddProposal_InvalidTaxonId_IsRejected()
    {
        SeedTaxonomy();
        var fake = new FakeHttpService();
        fake.Cheeps.Add(new Cheep(1, "alice", "Saw a bird", 123, "Copenhagen"));

        await Proposals.AddProposal(1, "INVALID_TAXON", fake);

        Assert.Empty(fake.Proposals);
    }

    [Fact]
    public async Task AddProposal_ValidTaxonId_IsStored()
    {
        SeedTaxonomy();
        var fake = new FakeHttpService();
        fake.Cheeps.Add(new Cheep(1, "alice", "Saw a bird", 123, "Copenhagen"));

        await Proposals.AddProposal(1, ValidTaxon, fake);

        Assert.Single(fake.Proposals);
        Assert.Equal(ValidTaxon, fake.Proposals[0].TaxonId);
        Assert.Equal(1, fake.Proposals[0].ObservationId);
    }

    [Fact]
    public async Task ShowProposals_ReturnsOnlyMatchingObservation()
    {
        SeedTaxonomy();
        var fake = new FakeHttpService();

        fake.Cheeps.Add(new Cheep(1, "alice", "Saw a bird", 123, "Copenhagen"));
        fake.Cheeps.Add(new Cheep(2, "bob", "Saw a fox", 456, "Aarhus"));

        await Proposals.AddProposal(1, ValidTaxon, fake);
        await Proposals.AddProposal(2, ValidTaxon, fake);

        var proposalsFor1 = fake.Proposals.Where(p => p.ObservationId == 1).ToList();

        Assert.Single(proposalsFor1);
        Assert.Equal(1, proposalsFor1[0].ObservationId);
    }
}
