// BusinessLayers/Interfaces/ILeaveCalculator.cs
namespace NovaStaff.BusinessLayers.Interfaces;

public interface ILeaveCalculator
{
    double CalculateTotalDays(
        DateTime from,
        DateTime to,
        bool isHalfDayStart,
        bool isHalfDayEnd);
}