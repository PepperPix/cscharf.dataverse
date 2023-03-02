using CScharf.Dataverse.Visitors;

namespace CScharf.Dataverse;

public static class QueryExpressionExtensions
{
    public static IEnumerable<object> GetConditionValues(this QueryExpression query, string attributeName, ConditionOperator @operator)
    {
        var visitor = new GetConditionValuesVisitor(attributeName, @operator);
        visitor.Visit(query);

        return visitor.ConditionValues;
    }

    public static QueryExpression AddLinkedEntityWithCondition(this QueryExpression query, string linkToEntity, string linkFromAttribute, string linkToAttribute, ConditionExpression condition) => new AddLinkedEntityWithConditionVisitor(linkToEntity, linkFromAttribute, linkToAttribute, condition).Visit(query);

    public static QueryExpression RemoveCondition(this QueryExpression query, string attribute, ConditionOperator @operator, object value) => new RemoveConditionVisitor(attribute, @operator, value).Visit(query);
}
