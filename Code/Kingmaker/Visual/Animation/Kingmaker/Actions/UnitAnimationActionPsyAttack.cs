namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionPsyAttack : UnitAnimationActionClip
{
	public override UnitAnimationType Type => UnitAnimationType.PsyAttack;

	public override bool IsAdditive => false;
}
