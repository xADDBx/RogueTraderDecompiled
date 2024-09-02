using System;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventChangeWeaponsVisibility : AnimationClipEvent
{
	public bool IsActive;

	public AnimationClipEventChangeWeaponsVisibility()
	{
	}

	public AnimationClipEventChangeWeaponsVisibility(float time)
		: base(time)
	{
	}

	public AnimationClipEventChangeWeaponsVisibility(float time, bool isActive)
		: base(time)
	{
		IsActive = isActive;
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().ChangeShowingWeapon(IsActive);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventChangeWeaponsVisibility(base.Time, IsActive);
	}

	public override string ToString()
	{
		return $"Changing the display of weapons at {base.Time}";
	}
}
