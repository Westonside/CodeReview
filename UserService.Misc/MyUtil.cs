using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace UserService.Misc;

public static class MyUtil
{
    // from https://www.cnblogs.com/wz122889488/p/6897595.html
    /// <summary>
    ///     generate random str
    /// </summary>
    /// <param name="length">length</param>
    /// <param name="useNum">contains numbers or not</param>
    /// <param name="useLow">contains lower letters or not</param>
    /// <param name="useUpp">contains upper letters or not</param>
    /// <param name="useSpe">contains special characters or not</param>
    /// <param name="custom">DIY char list</param>
    /// <returns>random string</returns>
    public static string GetRandomString(int length, bool useNum = true, bool useLow = true, bool useUpp = true,
        bool useSpe = false, string custom = "")
    {
        var str = custom;
        if (useNum)
        {
            str += "0123456789";
        }

        if (useLow)
        {
            str += "abcdefghijklmnopqrstuvwxyz";
        }

        if (useUpp)
        {
            str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }

        if (useSpe)
        {
            str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        }

        StringBuilder sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            sb.Append(str.AsSpan(RandomNumberGenerator.GetInt32(0, str.Length - 1), 1));
        }

        return sb.ToString();
    }

    private static string UnderScorePascalCase(string str) =>
        string.Join("",
            str.Split('_')
               .Select(a => a.Length > 0
                   ? string.Concat(char.ToUpper(a[0], CultureInfo.InvariantCulture), a[1..])
                   : ""));

    /// <summary>
    ///     fix SQL parameter
    /// </summary>
    /// <param name="str">SQL field string</param>
    /// <returns>fixed SQL string</returns>
    public static string SqlParamFixer(string str)
    {
        List<string> arrayParams = new();
        str.Trim().Split(',').Where(s => s.Trim() != "").ToList()
           .ForEach(r => arrayParams.Add(r + " as " + UnderScorePascalCase(r)));
        return string.Join(',', arrayParams);
    }
}
