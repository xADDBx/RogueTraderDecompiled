using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;

namespace Kingmaker.AreaLogic.Cutscenes;

[KnowledgeDatabaseID("67541fe556b14910b86f76ff65d5e5e0")]
public class CutsceneAnchorView : EntityViewBase
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutsceneAnchorEntity(this));
	}
}
