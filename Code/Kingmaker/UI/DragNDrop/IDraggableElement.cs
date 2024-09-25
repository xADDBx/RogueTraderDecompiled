using UnityEngine.EventSystems;

namespace Kingmaker.UI.DragNDrop;

public interface IDraggableElement
{
	void StartDrag();

	void EndDrag(PointerEventData eventData);

	bool SetDragSlot(DragNDropManager slot);

	void CancelDrag();
}
