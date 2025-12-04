namespace Gov2Biz.DocumentService.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default);
        Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storagePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _storagePath = configuration["FileStorage:Path"] ?? "uploads";
            
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<string> SaveFileAsync(string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_storagePath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, content, cancellationToken);

            return filePath;
        }

        public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }

        public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
