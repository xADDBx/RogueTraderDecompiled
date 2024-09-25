using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("c89b71d430ead794ca332950154b7004")]
public class EtudeBracketOverrideActionsOnClick : EtudeBracketOverrideInteraction, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public ActionList Actions;

	protected override void OnEnter()
	{
		UnitPartInteractions orCreate = Unit.GetValue().GetOrCreate<UnitPartInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Interaction = new EtudeBracketOverrideUnitInteraction(this);
		orCreate.AddInteraction(componentData.Interaction);
	}

	protected override void OnExit()
	{
		UnitPartInteractions orCreate = Unit.GetValue().GetOrCreate<UnitPartInteractions>();
		ComponentData componentData = RequestTransientData<ComponentData>();
		orCreate.RemoveInteraction(componentData.Interaction);
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				Actions.Run();
			}
		}
		return AbstractUnitCommand.ResultType.Success;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
