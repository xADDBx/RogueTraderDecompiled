using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Networking;

namespace Kingmaker.GameCommands.Contexts;

public class GameCommandPlayer : ContextData<GameCommandPlayer>
{
	public GameCommand Command { get; private set; }

	public NetPlayer Player { get; private set; }

	public static bool IsCommandExecution(Type commandType)
	{
		if (ContextData<GameCommandPlayer>.Current != null)
		{
			return ContextData<GameCommandPlayer>.Current.Command.GetType() == commandType;
		}
		return false;
	}

	public static NetPlayer GetPlayerOrEmpty()
	{
		return GetPlayerOr(NetPlayer.Empty);
	}

	public static NetPlayer GetPlayerOr(NetPlayer defaultValue)
	{
		return ContextData<GameCommandPlayer>.Current?.Player ?? defaultValue;
	}

	public GameCommandPlayer Setup(GameCommand command, NetPlayer player)
	{
		Command = command;
		Player = player;
		return this;
	}

	protected override void Reset()
	{
		Command = null;
		Player = NetPlayer.Empty;
	}
}
