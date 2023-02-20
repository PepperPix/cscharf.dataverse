using System.Globalization;

namespace CScharf.Dataverse;

public static class UserExtension
{
    /// <summary>
    ///     Retrieves User Culture according his UI language
    /// </summary>
    /// <param name="service" cref="IOrganizationService">Organization service</param>
    /// <param name="userId">ID of Systemuser</param>
    /// <param name="defaultCulture">Culture if User Culture is indeterminated</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CultureInfo UserCulture(this IOrganizationService service, Guid userId, CultureInfo? defaultCulture = null)
    {
        var cacheKey = $"UserExtension-{userId:N}";

        if (TryGetCache(cacheKey, out var value))
        {
            return (CultureInfo)value!;
        }

        var query = new QueryExpression("usersettings")
        {
            NoLock = true
        };
        query.ColumnSet.AddColumn("uilanguageid");
        query.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);

        var result = service.RetrieveMultiple(query).Entities.SingleOrDefault()?.GetAttributeValue<int>("uilanguageid");


        if (result is > 0)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(result.Value);

            SetCacheSliding(cacheKey, cultureInfo, 300);
            return cultureInfo;
        }


        if (defaultCulture != null)
        {
            return defaultCulture;
        }

        throw new InvalidPluginExecutionException("User does not have UI language");
    }

    private static bool TryGetCache(string key, out object? value)
    {
        value = MemoryCache.Default.Get(key);
        return value != null;
    }

    private static void SetCacheSliding(string key, object? value, int ttl)
    {
        if (value == null)
        {
            return;
        }

        if (ttl < 15)
        {
            ttl = 15;
        }

        MemoryCache.Default.Set(key, value, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(ttl) });
    }
}
