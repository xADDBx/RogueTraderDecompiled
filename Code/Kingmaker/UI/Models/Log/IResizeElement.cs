using UnityEngine;

namespace Kingmaker.UI.Models.Log;

public interface IResizeElement
{
	void SetSizeDelta(Vector2 size);

	Vector2 GetSize();

	RectTransform GetTransform();
}
