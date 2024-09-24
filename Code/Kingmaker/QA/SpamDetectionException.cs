using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.GameModes;

namespace Kingmaker.QA;

internal class SpamDetectionException : Exception
{
	public readonly GameModeType GameModeType;

	private readonly IEnumerable<LogItem> m_Items;

	private string m_CachedMessage;

	public override string Message
	{
		get
		{
			if (m_CachedMessage != null)
			{
				return m_CachedMessage;
			}
			StringBuilder stringBuilder = new StringBuilder("Logging calls that triggered a Spam exception\n");
			foreach (LogItem item in m_Items)
			{
				stringBuilder.AppendLine(item.Message);
				stringBuilder.AppendLine(item.Callstack);
			}
			m_CachedMessage = stringBuilder.ToString();
			return m_CachedMessage;
		}
	}

	public override string StackTrace => (from s in m_Items.Select((LogItem item) => item.Callstack).ToHashSet()
		orderby s
		select s).Aggregate(new StringBuilder(1024), (StringBuilder builder, string s) => builder.AppendLine(s)).ToString();

	public SpamDetectionException(GameModeType gameModeType, IEnumerable<LogItem> items)
	{
		GameModeType = gameModeType;
		m_Items = items;
	}
}
