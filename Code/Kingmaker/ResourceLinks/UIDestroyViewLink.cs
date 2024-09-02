using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
public class UIDestroyViewLink<TView, TViewModel> : UIViewLink<TView, TViewModel> where TView : ViewBase<TViewModel> where TViewModel : class, IViewModel
{
	protected override void Unbind()
	{
		UnityEngine.Object.Destroy(View.gameObject);
		View = null;
		base.Unbind();
	}
}
