using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UniRx;

namespace Kingmaker.Utility;

public class ReportCombatLogManager : IDisposable, IPartyCombatHandler, ISubscriber
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("ReportCombatLogManager");

	private readonly ReportingUtils m_ReportingUtils;

	private int m_CurrentCombatId;

	private bool m_IsCombatActive;

	private string m_FilePath;

	private readonly IDisposable[] m_Disposables;

	[NotNull]
	private readonly CancellationTokenSource m_Cts = new CancellationTokenSource();

	private readonly StringBuilder m_StringBuilder = new StringBuilder();

	private static readonly string[] Delimiters = new string[2] { "\r\n", "\n" };

	public void Dispose()
	{
		m_Cts.Cancel();
		m_Cts.Dispose();
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		EventBus.Unsubscribe(this);
	}

	public ReportCombatLogManager(string fileFolder, string fileName, ReportingUtils reportingUtils)
	{
		m_ReportingUtils = reportingUtils;
		InitAsync(fileFolder, fileName, m_Cts.Token);
		EventBus.Subscribe(this);
		m_Disposables = LogThreadService.Instance.AllThreads.Select((LogThreadBase v) => v.ObserveAdd().Subscribe(delegate(CollectionAddEvent<CombatLogMessage> msg)
		{
			LogCombatLogMessage(msg.Value);
		})).ToArray();
	}

	private void LogCombatLogMessage(CombatLogMessage msg)
	{
		ManageCombatMessageData(msg, (MechanicEntity)(IMechanicEntity)GameLogContext.SourceEntity, (MechanicEntity)(IMechanicEntity)GameLogContext.TargetEntity);
	}

	private StreamWriter OpenFile(string fileFolder, string fileName)
	{
		try
		{
			m_FilePath = Path.Combine(fileFolder, fileName);
			return new StreamWriter(m_FilePath, append: false)
			{
				AutoFlush = true
			};
		}
		catch (AccessViolationException)
		{
		}
		catch (Exception ex2)
		{
			m_ReportingUtils?.LogReporterError(ex2.StackTrace);
			throw;
		}
		try
		{
			m_FilePath = Path.Combine(fileFolder, Guid.NewGuid().ToString() + "_" + fileName);
			return new StreamWriter(m_FilePath, append: false)
			{
				AutoFlush = true
			};
		}
		catch (Exception ex3)
		{
			m_ReportingUtils?.LogReporterError(ex3.StackTrace);
			throw;
		}
	}

	private async Task InitAsync(string fileFolder, string fileName, CancellationToken ct)
	{
		StreamWriter logFileWriter = OpenFile(fileFolder, fileName);
		try
		{
			await Task.Run(() => AsyncFileWrite(logFileWriter, ct), ct);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			m_ReportingUtils?.LogReporterError(ex2.StackTrace);
			throw;
		}
		finally
		{
			if (logFileWriter != null)
			{
				await ((IAsyncDisposable)logFileWriter).DisposeAsync();
			}
		}
	}

	private async Task AsyncFileWrite(StreamWriter logFileWriter, CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromSeconds(1.0), ct);
			try
			{
				string text;
				lock (m_StringBuilder)
				{
					if (m_StringBuilder.Length == 0)
					{
						continue;
					}
					text = m_StringBuilder.ToString();
					m_StringBuilder.Clear();
					goto IL_0142;
				}
				IL_0142:
				await logFileWriter.WriteAsync(text.AsMemory(), ct);
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
		}
	}

	public void CopyFile(string path, ReportingUtils reportingUtils)
	{
		try
		{
			lock (m_StringBuilder)
			{
				File.Copy(m_FilePath, path, overwrite: true);
				try
				{
					string input = File.ReadAllText(path);
					input = Regex.Replace(input, "<(color|/color|b|/b)[^>]{0,}>", "");
					File.WriteAllText(path, input);
				}
				catch (Exception ex)
				{
					Logger.Exception(ex);
				}
			}
		}
		catch (Exception ex2)
		{
			reportingUtils.LogReporterError("Failed edit copied combat log file: \n" + ex2.Message + "\n" + ex2.StackTrace);
		}
	}

	public void HandlePartyCombatStateChanged(bool isStarted)
	{
		if (isStarted)
		{
			CombatStarted();
		}
		else
		{
			CombatEnded();
		}
	}

	private void CombatStarted()
	{
		lock (m_StringBuilder)
		{
			m_IsCombatActive = true;
			m_CurrentCombatId++;
			m_StringBuilder.Append("Combat Started [").Append(m_CurrentCombatId).Append("]")
				.AppendLine();
			m_StringBuilder.AppendLine();
			m_StringBuilder.Append("PlayerCharacter Name: ").Append(Game.Instance.Player.MainCharacter.Entity.CharacterName);
			m_StringBuilder.AppendLine();
			m_StringBuilder.AppendLine();
			m_StringBuilder.Append("DifficultyType: ").Append(SettingsRoot.Difficulty.GameDifficulty.GetValue()).Append("    ");
			m_StringBuilder.AppendLine();
			UnitGroupEnumerator enumerator = Game.Instance.Player.Group.GetEnumerator();
			while (enumerator.MoveNext())
			{
				BaseUnitEntity current = enumerator.Current;
				using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
				StringBuilder builder = pooledStringBuilder.Builder;
				builder.Append(current.CharacterName).Append(" (");
				foreach (ClassData @class in current.Progression.Classes)
				{
					builder.Append(@class.CharacterClass.NameSafe()).Append(": ").Append(@class.Level)
						.Append(" ");
				}
				builder.Append(")\n");
				m_StringBuilder.Append(builder.ToString());
			}
			m_StringBuilder.AppendLine();
			m_StringBuilder.AppendLine();
		}
	}

	private void CombatEnded()
	{
		lock (m_StringBuilder)
		{
			m_IsCombatActive = false;
			m_StringBuilder.AppendLine();
			m_StringBuilder.AppendLine();
			m_StringBuilder.Append("Combat Ended [").Append(m_CurrentCombatId).Append("]");
		}
	}

	private void ManageCombatMessageData(CombatLogMessage combatLogMessage, MechanicEntity source = null, MechanicEntity target = null)
	{
		using (ProfileScope.New("ReportCombatLog"))
		{
			try
			{
				if (!m_IsCombatActive)
				{
					return;
				}
				string text;
				string tooltipHeader;
				if (combatLogMessage.Tooltip is TooltipTemplateSimple tooltipTemplateSimple)
				{
					tooltipHeader = tooltipTemplateSimple.Header ?? "";
					text = tooltipTemplateSimple.Description;
				}
				else
				{
					tooltipHeader = combatLogMessage.Message ?? "";
					text = string.Empty;
				}
				tooltipHeader = ManageTooltipHeader(tooltipHeader, source, target);
				lock (m_StringBuilder)
				{
					m_StringBuilder.AppendLine(tooltipHeader);
					if (!string.IsNullOrWhiteSpace(text))
					{
						ManageTooltipDescription(m_StringBuilder, text);
					}
					m_StringBuilder.AppendLine();
				}
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
		}
	}

	private static string ManageTooltipHeader(string tooltipHeader, MechanicEntity source, MechanicEntity target)
	{
		string text = source?.GetDescriptionOptional()?.Name;
		if (source != null && !string.IsNullOrEmpty(text))
		{
			tooltipHeader = ReplaceNameWithBlueprint(tooltipHeader, source, text);
		}
		string text2 = target?.GetDescriptionOptional()?.Name;
		if (target != null && !string.IsNullOrEmpty(text2))
		{
			tooltipHeader = ReplaceNameWithBlueprint(tooltipHeader, target, text2);
		}
		return tooltipHeader;
	}

	private static void ManageTooltipDescription(StringBuilder sb, string tooltipDescription)
	{
		string[] array = tooltipDescription.Split(Delimiters, StringSplitOptions.None);
		foreach (string value in array)
		{
			if (!string.IsNullOrEmpty(value))
			{
				sb.Append('\t').AppendLine(value);
			}
		}
	}

	private static string ReplaceNameWithBlueprint(string str, MechanicEntity entity, string name)
	{
		if (name == null)
		{
			return str;
		}
		string text = entity.UniqueId;
		string text2 = entity.Blueprint.NameSafe();
		int num = text.IndexOf('-');
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		string newValue = text2 + "[" + text + "]";
		return str.Replace(name, newValue);
	}
}
