namespace NovaStaff.DataLayers.Interfaces;

public interface ITimeZoneProvider
{
    DateTimeOffset LocalNow { get; }
    DateOnly TodayLocal { get; }
    DateTimeOffset ToLocal(DateTimeOffset utc);
}