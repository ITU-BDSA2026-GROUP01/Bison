using SimpleDB;
using Bison.CLI;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var observationsDb = CSVDatabase<Cheep>.GetInstance("data/observations.csv");
var commentDb = CSVDatabase<Comment>.GetInstance("data/comments.csv");


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

app.Run();
