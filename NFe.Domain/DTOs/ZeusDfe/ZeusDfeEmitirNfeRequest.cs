namespace NFe.Domain.DTOs.ZeusDfe
{
    /// <summary>
    /// Requisição de emissão de NFe na API zeus.dfe
    /// </summary>
    public class ZeusDfeEmitirNfeRequest
    {
        /// <summary>
        /// CNPJ do emitente
        /// </summary>
        public string EmitterCnpj { get; set; }

        /// <summary>
        /// CNPJ do destinatário
        /// </summary>
        public string RecipientCnpj { get; set; }

        /// <summary>
        /// Razão social do destinatário
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Série da NF
        /// </summary>
        public string Series { get; set; }

        /// <summary>
        /// Número da NF
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Natureza da operação
        /// </summary>
        public string NatureOfOperation { get; set; }

        /// <summary>
        /// Tipo de operação (1-saída, 0-entrada)
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Itens da NF
        /// </summary>
        public List<ZeusDfeItemRequest> Items { get; set; } = new();

        /// <summary>
        /// Data de emissão (ISO 8601)
        /// </summary>
        public string IssueDate { get; set; }

        /// <summary>
        /// Observações
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Tipo de documento (NFe ou NFCe)
        /// </summary>
        public string DocumentType { get; set; } = "NFe";
    }

    /// <summary>
    /// Item de uma requisição de NF
    /// </summary>
    public class ZeusDfeItemRequest
    {
        /// <summary>
        /// Código do item
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Descrição do item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quantidade
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unidade de medida (UN, KG, etc)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Valor unitário
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Valor total (Quantity * UnitPrice)
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// ICMS (deixar 0 se isento)
        /// </summary>
        public decimal Icms { get; set; }

        /// <summary>
        /// IPI
        /// </summary>
        public decimal Ipi { get; set; }

        /// <summary>
        /// PIS
        /// </summary>
        public decimal Pis { get; set; }

        /// <summary>
        /// COFINS
        /// </summary>
        public decimal Cofins { get; set; }
    }
}
