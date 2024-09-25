using JetBrains.Annotations;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components;

public class SlideSelectorCommonView : BaseCharGenAppearancePageComponentView<SlideSequentialSelectorVM>, IScrollHandler, IEventSystemHandler, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, IConsoleEntity, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler
{
	[SerializeField]
	private Slider m_Slider;

	[SerializeField]
	private bool m_CalculateHandleSize = true;

	[SerializeField]
	[ConditionalShow("m_CalculateHandleSize")]
	private RectTransform m_SliderRect;

	[SerializeField]
	[ConditionalShow("m_CalculateHandleSize")]
	private RectTransform m_SliderSlideArea;

	[SerializeField]
	[CanBeNull]
	private OwlcatMultiButton m_ButtonNext;

	[SerializeField]
	[CanBeNull]
	private OwlcatMultiButton m_ButtonPrevious;

	[SerializeField]
	[CanBeNull]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	[CanBeNull]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	[CanBeNull]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public bool IsActive => base.ViewModel?.IsAvailable.Value ?? false;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_Slider.minValue = 1f;
		m_Slider.wholeNumbers = true;
		m_Slider.maxValue = 10f;
	}

	protected override void BindViewImplementation()
	{
		m_Slider.maxValue = base.ViewModel.TotalCount;
		if (m_ButtonNext != null)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_ButtonNext.OnLeftClickAsObservable(), delegate
			{
				OnNextHandler();
			}));
		}
		if (m_ButtonPrevious != null)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_ButtonPrevious.OnLeftClickAsObservable(), delegate
			{
				OnPreviousHandler();
			}));
		}
		CheckCoopButtons(base.ViewModel.IsAvailable.Value, base.ViewModel.IsMainCharacter.Value);
		AddDisposable(base.ViewModel.Title.Subscribe(SetTitleText));
		AddDisposable(base.ViewModel.CurrentIndex.Subscribe(SetCurrentIndex));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(delegate(bool value)
		{
			CheckCoopButtons(value, base.ViewModel.IsMainCharacter.Value);
			base.gameObject.SetActive(value);
		}));
		AddDisposable(m_Slider.OnValueChangedAsObservable().Subscribe(OnSliderChangedValue));
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.ValuesUpdated, delegate
		{
			m_Slider.maxValue = base.ViewModel.TotalCount;
			SetCurrentIndex(base.ViewModel.CurrentIndex.Value);
		}));
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(delegate(bool value)
		{
			CheckCoopButtons(base.ViewModel.IsAvailable.Value, value);
		}));
		DelayedInvoker.InvokeInFrames(CalculateHandleSize, 1);
	}

	private void CheckCoopButtons(bool isAvailable, bool isMainCharacter)
	{
		m_ButtonNext.Or(null)?.SetInteractable(isAvailable && isMainCharacter);
		m_ButtonPrevious.Or(null)?.SetInteractable(isAvailable && isMainCharacter);
		m_Slider.interactable = isAvailable && isMainCharacter;
	}

	private void CalculateHandleSize()
	{
		if (m_CalculateHandleSize)
		{
			float num = ((m_Slider.maxValue >= 1f) ? m_Slider.maxValue : 1f);
			m_Slider.handleRect.sizeDelta = new Vector2(m_SliderRect.sizeDelta.x / num, m_Slider.handleRect.sizeDelta.y);
			m_SliderSlideArea.offsetMin = new Vector2(m_Slider.handleRect.sizeDelta.x / 2f, m_SliderSlideArea.offsetMin.y);
			m_SliderSlideArea.offsetMax = new Vector2((0f - m_Slider.handleRect.sizeDelta.x) / 2f, m_SliderSlideArea.offsetMin.y);
		}
	}

	public void OnSliderChangedValue(float value)
	{
		int currentIndex = Mathf.RoundToInt(value) - 1;
		base.ViewModel.SetCurrentIndex(currentIndex);
	}

	private void SetCurrentIndex(int index)
	{
		if (m_Value != null)
		{
			m_Value.text = base.ViewModel.Value.Value;
		}
		if (m_Counter != null)
		{
			m_Counter.text = $"{index + 1} / {base.ViewModel.TotalCount}";
		}
		m_Slider.value = index + 1;
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	protected bool OnPreviousHandler()
	{
		base.ViewModel.OnLeft();
		return true;
	}

	protected bool OnNextHandler()
	{
		base.ViewModel.OnRight();
		return true;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
	}

	public void OnScroll(PointerEventData eventData)
	{
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
