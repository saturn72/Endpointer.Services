using Amazon.Runtime;
using Amazon.S3;
using Endpointer.Json.Remapping.Services.StorageProvider;

namespace Endpointer.Json.Remapping.Configurars.S3;

public class S3Configurar
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var providerName = "s3-1";
        var configSection = "s3_1";

        services.AddKeyedSingleton<IStorageProvider>(providerName, (sp, key) =>
        {
            var cnf = sp.GetRequiredService<IConfiguration>().GetSection(configSection);

            var serviceUrl = cnf.GetValue<string>("ServiceURL")
                             ?? throw new InvalidOperationException($"Missing '{configSection}:ServiceURL' configuration.");
            var accessKey = cnf.GetValue<string>("AccessKey")
                            ?? throw new InvalidOperationException($"Missing '{configSection}:AccessKey' configuration.");
            var secretKey = cnf.GetValue<string>("SecretKey")
                            ?? throw new InvalidOperationException($"Missing '{configSection}:SecretKey' configuration.");
            var bucketName = cnf.GetValue<string>("BucketName")
                                ?? throw new InvalidOperationException($"Missing '{configSection}:BucketName' configuration.");
            var forcePathStyle = cnf.GetValue("ForcePathStyle", true);

            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = forcePathStyle,
            };

            var s3 = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), config);

            return new MinioStorageProvider(s3, bucketName);
        });

        services.AddSingleton<IStorageProviderFactory>(sp =>
        {
            var p = new Dictionary<string, IStorageProvider>
            {
                [providerName] = sp.GetKeyedService<IStorageProvider>(providerName) ?? throw new InvalidOperationException($"Storage provider for key '{providerName}' not found.")
            };
            return new StorageProviderFactory(p);
        });
    }
}
