using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/AddUnitToSummonPool")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("8a4f41327b18c0f47b0a1d132429f14e")]
public class AddUnitToSummonPool : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetDescription()
	{
		return $"Добавляет юнита {Unit} в саммонпул {SummonPool}";
	}

	public override string GetCaption()
	{
		return $"Add {Unit} to summon pool {SummonPool}";
	}

	protected override void RunAction()
	{
		Game.Instance.SummonPools.Register(SummonPool, Unit.GetValue());
	}
}
