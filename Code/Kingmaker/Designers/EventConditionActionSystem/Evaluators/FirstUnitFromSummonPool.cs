using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("d182d1ff68a8efd45bba3d39c75f5d8d")]
[PlayerUpgraderAllowed(true)]
public class FirstUnitFromSummonPool : AbstractUnitEvaluator
{
	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	public ConditionsChecker Conditions;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return null;
		}
		foreach (AbstractUnitEntity unit in summonPool.Units)
		{
			using (ContextData<SummonPoolUnitData>.Request().Setup(unit))
			{
				if (unit.WillBeDestroyed || !Conditions.Check())
				{
					continue;
				}
				return unit;
			}
		}
		return null;
	}

	public override string GetCaption()
	{
		return $"First unit from {SummonPool}";
	}
}
