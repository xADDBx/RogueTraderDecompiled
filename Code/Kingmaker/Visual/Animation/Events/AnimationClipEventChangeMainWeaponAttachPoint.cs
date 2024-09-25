using System;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventChangeMainWeaponAttachPoint : AnimationClipEvent
{
	public bool InMainHand;

	public AnimationClipEventChangeMainWeaponAttachPoint()
	{
	}

	public AnimationClipEventChangeMainWeaponAttachPoint(float time)
		: base(time)
	{
	}

	public AnimationClipEventChangeMainWeaponAttachPoint(float time, bool inMainHand)
		: base(time)
	{
		InMainHand = inMainHand;
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().ChangeAttachPointForMainHandWeapon(InMainHand);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventChangeMainWeaponAttachPoint(base.Time, InMainHand);
	}

	public override string ToString()
	{
		return $"Changing the attach point of main hand weapon at {base.Time}";
	}
}
