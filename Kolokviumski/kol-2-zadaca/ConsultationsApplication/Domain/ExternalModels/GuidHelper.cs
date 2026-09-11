using System.Security.Cryptography;
using System.Text;

namespace Domain.ExternalModels;

public static class GuidHelper
{
    public static Guid FromLegacyId(string entityType, string legacyId)
    {
        var input = $"{entityType}:{legacyId}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}