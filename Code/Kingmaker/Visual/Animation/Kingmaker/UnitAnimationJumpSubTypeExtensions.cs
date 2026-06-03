namespace Kingmaker.Visual.Animation.Kingmaker;

public static class UnitAnimationJumpSubTypeExtensions
{
	public static UnitAnimationType ToAnimationType(this UnitAnimationJumpSubType jumpSubType)
	{
		return jumpSubType switch
		{
			UnitAnimationJumpSubType.HookPulled => UnitAnimationType.HookPulled, 
			UnitAnimationJumpSubType.HookPulling => UnitAnimationType.HookPulling, 
			UnitAnimationJumpSubType.JumpAugment => UnitAnimationType.JumpAugment, 
			UnitAnimationJumpSubType.WarpSlashJumpAugment => UnitAnimationType.WarpSlashJumpAugment, 
			_ => UnitAnimationType.Jump, 
		};
	}
}
