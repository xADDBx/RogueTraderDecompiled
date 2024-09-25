using System;
using Kingmaker.ElementsSystem;
using Kingmaker.QA;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

public abstract class CutsceneCommandException : OwlcatException
{
	public readonly CutscenePlayerData Player;

	public readonly CommandBase Command;

	private string m_Message;

	public override string Message => m_Message;

	public override string StackTrace => base.InnerException?.StackTrace ?? base.StackTrace;

	protected CutsceneCommandException(CutscenePlayerData player, CommandBase command, string message, Exception e)
		: base(e)
	{
		Player = player;
		Command = command;
		m_Message = message;
	}

	protected static string GetMessage(CutscenePlayerData player, CommandBase command, Exception e, string action)
	{
		if (!(e is ElementLogicException ex))
		{
			return "Failed to " + action + " cutscene command " + player.Cutscene.name + "." + command.name + " (" + command.AssetGuid + "): " + e.Message;
		}
		return "Failed to " + action + " cutscene command: " + ex.Message;
	}

	protected override void ReportInternal(LogChannel channel, UnityEngine.Object context, bool showReportWindow)
	{
		Owlcat.Runtime.Core.Logging.Logger.Instance.Log(channel, context, LogSeverity.Error, base.InnerException ?? this, Message, null);
		QAModeExceptionReporter.MaybeShowError(Message);
	}
}
