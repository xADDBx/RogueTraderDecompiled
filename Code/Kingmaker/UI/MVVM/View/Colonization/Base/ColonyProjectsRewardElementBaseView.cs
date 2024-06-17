using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
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

public class ColonyProjectsRewardElementBaseView : ViewBase<ColonyProjectsRewardElementVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IColonyProjectsDetailsEntity
{
	[Serializable]
	private struct ColonyProjectsRewardElementVisual
	{
		public RewardElementVisualType Type;

		public GameObject Container;

		public TextMeshProUGUI Description;

		public Image Icon;

		public bool HasAcronym;

		[ConditionalShow("HasAcronym")]
		public TextMeshProUGUI Acronym;

		public bool HasCount;

		[ConditionalShow("HasCount")]
		public TextMeshProUGUI CountText;

		public bool HasApplyToAllColonies;

		[ConditionalShow("HasApplyToAllColonies")]
		public GameObject ApplyToAllColoniesContainer;

		[ConditionalShow("HasApplyToAllColonies")]
		public Image ApplyToAllColoniesIcon;

		public bool HasArrows;

		[ConditionalShow("HasArrows")]
		public GameObject ArrowsContainer;

		[ConditionalShow("HasArrows")]
		public GameObject ArrowUp;

		[ConditionalShow("HasArrows")]
		public GameObject ArrowDown;

		public bool HasRefill;

		[ConditionalShow("HasRefill")]
		public TextMeshProUGUI RefillText;
	}

	[SerializeField]
	private ColonyProjectsRewardElementVisual[] m_VisualVariants;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiButton;

	private TooltipBaseTemplate m_Tooltip;

	private ColonyProjectsRewardElementVisual m_CurrentVariant;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		SetCurrentVariant();
		AddDisposable(base.ViewModel.Description.Subscribe(SetTitle));
		AddDisposable(base.ViewModel.Count.Subscribe(SetArrows));
		AddDisposable(base.ViewModel.CountText.Subscribe(SetCountText));
		AddDisposable(base.ViewModel.Icon.Subscribe(SetIcon));
		AddDisposable(base.ViewModel.IconColor.Subscribe(SetIconColor));
		AddDisposable(base.ViewModel.Acronym.Subscribe(SetAcronym));
		AddDisposable(base.ViewModel.ApplyToAllColonies.Subscribe(SetApplyToAllColonies));
		m_Tooltip = RewardUIFactory.GetReward(base.ViewModel.Reward).GetTooltip();
		if (m_Tooltip != null)
		{
			AddDisposable(m_MultiButton.SetTooltip(m_Tooltip));
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_Tooltip = null;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((ColonyProjectsRewardElementVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyProjectsRewardElementVM;
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
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
		ColonyProjectsRewardElementVisual[] visualVariants = m_VisualVariants;
		for (int i = 0; i < visualVariants.Length; i++)
		{
			ColonyProjectsRewardElementVisual currentVariant = visualVariants[i];
			if (currentVariant.Type != base.ViewModel.VisualType)
			{
				currentVariant.Container.SetActive(value: false);
				continue;
			}
			m_CurrentVariant = currentVariant;
			m_CurrentVariant.Container.SetActive(value: true);
			if (m_CurrentVariant.HasApplyToAllColonies)
			{
				m_CurrentVariant.ApplyToAllColoniesIcon.SetHint(UIStrings.Instance.ColonyProjectsRewards.ForAllColonies.Text);
			}
			if (m_CurrentVariant.HasRefill)
			{
				m_CurrentVariant.RefillText.text = UIStrings.Instance.ColonyProjectsRewards.Rechargeable.Text;
			}
		}
	}

	private void SetTitle(string text)
	{
		m_CurrentVariant.Description.text = text;
	}

	private void SetArrows(int count)
	{
		if (m_CurrentVariant.HasArrows)
		{
			if (count == 0)
			{
				m_CurrentVariant.ArrowsContainer.SetActive(value: false);
				return;
			}
			m_CurrentVariant.ArrowUp.SetActive(count > 0);
			m_CurrentVariant.ArrowDown.SetActive(count < 0);
			m_CurrentVariant.ArrowsContainer.SetActive(value: true);
		}
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

	private void SetIconColor(Color32 color)
	{
		m_CurrentVariant.Icon.color = color;
	}

	private void SetAcronym(string acronym)
	{
		if (m_CurrentVariant.HasAcronym)
		{
			if (string.IsNullOrWhiteSpace(acronym))
			{
				m_CurrentVariant.Acronym.gameObject.SetActive(value: false);
				return;
			}
			m_CurrentVariant.Acronym.text = acronym;
			m_CurrentVariant.Acronym.gameObject.SetActive(value: true);
		}
	}

	private void SetApplyToAllColonies(bool applyToAllColonies)
	{
		if (m_CurrentVariant.HasApplyToAllColonies)
		{
			m_CurrentVariant.ApplyToAllColoniesContainer.SetActive(applyToAllColonies);
		}
	}
}
