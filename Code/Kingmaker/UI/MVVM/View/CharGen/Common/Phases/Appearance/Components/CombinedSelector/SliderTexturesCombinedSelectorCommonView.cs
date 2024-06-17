using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.CombinedSelector;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.CombinedSelector;

public class SliderTexturesCombinedSelectorCommonView : BaseCharGenAppearancePageComponentView<SlideTextureCombinedSelectorVM>, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, IConsoleEntity, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler, IFunc02ClickHandler
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	private ClickablePageNavigation m_Paginator;

	[SerializeField]
	private SlideSelectorCommonView m_SlideSelectorCommonView;

	[SerializeField]
	private TextureSelectorPagedView m_TextureSelectorPagedView;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	[SerializeField]
	private ConsoleHint m_ConsoleHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentSlideSelector.Subscribe(m_SlideSelectorCommonView.Bind));
		AddDisposable(base.ViewModel.CurrentTextureSelector.Subscribe(m_TextureSelectorPagedView.Bind));
		AddDisposable(base.ViewModel.Title.Subscribe(SetTitleText));
		AddDisposable(base.ViewModel.TotalItems.CombineLatest(base.ViewModel.CurrentIndex, (int total, int current) => new { total, current }).Subscribe(value =>
		{
			m_Counter.text = $"{value.current + 1}/{value.total}";
		}));
		m_Paginator.Initialize(base.ViewModel.TotalItems.Value, delegate(int idx)
		{
			base.ViewModel.SetIndex(idx);
		});
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_NavigationBehaviour.AddRow<SlideSelectorCommonView>(m_SlideSelectorCommonView);
		m_NavigationBehaviour.AddRow<TextureSelectorPagedView>(m_TextureSelectorPagedView);
		AddDisposable(base.ViewModel.OnSetValues.Subscribe(delegate
		{
			UpdateInternalFocus();
		}));
		AddDisposable(base.ViewModel.CurrentIndex.Subscribe(delegate
		{
			UpdateInternalFocus();
		}));
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			m_NavigationBehaviour.FocusOnCurrentEntity();
		}
		else
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
		if (!(m_ConsoleHint == null))
		{
			if (value)
			{
				m_ConsoleHint.BindCustomAction(11, GamePad.Instance.CurrentInputLayer);
			}
			else
			{
				m_ConsoleHint.Dispose();
			}
		}
	}

	private void UpdateInternalFocus()
	{
		if (IsFocused.Value && m_NavigationBehaviour != null)
		{
			m_NavigationBehaviour.FocusOnCurrentEntity();
		}
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	public bool HandleUp()
	{
		return m_NavigationBehaviour.HandleUp();
	}

	public bool HandleDown()
	{
		return m_NavigationBehaviour.HandleDown();
	}

	public bool HandleLeft()
	{
		return m_NavigationBehaviour.HandleLeft();
	}

	public bool HandleRight()
	{
		return m_NavigationBehaviour.HandleRight();
	}

	public bool CanFunc02Click()
	{
		return true;
	}

	public void OnFunc02Click()
	{
		m_Paginator.NextPage();
	}

	public string GetFunc02ClickHint()
	{
		return UIStrings.Instance.CharGen.SwitchPageSet;
	}
}
