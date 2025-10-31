using System.Collections;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorGroupView : ViewBase<SelectionGroupRadioVM<TextureSelectorItemVM>>, IConsoleNavigationEntity, IConsoleEntity, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler, IConfirmClickHandler
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_DescriptionObject;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private TextureSelectorItemView m_ItemPrefab;

	[SerializeField]
	protected int m_ItemsPerRow;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected bool m_IsCooldownActive;

	public bool IsBusy => m_IsCooldownActive;

	protected override void BindViewImplementation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		DrawEntities();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_IsCooldownActive = false;
		StopAllCoroutines();
		base.gameObject.SetActive(value: false);
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}

	public void SetDescriptionText(string description)
	{
		if (!(m_Description == null))
		{
			m_DescriptionObject.SetActive(!string.IsNullOrEmpty(description));
			m_Description.text = description;
		}
	}

	protected virtual void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemPrefab));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		UpdateNavigation();
		base.gameObject.SetActive(value: true);
	}

	protected virtual void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetList.GetNavigationEntities(), m_ItemsPerRow);
	}

	public virtual bool HandleUp()
	{
		if (m_IsCooldownActive)
		{
			return true;
		}
		MainThreadDispatcher.StartCoroutine(HandleCooldown());
		return m_NavigationBehaviour.HandleUp();
	}

	public virtual bool HandleDown()
	{
		if (m_IsCooldownActive)
		{
			return true;
		}
		MainThreadDispatcher.StartCoroutine(HandleCooldown());
		return m_NavigationBehaviour.HandleDown();
	}

	public virtual bool HandleLeft()
	{
		if (m_IsCooldownActive)
		{
			return true;
		}
		MainThreadDispatcher.StartCoroutine(HandleCooldown());
		return m_NavigationBehaviour.HandleLeft();
	}

	public virtual bool HandleRight()
	{
		if (m_IsCooldownActive)
		{
			return true;
		}
		MainThreadDispatcher.StartCoroutine(HandleCooldown());
		return m_NavigationBehaviour.HandleRight();
	}

	public virtual void SetFocus(bool value)
	{
		if (value)
		{
			m_NavigationBehaviour.FocusOnEntityManual(GetSelectedEntity());
			return;
		}
		GetSelectedItemVM()?.SetSelected(state: true);
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	private TextureSelectorItemVM GetSelectedItemVM()
	{
		return (from TextureSelectorItemView view in m_WidgetList.Entries
			select view.GetViewModel() as TextureSelectorItemVM).FirstOrDefault((TextureSelectorItemVM vm) => vm?.IsSelected.Value ?? false);
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<TextureSelectorItemView>().FirstOrDefault((TextureSelectorItemView i) => (i.GetViewModel() as TextureSelectorItemVM)?.IsSelected.Value ?? false);
	}

	public bool IsValid()
	{
		return true;
	}

	public virtual bool CanConfirmClick()
	{
		return m_NavigationBehaviour.CanConfirmClick();
	}

	public virtual void OnConfirmClick()
	{
		m_NavigationBehaviour.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	private IEnumerator HandleCooldown()
	{
		m_IsCooldownActive = true;
		yield return new WaitForSecondsRealtime(0.2f);
		m_IsCooldownActive = false;
	}
}
