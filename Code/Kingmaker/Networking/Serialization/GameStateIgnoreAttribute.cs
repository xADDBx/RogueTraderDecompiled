using System;

namespace Kingmaker.Networking.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class GameStateIgnoreAttribute : Attribute
{
	private string m_Reason;

	public GameStateIgnoreAttribute()
		: this(null)
	{
	}

	public GameStateIgnoreAttribute(string reason)
	{
		m_Reason = reason;
	}
}
