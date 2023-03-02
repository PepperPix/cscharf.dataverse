namespace CScharf.Dataverse;

public static class OrganizationServiceExtensions
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="service"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IEnumerable<TEntity> RetrieveMultiple<TEntity>(this IOrganizationService service, QueryBase query)
        where TEntity : Entity =>
        service.RetrieveMultiple(query)
            ?.Entities
            ?.Select(x => x.ToEntity<TEntity>())
            .ToList()
        ?? Enumerable.Empty<TEntity>();

    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="service"></param>
    /// <param name="entityReference"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    public static TEntity Retrieve<TEntity>(this IOrganizationService service, EntityReference entityReference, ColumnSet columns)
        where TEntity : Entity =>
        service.Retrieve<TEntity>(entityReference.LogicalName, entityReference.Id, columns);

    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="service"></param>
    /// <param name="logicalName"></param>
    /// <param name="id"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    public static TEntity Retrieve<TEntity>(this IOrganizationService service, string logicalName, Guid id, ColumnSet columns)
        where TEntity : Entity =>
        service.Retrieve(logicalName, id, columns).ToEntity<TEntity>();

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="record"></param>
    /// <param name="ownerReference"></param>
    /// <returns></returns>
    public static bool TryAssign(this IOrganizationService service, EntityReference record, EntityReference ownerReference)
    {
        try
        {
            service.Execute(new AssignRequest
            {
                Target = record,
                Assignee = ownerReference
            });
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="reference"></param>
    /// <param name="state"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool TrySetState(this IOrganizationService service, EntityReference reference, int state, int status)
    {
        try
        {
            service.Update(new Entity(reference.LogicalName, reference.Id)
            {
                ["statecode"] = new OptionSetValue(state),
                ["statuscode"] = new OptionSetValue(status)
            });
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool TryCreate(this IOrganizationService service, Entity entity, out Guid id)
    {
        id = Guid.Empty;
        try
        {
            id = service.Create(entity);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool TryUpdate(this IOrganizationService service, Entity entity)
    {
        try
        {
            service.Update(entity);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool TryDelete(this IOrganizationService service, string name, Guid id)
    {
        try
        {
            service.Delete(name, id);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="relationship"></param>
    /// <param name="references"></param>
    /// <returns></returns>
    public static bool TryAssociate(this IOrganizationService service, string name, Guid id, Relationship relationship, EntityReferenceCollection references)
    {
        try
        {
            service.Associate(name, id, relationship, references);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="relationship"></param>
    /// <param name="references"></param>
    /// <returns></returns>
    public static bool TryDisassociate(this IOrganizationService service, string name, Guid id, Relationship relationship, EntityReferenceCollection references)
    {
        try
        {
            service.Disassociate(name, id, relationship, references);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="service"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static bool TryExecute<TR, TO>(this IOrganizationService service, TR request, out TO response) where TR : OrganizationRequest where TO : OrganizationResponse
    {
        response = default!;
        try
        {
            response = (TO)service.Execute(request);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="service"></param>
    /// <param name="entityName"></param>
    /// <param name="entityId"></param>
    /// <param name="columns"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool TryRetrieve<T>(this IOrganizationService service, string entityName, Guid entityId, ColumnSet columns, out T entity) where T : Entity
    {
        entity = default!;
        try
        {
            entity = service.Retrieve(entityName, entityId, columns).ToEntity<T>();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <param name="service"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static TO Execute<TR, TO>(this IOrganizationService service, TR request)
        where TR : OrganizationRequest
        where TO : OrganizationResponse =>
        (TO)service.Execute(request);
}
