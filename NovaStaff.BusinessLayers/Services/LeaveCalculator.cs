// BusinessLayers/Services/LeaveCalculator.cs
using NovaStaff.BusinessLayers.Interfaces;

namespace NovaStaff.BusinessLayers.Services;

public class LeaveCalculator : ILeaveCalculator
{
    public double CalculateTotalDays(
        DateTime from,
        DateTime to,
        bool isHalfDayStart,
        bool isHalfDayEnd)
    {
        if (to < from)
            throw new ArgumentException("ToDate phải >= FromDate");

        // Tổng số ngày (inclusive)
        var totalDays = (to.Date - from.Date).TotalDays + 1;

        if (isHalfDayStart) totalDays -= 0.5;
        if (isHalfDayEnd) totalDays -= 0.5;

        return totalDays;
    }
}