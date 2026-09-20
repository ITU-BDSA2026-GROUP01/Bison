namespace test;

using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Bison.CLI;
using SimpleDB;

public class UnitTests
{
    private readonly ITestOutputHelper output;

    public UnitTests(ITestOutputHelper output)
    {
        this.output = output;

        // Reset EVERYTHING before each test
        DbPaths.Reset();
        ResetDatabaseSingletons();
        Observations.Reset();
        Comments.Reset();
        Taxonomy.Reset();
    }

    private static void ResetDatabaseSingletons()
    {
        CSVDatabase<Cheep>.Reset();
        CSVDatabase<Comment>.Reset();
        CSVDatabase<Proposal>.Reset();
    }

    // ---------------------------
    //  ORIGINAL TESTS (cleaned)
    // ---------------------------

    [Fact]
    public void StoringInvalidRecord()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bison_test_{Guid.NewGuid():N}.csv");

        try
        {
            File.WriteAllText(path, "Id,Author,Message,Timestamp,Location\n");
            var database = CSVDatabase<Cheep>.GetInstance(path);

            database.Store(new Cheep());
            database.Store(new Cheep(0, null!, null!, 0, null!));

            var actual = database.Read().ToList();

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
        var ts = 1690891760L;
        var cheep = new Cheep(1, "alice", "Hello", ts, "DR Byen");

        var expectedLocal = DateTimeOffset.FromUnixTimeSeconds(ts).ToLocalTime();

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
    public async System.Threading.Tasks.Task CommentInvalidIdTest()
    {
        // Override DB paths with temp files
        var observeTemp = TempFile.Create("observe_test.csv", "Id,Author,Message,Timestamp,Location\n");
        var commentTemp = TempFile.Create("comment_test.csv", "ObservationId,Author,Message,Timestamp\n");

        DbPaths.Override("bison_observe_cli_db.csv", observeTemp.Path);
        DbPaths.Override("bison_comment_cli_db.csv", commentTemp.Path);

        // Add one valid observation
        Observations.AddObservation("Test obs", "Copenhagen");

        var args = new[] { "comment", "0", "test message" };

        var originalOut = Console.Out;
        var originalErr = Console.Error;
        using var captured = new StringWriter();
        Console.SetOut(captured);
        Console.SetError(captured);

        int exitCode;
        try
        {
            exitCode = await Program.Main(args);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }

        var output1 = captured.ToString();
        Assert.Contains("does not exist", output1);

        // Valid observation
        args = new[] { "comment", "1", "test message1" };

        using var captured2 = new StringWriter();
        Console.SetOut(captured2);
        Console.SetError(captured2);

        try
        {
            exitCode = await Program.Main(args);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
        }

        var output2 = captured2.ToString();
        Assert.DoesNotContain("does not exist", output2);
    }

    [Fact]
    public void Loads_taxa_and_links_parents_by_taxon_id()
    {
        var csv = """
        dwc:taxonID,dwc:parentNameUsageID,dwc:acceptedNameUsageID,dwc:taxonomicStatus,dwc:taxonRank,dwc:scientificName,dwc:scientificNameAuthorship,dcterms:language,dwc:vernacularName,clb:merged
        MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea,,,accepted,order,Pelecaniformes,,dan,Årefodede,false
        MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea,MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea,,accepted,family,Ardeidae,,dan,Hejrer,false
        MSTSNM:Arter:7f9ef9f3-f785-ea11-aa77-501ac539d1ea,MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea,,accepted,genus,Ardea,,,,
        MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea,MSTSNM:Arter:7f9ef9f3-f785-ea11-aa77-501ac539d1ea,,accepted,species,Ardea cinerea,"Linnaeus, 1758",dan,Fiskehejre,false
        """;

        var path = Path.GetTempFileName();

        try
        {
            File.WriteAllText(path, csv);

            var taxa = TaxonLoader.Load(path);

            Assert.True(taxa.ContainsKey("MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea"));
            Assert.True(taxa.ContainsKey("MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea"));

            var species = taxa["MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea"];
            Assert.Equal("Ardea", species.Parent?.ScientificName);
            Assert.Equal("Ardeidae", species.Parent?.Parent?.ScientificName);
            Assert.Equal("Pelecaniformes", species.Parent?.Parent?.Parent?.ScientificName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // ---------------------------
    //  PROPOSAL TESTS (clean)
    // ---------------------------

    [Fact]
    public void AddProposal_InvalidTaxonId_IsRejected()
    {
        var proposalTemp = TempFile.Create("proposal_test.csv",
            "ObservationId,Author,TaxonId,Timestamp\n");

        var observeTemp = TempFile.Create("observe_test.csv",
            "Id,Author,Message,Timestamp,Location\n");

        DbPaths.Override("bison_proposal_cli_db.csv", proposalTemp.Path);
        DbPaths.Override("bison_observe_cli_db.csv", observeTemp.Path);

        Observations.AddObservation("Saw a bird", "Copenhagen");

        Proposals.AddProposal(1, "INVALID_TAXON");

        var proposals = CSVDatabase<Proposal>.GetInstance(proposalTemp.Path).Read().ToList();
        Assert.Empty(proposals);
    }

    [Fact]
    public void AddProposal_ValidTaxonId_IsStored()
    {
        var proposalTemp = TempFile.Create("proposal_test.csv",
            "ObservationId,Author,TaxonId,Timestamp\n");

        var observeTemp = TempFile.Create("observe_test.csv",
            "Id,Author,Message,Timestamp,Location\n");

        DbPaths.Override("bison_proposal_cli_db.csv", proposalTemp.Path);
        DbPaths.Override("bison_observe_cli_db.csv", observeTemp.Path);

        Observations.AddObservation("Saw a bird", "Copenhagen");

        string validTaxon = "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea";

        Proposals.AddProposal(1, validTaxon);

        var proposals = CSVDatabase<Proposal>.GetInstance(proposalTemp.Path).Read().ToList();
        Assert.Single(proposals);
        Assert.Equal(validTaxon, proposals[0].TaxonId);
    }

    [Fact]
    public void ShowProposals_ReturnsOnlyMatchingObservation()
    {
        var proposalTemp = TempFile.Create("proposal_test.csv",
            "ObservationId,Author,TaxonId,Timestamp\n");

        var observeTemp = TempFile.Create("observe_test.csv",
            "Id,Author,Message,Timestamp,Location\n");

        DbPaths.Override("bison_proposal_cli_db.csv", proposalTemp.Path);
        DbPaths.Override("bison_observe_cli_db.csv", observeTemp.Path);

        Observations.AddObservation("Saw a bird", "Copenhagen");
        Observations.AddObservation("Saw a fox", "Aarhus");

        string taxon = "MSTSNM:Arter:c28811f4-f785-ea11-aa77-501ac539d1ea";

        Proposals.AddProposal(1, taxon);
        Proposals.AddProposal(2, taxon);

        var proposalsFor1 = CSVDatabase<Proposal>.GetInstance(proposalTemp.Path)
            .Read()
            .Where(p => p.ObservationId == 1)
            .ToList();

        Assert.Single(proposalsFor1);
        Assert.Equal(1, proposalsFor1[0].ObservationId);
    }
}
