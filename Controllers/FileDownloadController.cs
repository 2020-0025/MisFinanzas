using Microsoft.AspNetCore.Mvc;
using MisFinanzas.Infrastructure.Services;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Controllers 
{
    [ApiController]

    [Route("api/[controller]")]

    public class FileDownloadController : ControllerBase

    {

        private readonly ITemporaryFileCache _fileCache;
        public FileDownloadController(ITemporaryFileCache fileCache)

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



