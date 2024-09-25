using System;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SaveGameCommand : GameCommand, IMemoryPackable<SaveGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SaveGameCommandFormatter : MemoryPackFormatter<SaveGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveGameCommand value)
		{
			SaveGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveGameCommand value)
		{
			SaveGameCommand.Deserialize(ref reader, ref value);
		}
	}

	private readonly SaveInfo m_SaveInfo;

	private readonly string m_SaveName;

	private readonly Action m_Callback;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly SaveInfo.SaveType m_Type;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SaveGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SaveGameCommand(SaveInfo.SaveType m_type)
	{
		m_Type = m_type;
	}

	public SaveGameCommand([CanBeNull] SaveInfo saveInfo, [CanBeNull] string saveName, Action callback = null)
		: this(saveInfo?.Type ?? SaveInfo.SaveType.Manual)
	{
		m_SaveInfo = saveInfo;
		m_SaveName = saveName;
		m_Callback = callback;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		if (!playerOrEmpty.IsLocal)
		{
			AnotherPlayerSavesGame(playerOrEmpty, m_Type);
			return;
		}
		SaveInfo saveInfo = m_SaveInfo ?? Game.Instance.SaveManager.CreateNewSave(m_SaveName);
		if (!string.IsNullOrEmpty(m_SaveName))
		{
			saveInfo.Name = m_SaveName;
		}
		Game.Instance.SaveGame(saveInfo, m_Callback);
	}

	public static bool PreSaveGame(SaveInfo.SaveType saveType)
	{
		if (!Game.Instance.SaveManager.IsSaveAllowed(saveType))
		{
			return false;
		}
		Game.Instance.SaveManager.PreSave();
		return true;
	}

	private static void AnotherPlayerSavesGame(NetPlayer player, SaveInfo.SaveType saveType)
	{
		if (PreSaveGame(saveType) && NetworkingManager.GetNickName(player, out var nickName))
		{
			PFLog.GameCommands.Log("[SaveGameCommand] Player '" + nickName + "' is saving the game...");
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.GameSavedInProgress);
			});
		}
	}

	static SaveGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SaveGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveInfo.SaveType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SaveInfo.SaveType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Type);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		SaveInfo.SaveType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<SaveInfo.SaveType>(out value2);
			}
			else
			{
				value2 = value.m_Type;
				reader.ReadUnmanaged<SaveInfo.SaveType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Type : SaveInfo.SaveType.Manual);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<SaveInfo.SaveType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SaveGameCommand(value2);
	}
}
