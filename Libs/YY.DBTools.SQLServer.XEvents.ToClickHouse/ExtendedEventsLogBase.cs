namespace YY.DBTools.SQLServer.XEvents.ToClickHouse
{
    public class ExtendedEventsLogBase
    {
        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
