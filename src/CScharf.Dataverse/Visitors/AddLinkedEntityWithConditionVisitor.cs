namespace CScharf.Dataverse.Visitors;

public class AddLinkedEntityWithConditionVisitor : QueryExpressionVisitorBase
{
    private readonly ConditionExpression _condition;
    private readonly string _linkFromAttribute;
    private readonly string _linkToAttribute;
    private readonly string _linkToEntity;

    public AddLinkedEntityWithConditionVisitor(string linkToEntity, string linkFromAttribute, string linkToAttribute, ConditionExpression condition)
    {
        _linkToEntity = linkToEntity;
        _linkFromAttribute = linkFromAttribute;
        _linkToAttribute = linkToAttribute;
        _condition = condition;
    }

    public override QueryExpression Visit(QueryExpression query)
    {
        var linkEntity = query.AddLink(_linkToEntity, _linkFromAttribute, _linkToAttribute);
        linkEntity.LinkCriteria.AddCondition(_condition);

        return base.Visit(query);
    }
}
