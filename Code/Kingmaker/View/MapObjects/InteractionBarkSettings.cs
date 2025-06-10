using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionBarkSettings : InteractionSettings
{
	[SerializeField]
	private bool UseRandomBark;

	[SerializeField]
	[HideIf("UseRandomBark")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	private SharedStringAsset? Bark;

	[SerializeField]
	[ShowIf("UseRandomBark")]
	private bool DoNotRepeatLastBark;

	[SerializeField]
	[ShowIf("UseRandomBark")]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	private SharedStringAsset[] RandomBarks = new SharedStringAsset[0];

	[NonSerialized]
	private int _lastRandomBarkIdx = -1;

	[Tooltip("Play Bark VoiceOver.")]
	public bool BarkPlayVoiceOver;

	public ActionsReference? BarkActions;

	public bool RunActionsOnce;

	[Tooltip("Show bark on MapObject user. If false, bark will be shown on TargetUnit or TargetMapObject, or this object, if both are null.")]
	public bool ShowOnUser;

	[SerializeReference]
	[HideIf("ShowOnUser")]
	public AbstractUnitEvaluator? TargetUnit;

	[SerializeReference]
	[HideIf("ShowOnUser")]
	public MapObjectEvaluator? TargetMapObject;

	public ConditionsReference? Condition;

	public override bool ShouldShowUseAnimationState => false;

	public override bool ShouldShowDialog => false;

	public override bool ShouldShowUnlimitedInteractionsPerRound => false;

	public override bool ShouldShowOverrideActionPointsCost => false;

	public override bool ShouldShowInteractWithMeltaChargeFXData => false;

	public bool ActionsRan { get; set; }

	public SharedStringAsset? GetBark()
	{
		if (UseRandomBark)
		{
			int nextRandomIdx = InteractionHelper.GetNextRandomIdx(RandomBarks.Length, DoNotRepeatLastBark, ref _lastRandomBarkIdx);
			if (nextRandomIdx >= 0)
			{
				return RandomBarks[nextRandomIdx];
			}
			return null;
		}
		return Bark;
	}
}
