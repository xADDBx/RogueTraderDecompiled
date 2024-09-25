using System;

namespace Kingmaker.Networking;

public readonly struct NetPlayer : IEquatable<NetPlayer>, IComparable<NetPlayer>
{
	private const byte NotLocalMask = 0;

	private const byte IsLocalMask = 1;

	private const byte OfflineIndex = 0;

	private const byte MinValue = 1;

	private const byte MaxValue = 6;

	public static readonly NetPlayer Empty = new NetPlayer((byte)0, isLocal: false);

	public static readonly NetPlayer Offline = new NetPlayer((byte)0, isLocal: true);

	private readonly byte m_Index;

	private readonly byte m_IsLocal;

	public int Index => m_Index;

	public bool IsLocal => m_IsLocal == 1;

	public bool IsEmpty => Empty.Equals(this);

	public bool IsOffline => Offline.Equals(this);

	public int Mask => (Math.Sign(Index) * (1 << Index)) | m_IsLocal;

	private NetPlayer(byte index, bool isLocal)
	{
		m_Index = index;
		m_IsLocal = (byte)(isLocal ? 1 : 0);
	}

	public NetPlayer(int playerIndex, bool isLocal = false)
	{
		if (playerIndex < 1 || 6 < playerIndex)
		{
			throw new ArgumentOutOfRangeException($"NetPlayer i={playerIndex}");
		}
		m_Index = (byte)playerIndex;
		m_IsLocal = (byte)(isLocal ? 1 : 0);
	}

	public override string ToString()
	{
		return Convert.ToString(Mask, 2);
	}

	public bool Equals(NetPlayer other)
	{
		if (m_Index == other.m_Index)
		{
			return m_IsLocal == other.m_IsLocal;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is NetPlayer other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		byte index = m_Index;
		int hashCode = index.GetHashCode();
		index = m_IsLocal;
		return hashCode ^ index.GetHashCode();
	}

	public int CompareTo(NetPlayer other)
	{
		byte index = m_Index;
		return index.CompareTo(other.m_Index);
	}
}
