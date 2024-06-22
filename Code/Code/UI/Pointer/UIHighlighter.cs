using System;
using Code.UI.Common.Animations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UniRx;
using UnityEngine;

namespace Code.UI.Pointer;

public class UIHighlighter : MonoBehaviour, IUIHighlighter, ISubscriber
{
	[SerializeField]
	private string m_Key;

	[SerializeField]
	private ScaleAnimator m_Animation;

	private Func<bool> m_HighlightEnabled = () => true;

	public RectTransform RectTransform => base.transform as RectTransform;

	public void Initialize(Func<bool> highlightEnabled = null)
	{
		DoStopHighlight();
		if (highlightEnabled != null)
		{
			m_HighlightEnabled = highlightEnabled;
		}
	}

	public void SetKey(string key)
	{
		m_Key = key;
		DoStopHighlight();
	}

	public IDisposable Subscribe()
	{
		IDisposable disposable = EventBus.Subscribe(this);
		return Disposable.Create(delegate
		{
			DoStopHighlight();
			disposable.Dispose();
			m_Animation.DestroyViewImplementation();
		});
	}

	public void StartHighlight(string key)
	{
		if (!(key != m_Key) && m_HighlightEnabled())
		{
			DoStartHighlight();
		}
	}

	private void DoStartHighlight()
	{
		m_Animation.gameObject.SetActive(value: true);
		m_Animation.AppearAnimation();
	}

	public void StopHighlight(string key)
	{
		if (!(key != m_Key))
		{
			DoStopHighlight();
		}
	}

	private void DoStopHighlight()
	{
		m_Animation.DisappearAnimation();
		m_Animation.gameObject.SetActive(value: false);
	}

	public void Highlight(string key)
	{
		if (!(key != m_Key) && m_HighlightEnabled())
		{
			DoStartHighlight();
		}
	}

	public void HighlightOnce(string key)
	{
		if (!(key != m_Key) && m_HighlightEnabled())
		{
			m_Animation.gameObject.SetActive(value: true);
			m_Animation.AppearAnimation(delegate
			{
				m_Animation.DisappearAnimation();
				m_Animation.gameObject.SetActive(value: false);
			});
		}
	}
}
