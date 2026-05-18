using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.Shared.Email;

public record EmailMessage(
    string To,
    string Subject,
    string HtmlBody
);
