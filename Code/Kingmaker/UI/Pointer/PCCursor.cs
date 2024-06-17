using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class PCCursor : BaseCursor, IRewiredCursorController
{
	public static PCCursor Instance { get; private set; }

	bool IRewiredCursorController.Enabled
	{
		get
		{
			return base.IsActive;
		}
		set
		{
			SetActive(value);
		}
	}

	GameObject IRewiredCursorController.Cursor => m_CursorTransform.Or(null)?.gameObject;

	protected override void OnBind()
	{
		Instance = this;
		UIKitRewiredCursorController.SetRewiredCursorController(this);
		m_Disposable.Add(MainThreadDispatcher.UpdateAsObservable().Subscribe(OnUpdate));
	}

	protected override void OnDispose()
	{
		Instance = null;
	}

	private void OnUpdate()
	{
		base.Position = Input.mousePosition;
	}
}
