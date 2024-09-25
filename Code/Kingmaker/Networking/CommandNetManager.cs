using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Controllers.Net;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.MemoryPack.Formatters;
using Kingmaker.Networking.Settings;
using Kingmaker.QA.Overlays;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using MemoryPack;
using Pathfinding;

namespace Kingmaker.Networking;

public class CommandNetManager
{
	static CommandNetManager()
	{
		RegisterCustomFormatters();
	}

	private static void RegisterCustomFormatters()
	{
		CodeDynamicUnionFormatters.Register();
		PathFormatterInitializer.RegisterFormatter();
		BaseSettingNetDataFormatterInitializer.RegisterFormatter();
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintQuest>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintPortrait>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintArea>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintAreaPart>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintPath>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintSelection>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintFeature>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintPointOfInterest>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintDialog>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintFact>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintAbility>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintAbilityFXSettings>());
		MemoryPackFormatterProvider.Register(new BlueprintFormatter<BlueprintScriptableObject>());
	}

	public void SendAllCommands(int tickIndex, List<GameCommand> gameCommands, List<UnitCommandParams> unitCommands, List<SynchronizedData> synchronizedData)
	{
		ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(new UnitCommandMessage(tickIndex, gameCommands, unitCommands, synchronizedData));
		NetworkingOverlay.AddSentBytes(byteArraySlice.Count);
		if (!PhotonManager.Instance.SendMessageToOthers(7, byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count))
		{
			PFLog.Net.Error("Error when trying to send commands!");
		}
		byteArraySlice.Release();
	}

	public void OnCommandsReceived(NetPlayer player, ReadOnlySpan<byte> packet)
	{
		NetworkingOverlay.AddReceivedBytes(packet.Length);
		UnitCommandMessage unitCommandMessage;
		try
		{
			unitCommandMessage = NetMessageSerializer.DeserializeFromSpan<UnitCommandMessage>(packet);
			unitCommandMessage.AfterDeserialization();
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex, "Can't parse RecordElement!");
			return;
		}
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		int tickIndex = unitCommandMessage.tickIndex;
		if (currentNetworkTick - 9 + 1 > tickIndex || tickIndex > currentNetworkTick + 18)
		{
			PFLog.Net.Error($"UnitCommandLockStep.LoadUnitCommandsInProcess: unexpected lockStepIndex! Current={currentNetworkTick} from packet={tickIndex} max={9}");
			return;
		}
		Game.Instance.GameCommandQueue.PushCommandsForPlayer(player, unitCommandMessage.tickIndex, unitCommandMessage.gameCommandList);
		Game.Instance.UnitCommandBuffer.PushCommandsForPlayer(player, unitCommandMessage.tickIndex, unitCommandMessage.unitCommandList);
		Game.Instance.SynchronizedDataController.PushDataForPlayer(player, unitCommandMessage.tickIndex, unitCommandMessage.synchronizedDataList);
	}
}
