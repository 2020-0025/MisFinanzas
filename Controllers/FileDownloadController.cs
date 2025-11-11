using Microsoft.AspNetCore.Mvc;
using MisFinanzas.Infrastructure.Services;

namespace MisFinanzas.Controllers 
{
    [ApiController]

    [Route("api/[controller]")]

    public class FileDownloadController : ControllerBase

    {

        private readonly TemporaryFileCache _fileCache;
        public FileDownloadController(TemporaryFileCache fileCache)

        {

            _fileCache = fileCache;

        }

        [HttpGet("{fileId}")]

        public IActionResult DownloadFile(string fileId)

        {

            var cachedFile = _fileCache.GetFile(fileId);

            if (cachedFile == null)

            {

                return NotFound(new { message = "Archivo no encontrado o expirado" });

            }

            // Eliminar del caché después de obtenerlo (descarga única)

            _fileCache.RemoveFile(fileId);

            // Devolver el archivo

            return File(cachedFile.Content, cachedFile.ContentType, cachedFile.FileName);

        }

    }

}



