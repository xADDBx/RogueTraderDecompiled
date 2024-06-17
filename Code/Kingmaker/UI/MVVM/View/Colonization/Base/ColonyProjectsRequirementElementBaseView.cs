using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsRequirementElementBaseView : ViewBase<ColonyProjectsRequirementElementVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IColonyProjectsDetailsEntity
{
	[Serializable]
	private struct ColonyProjectsRequirementElementVisual
	{
		public RequirementElementVisualType Type;

		public GameObject Container;

		public Image Icon;

		public TextMeshProUGUI Description;

		public bool HasCheckmark;

		public bool HasCount;

		[ConditionalShow("HasCount")]
		public TextMeshProUGUI CountText;
	}

	[SerializeField]
	private ColonyProjectsRequirementElementVisual[] m_VisualVariants;

	[SerializeField]
	private OwlcatMultiSelectable m_MainButton;

	[SerializeField]
	private OwlcatMultiSelectable m_BackgroundSelectable;

	[SerializeField]
	private GameObject m_CheckmarkIcon;

	private ColonyProjectsRequirementElementVisual m_CurrentVariant;

	private TooltipBaseTemplate m_Tooltip;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		SetCurrentVariant();
		AddDisposable(base.ViewModel.Icon.Subscribe(SetIcon));
		AddDisposable(base.ViewModel.Description.Subscribe(SetDescription));
		AddDisposable(base.ViewModel.CountText.Subscribe(SetCountText));
		base.ViewModel.CheckRequirementValueStatus();
		AddDisposable(base.ViewModel.IsChecked.Subscribe(SetChecked));
		m_Tooltip = RequirementUIFactory.GetRequirement(base.ViewModel.Requirement).GetTooltip();
		if (m_Tooltip != null)
		{
			AddDisposable(m_MainButton.SetTooltip(m_Tooltip));
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_Tooltip = null;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((ColonyProjectsRequirementElementVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyProjectsRequirementElementVM;
	}

	public void SetFocus(bool value)
	{
		m_BackgroundSelectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public void ShowTooltip()
	{
		if (m_Tooltip != null)
		{
			this.ShowTooltip(m_Tooltip);
		}
	}

	private void SetCurrentVariant()
	{
		ColonyProjectsRequirementElementVisual[] visualVariants = m_VisualVariants;
		for (int i = 0; i < visualVariants.Length; i++)
		{
			ColonyProjectsRequirementElementVisual currentVariant = visualVariants[i];
			if (currentVariant.Type != base.ViewModel.VisualType)
			{
				currentVariant.Container.SetActive(value: false);
				continue;
			}
			m_CurrentVariant = currentVariant;
			m_CurrentVariant.Container.SetActive(value: true);
		}
	}

	private void SetDescription(string text)
	{
		m_CurrentVariant.Description.text = text;
	}

	private void SetCountText(string text)
	{
		if (m_CurrentVariant.HasCount)
		{
			if (text == null)
			{
				m_CurrentVariant.CountText.gameObject.SetActive(value: false);
				return;
			}
			m_CurrentVariant.CountText.text = text;
			m_CurrentVariant.CountText.gameObject.SetActive(value: true);
		}
	}

	private void SetIcon(Sprite sprite)
	{
		if (sprite == null)
		{
			m_CurrentVariant.Icon.gameObject.SetActive(value: false);
			return;
		}
		m_CurrentVariant.Icon.sprite = sprite;
		m_CurrentVariant.Icon.gameObject.SetActive(value: true);
	}

	private void SetChecked(bool isChecked)
	{
		m_BackgroundSelectable.SetActiveLayer(isChecked ? "Checked" : "NotChecked");
		m_CheckmarkIcon.gameObject.SetActive(m_CurrentVariant.HasCheckmark && isChecked);
	}
}
