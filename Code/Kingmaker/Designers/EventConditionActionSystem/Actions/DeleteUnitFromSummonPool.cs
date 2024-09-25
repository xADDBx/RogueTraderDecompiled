using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/DeleteUnitFromSummonPool")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("34705b1b862f3334aa6499153fbefaf4")]
public class DeleteUnitFromSummonPool : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintSummonPool SummonPool
	{
		get
		{
			return m_SummonPool?.Get();
		}
		set
		{
			m_SummonPool = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintSummonPoolReference>();
		}
	}

	public override string GetDescription()
	{
		return $"Удаляет юнита {Unit} из саммонпула {SummonPool}";
	}

	public override string GetCaption()
	{
		return $"Delete {Unit} from summon pool {SummonPool}";
	}

	protected override void RunAction()
	{
		Game.Instance.SummonPools.Unregister(SummonPool, Unit.GetValue());
	}
}
