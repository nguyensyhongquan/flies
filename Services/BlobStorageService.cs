using Azure.Storage.Blobs;

namespace FliesProject.Services
{
    public class BlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AzureStorage:ConnectionString");
            _containerName = "avater";//lưu trữ ảnh 
                                      //containerName có thể thay thế ="videocourse" (dùng để lưu trữ video )
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                // Ensure the container exists
                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(fileName);

                // Upload the file
                await blobClient.UploadAsync(fileStream, overwrite: true);

                // Return the URL to the uploaded file
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error uploading file to Azure Blob Storage", ex);
            }
        }
    }
}
