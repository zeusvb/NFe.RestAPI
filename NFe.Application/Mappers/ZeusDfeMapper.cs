using NFe.Application.DTOs.NFe;
using NFe.Domain.DTOs.ZeusDfe;

namespace NFe.Application.Mappers
{
    /// <summary>
    /// Mapeador para converter DTOs de requisição para formato zeus.dfe
    /// </summary>
    public static class ZeusDfeMapper
    {
        /// <summary>
        /// Converte EmitirNfeRequest para ZeusDfeEmitirNfeRequest
        /// </summary>
        public static ZeusDfeEmitirNfeRequest MapToZeusDfeRequest(EmitirNfeRequest request, string emitterCnpj)
        {
            var zeusDfeRequest = new ZeusDfeEmitirNfeRequest
            {
                EmitterCnpj = emitterCnpj,
                RecipientCnpj = request.DestinationCnpj,
                RecipientName = request.DestinationName,
                Series = request.Series ?? "1",
                NatureOfOperation = request.NatureOfOperation ?? "VENDA",
                OperationType = request.OperationType ?? "1",
                IssueDate = request.IssueDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                Notes = request.Notes,
                DocumentType = "NFe",
                Items = request.Items.Select(MapToZeusDfeItem).ToList()
            };

            return zeusDfeRequest;
        }

        /// <summary>
        /// Converte NfeItemDto para ZeusDfeItemRequest
        /// </summary>
        private static ZeusDfeItemRequest MapToZeusDfeItem(NfeItemDto item)
        {
            var totalValue = item.Quantity * item.UnitValue;

            return new ZeusDfeItemRequest
            {
                Code = item.Code ?? Guid.NewGuid().ToString(),
                Description = item.Description,
                Quantity = item.Quantity,
                Unit = item.Unit ?? "UN",
                UnitPrice = item.UnitValue,
                TotalValue = totalValue,
                Icms = item.IcmsRate ?? 0,
                Ipi = 0, // Not implemented
                Pis = item.PisRate ?? 0,
                Cofins = item.CofinsRate ?? 0
            };
        }

        /// <summary>
        /// Converte ZeusDfeEmitirNfeResponse para NfeResponse
        /// </summary>
        public static NfeResponse MapFromZeusDfeResponse(ZeusDfeEmitirNfeResponse zeusDfeResponse)
        {
            return new NfeResponse
            {
                Success = zeusDfeResponse.Success,
                Message = zeusDfeResponse.Message,
                ProtocolNumber = zeusDfeResponse.ProtocolNumber,
                AccessKey = zeusDfeResponse.AccessKey,
                Status = zeusDfeResponse.Status,
                XmlContent = zeusDfeResponse.XmlContent,
                DanfeBase64 = zeusDfeResponse.DanfeBase64,
                ProcessedAt = zeusDfeResponse.ProcessedAt,
                Errors = zeusDfeResponse.Errors?.Select(e => new { e.Code, e.Message }).ToList() as dynamic,
                Warnings = zeusDfeResponse.Warnings?.Select(w => new { w.Code, w.Message }).ToList() as dynamic
            };
        }
    }
}
