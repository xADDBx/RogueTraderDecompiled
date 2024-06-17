using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Pantograph;

public class PantographConfig
{
	public Transform Transform;

	public string Text;

	public List<Sprite> Icons;

	public bool UseLargeView;

	public IWidgetView View;

	public IViewModel ViewModel;

	public PantographConfig(Transform transform, string text, List<Sprite> icons = null, bool useLargeView = false)
	{
		Transform = transform;
		Text = text;
		Icons = icons;
		UseLargeView = useLargeView;
	}

	public PantographConfig(Transform transform, IWidgetView itemView, IViewModel itemVM, bool useLargeView = false)
		: this(transform, string.Empty, null, useLargeView)
	{
		View = itemView;
		ViewModel = itemVM;
	}
}
