namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionBadlyHurt : UnitAnimationActionClip
{
	public override UnitAnimationType Type => UnitAnimationType.BadlyHurt;

	public override bool IsAdditive => false;
}
