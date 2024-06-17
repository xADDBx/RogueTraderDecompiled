namespace Kingmaker.EntitySystem.Entities.Base;

public interface IEntityPartOwner
{
}
public interface IEntityPartOwner<TPart> : IEntityPartOwner where TPart : EntityPart
{
}
