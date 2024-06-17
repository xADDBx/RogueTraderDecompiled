using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IEntityViewBase
{
	Transform ViewTransform { get; }

	string GameObjectName { get; set; }

	GameObject GO { get; }

	bool IsVisible { get; }

	bool IsInGame { get; }

	string UniqueViewId { get; set; }

	bool IsInGameBySettings { get; }

	IEntity Data { get; }

	void UpdateViewActive();

	void OnInFogOfWarChanged();

	void AttachToData(IEntity data);

	void DetachFromData();

	void DestroyViewObject();
}
