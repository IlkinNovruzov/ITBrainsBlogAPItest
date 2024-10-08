﻿using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ITBrainsBlogAPI.Services
{
    public class FirebaseStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _bucketName = _configuration["Firebase:StorageBucket"];
            var credentialsPath = _configuration["Firebase:CredentialsPath"];

            // Kimlik doğrulama dosyasının yolunu ortam değişkeni olarak ayarla
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
    
                _storageClient = StorageClient.Create();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        { 
            if (file == null || !FileExtensions.IsImage(file))
            {
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }

            var objectName = Path.GetRandomFileName();
            var contentType = file.ContentType;
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await _storageClient.UploadObjectAsync(_bucketName, objectName, contentType, stream);
                }
            }
            catch (Exception ex)
            {
                // Hatanın detaylarını loglama veya yazdırma
                Console.Error.WriteLine($"Dosya yükleme hatası: {ex.Message}");
                throw;
            }
            var url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{objectName}?alt=media";
          //  var uxrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
            return url;
        }

    }
}
