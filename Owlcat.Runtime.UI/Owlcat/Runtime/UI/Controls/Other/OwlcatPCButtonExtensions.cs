using System;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;

namespace Owlcat.Runtime.UI.Controls.Other;

public static class OwlcatPCButtonExtensions
{
	public static IObservable<Unit> OnLeftClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClick.AsObservable();
	}

	public static IObservable<Unit> OnRightClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClick.AsObservable();
	}

	public static IObservable<Unit> OnSingleLeftClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClick.AsObservable();
	}

	public static IObservable<Unit> OnSingleRightClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClick.AsObservable();
	}

	public static IObservable<Unit> OnLeftDoubleClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClick.AsObservable();
	}

	public static IObservable<Unit> OnRightDoubleClickAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClick.AsObservable();
	}

	public static IObservable<Unit> OnLeftClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClick.AsObservable();
	}

	public static IObservable<Unit> OnRightClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClick.AsObservable();
	}

	public static IObservable<Unit> OnSingleLeftClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClick.AsObservable();
	}

	public static IObservable<Unit> OnSingleRightClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClick.AsObservable();
	}

	public static IObservable<Unit> OnLeftDoubleClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClick.AsObservable();
	}

	public static IObservable<Unit> OnRightDoubleClickAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClick.AsObservable();
	}

	public static IObservable<Unit> OnLeftClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnRightClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnSingleLeftClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnSingleRightClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnLeftDoubleClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnRightDoubleClickNotInteractableAsObservable(this OwlcatButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnLeftClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnRightClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnSingleLeftClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleLeftClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnSingleRightClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnSingleRightClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnLeftDoubleClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnLeftDoubleClickNotInteractable.AsObservable();
	}

	public static IObservable<Unit> OnRightDoubleClickNotInteractableAsObservable(this OwlcatMultiButton button)
	{
		if (button == null)
		{
			return Observable.Empty<Unit>();
		}
		return button.OnRightDoubleClickNotInteractable.AsObservable();
	}

	public static IObservable<bool> OnHoverAsObservable(this OwlcatSelectable selectable)
	{
		if (selectable == null)
		{
			return Observable.Empty<bool>();
		}
		return selectable.OnHover.AsObservable();
	}
}
