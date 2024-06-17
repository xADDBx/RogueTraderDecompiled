using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.Common.Animations;

public class UIAnimationFly : MonoBehaviour
{
	public TextMeshProUGUI Text;

	public CanvasGroup CanvasGroup;

	public float Distance;

	public float Time;

	private Vector2 m_BasicPosition;

	private bool isBusy;

	private bool isInit;

	private List<UIAnimationFly> m_Dublicates;

	public void Init()
	{
		if (!isInit)
		{
			CanvasGroup.alpha = 0f;
			isInit = true;
			base.gameObject.SetActive(value: false);
			m_Dublicates = new List<UIAnimationFly>();
		}
	}

	public void SetBasicPosition(Vector2 pos)
	{
		m_BasicPosition = pos;
	}

	public void Run(string text, Color32 color)
	{
		if (!isInit)
		{
			Init();
			SetBasicPosition(base.transform.localPosition);
		}
		if (isBusy)
		{
			if (!TryFindDublicate(text, color))
			{
				CreateDublicate(text, color);
			}
			return;
		}
		Text.text = text;
		Text.color = color;
		CanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: true);
		isBusy = true;
		base.transform.localPosition = m_BasicPosition;
		base.transform.DOMoveY(m_BasicPosition.y + Distance, Time).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			isBusy = false;
		});
		CanvasGroup.DOFade(1f, Time / 3f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			CanvasGroup.DOFade(0f, Time * 2f / 3f).SetUpdate(isIndependentUpdate: true);
		});
	}

	private bool TryFindDublicate(string text, Color32 color)
	{
		foreach (UIAnimationFly dublicate in m_Dublicates)
		{
			if (!dublicate.isBusy)
			{
				dublicate.Run(text, color);
				return true;
			}
		}
		return false;
	}

	private void CreateDublicate(string text, Color32 color)
	{
		GameObject gameObject = Object.Instantiate(base.gameObject);
		gameObject.transform.SetParent(base.transform.parent, worldPositionStays: false);
		m_Dublicates.Add(gameObject.GetComponent<UIAnimationFly>());
		m_Dublicates[m_Dublicates.Count - 1].Init();
		m_Dublicates[m_Dublicates.Count - 1].SetBasicPosition(m_BasicPosition);
		m_Dublicates[m_Dublicates.Count - 1].Run(text, color);
	}
}
