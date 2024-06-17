using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("235f6ba3591e41dcbc434b0c2ba4db2d")]
[PlayerUpgraderAllowed(false)]
public class SummonPoolNonExistsOrEmpty : Condition
{
	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	protected override string GetConditionCaption()
	{
		return $"({m_SummonPool}) pool non exists or empty";
	}

	protected override bool CheckCondition()
	{
		ISummonPool summonPool = Game.Instance.SummonPools.Get(m_SummonPool);
		return summonPool == null || summonPool.Count == 0;
	}
}
