using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CallXApi.ViewModels;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.ObjectPool;

namespace CallXApi.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        string accessKey = string.Empty;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlobService(BlobServiceClient blobServiceClient, IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _blobServiceClient = blobServiceClient;
            _webHostEnvironment = webHostEnvironment;
            this.accessKey = config.GetValue<string>("AzureBlobStorage");
        }

        public static async Task UploadFromStreamAsync(BlobContainerClient containerClient, string localFilePath)
        {
            string fileName = Path.GetFileName(localFilePath);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            FileStream fileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(fileStream, true);
            fileStream.Close();
        }

        private BlobContainerClient GetContainerClient(string blobContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);
            return containerClient;
        }

        //public void Delete()
        //{
        //    FileNameList = fileName.Split(',').Where(t => t.ToString().Trim() != "").ToList();
        //}


        public string DownloadBlobPath(string contName, string fileName)
        {
            var containerClient = GetContainerClient(contName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.AbsoluteUri;

        }

        public long Lengther(long length)
        {
            return Convert.ToInt64(Math.Round(length / 1024.0));
        }

        public async Task<string> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName)
        {
            BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            //var storageAccount = CloudStorageAccount.Parse(accessKey);
            //var bloClient = storageAccount.CreateCloudBlobClient();
            // var serviceProperties = await bloClient.GetSer vicePropertiesAsync();
            // serviceProperties.DefaultServiceVersion = "2020-12-06";
            // await bloClient.SetServicePropertiesAsync(serviceProperties);

            //BlobContainerClient container = await bloClient.CreateBlobContainerAsync(blobContainerName, PublicAccessType.BlobContainer);

            // if (await container.ExistsAsync())
            // {
            //     Console.WriteLine("Created container {0}", container.Name);
            //     return container;
            // }

            var containerClient = bloClient.GetBlobContainerClient(blobContainerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.AbsoluteUri;
        }

        public BlobServiceClient GetBlobServiceClient(string accountName)
        {
            BlobServiceClient client = new(new Uri($"https://{accountName}.blob.core.windows.net"), new DefaultAzureCredential());
            return client;
        }

        public async Task<DownReturn> DownloadBlob(string contName, string fileName)
        {

            BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            var containerClient = bloClient.GetBlobContainerClient(contName);
            var blobClient = containerClient.GetBlobClient(fileName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            Stream blobStream = null;

            using (FileStream fs = File.OpenWrite(blobClient.Uri.AbsoluteUri))
            {
                await download.Content.CopyToAsync(fs);
                blobStream = fs;
                fs.Close();
            }


            return new DownReturn { stream = blobStream, contentType = blobClient.GetProperties().Value.ContentType, name = blobClient.Name };
        }

        public async Task<DownReturn> DownloadBlobFromUrl(string blobUrl)
        {

            BlobServiceClient bloClient = new BlobServiceClient(accessKey);

            Uri uri = new Uri(blobUrl);
            string containerName = uri.Segments[1].TrimEnd('/');
            string blobName = string.Join("", uri.Segments, 2, uri.Segments.Length - 2);

            var containerClient = bloClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            Stream blobStream = download.Content;

            // using (FileStream fs = File.OpenWrite(blobClient.Uri.AbsoluteUri))
            // {
            //     await download.Content.CopyToAsync(fs);
            //     blobStream = fs;
            //     fs.Close();
            // }
            return new DownReturn { stream = blobStream, contentType = blobClient.GetProperties().Value.ContentType, name = blobClient.Name };
        }

        //YEAR 10A
        //YEAR 7A
        //YEAR 7N
        //YEAR 8A
        //YEAR 9A
        //YEAR 9N
        //YEAR 10N



        public async Task<int> PropFolder(string blobContainerName)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "yr8n");
            BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
            await containerClient.CreateIfNotExistsAsync();

            // string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
            string[] pdfFiles = Directory.GetFiles(folderPath, "*.png");
            var pngFiles = Directory
    .EnumerateFiles(folderPath)
    .Where(file => Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase))
    .ToArray();

            foreach (var filePath in pdfFiles)
            {
                string fileName = Path.GetFileName(filePath);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                Console.WriteLine($"Uploading {fileName}...");
                await blobClient.UploadAsync(filePath, overwrite: true);
            }
            return 1;
        }



        public async Task<List<KeyValuePair<string, string>>> PopulateStudentPassport()
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "yr8n");
            // BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            // BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
            // await containerClient.CreateIfNotExistsAsync();

            // string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
            string[] pdfFiles = Directory.GetFiles(folderPath, "*.png");
            var kvplist = new List<KeyValuePair<string, string>>();
            //         var pngFiles = Directory
            // .EnumerateFiles(folderPath)
            // .Where(file => Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase))
            // .ToArray();

            foreach (var filePath in pdfFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string withoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string convertedAdmissionNo = withoutExtension.Replace('_', '/');
                kvplist.Add(new KeyValuePair<string, string>(convertedAdmissionNo, fileName));

                //BlobClient blobClient = containerClient.GetBlobClient(fileName);

                Console.WriteLine($"Uploading {fileName}...");
                //await blobClient.UploadAsync(filePath, overwrite: true);
            }
            return kvplist;
        }


        public async Task<int> UpPdfFolder(string blobContainerName)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "JS2F");
            BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
            await containerClient.CreateIfNotExistsAsync();

            string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
            var op = 0;
            foreach (var filePath in pdfFiles)
            {
                op = op + 1;
                string fileName = Path.GetFileName(filePath);
                BlobClient blobClient = containerClient.GetBlobClient($"holyrosaannual3/{fileName}");

                Console.WriteLine($"Uploading {fileName}... {op} out of {pdfFiles.Length}");

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "application/pdf"
                    }
                };

                using FileStream fileStream = File.OpenRead(filePath);
                await blobClient.UploadAsync(fileStream, uploadOptions);
            }

            return 1;
        }

        // public async Task<int> UpFolderFiles(string blobContainerName)
        // {
        //     string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "behaviour_louisville");
        //     BlobServiceClient bloClient = new BlobServiceClient(accessKey);
        //     BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
        //     await containerClient.CreateIfNotExistsAsync();

        //     string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
        //     var op = 0;
        //     foreach (var filePath in pdfFiles)
        //     {
        //         op = op + 1;
        //         string fileName = Path.GetFileName(filePath);
        //         BlobClient blobClient = containerClient.GetBlobClient($"louisville/{fileName}");

        //         Console.WriteLine($"Uploading {fileName}... {op} out of {pdfFiles.Length}");

        //         var uploadOptions = new BlobUploadOptions
        //         {
        //             HttpHeaders = new BlobHttpHeaders
        //             {
        //                 ContentType = "application/pdf"
        //             }
        //         };

        //         using FileStream fileStream = File.OpenRead(filePath);
        //         await blobClient.UploadAsync(fileStream, uploadOptions);
        //     }

        //     return 1;
        // }

        public async Task<int> UpFolderDocxFiles(string blobContainerName)
{
    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "louis_behave");
    BlobServiceClient bloClient = new BlobServiceClient(accessKey);
    BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
    await containerClient.CreateIfNotExistsAsync();

    string[] docxFiles = Directory.GetFiles(folderPath, "*.docx");
    var op = 0;
    foreach (var filePath in docxFiles)
    {
        op = op + 1;
        string fileName = Path.GetFileName(filePath);
        BlobClient blobClient = containerClient.GetBlobClient($"louisville_behavioural/{fileName}");

        Console.WriteLine($"Uploading {fileName}... {op} out of {docxFiles.Length}");

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            }
        };

        using FileStream fileStream = File.OpenRead(filePath);
        await blobClient.UploadAsync(fileStream, uploadOptions);
    }

    return 1;
}



        public async Task<int> UpProfileImgFolder(string blobContainerName)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "nteje");
            BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
            await containerClient.CreateIfNotExistsAsync();

            string[] imgFiles = Directory.GetFiles(folderPath, "*.jpg");
            var op = 0;
            foreach (var filePath in imgFiles)
            {
                op = op + 1;
                string fileName = Path.GetFileName(filePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                string admissionNo = fileNameWithoutExt.Replace("_", "/");

                BlobClient blobClient = containerClient.GetBlobClient($"{fileName}");

                Console.WriteLine($"Uploading {fileName}... {op} out of {imgFiles.Length}");

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/jpeg"
                    }
                };


                using FileStream fileStream = File.OpenRead(filePath);
                await blobClient.UploadAsync(fileStream, uploadOptions);
            }

            return 1;
        }




        public async Task<int> UploadPdfToBlob(IFormFile file, string _containerName, string subdir)
        {
            //var _containerName = "testimonial";

            BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            BlobContainerClient containerClient = new BlobContainerClient(accessKey, _containerName);
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

            var blobClient = containerClient.GetBlobClient($"{subdir}/{file.FileName}");

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return 1;
        }


        public List<string> FromAdmissionNoToPassport()
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "nteje");
            //BlobServiceClient bloClient = new BlobServiceClient(accessKey);
            //BlobContainerClient containerClient = new BlobContainerClient(accessKey, blobContainerName);
            //await containerClient.CreateIfNotExistsAsync();
            var adlist = new List<string>();

            string[] imgFiles = Directory.GetFiles(folderPath, "*.jpg");
            var op = 0;
            foreach (var filePath in imgFiles)
            {
                op = op + 1;
                string fileName = Path.GetFileName(filePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                string admissionNo = fileNameWithoutExt.Replace("_", "/");
                adlist.Add(admissionNo);
            }

            return adlist;
        }


    }
}
