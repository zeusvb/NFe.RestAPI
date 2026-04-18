namespace NFe.Domain.DTOs.ZeusDfe
{
    /// <summary>
    /// Requisição de consulta de NF na API zeus.dfe
    /// </summary>
    public class ZeusDfeConsultarRequest
    {
        /// <summary>
        /// Chave de acesso da NF
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// CNPJ do emitente (para validação)
        /// </summary>
        public string EmitterCnpj { get; set; }
    }

    /// <summary>
    /// Resposta de consulta de NF
    /// </summary>
    public class ZeusDfeConsultarResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string ProtocolNumber { get; set; }
        public string AccessKey { get; set; }
        public string XmlContent { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<ZeusDfeError> Errors { get; set; } = new();
    }

    /// <summary>
    /// Requisição de cancelamento de NF
    /// </summary>
    public class ZeusDfeCancelarRequest
    {
        /// <summary>
        /// Chave de acesso da NF
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// CNPJ do emitente
        /// </summary>
        public string EmitterCnpj { get; set; }

        /// <summary>
        /// Justificativa do cancelamento
        /// </summary>
        public string Justification { get; set; }
    }

    /// <summary>
    /// Resposta de cancelamento de NF
    /// </summary>
    public class ZeusDfeCancelarResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string ProtocolNumber { get; set; }
        public string AccessKey { get; set; }
        public DateTime CancelledAt { get; set; }
        public List<ZeusDfeError> Errors { get; set; } = new();
    }
}
