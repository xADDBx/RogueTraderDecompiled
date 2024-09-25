using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentBoneVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private Action<DismembermentBoneVM> m_RemoveCallback;

	public UnitDismembermentManager.DismembermentBone DismembermentBone { get; private set; }

	public DismembermentBoneVM(UnitDismembermentManager.DismembermentBone bone, Action<DismembermentBoneVM> removeCallback)
	{
		DismembermentBone = bone;
		m_RemoveCallback = removeCallback;
	}

	protected override void DisposeImplementation()
	{
	}

	internal void OnSliceOffsetChanged(float sliceOffset)
	{
		DismembermentBone.SliceOffset = sliceOffset;
	}

	internal void OnSliceOrientationXChanged(float x)
	{
		DismembermentBone.SliceOrientationEuler.x = x;
	}

	internal void OnSliceOrientationYChanged(float y)
	{
		DismembermentBone.SliceOrientationEuler.y = y;
	}

	internal void OnSliceOrientationZChanged(float z)
	{
		DismembermentBone.SliceOrientationEuler.z = z;
	}

	internal void OnRemove()
	{
		m_RemoveCallback?.Invoke(this);
	}
}
