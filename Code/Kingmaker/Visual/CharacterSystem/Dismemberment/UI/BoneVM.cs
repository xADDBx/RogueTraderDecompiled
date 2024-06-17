using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class BoneVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Transform Bone;

	public BoolReactiveProperty IsMarkedForSevering = new BoolReactiveProperty(initialValue: false);

	public BoneVM(Transform bone)
	{
		Bone = bone;
	}

	protected override void DisposeImplementation()
	{
	}

	internal void OnSeveringChanged(bool value)
	{
		IsMarkedForSevering.Value = value;
		BoneListVM.BoneMarkedForSeveringChanged?.Invoke(this);
	}
}
