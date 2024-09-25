using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("0f1d13b6f0674519bef963e4457dc164")]
public class TutorialTriggerStarshipLevelUp : TutorialTrigger, IStarshipLevelUpHandler, ISubscriber<IStarshipEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private int m_Level = 2;

	public void HandleStarshipLevelUp(int newLevel, LevelUpManager manager)
	{
		if (newLevel == m_Level)
		{
			TryToTrigger(null);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
