using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public class CareerPathDescriptionConsoleView : BaseCareerPathSelectionTabConsoleView<CareerPathVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_ScrollHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly BoolReactiveProperty m_Unfocused = new BoolReactiveProperty();

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		SetHeader(null);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NavigationBehaviour.Clear();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		m_NavigationBehaviour.Dispose();
		m_NavigationBehaviour = m_InfoView.GetNavigationBehaviour();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChange));
		return m_NavigationBehaviour;
	}

	public override void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (!InputAdded)
		{
			InputBindStruct inputBindStruct = inputLayer.AddAxis(delegate(InputActionEventData _, float f)
			{
				m_InfoView.Scroll(f);
			}, 3, m_Unfocused);
			AddDisposable(m_ScrollHint.Bind(inputBindStruct));
			AddDisposable(inputBindStruct);
			AddDisposable(inputLayer.AddButton(delegate
			{
				OnDecline();
			}, 9));
			InputAdded = true;
		}
	}

	private void OnFocusChange(IConsoleEntity entity)
	{
		m_Unfocused.Value = entity == null;
		if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			TooltipBaseTemplate entryTooltip = hasTooltipTemplate.TooltipTemplate();
			if (entryTooltip is TooltipTemplateGlossary tooltipTemplateGlossary)
			{
				entryTooltip = new TooltipTemplateGlossary(tooltipTemplateGlossary.GlossaryEntry);
			}
			EventBus.RaiseEvent(delegate(ISetTooltipHandler h)
			{
				h.SetTooltip(entryTooltip);
			});
		}
	}

	private void OnDecline()
	{
		if (m_NavigationBehaviour.IsFocused)
		{
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(null);
			});
		}
	}
}
