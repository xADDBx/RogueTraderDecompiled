using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("dda6e40519d211546ad63a00860b2f6f")]
public class UnitFromSummonPool : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	protected override string GetConditionCaption()
	{
		return $"Unit ({Unit}) from ({SummonPool}) pool";
	}

	protected override bool CheckCondition()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return false;
		}
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			if (Unit.GetValue() == unit)
			{
				return true;
			}
		}
		return false;
	}
}
