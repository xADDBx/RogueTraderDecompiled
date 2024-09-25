using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("2f27bef52f4606843a1bea657367eda0")]
public class WarhammerRemoveBuffAfterSpaceCombat : UnitBuffComponentDelegate, IAreaHandler, ISubscriber, IHashable
{
	public void OnAreaBeginUnloading()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
		{
			base.Buff.Remove();
		}
	}

	public void OnAreaDidLoad()
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
