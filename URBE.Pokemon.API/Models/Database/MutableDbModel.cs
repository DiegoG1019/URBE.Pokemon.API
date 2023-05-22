namespace URBE.Pokemon.API.Models.Database;

public abstract class MutableDbModel
{
    public DateTimeOffset CreationDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }
}
