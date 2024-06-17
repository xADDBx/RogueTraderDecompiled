using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("1db2a7f1e66ae3d47b9e16af07f04a25")]
public class EtudeBracketOverrideBark : EtudeBracketOverrideInteraction, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public SharedStringAsset WhatToBarkShared;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

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
		float duration = (BarkDurationByText ? UIUtility.GetBarkDuration(WhatToBarkShared.String) : UIUtility.DefaultBarkTime);
		BarkPlayer.Bark(target, WhatToBarkShared.String, duration, BarkDurationByText, user);
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
