using System;
using System.Collections.Generic;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UI.Models.Log;

public static class GameLogEventsFactory
{
	private static readonly Dictionary<Type, Func<RulebookEvent, GameLogEvent>> Creators;

	static GameLogEventsFactory()
	{
		Creators = new Dictionary<Type, Func<RulebookEvent, GameLogEvent>>();
		RegisterCreator((RulePerformAttack rule) => new GameLogEventAttack(rule));
		SetupDefaultCreators();
	}

	private static void RegisterCreator<T>(Func<T, GameLogEvent> creator) where T : RulebookEvent
	{
		Creators.Add(typeof(T), (RulebookEvent rule) => creator((T)rule));
	}

	public static GameLogEvent Create(RulebookEvent rule)
	{
		Type type = rule.GetType();
		return Creators.Get(type)?.Invoke(rule) ?? throw new Exception("Missing creator for GameLogRuleEvent<" + type.Name + ">");
	}

	private static void SetupDefaultCreators()
	{
		foreach (Type subclass in typeof(RulebookEvent).GetSubclasses())
		{
			if (!Creators.ContainsKey(subclass))
			{
				Type gameLogEventType = typeof(GameLogRuleEvent<>).MakeGenericType(subclass);
				Creators.Add(subclass, (RulebookEvent rule) => (GameLogEvent)Activator.CreateInstance(gameLogEventType, rule));
			}
		}
	}
}
