using System;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class ExpandableCollapseMultiButtonBase : MonoBehaviour, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	[UsedImplicitly]
	private Transform m_CollapseImage;

	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	[UsedImplicitly]
	private bool m_IsOn = true;

	[SerializeField]
	[UsedImplicitly]
	private bool m_IsSwitchMultiButtonLayer = true;

	private ReactiveProperty<bool> m_IsOnProperty;

	private IDisposable m_DisposableButton;

	private readonly Vector3 m_CollapseImageEndRotation = new Vector3(0f, 0f, -180f);

	private readonly float m_AnimDuration = 0.2f;

	[HideInInspector]
	public bool LayerIsOnAlways;

	[HideInInspector]
	public bool LayerIsOffAlways;

	public IReadOnlyReactiveProperty<bool> IsOn => m_IsOnProperty;

	private void Awake()
	{
		m_IsOnProperty = new ReactiveProperty<bool>(m_IsOn);
		SwitchStateImmediately(m_IsOnProperty.Value);
	}

	private void OnEnable()
	{
		m_DisposableButton = m_MultiButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnMultiButtonClick();
		});
	}

	public void SetValue(bool isOn, bool isImmediately = false)
	{
		if (m_IsOnProperty.Value != isOn)
		{
			if (isImmediately)
			{
				SwitchStateImmediately(isOn);
			}
			else
			{
				SwitchState(isOn);
			}
		}
	}

	private void SwitchStateImmediately(bool isOn)
	{
		m_IsOnProperty.Value = isOn;
		if (m_IsSwitchMultiButtonLayer)
		{
			SetActiveLayer(isOn);
		}
		m_CollapseImage.localEulerAngles = (isOn ? Vector3.zero : m_CollapseImageEndRotation);
		if (!(m_FadeAnimator == null))
		{
			if (isOn)
			{
				m_FadeAnimator.AppearAnimation();
			}
			else
			{
				m_FadeAnimator.DisappearAnimation();
			}
		}
	}

	private void SwitchState(bool isOn)
	{
		m_IsOnProperty.Value = isOn;
		if (m_IsSwitchMultiButtonLayer)
		{
			SetActiveLayer(isOn);
		}
		if (isOn)
		{
			Expand();
		}
		else
		{
			Collapse();
		}
	}

	public void Collapse()
	{
		if (m_FadeAnimator != null)
		{
			m_FadeAnimator.DisappearAnimation();
		}
		m_CollapseImage.DOKill();
		m_CollapseImage.transform.DOLocalRotate(Vector3.zero, m_AnimDuration).SetUpdate(isIndependentUpdate: true);
		m_CollapseImage.DOKill();
		m_CollapseImage.transform.DOLocalRotate(m_CollapseImageEndRotation, m_AnimDuration).SetUpdate(isIndependentUpdate: true);
		m_IsOnProperty.Value = false;
	}

	public void Expand()
	{
		if (m_FadeAnimator != null)
		{
			m_FadeAnimator.AppearAnimation();
		}
		m_CollapseImage.DOKill();
		m_CollapseImage.transform.DOLocalRotate(m_CollapseImageEndRotation, m_AnimDuration).SetUpdate(isIndependentUpdate: true);
		m_CollapseImage.DOKill();
		m_CollapseImage.transform.DOLocalRotate(Vector3.zero, m_AnimDuration).SetUpdate(isIndependentUpdate: true);
		m_IsOnProperty.Value = true;
	}

	private void OnMultiButtonClick()
	{
		SwitchState(!m_IsOnProperty.Value);
	}

	private void OnDisable()
	{
		m_DisposableButton?.Dispose();
	}

	private void Dispose()
	{
		m_IsOnProperty.Dispose();
		LayerIsOnAlways = false;
		LayerIsOffAlways = false;
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public void SetActiveLayer(bool isOn)
	{
		if (LayerIsOnAlways || LayerIsOffAlways)
		{
			m_MultiButton.SetActiveLayer(LayerIsOnAlways ? "On" : "Off");
		}
		else
		{
			m_MultiButton.SetActiveLayer(isOn ? "On" : "Off");
		}
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_MultiButton.IsValid();
	}

	public void OnConfirmClick()
	{
		OnMultiButtonClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
