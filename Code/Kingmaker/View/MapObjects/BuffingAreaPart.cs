using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class BuffingAreaPart : InteractionPart<BuffingAreaSettings>, IHashable
{
	protected override void OnInteract(BaseUnitEntity user)
	{
		GameHelper.ApplyBuff(user, base.Settings.Buff, base.Settings.BuffDuration);
		PFLog.Default.Log("BuffApplied");
	}

	public override void OnUnitLeftProximity(BaseUnitEntity unitEntityData)
	{
		GameHelper.RemoveBuff(unitEntityData, base.Settings.Buff);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
