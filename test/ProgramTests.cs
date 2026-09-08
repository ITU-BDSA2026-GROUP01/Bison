namespace test;

using System;
using System.Collections.Generic;

public class ProgramTests
{
    [Fact]
    public void UNIXTimestamps()
    {
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Time = DateTimeOffset
                .FromUnixTimeSeconds(unixTime)
                .ToLocalTime();



        Assert.IsNotType<ToUnixTimeSeconds>(Time);
        Assert.IsType<DateTimeOffset>(Time);

    }
}