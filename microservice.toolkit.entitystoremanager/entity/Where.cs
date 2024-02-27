namespace microservice.toolkit.entitystoremanager.entity
{
    public interface IWhere
    {
    }

    public class Where : IWhere
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public Operator Operator { get; set; }
    }
    
    public class IsNullWhere : IWhere
    {
        public string Key { get; set; }
    }
    
    public class InWhere : IWhere
    {
        public string Key { get; set; }
        public object[] Values { get; set; }
    }

    public abstract class LogicWhere : IWhere
    {
        public LogicalOperator Op { get; }

        protected LogicWhere(LogicalOperator op)
        {
            this.Op = op;
        }

        public IWhere[] Conditions { get; set; }
    }

    public class AndWhere : LogicWhere
    {
        public AndWhere() : base(LogicalOperator.And)
        {
        }
    }

    public class OrWhere : LogicWhere
    {
        public OrWhere() : base(LogicalOperator.Or)
        {
        }
    }
}