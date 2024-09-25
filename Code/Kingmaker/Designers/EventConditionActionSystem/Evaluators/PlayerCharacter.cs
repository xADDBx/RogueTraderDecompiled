using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/PlayerCharacter")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("25c132cb07bfaef4683b062a74f6e012")]
public class PlayerCharacter : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return GameHelper.GetPlayerCharacterOriginal();
	}

	public override string GetCaption()
	{
		return "Player Character";
	}
}
