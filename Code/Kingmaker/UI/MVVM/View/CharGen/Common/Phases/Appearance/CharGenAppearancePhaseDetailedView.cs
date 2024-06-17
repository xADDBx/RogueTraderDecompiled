using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.CombinedSelector;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Voice;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Pages;
using Kingmaker.UI.MVVM.View.CharGen.Common.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.CombinedSelector;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance;

public class CharGenAppearancePhaseDetailedView : CharGenPhaseDetailedView<CharGenAppearanceComponentAppearancePhaseVM>
{
	[SerializeField]
	protected CharGenAppearancePageSelectorView m_PageSelectorView;

	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[Header("Components")]
	[SerializeField]
	private StringSequentialSelectorView m_StringSequentialSelectorView;

	[SerializeField]
	private SlideSelectorCommonView m_SlideSelectorCommonView;

	[SerializeField]
	private TextureSequentialSelectorView m_TextureSequentialSelectorView;

	[SerializeField]
	private TextureSelectorCommonView m_TextureSelectorCommonView;

	[SerializeField]
	private TextureSelectorPagedView m_TextureSelectorPagedView;

	[SerializeField]
	private TextureSelectorTabsView m_TextureSelectorTabsView;

	[SerializeField]
	private PortraitSelectorCommonView m_PortraitSelectorCommonView;

	[SerializeField]
	private CharGenVoiceSelectorCommonView m_CharGenVoiceSelectorCommonView;

	protected override bool HasYScrollBindInternal => false;

	public override void Initialize()
	{
		base.Initialize();
		m_VirtualList.Initialize(new VirtualListElementTemplate<StringSequentialSelectorVM>(m_StringSequentialSelectorView), new VirtualListElementTemplate<SlideSequentialSelectorVM>(m_SlideSelectorCommonView), new VirtualListElementTemplate<TextureSequentialSelectorVM>(m_TextureSequentialSelectorView), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorCommonView, 0), new VirtualListElementTemplate<TextureSelectorVM>(m_TextureSelectorPagedView, 1), new VirtualListElementTemplate<CharGenPortraitsSelectorVM>(m_PortraitSelectorCommonView), new VirtualListElementTemplate<CharGenVoiceSelectorVM>(m_CharGenVoiceSelectorCommonView), new VirtualListElementTemplate<TextureSelectorTabsVM>(m_TextureSelectorTabsView));
		m_PortraitFullView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_PageSelectorView.Bind(base.ViewModel.PagesSelectionGroupRadioVM);
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.VirtualListCollection));
		AddDisposable(base.ViewModel.PortraitVM.Subscribe(m_PortraitFullView.Bind));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.ViewModel.DollState.ShowHelmTemp = true;
		base.ViewModel.DollState.ShowClothTemp = true;
		m_CharacterController.ZoomMax();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
	}

	public override IReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty()
	{
		return new BoolReactiveProperty(initialValue: true);
	}

	public void HandleComponentChanged(CharGenAppearancePageComponent pageComponent)
	{
		switch (pageComponent)
		{
		case CharGenAppearancePageComponent.FaceType:
		case CharGenAppearancePageComponent.ScarsType:
			m_CharacterController.ZoomMin();
			break;
		case CharGenAppearancePageComponent.BodyType:
			m_CharacterController.ZoomMax();
			break;
		}
	}
}
