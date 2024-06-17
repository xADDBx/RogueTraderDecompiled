using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;

public class EncyclopediaPageImageVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite Image;

	private readonly Action<EncyclopediaPageImageVM> m_ZoomAction;

	public bool IsZoomAllowed => m_ZoomAction != null;

	public EncyclopediaPageImageVM(Sprite image, Action<EncyclopediaPageImageVM> zoomAction = null)
	{
		Image = image;
		m_ZoomAction = zoomAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleZoomClick()
	{
		m_ZoomAction?.Invoke(this);
	}
}
