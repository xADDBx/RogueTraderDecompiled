using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.UIVisibility;

public class UIVisibilityView : ViewBase<UIVisibilityVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private UIVisibilityFlags UIVisibilityFlag;

	public void Initialize()
	{
		if (m_CanvasGroup == null)
		{
			m_CanvasGroup = GetComponent<CanvasGroup>();
		}
		m_CanvasGroup.alpha = 1f;
	}

	protected override void BindViewImplementation()
	{
		if (UIVisibilityFlag == UIVisibilityFlags.None)
		{
			UberDebug.LogError("Error: UIVisibilityFlag is None");
		}
		AddDisposable(UIVisibilityState.VisibilityPreset.Subscribe(delegate(UIVisibilityFlags flags)
		{
			m_CanvasGroup.alpha = (flags.HasFlag(UIVisibilityFlag) ? 1f : 0f);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
