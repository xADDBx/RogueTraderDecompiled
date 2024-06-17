using System;
using Kingmaker.GameModes;

namespace Kingmaker.QA;

internal class SpamDetectingException : Exception
{
	public readonly GameModeType GameModeType;

	public SpamDetectingException(string exceptionMessage, GameModeType gameModeType)
		: base(exceptionMessage)
	{
		GameModeType = gameModeType;
	}
}
