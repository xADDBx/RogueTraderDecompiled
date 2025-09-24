using System;
using Kingmaker.Code.UI.MVVM;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
[RequireSeparateBundle]
public class UIViewLink<TView, TViewModel> : WeakResourceLink<TView> where TView : ViewBase<TViewModel> where TViewModel : class, IViewModel
{
	public Transform Target;

	protected TView View;

	[HideInInspector]
	public Action<TView> CustomInitialize;

	public TView ViewInstance => View;

	public void Bind(TViewModel vm)
	{
		if (vm == null)
		{
			if (View != null)
			{
				Unbind();
			}
		}
		else
		{
			CreateView();
			View.Bind(vm);
			vm.OnDispose += Unbind;
		}
	}

	protected virtual void Unbind()
	{
		ForceUnload();
	}

	public void PrewarmView()
	{
		CreateView();
	}

	private void CreateView()
	{
		if (!(View != null))
		{
			TView val = Load();
			View = UnityEngine.Object.Instantiate(val.gameObject, Target).GetComponent<TView>();
			CustomInitialize?.Invoke(View);
			if (View is IInitializable initializable)
			{
				initializable.Initialize();
			}
		}
	}
}
