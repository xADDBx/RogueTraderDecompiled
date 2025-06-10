using System;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker;

public class PFLog : LogChannelFactory
{
	public static class History
	{
		public static readonly LogChannel System = GetOrCreateHistoryChannel("History.System");

		public static readonly LogChannel Area = GetOrCreateHistoryChannel("History.Area");

		public static readonly LogChannel Rest = GetOrCreateHistoryChannel("History.Rest");

		public static readonly LogChannel Items = GetOrCreateHistoryChannel("History.Items");

		public static readonly LogChannel Party = GetOrCreateHistoryChannel("History.Party");

		public static readonly LogChannel Skill = GetOrCreateHistoryChannel("History.Skill");

		public static readonly LogChannel Combat = GetOrCreateHistoryChannel("History.Combat");

		public static readonly LogChannel Dialog = GetOrCreateHistoryChannel("History.Dialog");

		public static readonly LogChannel Quests = GetOrCreateHistoryChannel("History.Quests");

		public static readonly LogChannel Unlocks = GetOrCreateHistoryChannel("History.Unlocks");

		public static readonly LogChannel SectorMap = GetOrCreateHistoryChannel("History.SectorMap");

		public static readonly LogChannel Etudes = GetOrCreateHistoryChannel("History.Etudes");

		public static readonly LogChannel Mods = GetOrCreateHistoryChannel("History.Mods");

		public static readonly LogChannel StarSystemMap = GetOrCreateHistoryChannel("History.StarSystemMap");

		public static readonly LogChannel Colonization = GetOrCreateHistoryChannel("History.Colonization");

		private static LogChannel GetOrCreateHistoryChannel(string name)
		{
			return LogChannelFactory.GetOrCreate(name, 1);
		}
	}

	[Flags]
	public enum SinkFlag
	{
		None = 0,
		History = 1,
		ActionReport = 2
	}

	public static readonly LogChannel LevelUp = LogChannelFactory.GetOrCreate("LevelUp");

	public static readonly LogChannel AI = LogChannelFactory.GetOrCreate("AI");

	public static readonly LogChannel UI = LogChannelFactory.GetOrCreate("UI");

	public static readonly LogChannel Entity = LogChannelFactory.GetOrCreate("Entity");

	public static readonly LogChannel Etudes = LogChannelFactory.GetOrCreate("Etudes");

	public static readonly LogChannel EntityFact = LogChannelFactory.GetOrCreate("EntityFact");

	public static readonly LogChannel Ability = LogChannelFactory.GetOrCreate("Ability");

	public static readonly LogChannel BlueprintTest = LogChannelFactory.GetOrCreate("BlueprintTest");

	public static readonly LogChannel LightmapBaker = LogChannelFactory.GetOrCreate("LightmapBaker");

	public static readonly LogChannel Space = LogChannelFactory.GetOrCreate("Space");

	public static readonly LogChannel Pathfinding = LogChannelFactory.GetOrCreate("Pathfinding");

	public static readonly LogChannel OGL = LogChannelFactory.GetOrCreate("OGL");

	public static readonly LogChannel EditorValidation = LogChannelFactory.GetOrCreate("EditorValidation");

	public static readonly LogChannel SceneLoader = LogChannelFactory.GetOrCreate("SceneLoader");

	public static readonly LogChannel Rest = LogChannelFactory.GetOrCreate("Rest");

	public static readonly LogChannel UnitCommands = LogChannelFactory.GetOrCreate("UnitCommands");

	public static readonly LogChannel GameCommands = LogChannelFactory.GetOrCreate("GameCommands");

	public static readonly LogChannel Strings = LogChannelFactory.GetOrCreate("Strings");

	public static readonly LogChannel MainMenuLight = LogChannelFactory.GetOrCreate("MainMenuLight");

	public static readonly LogChannel SmartConsole = LogChannelFactory.GetOrCreate("Console");

	public static readonly LogChannel Build = LogChannelFactory.GetOrCreate("Build");

	public static readonly LogChannel SmokeTest = LogChannelFactory.GetOrCreate("SmokeTest");

	public static readonly LogChannel Clockwork = LogChannelFactory.GetOrCreate("Clockwork");

	public static readonly LogChannel Bebilith = LogChannelFactory.GetOrCreate("Bebilith");

	public static readonly LogChannel UnitDescriptionExport = LogChannelFactory.GetOrCreate("UnitDescriptionExport");

	public static readonly LogChannel Actions = LogChannelFactory.GetOrCreate("Actions");

	public static readonly LogChannel GameStatistics = LogChannelFactory.GetOrCreate("GameStatistics");

	public static readonly LogChannel Tutorial = LogChannelFactory.GetOrCreate("Tutorial");

	public static readonly LogChannel Bundles = LogChannelFactory.GetOrCreate("Bundles");

	public static readonly LogChannel Settings = LogChannelFactory.GetOrCreate("Settings");

	public static readonly LogChannel TBM = LogChannelFactory.GetOrCreate("TBM");

	public static readonly LogChannel Replay = LogChannelFactory.GetOrCreate("Replay");

	public static readonly LogChannel Net = LogChannelFactory.GetOrCreate("Net");

	public static readonly LogChannel Items = LogChannelFactory.GetOrCreate("Items");

	public static readonly LogChannel Tick = LogChannelFactory.GetOrCreate("Tick");

	public static readonly LogChannel Coroutine = LogChannelFactory.GetOrCreate("Coroutine");

	public static readonly LogChannel UnityModManager = LogChannelFactory.GetOrCreate("UnityModManager");

	public static readonly LogChannel DesignerDebug = LogChannelFactory.GetOrCreate("DesignerDebug");

	public static readonly LogChannel EventSystemDebug = LogChannelFactory.GetOrCreate("EventSystemDebug");

	public static readonly LogChannel WWiseRTPC = LogChannelFactory.GetOrCreate("WWiseRTPC");

	public static readonly LogChannel Cutscene = LogChannelFactory.GetOrCreate("Cutscene");

	public static readonly LogChannel Animations = LogChannelFactory.GetOrCreate("Animations");

	public static LogChannel Unity => LogChannel.Unity;

	public static LogChannel Audio => LogChannel.Audio;

	public static LogChannel System => LogChannel.System;

	public static LogChannel TechArt => LogChannel.TechArt;

	public static LogChannel Default => LogChannel.Default;

	public static LogChannel Resources => LogChannel.Resources;

	public static LogChannel Craft => LogChannel.Craft;

	public static LogChannel Mods => LogChannel.Mods;
}
