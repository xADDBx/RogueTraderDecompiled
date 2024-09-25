using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.Dependencies;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public class ConsoleHint : MonoBehaviour, IWidget, IDisposable
{
	private enum ConsoleHintAnimation
	{
		None,
		Scale,
		Move
	}

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private Image m_ProgressBar;

	[SerializeField]
	private Image m_ProgressHint;

	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	private ConsoleHintAnimation m_Animation;

	[SerializeField]
	private float m_ScaleFactor = 0.1f;

	[SerializeField]
	private float m_AnimTime = 0.4f;

	private List<int> m_ActionIds = new List<int>();

	private bool m_IsLongPress;

	private bool m_LongPressButtonSoundIsPlaying;

	private bool m_IsBinded;

	internal IDisposable m_BindSubscription;

	private CompositeDisposable m_BindStructDisposable;

	private IDisposable m_ConsoleTypeSubscription;

	private RectTransform m_HintContainer;

	private RectTransform HintContainer
	{
		get
		{
			if (!(m_HintContainer != null))
			{
				return m_HintContainer = GetComponent<RectTransform>();
			}
			return m_HintContainer;
		}
	}

	private CanvasGroup CanvasGroup => GetCanvasGroup();

	private CanvasGroup GetCanvasGroup()
	{
		if (m_CanvasGroup != null)
		{
			return m_CanvasGroup;
		}
		m_CanvasGroup = base.gameObject.GetComponent<CanvasGroup>();
		if (m_CanvasGroup != null)
		{
			return m_CanvasGroup;
		}
		m_CanvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		return m_CanvasGroup;
	}

	private void Awake()
	{
		if (!m_IsBinded)
		{
			CanvasGroup.alpha = 0f;
		}
	}

	public IDisposable Bind(InputBindStruct inputBindStruct)
	{
		PartialDispose();
		m_IsBinded = true;
		CanvasGroup.alpha = 1f;
		CompositeDisposable compositeDisposable = new CompositeDisposable();
		if (m_BindStructDisposable == null)
		{
			m_BindStructDisposable = new CompositeDisposable();
		}
		m_BindStructDisposable.Add(inputBindStruct);
		InputLayer inputLayer = inputBindStruct.InputLayer;
		m_IsLongPress = inputBindStruct.IsLongPress;
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = null;
		foreach (BindDescription bind2 in inputBindStruct.Binds)
		{
			if (bind2.Enabled != null)
			{
				readOnlyReactiveProperty = bind2.Enabled;
				break;
			}
		}
		ReadOnlyReactiveProperty<bool> readOnlyReactiveProperty2 = ((readOnlyReactiveProperty == null) ? new ReadOnlyReactiveProperty<bool>(inputLayer.LayerBinded) : inputLayer.LayerBinded.And(readOnlyReactiveProperty).ToReadOnlyReactiveProperty());
		compositeDisposable.Add(readOnlyReactiveProperty2);
		compositeDisposable.Add(readOnlyReactiveProperty2.Subscribe(SetActive));
		compositeDisposable.Add(Disposable.Create(delegate
		{
			m_IsBinded = false;
			CanvasGroup.alpha = 0f;
			SetActive(state: false);
		}));
		foreach (BindDescription bind in inputBindStruct.Binds)
		{
			int num = bind.ActionId;
			if (GamePad.Instance.SwapButtonsForJapanese)
			{
				switch (num)
				{
				case 8:
					num = 9;
					break;
				case 9:
					num = 8;
					break;
				}
			}
			m_ActionIds.Add(num);
			BindDescription bindDescription = bind;
			bindDescription.HintsHandler = (Action<InputActionEventData>)Delegate.Combine(bindDescription.HintsHandler, new Action<InputActionEventData>(PlayAnim));
			compositeDisposable.Add(Disposable.Create(delegate
			{
				BindDescription bindDescription2 = bind;
				bindDescription2.HintsHandler = (Action<InputActionEventData>)Delegate.Remove(bindDescription2.HintsHandler, new Action<InputActionEventData>(PlayAnim));
			}));
		}
		m_ProgressHint.gameObject.SetActive(m_IsLongPress);
		m_ProgressBar.gameObject.SetActive(m_IsLongPress);
		if (m_IsLongPress)
		{
			inputBindStruct.Percentage.Value = 0f;
			compositeDisposable.Add(inputBindStruct.Percentage.Subscribe(delegate(float value)
			{
				UpdateProgressBar(value);
				UpdateProgressBarSound(value);
			}));
		}
		UpdateIcon();
		m_BindSubscription = compositeDisposable;
		return this;
	}

	public IDisposable BindCustomAction(int actionId, InputLayer inputLayer, IReadOnlyReactiveProperty<bool> hintIsActive = null)
	{
		return BindCustomAction(new List<int> { actionId }, inputLayer, hintIsActive);
	}

	public IDisposable BindCustomAction(List<int> actionIds, InputLayer inputLayer, IReadOnlyReactiveProperty<bool> hintIsActive = null)
	{
		PartialDispose();
		m_IsBinded = true;
		CanvasGroup.alpha = 1f;
		m_ActionIds = actionIds;
		UpdateIcon();
		CompositeDisposable compositeDisposable = new CompositeDisposable();
		ReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = ((hintIsActive == null) ? new ReadOnlyReactiveProperty<bool>(inputLayer.LayerBinded) : inputLayer.LayerBinded.And(hintIsActive).ToReadOnlyReactiveProperty());
		compositeDisposable.Add(readOnlyReactiveProperty);
		compositeDisposable.Add(readOnlyReactiveProperty.Subscribe(SetActive));
		compositeDisposable.Add(Disposable.Create(delegate
		{
			m_IsBinded = false;
			CanvasGroup.alpha = 0f;
			SetActive(state: false);
		}));
		m_BindSubscription = compositeDisposable;
		m_IsLongPress = false;
		m_ProgressHint.gameObject.SetActive(m_IsLongPress);
		m_ProgressBar.gameObject.SetActive(m_IsLongPress);
		return this;
	}

	public void PartialDispose()
	{
		m_ActionIds.Clear();
		m_BindSubscription?.Dispose();
		m_BindSubscription = null;
	}

	public void Dispose()
	{
		PartialDispose();
		m_BindStructDisposable?.Dispose();
		m_BindStructDisposable = null;
	}

	protected virtual void OnEnable()
	{
		if (!m_IsBinded)
		{
			CanvasGroup.alpha = 0f;
		}
		if (m_IsLongPress)
		{
			m_ProgressBar.fillAmount = 0f;
			m_ProgressHint.gameObject.SetActive(value: true);
		}
		m_ConsoleTypeSubscription = GamePad.Instance.ConsoleTypeProperty.Subscribe(delegate
		{
			UpdateIcon();
		});
	}

	protected virtual void OnDisable()
	{
		if (m_LongPressButtonSoundIsPlaying)
		{
			m_LongPressButtonSoundIsPlaying = false;
			UIKitSoundManager.PlayConsoleHintHoldSoundStop();
			UIKitSoundManager.PlayConsoleHintHoldSoundSetRtpcValue(0f);
		}
		m_ConsoleTypeSubscription?.Dispose();
		m_ConsoleTypeSubscription = null;
	}

	public void OnWidgetInstantiated()
	{
	}

	public void OnWidgetTaken()
	{
	}

	public void OnWidgetReturned()
	{
	}

	public void SetActive(bool state)
	{
		try
		{
			base.gameObject.SetActive(state);
		}
		catch
		{
		}
	}

	public void SetLabel(string label)
	{
		if (!(m_Label == null))
		{
			m_Label.text = label;
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(label));
		}
	}

	private void UpdateIcon()
	{
		if (GamePadIcons.Instance != null && m_ActionIds != null && m_ActionIds.Any())
		{
			Sprite sprite = GamePadIcons.Instance.GetCustomDPadIcon(m_ActionIds) ?? GamePadIcons.Instance.GetIcon(m_ActionIds.FirstOrDefault());
			if (sprite != m_Icon.sprite)
			{
				m_Icon.sprite = sprite;
			}
		}
	}

	public void PlayAnim(InputActionEventData eventData)
	{
		if (m_IsLongPress)
		{
			UpdateProgressBar(0.1f);
			return;
		}
		if (DOTween.IsTweening(HintContainer))
		{
			HintContainer.DOComplete();
		}
		switch (m_Animation)
		{
		case ConsoleHintAnimation.Scale:
			PlayScale();
			break;
		case ConsoleHintAnimation.Move:
			PlayMove();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ConsoleHintAnimation.None:
			break;
		}
		if (m_Label != null && m_Label.gameObject.activeSelf && !m_IsLongPress && base.gameObject.activeInHierarchy)
		{
			UIKitSoundManager.PlayConsoleHintClickSound();
		}
	}

	private void PlayScale()
	{
		HintContainer.DOPunchScale(Vector3.one * m_ScaleFactor, m_AnimTime, 1).SetUpdate(isIndependentUpdate: true);
	}

	private void PlayMove()
	{
	}

	private void UpdateProgressBar(float percent)
	{
		if (!(m_ProgressBar == null) && (!(Math.Abs(percent) > Mathf.Epsilon) || !((double)percent < 0.25)))
		{
			m_ProgressBar.fillAmount = percent;
			m_ProgressHint.gameObject.SetActive(m_IsLongPress && percent <= Mathf.Epsilon);
		}
	}

	private void UpdateProgressBarSound(float percent)
	{
		if (!(m_ProgressBar == null) && base.gameObject.activeInHierarchy)
		{
			if (Math.Abs(percent) > Mathf.Epsilon && !m_LongPressButtonSoundIsPlaying)
			{
				UIKitSoundManager.PlayConsoleHintHoldSoundStart();
				m_LongPressButtonSoundIsPlaying = true;
			}
			if (m_LongPressButtonSoundIsPlaying)
			{
				UIKitSoundManager.PlayConsoleHintHoldSoundSetRtpcValue(percent);
			}
			if (percent <= Mathf.Epsilon && m_LongPressButtonSoundIsPlaying)
			{
				m_LongPressButtonSoundIsPlaying = false;
				UIKitSoundManager.PlayConsoleHintHoldSoundStop();
			}
		}
	}
}
