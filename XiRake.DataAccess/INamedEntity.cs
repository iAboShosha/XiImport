namespace XiRake.DataAccess
{
    public interface INamedEntity:IEntity
    {
        string Name { get; set; }
    }
}