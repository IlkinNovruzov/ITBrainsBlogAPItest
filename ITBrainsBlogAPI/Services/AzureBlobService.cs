using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ITBrainsBlogAPI.Services
{
    public class AzureBlobService
    {
        BlobServiceClient _blobServiceClient;
        BlobContainerClient _blobContainerClient;
        string azureConnectionstring = "DefaultEndpointsProtocol=https;AccountName=itbblogstorage;AccountKey=wdvMxzent7Z96HWY859Jl6ZkECtplUOLh8h12/R9Nri2o3rjNm1DrhBuK+PyJAIdDUkqVUq+jru/+AStcWOzwA==;EndpointSuffix=core.windows.net";

        public AzureBlobService()
        {
            _blobServiceClient = new BlobServiceClient(azureConnectionstring);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient("itbcontainer");
        }

        public async Task<List<BlobContentInfo>> UploadFiles(List<IFormFile> files)
        {
            var azureResponse = new List<BlobContentInfo>();
            foreach (var file in files)
            {
                string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    var client = await _blobContainerClient.UploadBlobAsync(fileName, memoryStream, default);
                    azureResponse.Add(client);
                }
            }
            return azureResponse;
        }
        public async Task<string> UploadFile(IFormFile file)
        {
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var client = await _blobContainerClient.UploadBlobAsync(fileName, memoryStream, default);
                return fileName;
            }
        }
        public async Task<List<BlobItem>> GetUploadedBlob()
        {
            var items = new List<BlobItem>();
            var UploadedFiles =  _blobContainerClient.GetBlobsAsync();
            await foreach (var file in UploadedFiles)
            {
                items.Add(file);
            }
            return items;
        }
    }
}
