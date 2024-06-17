using System;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.Logging;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Cheats;

[MemoryPackable(GenerateType.Object)]
public class RunExternalCheatGameCommand : GameCommand, IMemoryPackable<RunExternalCheatGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RunExternalCheatGameCommandFormatter : MemoryPackFormatter<RunExternalCheatGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RunExternalCheatGameCommand value)
		{
			RunExternalCheatGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RunExternalCheatGameCommand value)
		{
			RunExternalCheatGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Command;

	[JsonProperty]
	[MemoryPackInclude]
	private string m_CommandWithArgs;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RunExternalCheatGameCommand()
	{
	}

	[JsonConstructor]
	public RunExternalCheatGameCommand(string command, string commandWithArgs)
		: this(command, commandWithArgs, null)
	{
	}

	public RunExternalCheatGameCommand(string command, string commandWithArgs, TaskCompletionSource<bool> tcs)
	{
		m_Command = command;
		m_CommandWithArgs = commandWithArgs;
		m_Tcs = tcs;
	}

	protected override async void ExecuteInternal()
	{
		try
		{
			await ExecuteImpl(m_Command, m_CommandWithArgs);
			if (m_Tcs != null)
			{
				m_Tcs.SetResult(result: true);
				return;
			}
			CheatGameCommandSystem.Logger.Log("Executed external command {0} from other player", m_CommandWithArgs);
		}
		catch (Exception ex)
		{
			if (m_Tcs != null)
			{
				m_Tcs.SetException(ex);
			}
			else
			{
				CheatGameCommandSystem.Logger.Exception(ex);
			}
		}
	}

	private static Task ExecuteImpl(string command, string commandWithArgs)
	{
		return CheatsManagerHolder.System.ExternalExecutor.ExecuteExternalWithDefaultLogging(command, commandWithArgs);
	}

	public static Task Create(string command, string commandWithArgs)
	{
		if (!Game.HasInstance || Game.Instance.CurrentMode == GameModeType.None)
		{
			return ExecuteImpl(command, commandWithArgs);
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		RunExternalCheatGameCommand cmd = new RunExternalCheatGameCommand(command, commandWithArgs, taskCompletionSource);
		Game.Instance.GameCommandQueue.AddCommand(cmd);
		return taskCompletionSource.Task;
	}

	static RunExternalCheatGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RunExternalCheatGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RunExternalCheatGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RunExternalCheatGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RunExternalCheatGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RunExternalCheatGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Command);
		writer.WriteString(value.m_CommandWithArgs);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RunExternalCheatGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string command;
		string commandWithArgs;
		if (memberCount == 2)
		{
			if (value != null)
			{
				command = value.m_Command;
				commandWithArgs = value.m_CommandWithArgs;
				command = reader.ReadString();
				commandWithArgs = reader.ReadString();
				goto IL_0093;
			}
			command = reader.ReadString();
			commandWithArgs = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RunExternalCheatGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				command = null;
				commandWithArgs = null;
			}
			else
			{
				command = value.m_Command;
				commandWithArgs = value.m_CommandWithArgs;
			}
			if (memberCount != 0)
			{
				command = reader.ReadString();
				if (memberCount != 1)
				{
					commandWithArgs = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0093;
			}
		}
		value = new RunExternalCheatGameCommand
		{
			m_Command = command,
			m_CommandWithArgs = commandWithArgs
		};
		return;
		IL_0093:
		value.m_Command = command;
		value.m_CommandWithArgs = commandWithArgs;
	}
}
