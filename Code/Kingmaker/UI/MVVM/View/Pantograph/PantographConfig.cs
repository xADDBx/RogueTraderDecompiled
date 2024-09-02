using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Pantograph;

public class PantographConfig
{
	public readonly Transform Transform;

	public readonly string Text;

	public readonly List<Sprite> Icons;

	public readonly bool UseLargeView;

	public readonly string TextIcon;

	public readonly IWidgetView View;

	public readonly IViewModel ViewModel;

	public PantographConfig(Transform transform, string text, List<Sprite> icons = null, bool useLargeView = false, string textIcon = null)
	{
		Transform = transform;
		Text = text;
		Icons = icons;
		UseLargeView = useLargeView;
		TextIcon = textIcon;
	}

	public PantographConfig(Transform transform, IWidgetView itemView, IViewModel itemVM, bool useLargeView = false)
		: this(transform, string.Empty, null, useLargeView)
	{
		View = itemView;
		ViewModel = itemVM;
	}
}
