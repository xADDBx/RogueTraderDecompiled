namespace Kingmaker.Visual.Animation.Kingmaker;

internal static class UnitAnimationActionHanldeEx
{
	public static bool NullOrFinished(this UnitAnimationActionHandle h)
	{
		return h?.IsFinished ?? true;
	}
}
