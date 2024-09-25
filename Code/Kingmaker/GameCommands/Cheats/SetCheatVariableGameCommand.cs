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
public class SetCheatVariableGameCommand : GameCommand, IMemoryPackable<SetCheatVariableGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetCheatVariableGameCommandFormatter : MemoryPackFormatter<SetCheatVariableGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetCheatVariableGameCommand value)
		{
			SetCheatVariableGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetCheatVariableGameCommand value)
		{
			SetCheatVariableGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Command;

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Value;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetCheatVariableGameCommand()
	{
	}

	[JsonConstructor]
	public SetCheatVariableGameCommand(string command, string value)
		: this(command, value, null)
	{
	}

	[JsonConstructor]
	public SetCheatVariableGameCommand(string command, string value, TaskCompletionSource<bool> tcs)
	{
		m_Command = command;
		m_Value = value;
	}

	protected override async void ExecuteInternal()
	{
		try
		{
			await ExecuteImpl(m_Command, m_Value);
			if (m_Tcs != null)
			{
				m_Tcs.SetResult(result: true);
				return;
			}
			CheatGameCommandSystem.Logger.Log("Set variable {0} to {1} on command from other player", m_Command, m_Value);
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

	private static Task ExecuteImpl(string command, string value)
	{
		return CheatsManagerHolder.System.VariableExecutor.ExecuteSetVariableWithDefaultLogging(command, value);
	}

	public static Task Create(string command, string value)
	{
		if (!Game.HasInstance || Game.Instance.CurrentMode == GameModeType.None)
		{
			return ExecuteImpl(command, value);
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		SetCheatVariableGameCommand cmd = new SetCheatVariableGameCommand(command, value, taskCompletionSource);
		Game.Instance.GameCommandQueue.AddCommand(cmd);
		return taskCompletionSource.Task;
	}

	static SetCheatVariableGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetCheatVariableGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetCheatVariableGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetCheatVariableGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetCheatVariableGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetCheatVariableGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Command);
		writer.WriteString(value.m_Value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetCheatVariableGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string command;
		string value2;
		if (memberCount == 2)
		{
			if (value != null)
			{
				command = value.m_Command;
				value2 = value.m_Value;
				command = reader.ReadString();
				value2 = reader.ReadString();
				goto IL_0093;
			}
			command = reader.ReadString();
			value2 = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetCheatVariableGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				command = null;
				value2 = null;
			}
			else
			{
				command = value.m_Command;
				value2 = value.m_Value;
			}
			if (memberCount != 0)
			{
				command = reader.ReadString();
				if (memberCount != 1)
				{
					value2 = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0093;
			}
		}
		value = new SetCheatVariableGameCommand
		{
			m_Command = command,
			m_Value = value2
		};
		return;
		IL_0093:
		value.m_Command = command;
		value.m_Value = value2;
	}
}
