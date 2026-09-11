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
    public void Stores_and_read_multiple_records_in_order()
    {   
       
    }
}