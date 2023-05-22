namespace URBE.Pokemon.API.Models.Database;

public interface IKeyed<TModel> where TModel : class, IKeyed<TModel>
{
    public Id<TModel> Id { get; }
}
