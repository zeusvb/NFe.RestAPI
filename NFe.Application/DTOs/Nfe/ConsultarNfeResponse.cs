using System;

namespace NFe.Application.DTOs.NFe
{
    public class ConsultarNfeResponse
    {
        public bool Success { get; set; }
        public string ProtocolNumber { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string AccessKey { get; set; }
        public string XmlContent { get; set; }
        public DateTime StatusDate { get; set; }
    }
}
