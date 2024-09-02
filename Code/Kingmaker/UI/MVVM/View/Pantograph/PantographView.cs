using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dreamteck.Splines;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Pantograph;

public class PantographView : MonoBehaviour, IPantographHandler, ISubscriber, IDisposable
{
	[Header("Common")]
	[SerializeField]
	private GameObject m_FocusObject;

	[Header("Simple Item")]
	[SerializeField]
	private GameObject m_SimpleItemContainer;

	[SerializeField]
	private ScrambledTMP m_Label;

	[SerializeField]
	private List<Image> m_Images;

	[SerializeField]
	private TextMeshProUGUI m_TextIcon;

	[Header("Extended Item")]
	[SerializeField]
	private GameObject m_ExtendedItemContainer;

	[Header("Large Item")]
	[SerializeField]
	private GameObject m_LargeItemContainer;

	[Header("General")]
	[SerializeField]
	private Transform m_Head;

	[SerializeField]
	private Transform m_Tail;

	[SerializeField]
	private float m_HeadSpeed;

	[SerializeField]
	private float m_TailSpeed;

	[SerializeField]
	private float m_MinY;

	[SerializeField]
	private float m_MaxY;

	[SerializeField]
	private SplineComputer m_TailPath;

	private CompositeDisposable m_Disposable;

	private Transform m_BoundTransform;

	private float m_HeadY;

	private Tweener m_TweenerHead;

	private Tweener m_TweenerTail;

	private bool m_SoundIsPlaying;

	private IWidgetView m_ExtendedItem;

	private IWidgetView m_LargeItem;

	public IDisposable Show()
	{
		Dispose();
		m_Label.Initialize();
		base.gameObject.SetActive(value: true);
		SetFocus(focused: false);
		m_TailPath.RebuildImmediate(calculateSamples: true, forceUpdateAll: true);
		m_Disposable?.Dispose();
		m_Disposable = new CompositeDisposable();
		m_Disposable.Add(EventBus.Subscribe(this));
		m_Disposable.Add(MainThreadDispatcher.UpdateAsObservable().Subscribe(OnUpdate));
		return this;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		Dispose();
	}

	public void Bind(PantographConfig config)
	{
		m_BoundTransform = config.Transform;
		m_HeadY = m_MaxY;
		bool flag = config.View != null;
		m_SimpleItemContainer.SetActive(!flag && !config.UseLargeView);
		m_ExtendedItemContainer.SetActive(flag && !config.UseLargeView);
		m_LargeItemContainer.Or(null)?.SetActive(config.UseLargeView);
		if (config.UseLargeView && m_LargeItemContainer != null)
		{
			BindLarge(config);
		}
		else if (flag)
		{
			BindExtended(config);
		}
		else
		{
			BindSimple(config);
		}
	}

	private void BindSimple(PantographConfig config)
	{
		m_Label.SetText(string.Empty, config.Text);
		if (m_TextIcon != null)
		{
			bool flag = !string.IsNullOrWhiteSpace(config.TextIcon);
			m_TextIcon.gameObject.SetActive(flag);
			if (flag)
			{
				m_TextIcon.text = config.TextIcon;
			}
		}
		if (!m_Images.Any())
		{
			return;
		}
		for (int i = 0; i < m_Images.Count; i++)
		{
			if (!(m_Images[i] == null))
			{
				if (config.Icons == null || i >= config.Icons.Count)
				{
					m_Images[i].gameObject.SetActive(value: false);
					continue;
				}
				m_Images[i].gameObject.SetActive(value: true);
				m_Images[i].sprite = config.Icons[i];
			}
		}
	}

	private void BindExtended(PantographConfig config)
	{
		UnbindExtended();
		m_ExtendedItem = (IWidgetView)WidgetFactory.GetWidget(config.View.MonoBehaviour, activate: true, strictMatching: true);
		m_ExtendedItem.BindWidgetVM(config.ViewModel);
		m_ExtendedItem.MonoBehaviour.transform.SetParent(m_ExtendedItemContainer.transform, worldPositionStays: false);
	}

