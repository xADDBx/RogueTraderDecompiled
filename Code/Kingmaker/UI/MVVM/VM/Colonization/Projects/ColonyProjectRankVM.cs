using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectRankVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<ColonyProjectRank> Rank = new ReactiveProperty<ColonyProjectRank>();

	public ColonyProjectRankVM(ColonyProjectRank rank)
	{
		Rank.Value = rank;
	}

	protected override void DisposeImplementation()
	{
	}
}
