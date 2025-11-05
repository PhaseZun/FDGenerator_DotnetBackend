using Minio;
using Minio.DataModel.Args;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

namespace AuthApi.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minio;
        private readonly string _bucket;
        private readonly string _endpoint;

        public MinioService(IConfiguration config)
        {
            _endpoint = config["MinIO:Endpoint"] ?? throw new ArgumentNullException("MinIO:Endpoint");
            var accessKey = config["MinIO:AccessKey"] ?? throw new ArgumentNullException("MinIO:AccessKey");
            var secretKey = config["MinIO:SecretKey"] ?? throw new ArgumentNullException("MinIO:SecretKey");
            _bucket = config["MinIO:BucketName"] ?? throw new ArgumentNullException("MinIO:BucketName");

            bool useSsl = bool.TryParse(config["MinIO:UseSSL"], out var ssl) && ssl;

            _minio = new MinioClient()
                        .WithEndpoint(_endpoint)
                        .WithCredentials(accessKey, secretKey)
                        .WithSSL(useSsl)
                        .Build();
        }

        public async Task UploadPdfAsync(byte[] pdfData, string objectName)
        {
            bool found = await _minio.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucket)
            );

            if (!found)
            {
                await _minio.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucket)
                );
            }

            using var ms = new MemoryStream(pdfData);

            await _minio.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(objectName)
                    .WithStreamData(ms)
                    .WithObjectSize(ms.Length)
                    .WithContentType("application/pdf")
            );
        }
        public async Task<MemoryStream> DownloadPdfAsync(string objectName)
        {
         var ms = new MemoryStream();

         await _minio.GetObjectAsync(
         new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(ms);
            })
          );

          ms.Position = 0; // Reset stream position
          return ms;
        }


        public string GetPdfUrl(string objectName)
        {
            return $"http://{_endpoint}/{_bucket}/{objectName}";
        }
    }
}
