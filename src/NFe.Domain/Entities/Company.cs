using System;
using System.Collections.Generic;

namespace NFe.Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Cnpj { get; set; }
        public string CompanyName { get; set; }
        public string FancyName { get; set; }
        public string CertificateThumbprint { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<NfeDocument> NfeDocuments { get; set; } = new List<NfeDocument>();
    }
}