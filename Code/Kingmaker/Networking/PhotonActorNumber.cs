using System;

namespace Kingmaker.Networking;

public readonly struct PhotonActorNumber : IComparable<PhotonActorNumber>, IEquatable<PhotonActorNumber>
{
	public static PhotonActorNumber Invalid = new PhotonActorNumber(-1);

	public readonly int ActorNumber;

	public bool IsValid => ActorNumber != -1;

	public PhotonActorNumber(int actorNumber)
	{
		ActorNumber = actorNumber;
	}

	public NetPlayer ToNetPlayer(NetPlayer defaultValue)
	{
		int num = PhotonManager.Instance.ActorNumberToPlayerIndex(ActorNumber);
		if (num == -1)
		{
			return defaultValue;
		}
		return new NetPlayer(num, num == PhotonManager.Instance.LocalNetPlayer.Index);
	}

	public bool Equals(PhotonActorNumber other)
	{
		return ActorNumber == other.ActorNumber;
	}

	public override bool Equals(object obj)
	{
		if (obj is PhotonActorNumber other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ActorNumber;
	}

	public int CompareTo(PhotonActorNumber other)
	{
		int actorNumber = ActorNumber;
		return actorNumber.CompareTo(other.ActorNumber);
	}

	public override string ToString()
	{
		int actorNumber = ActorNumber;
		return "PhotonActorNumber " + actorNumber;
	}

	public static bool operator ==(PhotonActorNumber lhs, PhotonActorNumber rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(PhotonActorNumber lhs, PhotonActorNumber rhs)
	{
		return !(lhs == rhs);
	}
}
