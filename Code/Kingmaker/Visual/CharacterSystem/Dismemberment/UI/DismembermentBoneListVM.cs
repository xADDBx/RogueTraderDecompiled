using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentBoneListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private List<DismembermentBoneVM> m_Bones = new List<DismembermentBoneVM>();

	public List<DismembermentBoneVM> Bones => m_Bones;

	protected override void DisposeImplementation()
	{
	}
}
