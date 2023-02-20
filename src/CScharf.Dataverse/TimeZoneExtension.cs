namespace CScharf.Dataverse;

public static class TimeZoneExtension
{
    /// <summary>
    ///     Converts the given localTime to UTC time for the user identified by the userId
    /// </summary>
    /// <param name="service" cref="IOrganizationService">Organization service</param>
    /// <param name="userId"></param>
    /// <param name="localTime"></param>
    /// <param name="defaultTimeZoneCode"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static DateTime UtcTimeFromLocalTime(this IOrganizationService service, Guid userId, DateTime localTime, int? defaultTimeZoneCode = null)
    {
        var query = new QueryExpression("usersettings")
        {
            NoLock = true
        };
        query.ColumnSet.AddColumn("timezonecode");
        query.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);

        var result = service.RetrieveMultiple(query).Entities.SingleOrDefault()?.GetAttributeValue<int>("timezonecode");

        if (result != null)
        {
            var timezoneCode = (int)result;

            var utcTimeRequest = new UtcTimeFromLocalTimeRequest { TimeZoneCode = timezoneCode, LocalTime = localTime };
            var utcTimeResponse = (UtcTimeFromLocalTimeResponse)service.Execute(utcTimeRequest);

            return utcTimeResponse.UtcTime;
        }

        if (defaultTimeZoneCode.HasValue)
        {
            var utcTimeRequest = new UtcTimeFromLocalTimeRequest { TimeZoneCode = defaultTimeZoneCode.Value, LocalTime = localTime };
            var utcTimeResponse = (UtcTimeFromLocalTimeResponse)service.Execute(utcTimeRequest);

            return utcTimeResponse.UtcTime;
        }

        throw new InvalidPluginExecutionException("User does not have Timezone Code");
    }

    /// <summary>
    ///     Converts the given utcTime to local time for the user identified by the userId
    /// </summary>
    /// <param name="service" cref="IOrganizationService">Organization service</param>
    /// <param name="userId"></param>
    /// <param name="utcTime"></param>
    /// <param name="defaultTimeZoneCode"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static DateTime LocalTimeFromUtcTime(this IOrganizationService service, Guid userId, DateTime utcTime, int? defaultTimeZoneCode = null)
    {
        var query = new QueryExpression("usersettings")
        {
            NoLock = true
        };
        query.ColumnSet.AddColumn("timezonecode");
        query.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);

        var result = service.RetrieveMultiple(query).Entities.SingleOrDefault()?.GetAttributeValue<int>("timezonecode");

        if (result != null)
        {
            var timezoneCode = (int)result;

            var localTimeRequest = new LocalTimeFromUtcTimeRequest { TimeZoneCode = timezoneCode, UtcTime = utcTime };
            var localTimeResponse = (LocalTimeFromUtcTimeResponse)service.Execute(localTimeRequest);

            return localTimeResponse.LocalTime;
        }

        if (defaultTimeZoneCode.HasValue)
        {
            var localTimeRequest = new LocalTimeFromUtcTimeRequest { TimeZoneCode = defaultTimeZoneCode.Value, UtcTime = utcTime };
            var localTimeResponse = (LocalTimeFromUtcTimeResponse)service.Execute(localTimeRequest);

            return localTimeResponse.LocalTime;
        }

        throw new InvalidPluginExecutionException("User does not have Timezone Code");
    }
}
