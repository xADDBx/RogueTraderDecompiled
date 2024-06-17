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

	private TView m_View;

	[HideInInspector]
	public Action<TView> CustomInitialize;

	public void Bind(TViewModel vm)
	{
		if (vm == null)
		{
			if (m_View != null)
			{
				Unbind();
			}
			return;
		}
		if (m_View == null)
		{
			TView val = Load();
			m_View = UnityEngine.Object.Instantiate(val.gameObject, Target).GetComponent<TView>();
			if (CustomInitialize != null)
			{
				CustomInitialize(m_View);
			}
			if (m_View is IInitializable)
			{
				((IInitializable)m_View).Initialize();
			}
		}
		m_View.Bind(vm);
		vm.OnDispose += Unbind;
	}

	private void Unbind()
	{
		UnityEngine.Object.Destroy(m_View.gameObject);
		m_View = null;
		ForceUnload();
	}
}
