namespace CScharf.Dataverse.Visitors;

public class GetConditionValuesVisitor : QueryExpressionVisitorBase
{
    private readonly string _attributeName;
    private readonly ConditionOperator _operator;

    public GetConditionValuesVisitor(string attributeName, ConditionOperator @operator)
    {
        _attributeName = attributeName;
        _operator = @operator;
    }

    public IEnumerable<object> ConditionValues { get; private set; } = Enumerable.Empty<object>();

    protected override ConditionExpression VisitConditionExpression(ConditionExpression condition)
    {
        if (condition.AttributeName != _attributeName || condition.Operator != _operator)
        {
            return base.VisitConditionExpression(condition);
        }

        ConditionValues = condition.Values;

        return base.VisitConditionExpression(condition);
    }
}
