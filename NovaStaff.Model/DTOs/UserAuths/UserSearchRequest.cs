namespace NovaStaff.Model.DTOs.UserAuths
{
    public class UserSearchRequest
    {
        public string? SearchValue { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
