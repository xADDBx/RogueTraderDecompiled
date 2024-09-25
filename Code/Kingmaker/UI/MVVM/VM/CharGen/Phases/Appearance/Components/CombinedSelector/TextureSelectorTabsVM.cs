using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.CombinedSelector;

public class TextureSelectorTabsVM : BaseCharGenAppearancePageComponentVM
{
	private readonly AutoDisposingList<TextureSelectorVM> m_TabsSelectorVms = new AutoDisposingList<TextureSelectorVM>();

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	public readonly ReactiveProperty<TextureSelectorVM> CurrentTabSelector = new ReactiveProperty<TextureSelectorVM>();

	public readonly ReactiveProperty<int> TotalItems = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentIndex = new ReactiveProperty<int>();

	public readonly ReactiveCommand OnSetValues = new ReactiveCommand();

	private CharGenContext m_ChargenContext;

	public IReadOnlyReactiveProperty<string> Title => m_Title;

	public TextureSelectorTabsVM(CharGenContext charGenContext)
	{
		m_ChargenContext = charGenContext;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	private void Clear()
	{
		m_TabsSelectorVms.Clear();
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void SetValues(IEnumerable<TextureSelectorVM> tabsSelectors)
	{
		if (tabsSelectors.Any())
		{
			m_TabsSelectorVms.Clear();
			foreach (TextureSelectorVM tabsSelector in tabsSelectors)
			{
				m_TabsSelectorVms.Add(tabsSelector);
				AddDisposable(tabsSelector.OnChanged.Subscribe(delegate
				{
					Changed();
				}));
			}
		}
		TotalItems.Value = m_TabsSelectorVms.Count;
		SetIndex((CurrentIndex.Value < TotalItems.Value) ? CurrentIndex.Value : 0);
		OnSetValues.Execute();
	}

	public void SetIndex(int index)
	{
		if (index < 0 || index >= m_TabsSelectorVms.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		CurrentTabSelector.Value = m_TabsSelectorVms[index];
		CurrentIndex.Value = index;
		UpdateIndexForClient(index);
		EventBus.RaiseEvent(delegate(ICharGenTextureSelectorTabChangeHandler h)
		{
			h.HandleTextureSelectorTabChange(base.Type, index);
		});
		EventBus.RaiseEvent(delegate(ICharGenAppearanceComponentUpdateHandler h)
		{
			h.HandleAppearanceComponentUpdate(base.Type);
		});
	}

	protected override void SetUITattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		if (base.Type == CharGenAppearancePageComponent.Tattoo && !UINetUtility.IsControlMainCharacter())
		{
			PFLog.UI.Log($"{equipmentEntityLink.AssetId} / {index} / {tattooTabIndex}");
			SetIndex(tattooTabIndex);
			CurrentTabSelector.Value.SelectionGroup.TrySelectEntity(CurrentTabSelector.Value.SelectionGroup.EntitiesCollection[index]);
		}
	}

	private void UpdateIndexForClient(int index)
	{
		if (UINetUtility.IsControlMainCharacter() && base.Type == CharGenAppearancePageComponent.Tattoo)
		{
			int num = CurrentTabSelector.Value.SelectionGroup.EntitiesCollection.IndexOf(CurrentTabSelector.Value.SelectionGroup.SelectedEntity.Value);
			if (index < m_ChargenContext.Doll.Tattoos.Count && num < m_ChargenContext.Doll.Tattoos[index].Paints.Count)
			{
				EquipmentEntityLink tattoo = m_ChargenContext.Doll.Tattoos[index].Paints[num];
				Game.Instance.GameCommandQueue.CharGenSetTattoo(tattoo, num, index);
			}
		}
	}
}
