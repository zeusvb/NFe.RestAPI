namespace NFe.Domain.DTOs.ZeusDfe
{
    /// <summary>
    /// Resposta da API zeus.dfe ao emitir uma NF
    /// </summary>
    public class ZeusDfeEmitirNfeResponse
    {
        /// <summary>
        /// Indicador de sucesso
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem de resposta
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Número do protocolo de autorização
        /// </summary>
        public string ProtocolNumber { get; set; }

        /// <summary>
        /// Chave de acesso da NF
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// XML da NF assinado
        /// </summary>
        public string XmlContent { get; set; }

        /// <summary>
        /// DANFE (PDF) em base64
        /// </summary>
        public string DanfeBase64 { get; set; }

        /// <summary>
        /// Status da NF (autorizada, rejeitada, etc)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Erros de validação (se houver)
        /// </summary>
        public List<ZeusDfeError> Errors { get; set; } = new();

        /// <summary>
        /// Avisos (warnings)
        /// </summary>
        public List<ZeusDfeWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Timestamp de processamento
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Erro retornado pela API
    /// </summary>
    public class ZeusDfeError
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Aviso retornado pela API
    /// </summary>
    public class ZeusDfeWarning
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
