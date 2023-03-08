using microservice.toolkit.entitystoremanager.entity;

namespace microservice.toolkit.entitystoremanager.extension;

internal static class LogicalOperatorExtensions
{
    internal static string ToSqlServerOperatorString(this LogicalOperator op)
    {
        return op switch
        {
            LogicalOperator.Or => "OR",
            LogicalOperator.And => "AND",
            _ => throw new System.NotImplementedException(),
        };
    }
}