using System;

namespace OvgRlp.EgvpEpFetcher.Services
{
  internal class DatetimeHelper
  {
    public static string ReplaceDatetimeTags(string str, DateTime date)
    {
      string rval = str;
      rval = rval.Replace("[yyyy]", date.ToString("yyyy"));
      rval = rval.Replace("[MM]", date.ToString("MM"));
      return rval;
    }
  }
}