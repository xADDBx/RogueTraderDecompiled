using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsTwoColumnsPage : CreditElement, ICreditsBlock, ICreditsView
{
	[SerializeField]
	protected CanvasGroup m_Group;

	[SerializeField]
	protected RectTransform m_Content;

	[SerializeField]
	protected CreditsPersonElement m_PersonPrefab;

	public List<ICreditsElement> m_Rows = new List<ICreditsElement>();

	public Transform Content => m_Content;

	public void Initialize(ICreditsView view)
	{
		base.transform.SetParent(view.Content);
		base.transform.ResetAll();
		if (base.transform is RectTransform rectTransform)
		{
			rectTransform.sizeDelta = Vector2.zero;
			rectTransform.anchoredPosition = Vector2.zero;
		}
	}

	public virtual void Append(string row, BlueprintCreditsGroup group)
	{
		string personName = PageGenerator.ReadPerson(row);
		CreditsPersonElement instance = m_PersonPrefab.GetInstance<CreditsPersonElement>();
		instance.Initialize(personName, "", CreditsPersonElement.Align.Center, this);
		m_Rows.Add(instance);
	}

	public void Ping(int row)
	{
		if (row >= 0 && row < m_Rows.Count)
		{
			m_Rows[row].Ping();
		}
	}

	public void Show()
	{
		m_Group.DOFade(1f, 0.3f).SetUpdate(isIndependentUpdate: true);
	}

	public void Hide()
	{
		m_Group.DOFade(0f, 0.3f).SetUpdate(isIndependentUpdate: true).OnComplete(base.Destroy);
	}
}
