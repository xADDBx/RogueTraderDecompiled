using System;
using Newtonsoft.Json;

namespace Kingmaker.Utility.Reporting.Base;

[JsonObject]
public class ReportParameters
{
	[JsonProperty]
	public string Project;

	[JsonProperty]
	public string Email;

	[JsonProperty]
	public string Discord;

	[JsonProperty]
	public bool IsSendMarketingMats;

	[JsonProperty]
	public DateTime ReportDateTime;

	[JsonProperty]
	public string OperatingSystem;

	[JsonProperty]
	public string OperatingSystemFamily;

	[JsonProperty]
	public string PlayerLanguage;

	[JsonProperty]
	public string UniqueIdentifier;

	[JsonProperty]
	public string Blueprint;

	[JsonProperty]
	public string BlueprintArea;

	[JsonProperty]
	public string Chapter;

	[JsonProperty]
	public string Version;

	[JsonProperty]
	public string Guid;

	[JsonProperty]
	public string IssueType;

	[JsonProperty]
	public bool IsFeedback;

	[JsonProperty]
	public string StaticScene;

	[JsonProperty]
	public string LightScene;

	[JsonProperty]
	public string MainCharacter;

	[JsonProperty]
	public string KingdomDay;

	[JsonProperty]
	public string KingdomState;

	[JsonProperty]
	public string KingdomEvent;

	[JsonProperty]
	public string CurrentDialog;

	[JsonProperty]
	public string AreaDesigner;

	[JsonProperty]
	public string SuggestedAssignee;

	[JsonProperty]
	public string Context;

	[JsonProperty]
	public string Aspect;

	[JsonProperty]
	public string FixVersion;

	[JsonProperty]
	public string ExtendedContext;

	[JsonProperty]
	public string PartyContext;

	[JsonProperty]
	public string OtherContext;

	[JsonProperty]
	public string Cutscenes;

	[JsonProperty]
	public string ModifiedSaveFiles;

	[JsonProperty]
	public string Revision;

	[JsonProperty]
	public string BuildDateTime;

	[JsonProperty]
	public string Label;

	[JsonProperty]
	public string Labels;

	[JsonProperty]
	public string Store;

	[JsonProperty]
	public string ControllerModeType;

	[JsonProperty]
	public string Platform;

	[JsonProperty]
	public string ConsoleHardwareType;

	[JsonProperty]
	public string Exception;

	[JsonProperty]
	public string ModManagerMods;

	[JsonProperty]
	public string CoopPlayersCount;

	[JsonProperty]
	public string CameraPosition;
}
