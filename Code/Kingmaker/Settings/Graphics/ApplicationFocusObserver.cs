using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Settings.Graphics;

public class ApplicationFocusObserver : MonoSingleton<ApplicationFocusObserver>
{
	public event Action<bool> OnApplicationChangedFocus;

	private void OnApplicationFocus(bool hasFocus)
	{
		this.OnApplicationChangedFocus?.Invoke(hasFocus);
	}
}
