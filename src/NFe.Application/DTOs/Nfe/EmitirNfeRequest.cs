using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NFe.Application.DTOs.NFe
{
    public class EmitirNfeRequest
    {
        [Required]
        public int CompanyId { get; set; }

        [Required]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ inválido")]
        public string DestinationCnpj { get; set; }

        [Required]
        public string DestinationName { get; set; }

        public string NatureOfOperation { get; set; } = "VENDA";
        public string OperationType { get; set; } = "1";

        [Required]
        [MinLength(1, ErrorMessage = "Deve conter pelo menos 1 item")]
        public List<NfeItemDto> Items { get; set; } = new();

        public string Series { get; set; } = "1";
        public string Notes { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now;
    }
}