namespace Kingmaker.EntitySystem.Entities.Base;

public interface IEntityPartsManagerDelegate
{
	void OnPartAppears(EntityPart part);

	void OnPartDisappears(EntityPart part);
}
