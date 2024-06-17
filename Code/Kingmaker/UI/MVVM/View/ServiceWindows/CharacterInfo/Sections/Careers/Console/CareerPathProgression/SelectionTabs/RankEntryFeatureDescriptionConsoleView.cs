using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public class RankEntryFeatureDescriptionConsoleView : BaseCareerPathSelectionTabConsoleView<RankEntryFeatureItemVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	private readonly ReactiveProperty<bool> m_ScrollActive = new ReactiveProperty<bool>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly BoolReactiveProperty m_HasNextHint = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasScroll = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetHeader(UIStrings.Instance.CharacterSheet.HeaderFeatureDescriptionTab);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		AddDisposable(base.ViewModel.CareerPathVM.CanCommit.Subscribe(delegate(bool canCommit)
		{
			bool flag = canCommit && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel;
			SetNextButtonLabel(flag ? UIStrings.Instance.CharacterSheet.ToSummaryTab : UIStrings.Instance.CharGen.Next);
		}));
		AddDisposable(base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			ButtonActive.Value = !ro;
		}));
		AddDisposable(IsTabActiveProp.Subscribe(delegate(bool value)
		{
			m_ScrollActive.Value = value && m_InfoView.IsScrollActive;
		}));
		AddDisposable(m_NavigationBehaviour = m_InfoView.GetNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(UpdateFocus));
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		if (base.IsBinded)
		{
			if (base.ViewModel.CareerPathVM.CanCommit.Value && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
			{
				base.ViewModel.CareerPathVM.SetRankEntry(null);
				return;
			}
			base.ViewModel.CareerPathVM.SelectNextItem();
			UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	protected override void HandleFirstSelectableClick()
	{
		base.ViewModel.CareerPathVM.SetFirstSelectableRankEntry();
	}

	public override void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (InputAdded)
		{
			return;
		}
		InputBindStruct inputBindStruct = inputLayer.AddAxis(delegate(InputActionEventData _, float f)
		{
			m_InfoView.Scroll(f);
		}, 3, m_HasScroll);
		AddDisposable(m_ScrollHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		if ((bool)m_ConfirmHint)
		{
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				HandleClickNext();
			}, 8, m_HasNextHint.ToReactiveProperty());
			m_ConfirmHint.SetLabel(UIStrings.Instance.CharGen.Next);
			AddDisposable(m_ConfirmHint.Bind(inputBindStruct2));
			AddDisposable(inputBindStruct2);
		}
		InputAdded = true;
	}

	private void UpdateFocus(IConsoleEntity entity)
	{
		m_HasNextHint.Value = entity != null && (base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value?.CanSelect() ?? false);
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_HasScroll.Value = entity == null && m_InfoView.HasScroll;
		}, 1);
		if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			TooltipBaseTemplate entryTooltip = null;
			if (hasTooltipTemplate.TooltipTemplate() is TooltipTemplateGlossary tooltipTemplateGlossary)
			{
				entryTooltip = new TooltipTemplateGlossary(tooltipTemplateGlossary.GlossaryEntry);
			}
			EventBus.RaiseEvent(delegate(ISetTooltipHandler h)
			{
				h.SetTooltip(entryTooltip);
			});
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_NavigationBehaviour;
	}
}
