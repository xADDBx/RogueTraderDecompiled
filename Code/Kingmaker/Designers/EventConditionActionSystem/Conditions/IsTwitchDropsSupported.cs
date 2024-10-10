using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Twitch;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("8d5c262f397b402098425d9c51f8d434")]
public class IsTwitchDropsSupported : Condition
{
	protected override string GetConditionCaption()
	{
		return "Twitch Drops are supported on this platform";
	}

	protected override bool CheckCondition()
	{
		return TwitchDropsManager.Instance.IsPlatformSupported;
	}
}
