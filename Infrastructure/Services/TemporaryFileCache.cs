using System.Collections.Concurrent;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services

{
    public class TemporaryFileCache : ITemporaryFileCache

    {
        private readonly ConcurrentDictionary<string, CachedFile> _cache = new();

        private readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(5);

        public string StoreFile(byte[] fileContent, string fileName, string contentType)

        {

            var fileId = Guid.NewGuid().ToString("N");

            var cachedFile = new CachedFile

            {

                Content = fileContent,

                FileName = fileName,

                ContentType = contentType,

                ExpiresAt = DateTime.UtcNow.Add(_expirationTime)

            };

            _cache[fileId] = cachedFile;

            // Limpiar archivos expirados

            CleanExpiredFiles();

            return fileId;

        }

        public CachedFile? GetFile(string fileId)

        {

            if (_cache.TryGetValue(fileId, out var file))

            {

                if (DateTime.UtcNow <= file.ExpiresAt)

                {

                    return file;

                }

                // Archivo expirado, eliminarlo

                _cache.TryRemove(fileId, out _);

            }

            return null;

        }

        public void RemoveFile(string fileId)

        {

            _cache.TryRemove(fileId, out _);

        }

        private void CleanExpiredFiles()

        {
            var expiredKeys = _cache

                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiresAt)

                .Select(kvp => kvp.Key)

                .ToList();

            foreach (var key in expiredKeys)

            {

                _cache.TryRemove(key, out _);

            }
        }

        public class CachedFile

        {
            public byte[] Content { get; set; } = Array.Empty<byte>();

            public string FileName { get; set; } = string.Empty;

            public string ContentType { get; set; } = string.Empty;

            public DateTime ExpiresAt { get; set; }

        }
    }




}


