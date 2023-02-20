namespace CScharf.Dataverse;

public class DataAccessor
{
    private readonly OrganizationServiceContext _dataContext;

    public DataAccessor(OrganizationServiceContext dataContext)
    {
        _dataContext = dataContext;
    }

    /// <summary>
    /// Returns an entity by its id.
    /// Properties should be specified like this:
    /// x => new Account { Id = x.Id, Name = x.Name, ... }
    /// </summary>
    /// <param name="id">The id of the entity to be returned.</param>
    /// <param name="propertiesSelector">the properties of the entity to return.
    /// If not specified all properties are returned. Properties should be specified like this:
    /// x => new Account { Id = x.Id, Name = x.Name, ... }</param>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <returns>the queried entity of type TEntity or null if no entity found.</returns>
    public TEntity? GetById<TEntity>(Guid id, Expression<Func<TEntity, TEntity>>? propertiesSelector = null)
        where TEntity : Entity
    {
        IQueryable<TEntity?> query = QueryById<TEntity>(id);
        if (propertiesSelector != null)
        {
            query = query.Select(propertiesSelector!);
        }

        return query.SingleOrDefault();
    }

    /// <summary>
    /// Returns an IQueryable with an applied filter.
    /// It has no applied Select statement and therefore returns all columns of the queried entities.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <typeparam name="TEntity">The type of the entity to be queried for.</typeparam>
    /// <returns>An IQueryable of type TEntity.</returns>
    public IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : Entity
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        return _dataContext
            .CreateQuery<TEntity>()
            .Where(filter);
    }

    /// <summary>
    /// Returns an IQueryable of type TEntity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried for.</typeparam>
    /// <returns>An IQueryable of type TEntity.</returns>
    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : Entity
    {
        return _dataContext
            .CreateQuery<TEntity>();
    }

    /// <summary>
    /// Creates an entity in Microsoft Dataverse.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <typeparam name="TEntity">The type of the entity to be created</typeparam>
    public DataAccessor Add<TEntity>(TEntity entity) where TEntity : Entity
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _dataContext.AddObject(entity);

        return this;
    }

    /// <summary>
    /// Creates a collection of entities in Microsoft Dataverse.
    /// </summary>
    /// <param name="entities">the entities to be created.</param>
    /// <typeparam name="TEntity">The type of the entities to be created.</typeparam>
    public DataAccessor AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
    {
        foreach (var entity in entities)
        {
            Add(entity);
        }

        return this;
    }

    /// <summary>
    /// Updates an entity in Microsoft Dataverse.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <typeparam name="TEntity">The type of the entity to be updated.</typeparam>
    public DataAccessor Update<TEntity>(TEntity entity) where TEntity : Entity
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        if (!_dataContext.IsAttached(entity))
        {
            _dataContext.Attach(entity);
        }

        _dataContext.UpdateObject(entity);

        return this;
    }

    /// <summary>
    /// Updates a collection of entities in Microsoft Dataverse.
    /// </summary>
    /// <param name="entities">The entities to be updated.</param>
    /// <typeparam name="TEntity">The type of the entities to be updated</typeparam>
    public DataAccessor UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
    {
        foreach (var entity in entities)
        {
            Update(entity);
        }

        return this;
    }

    /// <summary>
    /// Deletes an entity in the Microsoft Dataverse.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <typeparam name="TEntity">The type of the entity to be deleted.</typeparam>
    public DataAccessor Delete<TEntity>(TEntity entity) where TEntity : Entity
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        if (!_dataContext.IsAttached(entity))
        {
            _dataContext.Attach(entity);
        }

        _dataContext.DeleteObject(entity, true);

        return this;
    }

    /// <summary>
    /// Deletes a collection of entities in Microsoft Dataverse.
    /// </summary>
    /// <param name="entities">The entitites to be deleted.</param>
    /// <typeparam name="TEntity">The type of the entities to be deleted.</typeparam>
    public DataAccessor DeleteRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
    {
        foreach (var entity in entities)
        {
            Delete(entity);
        }

        return this;
    }

    /// <summary>
    /// Deletes an entity in Microsoft Dataverse.
    /// </summary>
    /// <param name="id">The id of the entity to be deleted.</param>
    /// <typeparam name="TEntity">The type of the entity to be deleted.</typeparam>
    public DataAccessor Delete<TEntity>(Guid id) where TEntity : Entity
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"Invalid id: {id}", nameof(id));
        }

        var entity = Activator.CreateInstance<TEntity>();
        entity.Id = id;

        Delete(entity);

        return this;
    }

    /// <summary>
    /// Associates a parent entity with a collection of child entities for the given relationship.
    /// </summary>
    /// <param name="parentEntity">The parent entity to be associated.</param>
    /// <param name="relationshipName">The name of the relationship.</param>
    /// <param name="childEntities">The child entities to be associated.</param>
    /// <typeparam name="TParentEntity">The type of the parent entity.</typeparam>
    /// <typeparam name="TChildEntity">The type of the child entities.</typeparam>
    public DataAccessor Associate<TParentEntity, TChildEntity>(TParentEntity parentEntity, string relationshipName,
        params TChildEntity[] childEntities) where TParentEntity : Entity where TChildEntity : Entity
    {
        if (parentEntity == null)
        {
            throw new ArgumentNullException(nameof(parentEntity));
        }

        if (string.IsNullOrWhiteSpace(relationshipName))
        {
            throw new ArgumentNullException(nameof(relationshipName));
        }

        if (childEntities == null)
        {
            throw new ArgumentNullException(nameof(childEntities));
        }

        var relationship = new Relationship(relationshipName);

        return Associate(parentEntity, childEntities, relationship);
    }




    /// <summary>
    /// Associates a parent entity with a collection of child entities for the given relationship.
    /// </summary>
    /// <param name="parentEntity">The parent entity to be associated.</param>
    /// <param name="relationshipName">The name of the relationship.</param>
    /// <param name="childEntities">The child entity references to be associated.</param>
    /// <typeparam name="TParentEntity">The type of the parent entity.</typeparam>
    public DataAccessor Associate<TParentEntity>(TParentEntity parentEntity, string relationshipName,
        params EntityReference[] childEntities) where TParentEntity : Entity
    {
        if (childEntities == null)
        {
            throw new ArgumentNullException(nameof(childEntities));
        }

        return Associate(parentEntity, relationshipName, childEntities.Select(x => new Entity(x.LogicalName, x.Id)).ToArray());
    }

    /// <summary>
    /// Associates a parent entity with a collection of child entities for the given relationship.
    /// </summary>
    /// <param name="parentEntity">The parent entity reference to be associated.</param>
    /// <param name="relationshipName">The name of the relationship.</param>
    /// <param name="childEntities">The child entity references to be associated.</param>
    public DataAccessor Associate(EntityReference parentEntity, string relationshipName,
        params EntityReference[] childEntities)
    {
        if (parentEntity == null)
        {
            throw new ArgumentNullException(nameof(parentEntity));
        }

        if (childEntities == null)
        {
            throw new ArgumentNullException(nameof(childEntities));
        }

        return Associate(new Entity(parentEntity.LogicalName, parentEntity.Id), relationshipName,
            childEntities.Select(x => new Entity(x.LogicalName, x.Id)).ToArray());
    }

    /// <summary>
    /// Disassociates a parent entity from a collaction of child entities for the given relationship.
    /// </summary>
    /// <param name="parentEntity">The parent entity to be disassociated.</param>
    /// <param name="relationshipName">The name of the relationship.</param>
    /// <param name="childEntities">The child entities to be disassociated</param>
    /// <typeparam name="TParentEntity">The type of the parent entity.</typeparam>
    /// <typeparam name="TChildEntity">The type of the child entities.</typeparam>
    public DataAccessor Disassociate<TParentEntity, TChildEntity>(TParentEntity parentEntity,
        string relationshipName,
        params TChildEntity[] childEntities) where TParentEntity : Entity where TChildEntity : Entity
    {
        if (parentEntity == null)
        {
            throw new ArgumentNullException(nameof(parentEntity));
        }

        if (string.IsNullOrWhiteSpace(relationshipName))
        {
            throw new ArgumentNullException($"{nameof(relationshipName)}");
        }

        var relationship = new Relationship(relationshipName);

        if (!_dataContext.IsAttached(parentEntity))
        {
            _dataContext.Attach(parentEntity);
        }

        foreach (var childEntity in childEntities)
        {
            if (!_dataContext.IsAttached(childEntity))
            {
                _dataContext.Attach(childEntity);
            }

            _dataContext.DeleteLink(parentEntity, relationship, childEntity);
        }

        return this;
    }

    /// <summary>
    /// Disassociates a parent entity from a collaction of child entities for the given relationship.
    /// </summary>
    /// <param name="parentEntity">The parent entity to be disassociated.</param>
    /// <param name="relationshipName">The name of the relationship.</param>
    /// <param name="childEntities">The child entity references to be disassociated</param>
    /// <typeparam name="TParentEntity">The type of the parent entity.</typeparam>
    public DataAccessor Disassociate<TParentEntity>(TParentEntity parentEntity, string relationshipName,
        params EntityReference[] childEntities) where TParentEntity : Entity
    {
        if (childEntities == null)
        {
            throw new ArgumentNullException(nameof(childEntities));
        }

        return Disassociate(parentEntity, relationshipName, childEntities.Select(x => new Entity(x.LogicalName, x.Id)).ToArray());
    }

    /// <summary>
    /// Disassociates a parent entity from a collaction of child entities for the given relationship.
    /// </summary>
    /// <param name="parentEntity">The parent entity reference to be disassociated.</param>
    /// <param name="relationshipName">The name of the relationship.</param>
    /// <param name="childEntities">The child entity references to be disassociated</param>
    public DataAccessor Disassociate(EntityReference parentEntity, string relationshipName,
        params EntityReference[] childEntities)
    {
        if (parentEntity == null)
        {
            throw new ArgumentNullException(nameof(parentEntity));
        }

        if (childEntities == null)
        {
            throw new ArgumentNullException(nameof(childEntities));
        }

        return Disassociate(new Entity(parentEntity.LogicalName, parentEntity.Id), relationshipName,
            childEntities.Select(x => new Entity(x.LogicalName, x.Id)).ToArray());
    }

    /// <summary>
    /// Writes all changes to Microsoft Dataverse.
    /// </summary>
    public DataAccessor Commit()
    {
        _dataContext.SaveChanges(SaveChangesOptions.None);
        return this;
    }

    /// <summary>
    /// Rollbacks all changes made since the last commit.
    /// </summary>
    public DataAccessor Rollback()
    {
        _dataContext.ClearChanges();
        return this;
    }

    private DataAccessor Associate<TParentEntity, TChildEntity>(TParentEntity parentEntity, IEnumerable<TChildEntity> childEntities, Relationship relationship)
        where TParentEntity : Entity where TChildEntity : Entity
    {
        if (!_dataContext.IsAttached(parentEntity))
        {
            _dataContext.Attach(parentEntity);
        }

        foreach (var childEntity in childEntities)
        {
            if (!_dataContext.IsAttached(childEntity) && childEntity.Id != Guid.Empty)
            {
                _dataContext.Attach(childEntity);
            }

            if (childEntity.Id == Guid.Empty)
            {
                _dataContext.AddRelatedObject(parentEntity, relationship, childEntity);
            }
            else
            {
                _dataContext.AddLink(parentEntity, relationship, childEntity);
            }
        }

        return this;
    }

    private IQueryable<TEntity> QueryById<TEntity>(Guid id) where TEntity : Entity
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"Invalid id: '{id}'", nameof(id));
        }

        return _dataContext
            .CreateQuery<TEntity>()
            .Where(x => x.Id == id);
    }
}
