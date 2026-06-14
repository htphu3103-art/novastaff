// BusinessLayers/Interfaces/ILeaveCalculator.cs
namespace NovaStaff.BusinessLayers.Interfaces;

public interface ILeaveCalculator
{
    double CalculateTotalDays(
    DateOnly from,
    DateOnly to,
    bool isHalfDayStart,
    bool isHalfDayEnd);
}