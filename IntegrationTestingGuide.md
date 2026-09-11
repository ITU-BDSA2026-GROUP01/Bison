# Integration Testing Guide

## What an integration test is

An integration test checks that multiple parts of the application work together. In this project, the important integration boundary is:

`CSVDatabase<T> -> CSV file -> CSVDatabase<T>`

The test should use the real `CSVDatabase` and a real temporary file. Do not mock the file system or the repository when testing this boundary.

## The basic pattern

1. Create a temporary CSV file path.
2. Create a `CSVDatabase<T>` using that path.
3. Store one or more records.
4. Read the records back.
5. Assert the observable result.
6. Delete the temporary file in a `finally` block.

Example shape:

```csharp
[Fact]
public void StoresAndReadsARecord()
{
    var path = Path.GetTempFileName();

    try
    {
        var database = new CSVDatabase<Cheep>(path);
        var expected = new Cheep("alice", "Hello", 123);

        database.Store(expected);

        var actual = database.Read().Single();

        Assert.Equal(expected, actual);
    }
    finally
    {
        File.Delete(path);
    }
}
```

## Recommended test cases

### 1. Store and read one record

This verifies the complete round trip:

- a record can be written to the CSV file;
- the CSV mapping is correct;
- the record can be read back with the same values.

Use values that make mapping errors obvious, such as a non-empty author, message, and timestamp.

### 2. Store and read multiple records in order

This verifies that `Store` appends records instead of replacing the existing file contents:

```csharp
[Fact]
public void StoresAndReadsMultipleRecordsInOrder()
{
    var path = Path.GetTempFileName();

    try
    {
        var database = new CSVDatabase<Cheep>(path);
        var first = new Cheep("alice", "First", 1);
        var second = new Cheep("bob", "Second", 2);

        database.Store(first);
        database.Store(second);

        var records = database.Read().ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal(first, records[0]);
        Assert.Equal(second, records[1]);
    }
    finally
    {
        File.Delete(path);
    }
}
```

## What to assert

Prefer assertions about behavior that a caller can observe:

- the number of records returned;
- the values of each record;
- the order of appended records;
- whether data remains available after creating a new repository instance for the same file.

Avoid asserting implementation details such as private field names or the exact way `CsvHelper` is configured.

## Useful additional scenario

A good follow-up test is persistence across repository instances:

1. Create a repository and store a record.
2. Create a second repository using the same path.
3. Read from the second repository.
4. Assert that it receives the stored record.

This checks that the data is actually persisted in the file rather than only being available through one object instance.

## Test isolation rules

- Use a unique temporary path for every test.
- Always delete the temporary file in `finally`.
- Do not use the checked-in application CSV files in tests; tests could change their contents.
- Keep each test independent so it can run alone or in any order.
- Use deterministic timestamps and messages rather than the current time.

## Running the tests

From the repository root:

```powershell
dotnet test .\test\test.csproj
```

To run only integration tests, use a filter matching the test class or method name, for example:

```powershell
dotnet test .\test\test.csproj --filter FullyQualifiedName~IntegrationsTests
```

## Current implementation notes

- `CSVDatabase<Cheep>` registers the `SimpleDB.CheepMap`, which maps the CSV columns `Author`, `Observation`, and `Timestamp`.
- `Read(int? limit = null)` currently reads all records and does not apply the `limit` argument. A test for limiting results should be added only after that behavior is implemented or explicitly defined.
- The test project references both `SimpleDB` and `Bison.CLI`; use the `Cheep` type that matches the database mapping being tested.
