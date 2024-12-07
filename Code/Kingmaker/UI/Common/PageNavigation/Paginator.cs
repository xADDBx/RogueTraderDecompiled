using System;
using Kingmaker.Code.UI.Common.PageNavigation;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Common.PageNavigation;

public class Paginator : MonoBehaviour, IDisposable
{
	[SerializeField]
	private RectTransform m_ViewPort;

	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private PageNavigationPC m_PageNavigation;

	private readonly IntReactiveProperty m_PageNumber = new IntReactiveProperty();

	private readonly IntReactiveProperty m_PageIndex = new IntReactiveProperty();

	private float m_ViewPortHeight;

	private IDisposable m_IndexSubscription;

	public readonly ReactiveCommand UpdateViewTrigger = new ReactiveCommand();

	public IntReactiveProperty PageNumber => m_PageNumber;

	public IntReactiveProperty PageIndex => m_PageIndex;

	public void Initialize()
	{
		m_IndexSubscription = m_PageIndex.Subscribe(OnIndexChanged);
	}

	public void Dispose()
	{
		m_PageNavigation.Dispose();
		m_IndexSubscription?.Dispose();
	}

	public void UpdateView()
	{
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			m_ViewPortHeight = (m_ViewPort.gameObject.activeSelf ? m_ViewPort.rect.height : 0f);
			m_PageNumber.Value = ((m_ViewPortHeight > 0f) ? Mathf.CeilToInt(m_Content.rect.height / m_ViewPortHeight) : 0);
			m_PageIndex.Value = 0;
			m_PageNavigation.Initialize(m_PageNumber.Value, m_PageIndex);
			UpdateViewTrigger.Execute();
		});
	}

	private void OnIndexChanged(int index)
	{
		m_Content.anchoredPosition = new Vector2(0f, (float)index * m_ViewPortHeight);
	}
}
