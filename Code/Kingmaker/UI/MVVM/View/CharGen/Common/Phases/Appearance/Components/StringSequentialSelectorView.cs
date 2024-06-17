using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components;

public class StringSequentialSelectorView : BaseCharGenAppearancePageComponentView<StringSequentialSelectorVM>, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, IConsoleEntity, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler
{
	[SerializeField]
	private TextMeshProUGUI m_CurrentValue;

	[SerializeField]
	private GameObject m_DescriptionObject;

	[SerializeField]
	private TextMeshProUGUI m_CurrentDescription;

	[SerializeField]
	protected OwlcatMultiButton ButtonNext;

	[SerializeField]
	protected OwlcatMultiButton ButtonPrevious;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableExtensions.Subscribe(ButtonNext.OnLeftClickAsObservable(), delegate
		{
			OnNextHandler();
		}));
		AddDisposable(ObservableExtensions.Subscribe(ButtonPrevious.OnLeftClickAsObservable(), delegate
		{
			OnPreviousHandler();
		}));
		AddDisposable(base.ViewModel.Value.DefaultIfNull(string.Empty).Subscribe(delegate(string value)
		{
			m_CurrentValue.text = value;
		}));
		AddDisposable(base.ViewModel.SecondaryValue.Subscribe(SetDescriptionText));
		AddDisposable(base.ViewModel.CurrentIndex.Subscribe(SetCounter));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	private void SetDescriptionText(string description)
	{
		if (!(m_CurrentDescription == null))
		{
			m_DescriptionObject.SetActive(!string.IsNullOrEmpty(description));
			m_CurrentDescription.text = description;
		}
	}

	private void SetCounter(int currentIndex)
	{
		if (!(m_Counter == null))
		{
			m_Counter.text = $"{currentIndex + 1} / {base.ViewModel.TotalCount}";
		}
	}

	private bool OnPreviousHandler()
	{
		base.ViewModel.OnLeft();
		return true;
	}

	private bool OnNextHandler()
	{
		base.ViewModel.OnRight();
		return true;
	}

	public bool HandleUp()
	{
		return false;
	}

	public bool HandleDown()
	{
		return false;
	}

	public bool HandleLeft()
	{
		return OnPreviousHandler();
	}

	public bool HandleRight()
	{
		return OnNextHandler();
	}
}
