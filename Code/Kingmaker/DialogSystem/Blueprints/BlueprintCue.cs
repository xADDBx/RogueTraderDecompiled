using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("8eee9d45ddcfa614d99610c1892993e3")]
public class BlueprintCue : BlueprintCueBase, ISoulMarkShiftProvider
{
	public LocalizedString Text;

	public SoulMarkShift SoulMarkRequirement = new SoulMarkShift();

	public DialogSpeaker Speaker;

	public bool TurnSpeaker = true;

	public DialogAnimation Animation;

	[CanBeNull]
	[Tooltip("Listener portrait (main character by default)")]
	[SerializeField]
	[FormerlySerializedAs("Listener")]
	private BlueprintUnitReference m_Listener;

	public ActionList OnShow;

	public ActionList OnStop;

	public LocalizedString Description;

	[NotNull]
	public SoulMarkShift SoulMarkShift = new SoulMarkShift();

	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	public CueSelection Continue;

	public string DisplayText => Text;

	public BlueprintUnit Listener => m_Listener?.Get();

	SoulMarkShift ISoulMarkShiftProvider.SoulMarkShift => SoulMarkShift;

	public override bool CanShow()
	{
		if (!base.CanShow())
		{
			return false;
		}
		if (Speaker.NeedsEntity && Speaker.GetEntity(this) == null)
		{
			return false;
		}
		if (!IsSoulMarkRequirementSatisfied())
		{
			return false;
		}
		return true;
	}

	public bool IsSoulMarkRequirementSatisfied()
	{
		return SoulMarkShiftExtension.CheckShiftAtLeast(SoulMarkRequirement);
	}

	public IEnumerable<Reward> GetRewards()
	{
		return this.GetComponents<Reward>();
	}

	public void ReceiveRewards()
	{
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		foreach (Reward reward in GetRewards())
		{
			reward.ReceiveReward(colony);
		}
	}
}
