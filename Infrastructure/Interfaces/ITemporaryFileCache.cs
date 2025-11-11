using MisFinanzas.Infrastructure.Services;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ITemporaryFileCache
    {
        /// <summary>
        /// Guarda un archivo temporal en el cache
        /// </summary>
        /// <param name="fileContent">Contenido del archivo en bytes</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="contentType">Tipo MIME del archivo (ej: "application/pdf")</param>
        /// <returns>ID único del archivo guardado</returns>
        string StoreFile(byte[] fileContent, string fileName, string contentType);

        /// <summary>
        /// Obtiene un archivo temporal del cache
        /// </summary>
        /// <param name="fileId">ID del archivo</param>
        /// <returns>Objeto CachedFile o null si no existe/expiró</returns>
        TemporaryFileCache.CachedFile? GetFile(string fileId);

        /// <summary>
        /// Elimina un archivo temporal del cache
        /// </summary>
        /// <param name="fileId">ID del archivo</param>
        void RemoveFile(string fileId);
    }
}