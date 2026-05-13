namespace NovaStaff.Models.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, 404)
    {
    }
}