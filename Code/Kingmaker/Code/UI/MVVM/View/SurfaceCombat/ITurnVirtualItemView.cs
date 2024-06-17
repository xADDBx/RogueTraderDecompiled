using System;
using DG.Tweening;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public interface ITurnVirtualItemView
{
	ITurnVirtualItemData BoundData { get; }

	MonoBehaviour View { get; }

	OwlcatSelectable Selectable { get; }

	RectTransform RectTransform { get; }

	CanvasGroup CanvasGroup { get; }

	bool WillBeReused { get; set; }

	IViewModel GetViewModel();

	void ViewBind(ITurnVirtualItemData viewModel);

	Sequence GetHideAnimation(Action completeAction);

	Sequence GetMoveAnimation(Action completeAction, Vector2 targetPosition);

	Sequence GetShowAnimation(Action completeAction, Vector2 targetPosition);

	void SetAnchoredPosition(Vector2 position);

	void DestroyViewItem();
}
