// BusinessLayers/Services/LeaveCalculator.cs
using NovaStaff.BusinessLayers.Interfaces;

namespace NovaStaff.BusinessLayers.Services;

public class LeaveCalculator : ILeaveCalculator
{
    public double CalculateTotalDays(
    DateOnly from,
    DateOnly to,
    bool isHalfDayStart,
    bool isHalfDayEnd)
    {
        if (to < from)
            throw new ArgumentException("ToDate phải >= FromDate");

        // Tổng số ngày (inclusive)
        double totalDays = (to.DayNumber - from.DayNumber) + 1;

        if (isHalfDayStart)
            totalDays -= 0.5;

        if (isHalfDayEnd)
            totalDays -= 0.5;

        return totalDays;
    }
}