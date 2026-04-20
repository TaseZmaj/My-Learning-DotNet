using System.Security.Cryptography;
using System.Text;

namespace EventsManagement.Domain.ExternalModels;

public static class GuidHelper
{
    //Hash funkcija koja pretvara int id vo GUID -> Bidejki Legacy bazata od koja go zemame IDto ima int ids.
    public static Guid FromLegacyId(string entityType, int legacyId)
    {
        var input = $"{entityType}:{legacyId}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}