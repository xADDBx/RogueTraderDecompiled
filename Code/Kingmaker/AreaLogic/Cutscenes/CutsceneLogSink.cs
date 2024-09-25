using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutsceneLogSink : ILogSink, ICutsceneHandler, ISubscriber<CutscenePlayerData>, ISubscriber
{
	private class InternalData
	{
		public readonly Dictionary<CommandBase, LogInfo> CommandLastErrorIndex = new Dictionary<CommandBase, LogInfo>();

		public CutscenePlayerData CurrentPlayerData;

		public CommandBase CurrentCommand;
	}

	[CanBeNull]
	private static CutsceneLogSink s_Instance;

	private readonly InternalData m_Data;

	[NotNull]
	public static CutsceneLogSink Instance
	{
		get
		{
			Initialize();
			return s_Instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
		if (s_Instance == null)
		{
			s_Instance = new CutsceneLogSink(BuildModeUtility.IsDevelopment);
			Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(s_Instance, populateWithExistingMessages: false);
		}
	}

	private CutsceneLogSink(bool isDevelopment)
	{
		m_Data = (isDevelopment ? new InternalData() : null);
	}

	public void PrepareForLog(CutscenePlayerData player, [CanBeNull] CommandBase command)
	{
		if (m_Data != null)
		{
			m_Data.CurrentPlayerData = player;
			m_Data.CurrentCommand = command;
		}
	}

	public void Log(LogInfo logInfo)
	{
		if (m_Data != null && (logInfo.Source is Cutscene || logInfo.Source is CommandBase) && m_Data.CurrentPlayerData != null)
		{
			m_Data.CurrentPlayerData.LogList.Add(logInfo);
			if (logInfo.Severity == LogSeverity.Error && m_Data.CurrentCommand != null && m_Data.CurrentPlayerData != null)
			{
				m_Data.CommandLastErrorIndex[m_Data.CurrentCommand] = logInfo;
			}
			m_Data.CurrentCommand = null;
			m_Data.CurrentPlayerData = null;
		}
	}

	public bool TryGetCommandErrorInfo(CommandBase command, out LogInfo logInfo)
	{
		if (m_Data == null)
		{
			logInfo = null;
			return false;
		}
		return m_Data.CommandLastErrorIndex.TryGetValue(command, out logInfo);
	}

	public void Destroy()
	{
	}

	public void HandleCutsceneStarted(bool queued)
	{
	}

	public void HandleCutsceneRestarted()
	{
	}

	public void HandleCutscenePaused(CutscenePauseReason reason)
	{
	}

	public void HandleCutsceneResumed()
	{
	}

	public void HandleCutsceneStopped()
	{
		if (m_Data == null)
		{
			return;
		}
		CutscenePlayerData entity = EventInvokerExtensions.GetEntity<CutscenePlayerData>();
		if (entity.PreventDestruction)
		{
			return;
		}
		foreach (CommandBase failedCommand in entity.FailedCommands)
		{
			m_Data.CommandLastErrorIndex.Remove(failedCommand);
		}
	}
}
