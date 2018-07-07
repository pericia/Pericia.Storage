﻿using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Pericia.FileStorage;

namespace Pericia.Storage.Aws
{
    public class AwsStorage : IFileStorage
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsStorage(string container, string accessKey, string secretKey)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _s3Client = new AmazonS3Client(credentials);

            _bucketName = container;
        }

        public Task<string> SaveFile(Stream fileData)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SaveFile(Stream fileData, string fileId)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(fileData, _bucketName, fileId);

            return fileId;
        }

        public async Task<Stream> GetFile(string fileId)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileId
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == "NoSuchKey")
                {
                    // Le fichier n'existe pas
                    return null;
                }

                throw;
            }
        }

        public Task DeleteFile(string fileId)
        {
            return _s3Client.DeleteObjectAsync(_bucketName, fileId);
        }
    }
}