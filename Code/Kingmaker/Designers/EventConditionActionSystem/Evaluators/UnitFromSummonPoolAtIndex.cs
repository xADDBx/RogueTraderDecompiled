using System.Linq;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("9d5137eb894c09141942280f5aa15427")]
public class UnitFromSummonPoolAtIndex : AbstractUnitEvaluator
{
	[SerializeReference]
	public IntEvaluator Index;

	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetDescription()
	{
		return "Возвращает юнита из саммон пула по индексу, начиная с 0. Дизайнер, использующий этот эвалюатор, должен гарантировать, что юнит под таким индексом существует";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(SummonPool);
		if (summonPool == null)
		{
			return null;
		}
		int value = Index.GetValue();
		if (summonPool.Units.Count() <= value)
		{
			return null;
		}
		return summonPool.Units.ToList()[value] as BaseUnitEntity;
	}

	public override string GetCaption()
	{
		return $"{Index} unit from {SummonPool}";
	}
}
