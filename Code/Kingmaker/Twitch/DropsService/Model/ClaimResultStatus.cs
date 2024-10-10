using System.Runtime.Serialization;

namespace Kingmaker.Twitch.DropsService.Model;

public enum ClaimResultStatus
{
	[EnumMember(Value = "UNKNOWN")]
	Unknown,
	[EnumMember(Value = "SUCCESS")]
	Success,
	[EnumMember(Value = "INVALID_ID")]
	InvalidId,
	[EnumMember(Value = "NOT_FOUND")]
	NotFound,
	[EnumMember(Value = "UNAUTHORIZED")]
	Unauthorized,
	[EnumMember(Value = "UPDATE_FAILED")]
	UpdateFailed
}
