using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("8b4c3a5898712654596bc06311e73737")]
public class EtudeBracketOverrideDialog : EtudeBracketOverrideInteraction, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintDialogReference Dialog;

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
		if (!(target is BaseUnitEntity unit))
		{
			return AbstractUnitCommand.ResultType.Fail;
		}
		DialogData data = DialogController.SetupDialogWithUnit((BlueprintDialog)Dialog.GetBlueprint(), unit, user);
		Game.Instance.DialogController.StartDialog(data);
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
