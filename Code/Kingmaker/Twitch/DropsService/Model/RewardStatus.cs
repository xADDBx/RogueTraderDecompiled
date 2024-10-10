using System.Runtime.Serialization;

namespace Kingmaker.Twitch.DropsService.Model;

public enum RewardStatus
{
	[EnumMember(Value = "UNKNOWN")]
	Unknown,
	[EnumMember(Value = "CLAIMED")]
	Claimed,
	[EnumMember(Value = "FULFILLED")]
	Fulfilled
}
