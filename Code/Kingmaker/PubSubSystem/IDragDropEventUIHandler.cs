using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.PubSubSystem;

public interface IDragDropEventUIHandler : ISubscriber
{
	void OnBeginDrag(PointerEventData eventData, GameObject gObjectSource);

	void OnEndDrag(PointerEventData eventData);

	void OnDrag(PointerEventData eventData);

	void OnDrop(PointerEventData eventData, GameObject gObjectTarget);

	void Reset();
}
