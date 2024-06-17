using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("86139952581c4e7fa135794685e236f9")]
public class BlueprintColonyProject : BlueprintScriptableObject, IUIDataProvider, ISoulMarkShiftProvider
{
	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private LocalizedString m_MechanicString;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	public bool IsStartingProject;

	public int SegmentsToBuild;

	[SerializeField]
	public ActionList ActionsOnStart;

	[SerializeField]
	public ActionList ActionsOnFinish;

	[SerializeField]
	public ConditionsChecker AvailabilityConditions;

	[SerializeField]
	public ColonyProjectRank Rank = ColonyProjectRank.First;

	public string MechanicString => m_MechanicString;

	public string Name => m_Name;

	public string Description => m_Description;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => m_Name;

	public SoulMarkShift SoulMarkShift => this.GetComponent<RewardSoulMark>()?.SoulMarkShift;
}
