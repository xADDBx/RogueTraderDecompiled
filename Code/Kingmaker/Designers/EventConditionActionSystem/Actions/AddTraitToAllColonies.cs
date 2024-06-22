using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("86dba7bc4c194b46b64e3b1a30fdef39")]
[PlayerUpgraderAllowed(false)]
public class AddTraitToAllColonies : GameAction
{
	[NotNull]
	public BlueprintColonyTrait.Reference Trait;

	public override string GetCaption()
	{
		return "Add " + Trait.Get().Name + " to all existing colonies";
	}

	protected override void RunAction()
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			colony.Colony.AddTrait(Trait.Get());
		}
		Game.Instance.Player.ColoniesState.TraitsForAllColonies.Add(Trait.Get());
	}
}
