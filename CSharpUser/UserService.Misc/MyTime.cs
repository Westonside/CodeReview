namespace UserService.Misc;

public static class MyTime
{
    private static readonly DateTime s_timeStampBeginning = new(1970, 1, 1, 0, 0, 0, 0);


    public static ulong GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - s_timeStampBeginning;
        return Convert.ToUInt64(ts.TotalMilliseconds);
    }


    public static uint GetTimeStamp32()
    {
        TimeSpan ts = DateTime.UtcNow - s_timeStampBeginning;
        return Convert.ToUInt32(ts.TotalSeconds);
    }
}
