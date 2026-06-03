using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("132e18dde99b0194b908d9ef8587e1e6")]
public class FreezeAnimationByBuff : UnitBuffComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		SetDisabled(disabled: true);
		base.OnActivateOrPostLoad();
	}

	protected override void OnDeactivate()
	{
		SetDisabled(disabled: false);
		base.OnDeactivate();
	}

	protected override void OnViewDidAttach()
	{
		SetDisabled(disabled: true);
		base.OnViewDidAttach();
	}

	protected override void OnViewWillDetach()
	{
		SetDisabled(disabled: false);
		base.OnViewWillDetach();
	}

	private void SetDisabled(bool disabled)
	{
		UnitAnimationManager unitAnimationManager = base.Owner.View.Or(null)?.AnimationManager;
		if (unitAnimationManager != null)
		{
			unitAnimationManager.Disabled = disabled;
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
