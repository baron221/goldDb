namespace GoldbApi.Utils;

// Date-range filters (StartDate/EndDate) arrive as bare "yyyy-MM-dd" values meaning a
// calendar day in KST (Asia/Seoul) - the app's operating timezone (see KstDateTimeConverter
// in Program.cs, used for outgoing dates). CreatedAt columns are stored as true UTC
// (DateTime.UtcNow). Comparing a bare date directly against UTC by just relabeling its
// Kind as Utc skips the actual 9-hour shift, so a "today" filter silently leaks in
// several hours of the following KST day. These helpers do the real conversion once,
// instead of every call site relabeling the Kind and getting it wrong the same way.
public static class DateRangeUtils
{
    private static readonly TimeZoneInfo KstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    // Inclusive lower bound: the instant KST midnight of the given date occurs, in UTC.
    public static DateTime ToUtcRangeStart(DateTime date)
    {
        var kstMidnight = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(kstMidnight, KstZone);
    }

    // Exclusive upper bound: the instant KST midnight of the NEXT date occurs, in UTC -
    // use with `< bound` so the entire given KST day is included.
    public static DateTime ToUtcRangeEndExclusive(DateTime date)
    {
        var nextKstMidnight = DateTime.SpecifyKind(date.Date.AddDays(1), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(nextKstMidnight, KstZone);
    }
}
