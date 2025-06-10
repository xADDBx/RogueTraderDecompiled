using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.BuffsAndConditions;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;

public class CharInfoSummaryPCView : CharInfoComponentView<CharInfoSummaryVM>
{
	[SerializeField]
	protected CharInfoStatusEffectsView m_StatusEffectsView;

	[Header("Move Points")]
	[SerializeField]
	protected OwlcatMultiButton m_MovePointsButton;

	[SerializeField]
	private TextMeshProUGUI m_MovePointsLabel;

	[SerializeField]
	private TextMeshProUGUI m_MovePoints;

	[Header("Action Points")]
	[SerializeField]
	protected OwlcatMultiButton m_ActionPointsButton;

	[SerializeField]
	private TextMeshProUGUI m_ActionPointsLabel;

	[SerializeField]
	private TextMeshProUGUI m_ActionPoints;

	[Header("Summary")]
	[SerializeField]
	protected CharInfoAlignmentWheelPCView m_AlignmentWheelPCView;

	[FormerlySerializedAs("m_PetSummaryView")]
	[SerializeField]
	protected PetSummaryPCView m_PetSummaryPCView;

	[SerializeField]
	protected CharInfoCurrentCareerView m_CareerView;

	private AccessibilityTextHelper m_TextHelper;

	public override void Initialize()
	{
		base.Initialize();
		m_StatusEffectsView.Initialize();
		m_AlignmentWheelPCView.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_MovePointsLabel, m_MovePoints, m_ActionPointsLabel, m_ActionPoints);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ActionPointVM.Subscribe(delegate
		{
			UpdatePoints();
		}));
		AddDisposable(base.ViewModel.CareerPathVM.Subscribe(delegate
		{
			UpdateCareer();
		}));
		m_StatusEffectsView.Bind(base.ViewModel.StatusEffects.Value);
		m_AlignmentWheelPCView.Bind(base.ViewModel.CharInfoAlignmentVM);
		m_PetSummaryPCView.Or(null)?.Bind(base.ViewModel.PetSummaryVM);
		base.BindViewImplementation();
		SetupTexts();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_StatusEffectsView.Unbind();
		m_AlignmentWheelPCView.Unbind();
		m_TextHelper.Dispose();
	}

	private void UpdatePoints()
	{
		m_ActionPointsButton.gameObject.SetActive(!(base.ViewModel.Unit.Value?.IsPet ?? false));
		ActionPointsVM value = base.ViewModel.ActionPointVM.Value;
		if (value != null)
		{
			float num = (base.ViewModel.IsInCombat.Value ? value.BlueAP.Value : value.MaxBlueAP.Value);
			float num2 = (base.ViewModel.IsInCombat.Value ? value.YellowAP.Value : value.MaxYellowAP.Value);
			m_MovePoints.text = num + "/" + value.MaxBlueAP.Value;
			m_ActionPoints.text = num2 + "/" + value.MaxYellowAP.Value;
		}
	}

	private void UpdateCareer()
	{
		if (base.ViewModel.CareerPathVM.Value != null)
		{
			m_CareerView.gameObject.SetActive(base.ViewModel.CareerPathVM.Value != null);
			m_CareerView.Bind(base.ViewModel.CareerPathVM.Value);
		}
	}

	private void SetupTexts()
	{
		m_MovePointsLabel.text = UIStrings.Instance.ActionBar.MovementPoints;
		m_ActionPointsLabel.text = UIStrings.Instance.ActionBar.ActionPoints;
	}
}
