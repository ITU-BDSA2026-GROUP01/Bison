using SimpleDB;
using Bison.CLI;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var observationsDb = CSVDatabase<Cheep>.GetInstance("data/observations.csv");
var commentDb = CSVDatabase<Comment>.GetInstance("data/comments.csv");
var proposalDb = CSVDatabase<Proposal>.GetInstance("data/proposals.csv");

app.MapPost ("/observation", (Cheep observation) => 
{
    observationsDb.Store(observation);
    return Results.Created("/observations", observation);
});

app.MapGet ("/observations", () => observationsDb.Read());

app.MapPost("/comment", (Comment comment) =>
{
    commentDb.Store(comment);
    return Results.Created("/comments", comment);    

});

app.MapGet("/comments", (long id) =>
    commentDb.Read().Where(c => c.ObservationId == id
    
));

app.MapPost("/proposal", (Proposal proposal) =>
{
    // Validate observation exists
    if (!observationsDb.Read().Any(o => o.Id == proposal.ObservationId))
        return Results.BadRequest($"Observation {proposal.ObservationId} does not exist.");

    // Validate taxon exists
    if (!Taxonomy.Exists(proposal.TaxonId))
        return Results.BadRequest($"Taxon ID {proposal.TaxonId} is invalid.");

    proposalDb.Store(proposal);
    return Results.Created("/proposals", proposal);
});

app.MapGet("/proposals", (long id) =>
    proposalDb.Read().Where(p => p.ObservationId == id)
);


app.Run();
