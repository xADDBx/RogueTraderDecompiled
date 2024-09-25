using Kingmaker.Code.UI.MVVM.VM.Slots;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public interface IItemSlotView
{
	ItemSlotVM SlotVM { get; }

	RectTransform GetParentContainer();
}
