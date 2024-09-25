using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("aab1d9de84cd4cb5a934ac4d7ab7f1bd")]
public class StarshipCrewCountBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStarshipCrewMaxCount>, IRulebookHandler<RuleCalculateStarshipCrewMaxCount>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private ShipModuleType m_ModuleType;

	[SerializeField]
	private int m_Bonus;

	void IRulebookHandler<RuleCalculateStarshipCrewMaxCount>.OnEventAboutToTrigger(RuleCalculateStarshipCrewMaxCount evt)
	{
		if (evt.ShipModuleType != m_ModuleType)
		{
			evt.Bonus = m_Bonus;
			PFLog.Default.Log($"Add crew max count bonus {m_Bonus} for module {m_ModuleType}. {evt.Default}");
		}
	}

	void IRulebookHandler<RuleCalculateStarshipCrewMaxCount>.OnEventDidTrigger(RuleCalculateStarshipCrewMaxCount evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
