namespace Kingmaker.Twitch;

public enum TwitchDropsClaimStatus
{
	Unknown,
	PlatformNotSupported,
	ConnectionError,
	PlayerNotLinked,
	NoRewards,
	ReceivedAll,
	RewardsReceived
}
