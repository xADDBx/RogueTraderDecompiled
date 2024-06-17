using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorVM : BaseCharGenAppearancePageComponentVM, IVirtualListElementIdentifier
{
	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_NoItemsDesc = new ReactiveProperty<string>();

	private readonly bool m_HideIfNoElements;

	public readonly SelectionGroupRadioVM<TextureSelectorItemVM> SelectionGroup;

	public IReadOnlyReactiveProperty<string> Title => m_Title;

	public IReadOnlyReactiveProperty<string> Description => m_Description;

	public IReadOnlyReactiveProperty<string> NoItemsDesc => m_NoItemsDesc;

	public int VirtualListTypeId { get; }

	public TextureSelectorVM(SelectionGroupRadioVM<TextureSelectorItemVM> selectionGroup, TextureSelectorType typeId, bool hideIfNoElements = true)
	{
		m_HideIfNoElements = hideIfNoElements;
		AddDisposable(SelectionGroup = selectionGroup);
		AddDisposable(SelectionGroup.SelectedEntity.Subscribe(delegate
		{
			Changed();
		}));
		AddDisposable(SelectionGroup.EntitiesCollection.ObserveAdd().Subscribe(delegate
		{
			UpdateState();
		}));
		AddDisposable(SelectionGroup.EntitiesCollection.ObserveRemove().Subscribe(delegate
		{
			UpdateState();
		}));
		VirtualListTypeId = (int)typeId;
		UpdateState();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(TextureSelectorItemVM vm)
		{
			vm.Dispose();
		});
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void SetDescription(string description)
	{
		m_Description.Value = description;
	}

	public void SetNoItemsDescription(string description)
	{
		m_NoItemsDesc.Value = description;
	}

	private void UpdateState()
	{
		bool activeState = SelectionGroup.EntitiesCollection.Count > 0 || !m_HideIfNoElements;
		SetActiveState(activeState);
		m_IsAvailable.Value = SelectionGroup.EntitiesCollection.Count > 0;
	}

	public void SetActiveState(bool state)
	{
		Active.Value = state;
	}

	protected override void SetSelectUISkinColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.SkinColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetSelectUIHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.HairType)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetSelectUIHairColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.HairColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.EyebrowType)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIEyebrowsColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.EyebrowColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.BeardType)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIBeardColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.BeardColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUITattooColor(int rampIndex, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.TattooColor)
		{
			SelectEntityImpl(rampIndex);
		}
	}

	protected override void SetUIPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		if ((portNumber == 0 && base.Type == CharGenAppearancePageComponent.PortType1) || (portNumber == 1 && base.Type == CharGenAppearancePageComponent.PortType2))
		{
			SelectEntityImpl(index);
		}
	}

	private void SelectEntityImpl(int index)
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			SelectionGroup.TrySelectEntity(SelectionGroup.EntitiesCollection[index]);
		}
	}
}
