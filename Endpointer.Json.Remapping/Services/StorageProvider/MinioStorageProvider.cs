using Amazon.S3;
using Amazon.S3.Model;

namespace Endpointer.Json.Remapping.Services.StorageProvider;

public class MinioStorageProvider : IStorageProvider
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public MinioStorageProvider(IAmazonS3 s3, string bucketName)
    {
        _s3 = s3;
        _bucketName = bucketName;
    }

    public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken)
    {
        var (bucketName, objectKey) = ResolveObjectPath(path);

        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };

        using var response = await _s3.GetObjectAsync(request, cancellationToken);
        var stream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        return stream;
    }

    private (string BucketName, string ObjectKey) ResolveObjectPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Storage path cannot be empty.", nameof(path));
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.Scheme == "s3")
        {
            return (uri.Host, uri.AbsolutePath.TrimStart('/'));
        }

        var normalizedPath = path.TrimStart('/');
        var slashIndex = normalizedPath.IndexOf('/');
        if (slashIndex > 0)
        {
            return (normalizedPath[..slashIndex], normalizedPath[(slashIndex + 1)..]);
        }

        return (_bucketName, normalizedPath);
    }
}
