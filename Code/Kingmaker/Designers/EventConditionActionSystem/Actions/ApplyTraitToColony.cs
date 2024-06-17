using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("2ac5d227b9415e94d959e0c8198646db")]
[PlayerUpgraderAllowed(false)]
public class ApplyTraitToColony : GameAction
{
	public BlueprintColonyTrait.Reference Trait;

	[SerializeField]
	private BlueprintColonyReference m_Colony;

	private BlueprintColony Colony => m_Colony?.Get();

	public override string GetCaption()
	{
		return "Apply trait " + Trait.Get().Name + " to current colony in context";
	}

	public override void RunAction()
	{
		((Colony == null) ? ContextData<ColonyContextData>.Current?.Colony : Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == Colony)?.Colony)?.AddTrait(Trait.Get());
	}
}
