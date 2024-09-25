using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentPieceDescriptorListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public List<DismembermentPieceDescriptorVM> Pieces = new List<DismembermentPieceDescriptorVM>();

	protected override void DisposeImplementation()
	{
	}
}
