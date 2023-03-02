namespace CScharf.Dataverse;

public static class OptionSetExtension
{
    /// <summary>
    ///     Returns Value of OptionSet in select language or UserLanguage of Repository
    /// </summary>
    /// <param name="service" cref="IOrganizationService">Organization service</param>
    /// <param name="optionSetName">OptionSet</param>
    /// <param name="optionSetLabel">Label of OptionSet</param>
    /// <param name="lcid">Language - leave empty for UserLanguage</param>
    /// <returns>Label</returns>
    /// <exception cref="InvalidPluginExecutionException"></exception>
    public static int? GetOptionSetValue(this IOrganizationService service, string optionSetName, string optionSetLabel, int lcid = 1033)
    {
        var label = optionSetLabel.Trim();

        var cacheKey = $"OptionSetExtension-{optionSetName}-{label}-{lcid}";
        if (TryGetCache(cacheKey, out var value))
        {
            return (int?)value;
        }

        // Retrieve OptionSet
        var retrieveOptionSetRequest = new RetrieveOptionSetRequest
        {
            Name = optionSetName
        };
        var retrieveOptionSetResponse = (RetrieveOptionSetResponse)service.Execute(retrieveOptionSetRequest);
        var retrievedOptionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

        //TODO: check lcid, currently ignored

        // Read MetaData
        var optionList = retrievedOptionSetMetadata.Options.ToArray();
        foreach (var optionMetadata in optionList)
        {
            if (optionMetadata.Label.UserLocalizedLabel.Label.Trim() != label)
            {
                continue;
            }

            SetCacheAbsolute(cacheKey, optionMetadata.Value, 121);
            return optionMetadata.Value;
        }

        throw new InvalidPluginExecutionException($"OptionSet {label} not found in {optionSetName}!");
    }

    /// <summary>
    ///     Returns Label of OptionSet in select language or UserLanguage of Repository
    /// </summary>
    /// <param name="service" cref="IOrganizationService">Organization service</param>
    /// <param name="optionSetName">OptionSet</param>
    /// <param name="optionSetValue">Value of OptionSet</param>
    /// <param name="lcid">Language - leave empty for UserLanguage</param>
    /// <returns>Label</returns>
    /// <exception cref="InvalidPluginExecutionException"></exception>
    public static string GetOptionSetLabel(this IOrganizationService service, string optionSetName, int optionSetValue, int lcid = 1033)
    {
        var cacheKey = $"OptionSetExtension-{optionSetName}-{optionSetValue}-{lcid}";
        if (TryGetCache(cacheKey, out var label))
        {
            return (string)label!;
        }

        // Retrieve OptionSet
        var retrieveOptionSetRequest = new RetrieveOptionSetRequest
        {
            Name = optionSetName
        };
        var retrieveOptionSetResponse = (RetrieveOptionSetResponse)service.Execute(retrieveOptionSetRequest);
        var retrievedOptionSetMetadata = (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

        // Read MetaData
        var optionList = retrievedOptionSetMetadata.Options.ToArray();
        foreach (var optionMetadata in optionList)
        {
            if (optionMetadata.Value != optionSetValue)
            {
                continue;
            }

            SetCacheAbsolute(cacheKey, optionMetadata.Label.UserLocalizedLabel.Label.Trim(), 121);
            return optionMetadata.Label.LocalizedLabels.Single(l => l.LanguageCode == lcid).Label.Trim();
        }

        throw new InvalidPluginExecutionException($"OptionSet {optionSetValue} not found in {optionSetName}!");
    }

    private static bool TryGetCache(string key, out object? value)
    {
        value = MemoryCache.Default.Get(key);
        return value != null;
    }

    private static void SetCacheAbsolute(string key, object? value, int ttl)
    {
        if (value == null)
        {
            return;
        }

        if (ttl < 15)
        {
            ttl = 15;
        }

        MemoryCache.Default.Set(key, value, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(ttl) });
    }
}
