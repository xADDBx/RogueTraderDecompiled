using Kingmaker.Blueprints.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;

public abstract class BaseCharGenAppearancePageComponentVM : VirtualListElementVMBase, ICharGenDollStateHandler, ISubscriber
{
	public readonly ReactiveCommand<CharGenAppearancePageComponent> OnChanged = new ReactiveCommand<CharGenAppearancePageComponent>();

	public CharGenAppearancePageComponent Type { get; set; }

	public void Changed()
	{
		OnChanged.Execute(Type);
		EventBus.RaiseEvent(delegate(ICharGenAppearancePageComponentHandler h)
		{
			h.HandleComponentChanged(Type);
		});
	}

	public void Focused()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePageComponentHandler h)
		{
			h.HandleComponentChanged(Type);
		});
	}

	public virtual void OnBeginView()
	{
	}

	public void HandleSetGender(Gender gender, int index)
	{
		SetSelectUIGender(gender, index);
	}

	protected virtual void SetSelectUIGender(Gender gender, int index)
	{
	}

	public void HandleSetHead(EquipmentEntityLink head, int index)
	{
		SetSelectUIHead(head, index);
	}

	protected virtual void SetSelectUIHead(EquipmentEntityLink head, int index)
	{
	}

	public void HandleSetRace(BlueprintRaceVisualPreset blueprint, int index)
	{
		SetSelectUIRace(blueprint, index);
	}

	protected virtual void SetSelectUIRace(BlueprintRaceVisualPreset blueprint, int index)
	{
	}

	public void HandleSetSkinColor(int index)
	{
		SetSelectUISkinColor(index);
	}

	protected virtual void SetSelectUISkinColor(int index)
	{
	}

	public void HandleSetHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetSelectUIHair(equipmentEntityLink, index);
	}

	protected virtual void SetSelectUIHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	public void HandleSetHairColor(int index)
	{
		SetSelectUIHairColor(index);
	}

	protected virtual void SetSelectUIHairColor(int index)
	{
	}

	public void HandleSetEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetUIEyebrows(equipmentEntityLink, index);
	}

	protected virtual void SetUIEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	public void HandleSetEyebrowsColor(int index)
	{
		SetUIEyebrowsColor(index);
	}

	protected virtual void SetUIEyebrowsColor(int index)
	{
	}

	public void HandleSetBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetUIBeard(equipmentEntityLink, index);
	}

	protected virtual void SetUIBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	public void HandleSetBeardColor(int index)
	{
		SetUIBeardColor(index);
	}

	protected virtual void SetUIBeardColor(int index)
	{
	}

	public void HandleSetScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetUIScar(equipmentEntityLink, index);
	}

	protected virtual void SetUIScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	public void HandleSetTattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		SetUITattoo(equipmentEntityLink, index, tattooTabIndex);
	}

	protected virtual void SetUITattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
	}

	public void HandleSetTattooColor(int rampIndex, int index)
	{
		SetUITattooColor(rampIndex, index);
	}

	protected virtual void SetUITattooColor(int rampIndex, int index)
	{
	}

	public void HandleSetPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		SetUIPort(equipmentEntityLink, index, portNumber);
	}

	protected virtual void SetUIPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
	}

	public void HandleSetEquipmentColor(int primaryIndex, int secondaryIndex)
	{
	}

	void ICharGenDollStateHandler.HandleShowCloth(bool showCloth)
	{
	}
}
