namespace CScharf.Dataverse.Visitors;

public class RemoveConditionVisitor : QueryExpressionVisitorBase
{
    private readonly string _attribute;
    private readonly ConditionOperator _operator;
    private readonly object _value;

    public RemoveConditionVisitor(string attribute, ConditionOperator @operator, object value)
    {
        _attribute = attribute;
        _operator = @operator;
        _value = value;
    }

    protected override FilterExpression VisitFilterExpression(FilterExpression filter)
    {
        var condition = filter.Conditions.FirstOrDefault(x =>
            x.AttributeName == _attribute && x.Operator == _operator && x.Values.Contains(_value));

        if (condition != null)
        {
            filter.Conditions.Remove(condition);
        }

        return base.VisitFilterExpression(filter);
    }
}
