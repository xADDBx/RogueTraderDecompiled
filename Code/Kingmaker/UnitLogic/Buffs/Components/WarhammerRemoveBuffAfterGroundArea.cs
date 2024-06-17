using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("b9357f92dbb44df4882680382c77fb71")]
public class WarhammerRemoveBuffAfterGroundArea : UnitBuffComponentDelegate, IAreaHandler, ISubscriber, IHashable
{
	public List<BlueprintAreaReference> AreaExceptions = new List<BlueprintAreaReference>();

	public void OnAreaBeginUnloading()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.Default && !AreaExceptions.Contains(Game.Instance.CurrentlyLoadedArea.ToReference<BlueprintAreaReference>()))
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
