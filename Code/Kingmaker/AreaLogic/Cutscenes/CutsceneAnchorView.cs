using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutsceneAnchorView : EntityViewBase
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutsceneAnchorEntity(this));
	}
}
