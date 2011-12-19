using System;
using System.Collections.Generic;
using System.Linq;

namespace Ogdi.Data.DataLoader
{
    [Serializable]
    public class TimeZoneNotFoundException : Exception
    {
        public TimeZoneNotFoundException(String timeZoneName)
            : base(ConstructMessage(timeZoneName))
        {
        }

        private static string ConstructMessage(String timeZoneName)
        {
            string text = "TimeZone '" + timeZoneName + "' not found. Use one of:" + Environment.NewLine;
            List<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones().ToList();

            zones.Sort((z1, z2) => z1.BaseUtcOffset.Hours.CompareTo(z2.BaseUtcOffset.Hours));

            return zones.Aggregate(text, (current, t) => current + (t.DisplayName.Replace("&", "&amp;") + Environment.NewLine));
        }
    }
}