namespace Kingmaker.RuleSystem.Rules;

public static class AttackResultHelper
{
	public static bool IsHit(this AttackResult result)
	{
		if (result != AttackResult.Hit)
		{
			return result == AttackResult.RighteousFury;
		}
		return true;
	}
}
