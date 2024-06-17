using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class ClickablePageNavigation : MonoBehaviour, IDisposable
{
	[SerializeField]
	private ClickablePageNavigationEntity m_PointPrefab;

	[SerializeField]
	private Transform m_PointsContainer;

	private readonly List<ClickablePageNavigationEntity> m_Points = new List<ClickablePageNavigationEntity>();

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>();

	private int m_CurrentIndex;

	private int m_PageNumber;

	private Action<int> m_ChooseCallback;

	public void Initialize(int pageNumber, Action<int> chooseCallback = null)
	{
		Dispose();
		if (pageNumber <= 1)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_PageNumber = pageNumber;
		m_CurrentIndex = 0;
		FillPoints(pageNumber);
		base.gameObject.SetActive(value: true);
		m_ChooseCallback = chooseCallback;
		OnClickPage(0);
	}

	private void FillPoints(int number)
	{
		for (int i = 0; i < number; i++)
		{
			ClickablePageNavigationEntity widget = WidgetFactory.GetWidget(m_PointPrefab);
			m_Disposables.Add(widget);
			widget.transform.SetParent(m_PointsContainer, worldPositionStays: false);
			m_Points.Add(widget);
			int ii = i;
			widget.Initialize(i, delegate
			{
				OnClickPage(ii);
			});
		}
	}

	public void OnClickPage(int i)
	{
		m_CurrentIndex = i;
		foreach (ClickablePageNavigationEntity point in m_Points)
		{
			point.SetSelected(point.PageIndex == i);
		}
		m_ChooseCallback(i);
	}

	public void NextPage()
	{
		int num = m_CurrentIndex + 1;
		if (num >= m_PageNumber)
		{
			num = 0;
		}
		OnClickPage(num);
	}

	public void Dispose()
	{
		m_Points.ForEach(WidgetFactory.DisposeWidget);
		m_Points.Clear();
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_Disposables.Clear();
	}
}
