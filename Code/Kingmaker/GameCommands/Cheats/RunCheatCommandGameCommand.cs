using System;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.Logging;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameCommands.Cheats;

[MemoryPackable(GenerateType.Object)]
public class RunCheatCommandGameCommand : GameCommand, IMemoryPackable<RunCheatCommandGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RunCheatCommandGameCommandFormatter : MemoryPackFormatter<RunCheatCommandGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RunCheatCommandGameCommand value)
		{
			RunCheatCommandGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RunCheatCommandGameCommand value)
		{
			RunCheatCommandGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Command;

	[JsonProperty]
	[MemoryPackInclude]
	private string[] m_Args;

	private readonly TaskCompletionSource<bool> m_Tcs;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RunCheatCommandGameCommand()
	{
	}

	[JsonConstructor]
	public RunCheatCommandGameCommand(string command, string[] args)
		: this(command, args, null)
	{
	}

	public RunCheatCommandGameCommand(string command, string[] args, TaskCompletionSource<bool> tcs)
	{
		m_Command = command;
		m_Args = args;
		m_Tcs = tcs;
	}

	protected override async void ExecuteInternal()
	{
		try
		{
			await ExecuteImpl(m_Command, m_Args);
			if (m_Tcs != null)
			{
				m_Tcs.SetResult(result: true);
				return;
			}
			CheatGameCommandSystem.Logger.Log("Executed command {0} {1} received from other player", m_Command, string.Join(" ", m_Args));
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

	private static Task ExecuteImpl(string command, string[] args)
	{
		return CheatsManagerHolder.System.CommandExecutor.ExecuteCommandWithDefaultLogging(command, args);
	}

	public static Task Create(string command, string[] args)
	{
		if (!Application.isPlaying || !Game.HasInstance || Game.Instance.CurrentMode == GameModeType.None)
		{
			return ExecuteImpl(command, args);
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		RunCheatCommandGameCommand cmd = new RunCheatCommandGameCommand(command, args, taskCompletionSource);
		Game.Instance.GameCommandQueue.AddCommand(cmd);
		return taskCompletionSource.Task;
	}

	static RunCheatCommandGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RunCheatCommandGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RunCheatCommandGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RunCheatCommandGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RunCheatCommandGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RunCheatCommandGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Command);
		writer.WriteArray(value.m_Args);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RunCheatCommandGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string[] value2;
		string command;
		if (memberCount == 2)
		{
			if (value != null)
			{
				command = value.m_Command;
				value2 = value.m_Args;
				command = reader.ReadString();
				reader.ReadArray(ref value2);
				goto IL_0098;
			}
			command = reader.ReadString();
			value2 = reader.ReadArray<string>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RunCheatCommandGameCommand), 2, memberCount);
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
				value2 = value.m_Args;
			}
			if (memberCount != 0)
			{
				command = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadArray(ref value2);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0098;
			}
		}
		value = new RunCheatCommandGameCommand
		{
			m_Command = command,
			m_Args = value2
		};
		return;
		IL_0098:
		value.m_Command = command;
		value.m_Args = value2;
	}
}
