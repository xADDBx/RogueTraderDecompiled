using System.Collections.Generic;
using AK.Wwise;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("b6d078a4ae218fe4a82f3fb5707b7e1f")]
public class BlueprintBookPage : BlueprintCueBase
{
	public List<BlueprintCueBaseReference> Cues = new List<BlueprintCueBaseReference>();

	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	public ActionList OnShow;

	public SpriteLink ImageLink;

	[Header("Epilogue Character name")]
	public bool ShowMainCharacterName = true;

	[HideIf("ShowMainCharacterName")]
	public BlueprintUnitReference Companion;

	[Header("Epilogue Portrait")]
	public bool ShowMainCharacter = true;

	[FormerlySerializedAs("ForeImageLink")]
	[HideIf("ShowMainCharacter")]
	public BlueprintPortraitReference Portrait;

	[Header("Epilogue Background")]
	public bool UseBackgroundVideo;

	[HideIf("UseBackgroundVideo")]
	public SpriteLink BackgroundImageLink;

	[ShowIf("UseBackgroundVideo")]
	public VideoLink BackgroundVideoLink;

	[Header("Epilogue Sound")]
	public bool UseSound;

	[ShowIf("UseSound")]
	public AK.Wwise.Event SoundStartEvent;

	[ShowIf("UseSound")]
	public AK.Wwise.Event SoundStopEvent;
}
