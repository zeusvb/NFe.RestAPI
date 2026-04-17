using System;

namespace NFe.Application.DTOs.NFe
{
    public class ConsultarNfeResponse
    {
        public string ProtocolNumber { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime StatusDate { get; set; }
    }
}