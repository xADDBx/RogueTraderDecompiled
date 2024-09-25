using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("96610525fc0cf8c41bfef88a84117024")]
public class Cutscene : Gate, ICutscene
{
	public enum MarkedUnitHandlingType
	{
		Pause,
		Stop,
		PauseAndRestart
	}

	public const float AwakeRange = 24.2f;

	public CutscenePriority Priority;

	public bool NonSkippable;

	[Tooltip("If set, units moved by this cutscene cannot start a dialog")]
	public bool ForbidDialogs;

	[Tooltip("If set, units moved by this cutscene never play idle variants")]
	public bool ForbidRandomIdles = true;

	[Tooltip("If set, the cutscene auto-pauses when there's a dialog, rest, or exclusive cutscene playing")]
	public bool IsBackground;

	[Tooltip("If not set, cutscene is paused when all anchors are in fog of war or away enough from party")]
	public bool Sleepless;

	[Tooltip("If set, exact copies of this cutscene (with the same parameters) can play at the same time. You probably do not need to set this.")]
	public bool AllowCopies;

	public bool LockControl;

	public bool ShowOverlay;

	[Tooltip("Usually if unit is Marked by cutscene, Roaming is disabled. This option allows Roaming to remain enabled even if unit is Marked by this cutscene (other cutscenes may still prevent unit from roaming).")]
	public bool AllowRoaming;

	[HideIf("Sleepless")]
	[AllowedEntityType(typeof(CutsceneAnchorView))]
	public EntityReference[] Anchors = new EntityReference[0];

	[Tooltip("How to react when a unit marked by this cutscene is in combat or marked by a higher priority cutscene")]
	public MarkedUnitHandlingType MarkedUnitHandling;

	public ParametrizedContextSetter DefaultParameters;

	public ActionList OnStopped;

	public ActionList OnFinished;

	public string Name => name;
}