	private void BindLarge(PantographConfig config)
	{
		UnbindLarge();
		m_LargeItem = (IWidgetView)WidgetFactory.GetWidget(config.View.MonoBehaviour, activate: true, strictMatching: true);
		m_LargeItem.BindWidgetVM(config.ViewModel);
		m_LargeItem.MonoBehaviour.transform.SetParent(m_LargeItemContainer.transform, worldPositionStays: false);
	}

	public void Unbind()
	{
		m_BoundTransform = null;
		UnbindSimple();
		UnbindExtended();
		UnbindLarge();
		m_Label.StopText();
		SetFocus(focused: false);
	}

	public void SetCustomMaxY(float value)
	{
		m_MaxY = value;
	}

	public void SetFocus(bool focused)
	{
		m_FocusObject.Or(null)?.SetActive(focused);
	}

	private void UnbindSimple()
	{
		m_Label.SetText(string.Empty, string.Empty);
		if (m_TextIcon != null)
		{
			m_TextIcon.text = string.Empty;
		}
		if (!m_Images.Any())
		{
			return;
		}
		foreach (Image image in m_Images)
		{
			if (!(image == null))
			{
				image.gameObject.SetActive(value: false);
			}
		}
	}

	private void UnbindExtended()
	{
		if (m_ExtendedItem != null)
		{
			WidgetFactory.DisposeWidget(m_ExtendedItem.MonoBehaviour);
		}
	}

	private void UnbindLarge()
	{
		if (m_LargeItem != null)
		{
			WidgetFactory.DisposeWidget(m_LargeItem.MonoBehaviour);
		}
	}

	private void OnUpdate()
	{
		if ((bool)m_BoundTransform && !m_BoundTransform.gameObject.activeInHierarchy)
		{
			Unbind();
		}
		float num = CalculateHeadY(m_BoundTransform.Or(null)?.position.y ?? m_MaxY);
		if (Mathf.Abs(num - m_HeadY) < 0.1f)
		{
			return;
		}
		m_HeadY = num;
		float num2 = Math.Abs(m_Head.position.y - m_HeadY);
		if (num2 < 0.1f)
		{
			return;
		}
		m_TweenerHead.Kill();
		m_TweenerHead = m_Head.DOMoveY(m_HeadY, Mathf.Sqrt(num2 / m_HeadSpeed)).OnStart(delegate
		{
			if (!m_SoundIsPlaying)
			{
				UISounds.Instance.Sounds.Pantograph.PantographStart.Play();
				UISounds.Instance.Sounds.Pantograph.PantographLoopStart.Play();
				m_SoundIsPlaying = true;
			}
		}).OnComplete(delegate
		{
			UISounds.Instance.Sounds.Pantograph.PantographStop.Play();
			UISounds.Instance.Sounds.Pantograph.PantographLoopStop.Play();
			m_SoundIsPlaying = false;
		})
			.OnKill(delegate
			{
				UISounds.Instance.Sounds.Pantograph.PantographStop.Play();
				UISounds.Instance.Sounds.Pantograph.PantographLoopStop.Play();
				m_SoundIsPlaying = false;
			})
			.SetUpdate(isIndependentUpdate: true);
		m_TweenerTail.Kill();
		m_TweenerTail = m_Tail.DOMove(CalculateTailPos(m_HeadY), Mathf.Sqrt(num2 / m_TailSpeed)).SetUpdate(isIndependentUpdate: true);
	}

	private float CalculateHeadY(float boundTransformY)
	{
		return Mathf.Clamp(boundTransformY, m_MinY, m_MaxY);
	}

	private Vector3 CalculateTailPos(float headY)
	{
		float num = (headY - m_MinY) / (m_MaxY - m_MinY);
		return m_TailPath.EvaluatePosition(num);
	}

	public void Dispose()
	{
		if (m_SoundIsPlaying)
		{
			UISounds.Instance.Sounds.Pantograph.PantographStop.Play();
			UISounds.Instance.Sounds.Pantograph.PantographLoopStop.Play();
			m_SoundIsPlaying = false;
		}
		m_TweenerHead.Kill();
		m_TweenerHead = null;
		m_Label.StopText();
		SetFocus(focused: false);
		m_Disposable?.Clear();
		m_Disposable = null;
	}
}
