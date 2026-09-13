namespace test;

using System;
using System.Collections.Generic;

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
            File.WriteAllText(path, "Author,Observation,Timestamp\n");

            // Create a new CSVDatabase instance for Cheep records
            var database = new SimpleDB.CSVDatabase<SimpleDB.Cheep>(path);
            var expected = new SimpleDB.Cheep("alice", "Hello", 123);


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
            File.WriteAllText(path, "Author,Observation,Timestamp\n");

            // Create a new CSVDatabase instance for Cheep records
            var database = new SimpleDB.CSVDatabase<SimpleDB.Cheep>(path);

            // Create multiple Cheep records to store in the database
            var first = new SimpleDB.Cheep("alice", "Hello", 123);
            var second = new SimpleDB.Cheep("bob", "World", 456);
            var third = new SimpleDB.Cheep("carol", "Again", 789);

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
}