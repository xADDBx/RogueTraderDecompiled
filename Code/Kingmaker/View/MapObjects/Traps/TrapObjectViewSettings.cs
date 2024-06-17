using System;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.Decals;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps;

[Serializable]
public class TrapObjectViewSettings
{
	public FxDecal FxDecal;

	public Transform ActorPosition;

	public Transform TargetPoint;

	public ScriptZone ScriptZoneTrigger;

	[AkEventReference]
	public string TriggerSound;

	[AkEventReference]
	public string DisabledSound;

	[AkEventReference]
	public string DisableFailSound;
}
