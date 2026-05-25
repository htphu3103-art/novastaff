using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Models.DTOs.Auth
{
    public record LoginResponse(
        string AccessToken,
        string RefreshToken
);
}
