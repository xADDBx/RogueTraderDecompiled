using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostAbilityDetailedBaseView : ViewBase<PostAbilityVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IFunc01ClickHandler
{
	[Header("Main part")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_IconGrayScale;

	[SerializeField]
	private TextMeshProUGUI m_AbilityName;

	[SerializeField]
	private OwlcatMultiButton m_Selectable;

	[Header("Penalty blocks")]
	[SerializeField]
	private GameObject m_AttunedStateBlock;

	[SerializeField]
	private TextMeshProUGUI m_AttunedStateText;

	[Header("Duration")]
	[SerializeField]
	private Image m_DurationBlock;

	[SerializeField]
	private TextMeshProUGUI m_DurationTitle;

	[SerializeField]
	private TextMeshProUGUI m_DurationValue;

	[Header("Cooldown")]
	[SerializeField]
	private Image m_CooldownBlock;

	[SerializeField]
	private TextMeshProUGUI m_CooldownTitle;

	[SerializeField]
	private TextMeshProUGUI m_CooldownValue;

	[Header("Penalty blocks")]
	[SerializeField]
	private GameObject m_PenaltyBlock;

	[SerializeField]
	private TextMeshProUGUI m_PenaltyText;

	[Header("Locked blocks")]
	[SerializeField]
	private GameObject m_LockedBlock;

	[SerializeField]
	private TextMeshProUGUI m_LockedText;

	[Header("Locked blocks")]
	[SerializeField]
	private GameObject m_AttuneBlock;

	[SerializeField]
	private GameObject m_AttuneActionBlock;

	[SerializeField]
	private OwlcatMultiSelectable m_ScrapPrerequisiteSelectable;

	[SerializeField]
	private TextMeshProUGUI m_ScrapPrerequisiteText;

	[SerializeField]
	private OwlcatMultiSelectable m_UsingPrerequisiteSelectable;

	[SerializeField]
	private TextMeshProUGUI m_UsingPrerequisiteText;

	[SerializeField]
	private OwlcatMultiSelectable m_ShipFullHPSelectable;

	[SerializeField]
	private TextMeshProUGUI m_ShipFullHPText;

	[Header("AttuneAbilityBlock")]
	[SerializeField]
	private GameObject m_AttuneAbilityBlock;

	[SerializeField]
	private GameObject m_ArrowsAttuneAbilityBlock;

	[SerializeField]
	private Image m_AttuneAbilityIcon;

	[SerializeField]
	private TextMeshProUGUI m_AttuneName;

	[SerializeField]
	private TextMeshProUGUI m_AttuneRequirement;

	private IDisposable m_AttuneAbilityDisposable;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		UpdateView();
	}

	protected override void DestroyViewImplementation()
	{
		m_AttuneAbilityDisposable?.Dispose();
	}

	private void UpdateView()
	{
		m_Icon.sprite = base.ViewModel.Icon.Value;
		m_IconGrayScale.sprite = base.ViewModel.Icon.Value;
		m_AbilityName.text = base.ViewModel.DisplayName;
		SetupDuration();
		SetupCooldown();
		SetupPenalty();
		SetupLockPenalty();
		SetupAttuneBlock();
		SetupAttuneState();
		AddDisposable(m_Icon.SetTooltip(base.ViewModel.TooltipTemplateAbility));
	}

	private void SetupAttuneState()
	{
		m_AttunedStateBlock.gameObject.SetActive(base.ViewModel.IsAlreadyAttuned);
		m_AttunedStateText.text = UIStrings.Instance.ShipCustomization.AllreadyAttuned;
	}

	private void SetupCooldown()
	{
		m_CooldownBlock.gameObject.SetActive(base.ViewModel.HasCooldown);
		m_CooldownTitle.text = UIStrings.Instance.ShipCustomization.PostAbilityStartingCooldown;
		m_CooldownValue.text = base.ViewModel.Cooldown.ToString();
		AddDisposable(m_CooldownBlock.SetHint(UIStrings.Instance.ShipCustomization.PostAbilityStartingCooldownHint));
	}

	private void SetupDuration()
	{
		m_DurationBlock.gameObject.SetActive(base.ViewModel.HasDuration);
		m_DurationTitle.text = UIStrings.Instance.ShipCustomization.PostAbilityDuration;
		m_DurationValue.text = base.ViewModel.UltimateDuration.ToString();
		AddDisposable(m_DurationBlock.SetHint(UIStrings.Instance.ShipCustomization.PostAbilityDurationHint));
	}

	private void SetupPenalty()
	{
		m_PenaltyBlock.gameObject.SetActive(base.ViewModel.HasCooldown);
		m_PenaltyText.text = base.ViewModel.CooldownReason;
	}

	private void SetupLockPenalty()
	{
		m_LockedBlock.gameObject.SetActive(!base.ViewModel.IsUnlocked);
		m_LockedText.text = base.ViewModel.LockedReason;
		m_Selectable.SetActiveLayer((!base.ViewModel.IsUnlocked) ? 1 : 0);
	}

	protected virtual void SetupAttuneBlock()
	{
		m_AttuneBlock.SetActive(base.ViewModel.IsAttunable);
		m_AttuneAbilityBlock.SetActive(base.ViewModel.IsAttunable);
		m_ArrowsAttuneAbilityBlock.SetActive(base.ViewModel.IsAttunable);
		m_AttuneAbilityIcon.sprite = ((!(base.ViewModel.AttuneAbility?.Icon)) ? null : base.ViewModel.AttuneAbility?.Icon);
		m_AttuneName.text = base.ViewModel.AttuneAbility?.Name ?? string.Empty;
		m_AttuneRequirement.text = string.Concat(UIStrings.Instance.ColonyProjectsTexts.ProjectRequirements, ": ");
		if (base.ViewModel.IsAttunable)
		{
			m_AttuneActionBlock.gameObject.SetActive(!base.ViewModel.IsAlreadyAttuned);
			if (base.ViewModel.IsAttunable)
			{
				m_AttuneAbilityDisposable?.Dispose();
				m_AttuneAbilityDisposable = m_AttuneAbilityIcon.SetTooltip(base.ViewModel.TooltipTemplateAttunedAbility);
			}
			m_ScrapPrerequisiteText.text = $"{base.ViewModel.ScrapRequired} {UIStrings.Instance.ShipCustomization.Scrap.Text}";
			m_ScrapPrerequisiteSelectable.SetActiveLayer(base.ViewModel.IsEnoughScrapForAttune ? 1 : 0);
			m_UsingPrerequisiteText.text = UIStrings.Instance.ShipCustomization.UseOnseCurrentAbility.Text;
			m_UsingPrerequisiteSelectable.SetActiveLayer(base.ViewModel.IsUsed ? 1 : 0);
			m_ShipFullHPText.text = UIStrings.Instance.ShipCustomization.ShipHasFullHP.Text;
			m_ShipFullHPSelectable.SetActiveLayer(base.ViewModel.IsFullHP ? 1 : 0);
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as PostAbilityVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is PostAbilityVM;
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Selectable.Interactable;
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		TooltipHelper.ShowInfo(base.ViewModel.TooltipTemplates());
	}

	public string GetConfirmClickHint()
	{
		return UIStrings.Instance.CommonTexts.Information.Text;
	}

	public bool CanFunc01Click()
	{
		return base.ViewModel.CanAttune;
	}

	public void OnFunc01Click()
	{
		base.ViewModel.TryAttune();
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.ShipCustomization.Attune.Text;
	}
}
