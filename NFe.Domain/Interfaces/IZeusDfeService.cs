using NFe.Domain.DTOs.ZeusDfe;

namespace NFe.Domain.Interfaces
{
    /// <summary>
    /// Interface para integração com API zeus.dfe
    /// </summary>
    public interface IZeusDfeService
    {
        /// <summary>
        /// Emitir uma NFe na API zeus.dfe
        /// </summary>
        Task<ZeusDfeEmitirNfeResponse> EmitirNfeAsync(ZeusDfeEmitirNfeRequest request);

        /// <summary>
        /// Emitir uma NFCe na API zeus.dfe
        /// </summary>
        Task<ZeusDfeEmitirNfeResponse> EmitirNfceAsync(ZeusDfeEmitirNfeRequest request);

        /// <summary>
        /// Consultar status de uma NF
        /// </summary>
        Task<ZeusDfeConsultarResponse> ConsultarAsync(ZeusDfeConsultarRequest request);

        /// <summary>
        /// Cancelar uma NF autorizada
        /// </summary>
        Task<ZeusDfeCancelarResponse> CancelarAsync(ZeusDfeCancelarRequest request);

        /// <summary>
        /// Verificar conectividade com API zeus.dfe
        /// </summary>
        Task<bool> HealthCheckAsync();
    }
}