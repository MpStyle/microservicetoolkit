using microservice.toolkit.entitystoremanager.entity;

namespace microservice.toolkit.entitystoremanager.extension
{
    internal static class OperatorExtensions
    {
        internal static string ToSqlServerOperatorString(this Operator op)
        {
            return op switch
            {
                Operator.Equal => "=",
                Operator.LessThan => "<",
                Operator.LessEqualThan => "<=",
                Operator.GreaterThan => ">",
                Operator.GreaterEqualThan => ">=",
                Operator.Like => "LIKE",
                Operator.EndingLike => "LIKE",
                Operator.StartingLike => "LIKE",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}