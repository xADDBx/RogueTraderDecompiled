using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.PC;

public class ContextMenuPCView : ContextMenuView
{
	private bool m_Hover;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(OnUpdate));
		AddDisposable(this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			m_Hover = true;
		}));
		AddDisposable(this.OnPointerExitAsObservable().Subscribe(delegate
		{
			m_Hover = false;
		}));
	}

	private void OnUpdate()
	{
		if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !m_Hover)
		{
			ContextMenuHelper.HideContextMenu();
		}
	}
}
