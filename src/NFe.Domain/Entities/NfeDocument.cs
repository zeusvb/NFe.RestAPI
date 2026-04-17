using System;
using System.Collections.Generic;

namespace NFe.Domain.Entities
{
    public class NfeDocument
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string NfeNumber { get; set; }
        public string Series { get; set; }
        public string ProtocolNumber { get; set; }
        public string AccessKey { get; set; }
        public string Status { get; set; }
        public string XmlContent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Company Company { get; set; }
        public ICollection<NfeEvent> Events { get; set; } = new List<NfeEvent>();
    }
}