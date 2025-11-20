using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.Utility;

public class WidgetList : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private Transform m_Container;

	private readonly List<IWidgetView> m_PrefabsCollection = new List<IWidgetView>();

	private List<IWidgetView> m_Entries;

	private readonly List<IWidgetView> m_VisibleEntries = new List<IWidgetView>();

	private int m_CurrentCollectionLength;

	private bool m_StrictMatching;

	public List<IWidgetView> Entries => m_Entries;

	public List<IWidgetView> VisibleEntries => m_VisibleEntries;

	public Transform Container => m_Container;

	public IDisposable DrawEntries<TWidget>(IEnumerable<IViewModel> vmCollection, TWidget entryPrefab, bool strictMatching = false) where TWidget : IWidgetView
	{
		m_VisibleEntries.Clear();
		m_PrefabsCollection.Clear();
		m_PrefabsCollection.Add(entryPrefab);
		m_StrictMatching = strictMatching;
		return UpdateWidgets<TWidget>(vmCollection);
	}

	public IDisposable DrawMultiEntries<TWidget>(IEnumerable<IViewModel> vmCollection, List<TWidget> entryPrefabs) where TWidget : IWidgetView
	{
		m_VisibleEntries.Clear();
		m_PrefabsCollection.Clear();
		foreach (TWidget entryPrefab in entryPrefabs)
		{
			m_PrefabsCollection.Add(entryPrefab);
		}
		return UpdateWidgets<TWidget>(vmCollection);
	}

	public IDisposable DrawMultiEntries<TWidget>(IEnumerable<IViewModel> vmCollection, List<TWidget> entryPrefabs, bool strictMatching) where TWidget : IWidgetView
	{
		Clear();
		m_VisibleEntries.Clear();
		m_PrefabsCollection.Clear();
		foreach (TWidget entryPrefab in entryPrefabs)
		{
			m_PrefabsCollection.Add(entryPrefab);
		}
		m_StrictMatching = strictMatching;
		return UpdateWidgets<TWidget>(vmCollection);
	}

	private IDisposable UpdateWidgets<TWidget>(IEnumerable<IViewModel> vmCollection) where TWidget : IWidgetView
	{
		int num = 0;
		if (vmCollection != null)
		{
			foreach (IViewModel item in vmCollection)
			{
				GetNextItem<TWidget>(num, item)?.BindWidgetVM(item);
				num++;
			}
		}
		m_CurrentCollectionLength = num;
		HideNotUsed(m_CurrentCollectionLength);
		return Disposable.Create(Clear);
	}

	public void DrawEmptyEntities<TWidget>(TWidget prefab, int number) where TWidget : IWidgetView
	{
		if (m_Entries == null)
		{
			m_Entries = new List<IWidgetView>((IEnumerable<IWidgetView>)m_Container.GetComponentsInChildren<TWidget>().ToList());
		}
		for (int i = m_CurrentCollectionLength; i < m_CurrentCollectionLength + number; i++)
		{
			IWidgetView widgetView = m_Entries.FirstOrDefault((IWidgetView instantiatedEntity) => instantiatedEntity.GetType() == prefab.GetType() && !m_VisibleEntries.Contains(instantiatedEntity));
			if (widgetView == null)
			{
				widgetView = (IWidgetView)WidgetFactory.GetWidget(prefab.MonoBehaviour);
				widgetView.MonoBehaviour.transform.SetParent(m_Container, worldPositionStays: false);
				m_Entries.Add(widgetView);
			}
			widgetView.MonoBehaviour.gameObject.SetActive(value: true);
			widgetView.BindWidgetVM(null);
			m_VisibleEntries.Add(widgetView);
		}
	}

	public void Clear()
	{
		m_Entries?.ForEach(delegate(IWidgetView e)
		{
			WidgetFactory.DisposeWidget(e.MonoBehaviour);
		});
		m_Entries?.Clear();
		m_VisibleEntries.Clear();
		m_PrefabsCollection.Clear();
	}

	private IWidgetView GetNextItem<TWidget>(int i, IViewModel vm) where TWidget : IWidgetView
	{
		if (m_Entries == null)
		{
			m_Entries = new List<IWidgetView>((IEnumerable<IWidgetView>)(from v in m_Container.GetComponentsInChildren<TWidget>()
				where v.CheckType(vm)
				select v).ToList());
		}
		List<IWidgetView> source = m_Entries.Where((IWidgetView entry) => entry != null && entry.CheckType(vm) && !m_VisibleEntries.Contains(entry)).ToList();
		IWidgetView widgetView;
		if (source.Any())
		{
			widgetView = source.First();
		}
		else
		{
			widgetView = (IWidgetView)WidgetFactory.GetWidget(m_PrefabsCollection.FirstOrDefault((IWidgetView pref) => pref.CheckType(vm)).MonoBehaviour, activate: true, m_StrictMatching);
			widgetView.MonoBehaviour.transform.SetParent(m_Container, worldPositionStays: false);
			m_Entries.Add(widgetView);
		}
		widgetView.MonoBehaviour.gameObject.SetActive(value: true);
		widgetView.MonoBehaviour.transform.SetSiblingIndex(i);
		m_VisibleEntries.Add(widgetView);
		return widgetView;
	}

	public void HideAll()
	{
		if (m_Entries == null)
		{
			return;
		}
		foreach (IWidgetView entry in m_Entries)
		{
			entry.MonoBehaviour.gameObject.SetActive(value: false);
		}
	}

	public void HideNotUsed(int currentCollectionLength)
	{
		if (m_Entries != null)
		{
			for (int i = currentCollectionLength; i < m_Entries.Count; i++)
			{
				m_Entries[i].MonoBehaviour.gameObject.SetActive(value: false);
			}
		}
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		if (m_Entries != null && m_Entries.Any())
		{
			list.AddRange(m_Entries.Select((IWidgetView entry) => (IConsoleNavigationEntity)entry).ToList());
		}
		return list;
	}
}
