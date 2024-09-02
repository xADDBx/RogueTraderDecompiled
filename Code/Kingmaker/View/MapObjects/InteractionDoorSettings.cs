using System;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionDoorSettings : InteractionSettings
{
	public enum NavMeshCutActionSettings
	{
		DoNotTouchNavmeshCut,
		EnableNavmeshCutWhenOpen,
		EnableNavmeshCutWhenClosed
	}

	public AnimationClip ObstacleAnimation;

	public bool DisableOnOpen;

	public bool OpenByDefault;

	public NavMeshCutActionSettings NavmeshCutAction;

	public StaticRendererLink HideWhenOpen;

	[AkEventReference]
	public string OpenSound;

	[AkEventReference]
	public string CloseSound;

	public bool DonNotNeedNavmeshCut;
}
