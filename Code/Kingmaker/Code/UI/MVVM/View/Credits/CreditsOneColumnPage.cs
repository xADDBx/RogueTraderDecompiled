using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsOneColumnPage : CreditElement, ICreditsBlock, ICreditsView
{
	[SerializeField]
	protected CanvasGroup m_Group;

	[SerializeField]
	protected CreditsPersonElement PersonRolePrefab;

	[SerializeField]
	protected CreditsHeaderElement m_HeaderPrefab;

	[SerializeField]
	protected CreditsTextElement m_TextPrefab;

	[SerializeField]
	protected GameObject m_RoleSeparator;

	[SerializeField]
	protected RectTransform m_Content;

	public List<ICreditsElement> m_Rows = new List<ICreditsElement>();

	private bool m_AddSeparator = true;

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
		m_AddSeparator = false;
	}

	public virtual void Append(string row, BlueprintCreditsGroup group)
	{
		string text = PageGenerator.ReadPerson(row);
		string text2 = PageGenerator.ReadHeader(row);
		string text3 = PageGenerator.ReadText(row);
		if (!string.IsNullOrEmpty(text2))
		{
			CreditsHeaderElement instance = m_HeaderPrefab.GetInstance<CreditsHeaderElement>();
			instance.Initialize(text2, this);
			m_Rows.Add(instance);
			m_AddSeparator = false;
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (m_AddSeparator)
			{
				AddSeparator();
			}
			string role = group.RolesData?.GetRole(PageGenerator.ReadRole(row));
			CreditsPersonElement instance2 = PersonRolePrefab.GetInstance<CreditsPersonElement>();
			instance2.Initialize(text, role, CreditsPersonElement.Align.Center, this);
			m_Rows.Add(instance2);
			m_AddSeparator = false;
		}
		if (!string.IsNullOrEmpty(text3))
		{
			AddSeparator();
			CreditsTextElement instance3 = m_TextPrefab.GetInstance<CreditsTextElement>();
			instance3.Initialize(text3, this);
			m_Rows.Add(instance3);
			AddSeparator();
			m_AddSeparator = false;
		}
	}

	public void AddSeparator()
	{
		GameObject obj = Object.Instantiate(m_RoleSeparator, m_Content, worldPositionStays: true);
		obj.transform.ResetAll();
		obj.SetActive(value: true);
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
