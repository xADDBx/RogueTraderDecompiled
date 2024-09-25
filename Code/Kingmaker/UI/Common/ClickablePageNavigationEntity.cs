using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class ClickablePageNavigationEntity : MonoBehaviour, IDisposable
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>();

	private Action m_OnClickCallback;

	public int PageIndex { get; private set; }

	public void Initialize(int pageIndex, Action onClickCallback)
	{
		m_OnClickCallback = onClickCallback;
		base.gameObject.SetActive(value: true);
		PageIndex = pageIndex;
		m_Disposables.Add(m_MultiButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnClick();
		}));
		SetSelected(state: false);
		m_Label.text = (pageIndex + 1).ToString();
	}

	public void SetSelected(bool state)
	{
		m_MultiButton.SetActiveLayer(state ? 1 : 0);
	}

	private void OnClick()
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			SetSelected(state: true);
			m_OnClickCallback();
		}
	}

	public void Dispose()
	{
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
	}
}
