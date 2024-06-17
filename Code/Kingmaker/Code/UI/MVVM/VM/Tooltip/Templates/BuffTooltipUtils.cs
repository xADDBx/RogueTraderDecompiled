using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public static class BuffTooltipUtils
{
	public static string GetDuration(Buff buff)
	{
		if (buff.IsPermanent)
		{
			return Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.CommonTexts.PermanentBuffTimer;
		}
		if (buff.ExpirationInRounds <= 0)
		{
			return string.Empty;
		}
		string arg = ((buff.ExpirationInRounds == 1) ? Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round : Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds);
		return $"{buff.ExpirationInRounds} {arg}";
	}
}
