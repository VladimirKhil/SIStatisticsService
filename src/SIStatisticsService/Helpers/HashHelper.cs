using System.Security.Cryptography;
using System.Text;

namespace SIStatisticsService.Helpers;

internal static class HashHelper
{
    internal static int GetStableHostHash(this Uri uri)
    {
        var host = uri.Host;
        var bytes = Encoding.UTF8.GetBytes(host.ToLowerInvariant());
        var hash = SHA256.HashData(bytes);
        return BitConverter.ToInt32(hash, 0);
    }
}
