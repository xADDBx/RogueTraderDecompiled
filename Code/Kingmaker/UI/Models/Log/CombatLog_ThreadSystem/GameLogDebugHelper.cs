using System.IO;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;

public static class GameLogDebugHelper
{
	private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("Log Events");

	public static void Log(this GameLogEvent evt)
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		using StringWriter stringWriter = new StringWriter(pooledStringBuilder.Builder);
		stringWriter.WriteEventsTree(evt, 0);
		Channel.Log(stringWriter.ToString());
	}

	private static void WriteEventsTree(this TextWriter writer, GameLogEvent evt, int depth)
	{
		writer.WriteLine(evt, depth);
		foreach (GameLogEvent innerEvent in evt.InnerEvents)
		{
			writer.WriteEventsTree(innerEvent, depth + 1);
		}
	}

	private static void WriteLine(this TextWriter writer, GameLogEvent evt, int depth)
	{
		while (depth-- > 0)
		{
			writer.Write('\t');
		}
		RulebookEvent rulebookEvent = evt.AsRuleEvent()?.Rule;
		if (rulebookEvent != null)
		{
			writer.Write(rulebookEvent.GetType().Name);
			writer.Write(": ");
			writer.Write(rulebookEvent.Initiator);
			IMechanicEntity ruleTarget = rulebookEvent.GetRuleTarget();
			if (ruleTarget != null)
			{
				writer.Write(" -> ");
				writer.Write(ruleTarget);
			}
		}
		else if (evt is GameLogEventAbility gameLogEventAbility)
		{
			writer.Write("Use Ability: ");
			writer.Write(gameLogEventAbility.Ability);
			writer.Write("; target: ");
			writer.Write(gameLogEventAbility.Context.ClickedTarget);
		}
		else
		{
			writer.Write(evt.GetType().Name);
		}
		writer.WriteLine();
	}
}
