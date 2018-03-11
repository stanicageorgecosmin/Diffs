namespace ProjectedEntities
{
    public interface IProjectedEntity<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
    }
}
