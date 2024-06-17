using System.Collections.Generic;
using System.Diagnostics;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.Networking;

public static class MessageDebug
{
	public const bool Enabled = false;

	[Conditional("FALSE")]
	public static void Reset()
	{
	}

	[Conditional("FALSE")]
	public static void OnAdded(GameCommand gameCommand)
	{
	}

	[Conditional("FALSE")]
	public static void OnSend(List<GameCommand> gameCommands)
	{
	}

	[Conditional("FALSE")]
	public static void OnExecuted(List<(NetPlayer, GameCommand)> gameCommands)
	{
	}

	[Conditional("FALSE")]
	public static void OnAdded(UnitCommandParams unitCommandParams)
	{
	}

	[Conditional("FALSE")]
	public static void OnSend(List<UnitCommandParams> unitCommandParams)
	{
	}

	[Conditional("FALSE")]
	public static void OnExecuted(List<UnitCommandParams> unitCommandParams)
	{
	}

	[Conditional("FALSE")]
	public static void OnExecuted(UnitCommandMessage unitCommandMessage)
	{
	}
}
