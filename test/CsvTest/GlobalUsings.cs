global using Xunit;

//Disabled tests running in parallel.
//This is to be able to reuse tests code and not needing to modify it for each test. 
//Else the tests will run concurrently and use the sam database files
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]