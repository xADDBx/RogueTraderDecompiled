using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Career;

public class CharGenCareerPathListItemView : CareerPathListItemCommonView, IFunc02ClickHandler, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatButton m_InspectButton;

	protected override void BindViewImplementation()
	{
		ShouldShowTooltip = false;
		base.BindViewImplementation();
		AddDisposable(m_InspectButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetCareerPath();
			OnHoverEnd();
		}));
	}

	protected override void UpdateButtonState()
	{
		m_MainButton.SetActiveLayer(ItemState.Value.ToString());
		if ((bool)m_SelectedIcon)
		{
			m_SelectedIcon.gameObject.SetActive(base.ViewModel.IsSelected.Value);
		}
	}

	protected override void HandleClick()
	{
		if (base.ViewModel.Unit.IsDirectlyControllable)
		{
			if (!base.ViewModel.Unit.IsDirectlyControllable())
			{
				return;
			}
		}
		else if (!UINetUtility.IsControlMainCharacter())
		{
			return;
		}
		if (base.ViewModel.CareerPath.Tier == CareerPathTier.One)
		{
			base.ViewModel.SetSelectedFromView(state: true);
		}
	}

	public override bool CanConfirmClick()
	{
		return m_MainButton.CanConfirmClick();
	}

	public override string GetConfirmClickHint()
	{
		return ((base.ViewModel.UnitProgressionVM as UnitProgressionVM)?.CurrentCareer.Value == base.ViewModel) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
	}

	public new TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.CareerTooltip;
	}

	public bool CanFunc02Click()
	{
		return true;
	}

	public void OnFunc02Click()
	{
		base.ViewModel.SetCareerPath();
	}

	public string GetFunc02ClickHint()
	{
		return string.Empty;
	}
}
