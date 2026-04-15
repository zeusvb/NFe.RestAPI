using System.ComponentModel.DataAnnotations;

namespace NFe.Application.DTOs.NFe
{
    public class NfeItemDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitValue { get; set; }

        public decimal TotalValue => Quantity * UnitValue;

        public string Unit { get; set; } = "UN";

        public decimal? IcmsRate { get; set; }
        public decimal? PisRate { get; set; }
        public decimal? CofinsRate { get; set; }
    }
}