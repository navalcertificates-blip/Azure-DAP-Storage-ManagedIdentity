using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Identity;

namespace Azure_DAP_Storage_ManagedIdentity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class azurestorageapiController : ControllerBase
    {
        private readonly string blobServiceUri = "https://azstgnaval.blob.core.windows.net/";
        private readonly string containername = "democontainer";
        private readonly string blobName = "demoblob.txt";
        private readonly string localFilePath = "d:\\demoblob.txt";
        private readonly string userManagedIdentityClientId = "42813b25-c45f-4086-8fe9-be3a3ba5391d";

        //[HttpPost]

        //public async Task<IActionResult> Get_old_original()
        //{
        //    //var credential = new ManagedIdentityCredential(userManagedIdentityClientId);

        //    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        //    {
        //        ManagedIdentityClientId = userManagedIdentityClientId
        //    });

        //    var blobServiceClient = new BlobServiceClient(
        //        new Uri(blobServiceUri),
        //        credential
        //    );

        //    //var blobServiceClient = new BlobServiceClient(    new Uri(blobServiceUri),    new DefaultAzureCredential());

        //    //var blobServiceClient = new BlobServiceClient(new Uri(blobServiceUri), credential);

        //    var containerClient = blobServiceClient.GetBlobContainerClient(containername);

        //    await containerClient.CreateIfNotExistsAsync();

        //    var blobClient = containerClient.GetBlobClient(blobName);

        //    var content = "Hello, This is Naval Kishor Verma. Testing file upload using managed identity";

        //    using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
        //    {
        //        await blobClient.UploadAsync(stream, overwrite: true);
        //    }

        //    var downloadinfo = await blobClient.DownloadContentAsync();

        //    var downloadContent = downloadinfo.Value.Content.ToString();

        //    return Ok(new
        //    {
        //        Message = "File Uploaded and downloaded successfully",
        //        Content = downloadContent
        //    });
        //}

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]

        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a file.");
            }

            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = userManagedIdentityClientId
            });

            var blobServiceClient = new BlobServiceClient(
               new Uri(blobServiceUri),
               credential
           );

            var containerClient = blobServiceClient.GetBlobContainerClient(containername);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            var downloadinfo = await blobClient.DownloadContentAsync();

            var downloadContent = downloadinfo.Value.Content.ToString();

            return Ok(new
            {
                Message = "File Uploaded and downloaded successfully",
                FileName = file.FileName,
                Content = downloadContent
            });
        }

        [HttpGet("action")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            string storageAccountName = "azstgnaval";           

            //var blobClient = new BlobClient(
            //    new Uri($"https://{storageAccountName}.blob.core.windows.net/{containername}/demo.txt"),
            //    new DefaultAzureCredential());

            //using (var stream = new MemoryStream())
            //{
            //    await blobClient.DownloadToAsync(stream);
            //    stream.Position = 0;

            //    var contentType = (await blobClient.GetPropertiesAsync()).Value.ContentType;

            //    return File(stream.ToArray(), contentType, blobClient.Name);
            //}


            string connstring = "DefaultEndpointsProtocol=https;AccountName=azstgnaval;AccountKey=ku4cGCTKwwryO8QAWd0xMj9gBeMX5yfnvAF1VXEimJPFxtuP4vXqF0wZKlNTb3Mz8F/XXUSD9DSG+AStK6fEfQ==;EndpointSuffix=core.windows.net";
            
            BlobClient blobclient = new BlobClient(connstring, containername, fileName);
            using (var stream = new MemoryStream())
            {
                await blobclient.DownloadToAsync(stream);
                stream.Position = 0;
                var contenttype = (await blobclient.GetPropertiesAsync()).Value.ContentType;
                return File(stream.ToArray(), contenttype, blobclient.Name);
            }

            /*working code: remove any parameter from function
            string connstring = "DefaultEndpointsProtocol=https;AccountName=azuredapstorage;AccountKey=YxDYEyfwlfhiJvkfikZYwWGTecCq8IV1zFesW5YBLm0197rM/tsN5iqUjrnO7gkHAA6HQxLlxROQ+AStuH2XyQ==;EndpointSuffix=core.windows.net";

            BlobClient blobclient = new BlobClient(connstring, containername, "demo.txt");
            using (var stream = new MemoryStream())
            {
                await blobclient.DownloadToAsync(stream);
                stream.Position = 0;
                var contenttype = (await blobclient.GetPropertiesAsync()).Value.ContentType;
                return File(stream.ToArray(), contenttype, blobclient.Name);
            }
            */
        }
    }
}
