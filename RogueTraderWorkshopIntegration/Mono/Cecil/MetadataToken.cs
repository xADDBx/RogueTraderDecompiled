using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public struct MetadataToken : IEquatable<MetadataToken>
{
	private readonly uint token;

	public static readonly MetadataToken Zero = new MetadataToken(0u);

	public uint RID => token & 0xFFFFFFu;

	public TokenType TokenType => (TokenType)(token & 0xFF000000u);

	public MetadataToken(uint token)
	{
		this.token = token;
	}

	public MetadataToken(TokenType type)
		: this(type, 0)
	{
	}

	public MetadataToken(TokenType type, uint rid)
	{
		token = (uint)type | rid;
	}

	public MetadataToken(TokenType type, int rid)
	{
		token = (uint)type | (uint)rid;
	}

	public int ToInt32()
	{
		return (int)token;
	}

	public uint ToUInt32()
	{
		return token;
	}

	public override int GetHashCode()
	{
		return (int)token;
	}

	public bool Equals(MetadataToken other)
	{
		return other.token == token;
	}

	public override bool Equals(object obj)
	{
		if (obj is MetadataToken)
		{
			return ((MetadataToken)obj).token == token;
		}
		return false;
	}

	public static bool operator ==(MetadataToken one, MetadataToken other)
	{
		return one.token == other.token;
	}

	public static bool operator !=(MetadataToken one, MetadataToken other)
	{
		return one.token != other.token;
	}

	public override string ToString()
	{
		return string.Format("[{0}:0x{1}]", TokenType, RID.ToString("x4"));
	}
}
