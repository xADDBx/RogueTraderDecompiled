namespace Kingmaker.Blueprints;

public interface IEntityReference
{
	EntityReferenceType GetEntityUsagesType(string guid);
}
