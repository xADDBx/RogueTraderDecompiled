using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public class RankEntryEmptyFeaturesGroupVM : RankEntryFeatureGroupVM
{
	private readonly string m_Description;

	public RankEntryEmptyFeaturesGroupVM(string description)
		: base(new List<BaseRankEntryFeatureVM>())
	{
		m_Description = description;
	}

	public override List<VirtualListElementVMBase> GetAll()
	{
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		if (string.IsNullOrEmpty(m_Description))
		{
			return list;
		}
		RankEntryDescriptionVM disposable = new RankEntryDescriptionVM(m_Description);
		list.Add(AddDisposableAndReturn(disposable));
		return list;
	}
}
