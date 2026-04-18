using System;
using System.Threading.Tasks;

namespace NFe.Domain.Interfaces
{
    /// <summary>
    /// Interface para adapter ZeusFiscal que encapsula a emissão/consulta/cancelamento de NFe/NFCe
    /// </summary>
    public interface IZeusFiscalNfeService
    {
        /// <summary>
        /// Emite uma NFe usando a biblioteca ZeusFiscal
        /// </summary>
        /// <param name="emitterCnpj">CNPJ do emitente</param>
        /// <param name="xmlRequest">XML construído para transmissão</param>
        /// <returns>XML de retorno do SEFAZ (nfeProc)</returns>
        Task<string> EmitirNfeAsync(string emitterCnpj, string xmlRequest);

        /// <summary>
        /// Emite uma NFCe usando a biblioteca ZeusFiscal
        /// </summary>
        /// <param name="emitterCnpj">CNPJ do emitente</param>
        /// <param name="xmlRequest">XML construído para transmissão</param>
        /// <returns>XML de retorno do SEFAZ (nfeProc)</returns>
        Task<string> EmitirNfceAsync(string emitterCnpj, string xmlRequest);

        /// <summary>
        /// Consulta o status de uma NFe/NFCe pelo número de acesso
        /// </summary>
        /// <param name="emitterCnpj">CNPJ do emitente</param>
        /// <param name="accessKey">Chave de acesso da NF (44 dígitos)</param>
        /// <returns>XML de retorno da consulta</returns>
        Task<string> ConsultarNfeAsync(string emitterCnpj, string accessKey);

        /// <summary>
        /// Cancela uma NFe/NFCe mediante justificativa
        /// </summary>
        /// <param name="emitterCnpj">CNPJ do emitente</param>
        /// <param name="accessKey">Chave de acesso da NF (44 dígitos)</param>
        /// <param name="justification">Justificativa do cancelamento</param>
        /// <returns>XML de retorno do cancelamento</returns>
        Task<string> CancelarNfeAsync(string emitterCnpj, string accessKey, string justification);

        /// <summary>
        /// Verifica se a conexão com o SEFAZ está funcionando
        /// </summary>
        /// <returns>Status da conexão</returns>
        Task<bool> HealthCheckAsync();
    }
}
