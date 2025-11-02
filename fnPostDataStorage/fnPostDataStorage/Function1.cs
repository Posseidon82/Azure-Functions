using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fnPostDataStorage
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("dataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processando a imagem no storage.");

            try
            {
                if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    return new BadRequestObjectResult("O cabeçalho 'file-type' é obrigatório");
                }

                var fileType = fileTypeHeader.ToString();
                var form = await req.ReadFormAsync();
                var file = form.Files["file"];

                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult("O arquivo não foi enviado.");
                }

                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string contaierName = fileType;
                BlobClient blobClient = new BlobClient(connectionString, contaierName, file.FileName);
                BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, contaierName);

                await blobContainerClient.CreateIfNotExistsAsync();
                await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);
                
                string blobName = file.FileName;
                var blob = blobContainerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream, true);
                }

                _logger.LogInformation($"Arquivo '{file.FileName}' enviado com sucesso para o container '{contaierName}'.");

                return new OkObjectResult(new
                {
                    Message = "Arquivo armazenado com sucesso.",
                    BlobURI = blob.Uri.ToString()
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
