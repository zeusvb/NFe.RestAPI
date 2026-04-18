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

        /// <summary>
        /// Indica sucesso da operação
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem de resposta (sucesso ou erro)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Conteúdo XML da NF
        /// </summary>
        public string XmlContent { get; set; }

        /// <summary>
        /// DANFE em base64
        /// </summary>
        public string DanfeBase64 { get; set; }

        /// <summary>
        /// Timestamp de processamento
        /// </summary>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// Erros (se houver)
        /// </summary>
        public dynamic Errors { get; set; }

        /// <summary>
        /// Warnings (se houver)
        /// </summary>
        public dynamic Warnings { get; set; }
    }
}
