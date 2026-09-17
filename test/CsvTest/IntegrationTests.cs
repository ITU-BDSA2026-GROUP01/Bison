namespace test;

using System;
using System.Collections.Generic;
using SimpleDB;
using Bison.CLI;

public class IntegrationsTests
{
    [Fact]

    public void Recieved_and_stored_in_database()
    {   
        // Create a temporary CSV file with headers
        var path = Path.GetTempFileName();

        try
        { 
            // Write the CSV headers to the temporary file
            // Header must match the Cheep record's column names (Id, Author, Message, Timestamp).
            File.WriteAllText(path, "Id,Author,Message,Timestamp,Location\n");

            // Create a new CSVDatabase instance for Cheep records
            var database = CSVDatabase<Bison.CLI.Cheep>.GetInstance(path);
            var expected = new Bison.CLI.Cheep(2, "alice", "Hello", 123, "DR Byen");


            // Store the expected record in the database
            database.Store(expected);


            // Read the record back from the database and verify it matches the expected record
            var actual = database.Read().Single();


            // Assert that the actual record matches the expected record
            Assert.Equal(expected, actual);
        }
        finally
        {// Clean up the temporary file
            File.Delete(path);
        }
    }

    [Fact]
    // Test that multiple records can be stored and read in order
    public void Stores_and_read_multiple_records_in_order()
    {   // Create a temporary CSV file with headers
        var path = Path.GetTempFileName();

        try
        {
            // Write the CSV headers to the temporary file
            // Header must match the Cheep record's column names (Id, Author, Message, Timestamp).
            File.WriteAllText(path, "Id,Author,Message,Timestamp,Location\n");

            // Create a new CSVDatabase instance for Cheep records
            var database = CSVDatabase<Bison.CLI.Cheep>.GetInstance(path);

            // Create multiple Cheep records to store in the database
            var first = new Bison.CLI.Cheep(1, "alice", "Hello", 123, "Dr Byen");
            var second = new Bison.CLI.Cheep(2, "bob", "World", 456, "Amager");
            var third = new Bison.CLI.Cheep(3, "carol", "Again", 789, "DR Byen");

            // Store the records in the database
            database.Store(first);
            database.Store(second);
            database.Store(third);

            // Read the records back from the database and verify they match the expected records in order
            var actual = database.Read().ToList();

            // Assert that the actual records match the expected records in order
            Assert.Equal(new[] { first, second, third }, actual);
        }
        finally
        {
            // Clean up the temporary file
            File.Delete(path);
        }
    }


   [Fact] // Ny test der specifikt tester location

   public void FiltersObservationsByLocation() {

   var path = Path.GetTempFileName(); // laver midlertidig da det sikrer isolation. Ingen forurene vores faktisk data. 

   try 
    { 
        File.WriteAllText(path, "Id,Author,Message,Timestamp,Location\n");

        var database= CSVDatabase<Bison.CLI.Cheep>.GetInstance(path); // henter/opretter singleton af CSVdatabase

        var drByen = new Bison.CLI.Cheep(1, "alice", "Heron", 100, "DR Byen");
        var amager = new Bison.CLI.Cheep(2, "bob", "Sparrow", 200, "Amager");

        database.Store(drByen); // gemmer dem midlertidige CSV
        database.Store(amager);

            var filtered = database.Read().Where(c => c.Location == "DR Byen").ToList();
            
            Assert.Single(filtered); // præcis en record kom igennem filteret = fejlede
            Assert.Equal(drByen, filtered[0]); // Bekræfter den er rigtige record
        }    
        finally 
        {
        File.Delete(path);  // den midlertidig fil altid slettet efter testen.
        }
    }   
}