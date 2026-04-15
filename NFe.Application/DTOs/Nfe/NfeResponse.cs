using System;

namespace NFe.Application.DTOs.NFe
{
    public class NfeResponse
    {
        public int Id { get; set; }
        public string NfeNumber { get; set; }
        public string Series { get; set; }
        public string ProtocolNumber { get; set; }
        public string Status { get; set; }
        public string AccessKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}