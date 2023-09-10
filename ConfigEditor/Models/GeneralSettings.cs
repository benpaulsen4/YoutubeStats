#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigEditor.Models
{
    public record GeneralSettings
    {
        public string? ApiKey { get; set; }
        public string? ReportType { get; set; }
    }
}
