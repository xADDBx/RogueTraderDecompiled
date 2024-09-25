using System;

namespace Kingmaker.Networking.Player;

public readonly struct PlayerInfo : IComparable<PlayerInfo>
{
	public static readonly PlayerInfo Invalid = new PlayerInfo(PhotonActorNumber.Invalid, null, null, isActive: false);

	public readonly PhotonActorNumber Player;

	public readonly string UserId;

	public readonly string NickName;

	public readonly bool IsActive;

	public bool IsInvalid => Player.IsValid;

	public NetPlayer NetPlayer => Player.ToNetPlayer(NetPlayer.Empty);

	public PlayerInfo(PhotonActorNumber player, string userId, string nickName, bool isActive)
	{
		Player = player;
		UserId = userId;
		NickName = nickName;
		IsActive = isActive;
	}

	public PlayerInfo(PlayerInfo other, bool isActive)
		: this(other.Player, other.UserId, other.NickName, isActive)
	{
	}

	int IComparable<PlayerInfo>.CompareTo(PlayerInfo other)
	{
		int num = Player.CompareTo(other.Player);
		if (num != 0)
		{
			return num;
		}
		return string.Compare(UserId, other.UserId, StringComparison.Ordinal);
	}
}
