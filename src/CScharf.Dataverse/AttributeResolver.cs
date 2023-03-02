namespace CScharf.Dataverse;

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class AttributeResolver<TEntity> where TEntity : Entity
{
    private readonly ObjectCache _genericTypeCache = MemoryCache.Default;

    public AttributeResolver(Entity preEntity, Entity targetEntity)
    {
        PreEntity = preEntity.ToEntity<TEntity>();
        TargetEntity = targetEntity.ToEntity<TEntity>();
    }

    public AttributeResolver(TEntity preEntity, TEntity targetEntity)
    {
        PreEntity = preEntity;
        TargetEntity = targetEntity;
    }

    public AttributeResolver(Entity targetEntity)
    {
        PreEntity = default;
        TargetEntity = targetEntity.ToEntity<TEntity>();
    }

    public AttributeResolver(TEntity targetEntity)
    {
        PreEntity = default;
        TargetEntity = targetEntity;
    }

    public TEntity? PreEntity { get; }
    public TEntity TargetEntity { get; }

    /// <summary>
    ///     Get value TValue from entity. Lookup order 1st Entity, 2nd PreEntityImage, 3rd default!
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="attributeExpression">lookup attribute expression</param>
    /// <returns></returns>
    public TValue? Resolve<TValue>(Expression<Func<TEntity, TValue>> attributeExpression)
    {
        var value = default(TValue);

        var attributeName = GetAttributeLogicalName(attributeExpression);

        if (TargetEntity.Contains(attributeName))
        {
            value = (TValue)TargetEntity[attributeName];
        }
        else if (PreEntity != default(TEntity) && PreEntity.Contains(attributeName))
        {
            value = (TValue)PreEntity[attributeName];
        }

        return value;
    }

    /// <summary>
    ///     Merge Entity and PreEntityImage
    /// </summary>
    /// <returns>merged entity (like postimage)</returns>
    public TEntity Merge()
    {
        if (PreEntity == default(TEntity))
        {
            return TargetEntity;
        }

        var mergedEntity = new Entity { Id = PreEntity.Id, LogicalName = PreEntity.LogicalName };

        // return all AttributeLogicalNameAttribute from the given type
        var attributes = from property in typeof(TEntity).GetProperties()
            from attribute in
                property.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), false).OfType<AttributeLogicalNameAttribute>()
            select attribute;

        foreach (var attribute in attributes)
        {
            if (TargetEntity.Contains(attribute.LogicalName))
            {
                mergedEntity[attribute.LogicalName] = TargetEntity[attribute.LogicalName];
                if (TargetEntity.FormattedValues.ContainsKey(attribute.LogicalName))
                {
                    mergedEntity.FormattedValues.Add(attribute.LogicalName, TargetEntity.FormattedValues[attribute.LogicalName]);
                }
            }
            else if (PreEntity.Contains(attribute.LogicalName))
            {
                mergedEntity[attribute.LogicalName] = PreEntity[attribute.LogicalName];
                if (PreEntity.FormattedValues.ContainsKey(attribute.LogicalName))
                {
                    mergedEntity.FormattedValues.Add(attribute.LogicalName, PreEntity.FormattedValues[attribute.LogicalName]);
                }
            }
        }

        return mergedEntity.ToEntity<TEntity>();
    }

    /// <summary>
    ///     Evaluates if Target contains attribute and PreImage does not
    /// </summary>
    /// <param name="attributeExpression">lookup attribute expression</param>
    /// <returns>Target contains attribute and PreImage does not</returns>
    public bool IsNew<TValue>(Expression<Func<TEntity, TValue>> attributeExpression)
    {
        var attributeName = GetAttributeLogicalName(attributeExpression);

        return TargetEntity.Contains(attributeName) && (PreEntity == null || !PreEntity.Contains(attributeName));
    }

    /// <summary>
    ///     Evaluates if attribute in target is set and is different from preimage
    /// </summary>
    /// <param name="attributeExpression">Attribute Expression</param>
    /// <returns>Attribute in target is set and is different from preimage</returns>
    public bool IsChanged<TValue>(Expression<Func<TEntity, TValue>> attributeExpression)
    {
        var attributeName = GetAttributeLogicalName(attributeExpression);

        if (!TargetEntity.Contains(attributeName))
        {
            return false;
        }

        if (PreEntity == null)
        {
            return true;
        }

        IEqualityComparer<TValue>? typeEqualizer = null;
        if (_genericTypeCache.Contains(typeof(TValue).FullName!))
        {
            typeEqualizer = _genericTypeCache[typeof(TValue).FullName!] as EqualityComparer<TValue>;
        }

        if (typeEqualizer == null)
        {
            typeEqualizer = EqualityComparer<TValue>.Default;
            _genericTypeCache.Add(typeof(TValue).FullName!, typeEqualizer, new CacheItemPolicy());
        }

        return !(PreEntity.Contains(attributeName) && typeEqualizer.Equals((TValue)PreEntity[attributeName], (TValue)TargetEntity[attributeName]));
    }

    /// <summary>
    ///     valuates if attribute contained in target or preimage and not null.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="attributeExpression"></param>
    /// <returns></returns>
    public bool IsNullOrEmpty<TValue>(Expression<Func<TEntity, TValue>> attributeExpression)
    {
        var attributeName = GetAttributeLogicalName(attributeExpression);

        return (!TargetEntity.Contains(attributeName) || TargetEntity[attributeName] == null) &&
               (PreEntity == null || !PreEntity.Contains(attributeName) || PreEntity[attributeName] == null);
    }

    private static string GetAttributeLogicalName(LambdaExpression propertyExpression)
    {
        MemberExpression memberExpression;

        if (propertyExpression.Body is UnaryExpression unaryExpression)
        {
            memberExpression = (MemberExpression)unaryExpression.Operand;
        }
        else
        {
            memberExpression = (MemberExpression)propertyExpression.Body;
        }

        var logicalName = (from attr in memberExpression.Member.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), false).OfType<AttributeLogicalNameAttribute>() select attr.LogicalName).First();

        return logicalName;
    }
}
