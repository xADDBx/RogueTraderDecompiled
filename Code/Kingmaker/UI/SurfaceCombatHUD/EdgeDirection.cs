using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public readonly struct EdgeDirection : IEquatable<EdgeDirection>
{
	public const byte kIndexE = 0;

	public const byte kIndexN = 1;

	public const byte kIndexW = 2;

	public const byte kIndexS = 3;

	public static readonly EdgeDirection E = new EdgeDirection(0);

	public static readonly EdgeDirection N = new EdgeDirection(1);

	public static readonly EdgeDirection W = new EdgeDirection(2);

	public static readonly EdgeDirection S = new EdgeDirection(3);

	private readonly byte m_Index;

	public int Flag
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return 1 << (int)m_Index;
		}
	}

	public int Index
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Index;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeDirection(byte index)
	{
		m_Index = index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeDirection TurnCounterClockwise90()
	{
		return new EdgeDirection((byte)((m_Index + 1) % 4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeDirection TurnClockwise90()
	{
		return new EdgeDirection((byte)((m_Index + 3) % 4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeDirection Turn180()
	{
		return new EdgeDirection((byte)((m_Index + 2) % 4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(EdgeDirection other)
	{
		return m_Index == other.m_Index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		byte index = m_Index;
		return index.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is EdgeDirection other)
		{
			return Equals(other);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EdgeDirection operator -(EdgeDirection value)
	{
		return value.Turn180();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(EdgeDirection left, EdgeDirection right)
	{
		return left.m_Index == right.m_Index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(EdgeDirection left, EdgeDirection right)
	{
		return left.m_Index != right.m_Index;
	}
}
