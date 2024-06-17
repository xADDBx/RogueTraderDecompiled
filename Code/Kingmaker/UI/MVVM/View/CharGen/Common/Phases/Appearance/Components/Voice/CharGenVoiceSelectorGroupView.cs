using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Voice;

public class CharGenVoiceSelectorGroupView : ViewBase<SelectionGroupRadioVM<CharGenVoiceItemVM>>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharGenVoiceItemView m_ItemPrefab;

	[SerializeField]
	private float m_EnsureVisibleFocusDelta = 100f;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	public IConsoleEntity ConsoleEntityProxy => m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		DrawEntities();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemPrefab));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn(m_WidgetList.GetNavigationEntities());
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			m_NavigationBehaviour.FocusOnEntityManual(GetSelectedEntity());
		}
		else
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenVoiceItemView>().FirstOrDefault((CharGenVoiceItemView i) => (i.GetViewModel() as CharGenVoiceItemVM)?.IsSelected.Value ?? false);
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectExtended.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, m_EnsureVisibleFocusDelta, smoothly: false, needPinch: false);
		}
	}
}
