using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("83d292fccbb60dd4abaae4b14e12a03d")]
public class EtudeBracketSummonpoolOverrideDialog : EtudeBracketOverrideInteraction, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public EtudeBracketOverrideUnitInteraction Interaction;
	}

	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public BlueprintDialogReference Dialog;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	protected override void OnEnter()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		componentData.Interaction = new EtudeBracketOverrideUnitInteraction(this);
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return;
		}
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			unit.GetOrCreate<UnitPartInteractions>().AddInteraction(componentData.Interaction);
		}
	}

	protected override void OnExit()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			unit.GetOrCreate<UnitPartInteractions>().RemoveInteraction(componentData.Interaction);
		}
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
