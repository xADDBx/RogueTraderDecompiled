using System;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;

public class CharGenPortraitGroupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly PortraitCategory PortraitCategory;

	public readonly ReactiveCollection<CharGenPortraitSelectorItemVM> PortraitCollection = new ReactiveCollection<CharGenPortraitSelectorItemVM>();

	public readonly BoolReactiveProperty Expanded = new BoolReactiveProperty();

	public CharGenPortraitGroupVM()
	{
	}

	public CharGenPortraitGroupVM(PortraitCategory portraitCategory)
	{
		PortraitCategory = portraitCategory;
	}

	public void Add(CharGenPortraitSelectorItemVM portrait)
	{
		PortraitCollection.Add(portrait);
	}

	public void RemoveNonexistentItems()
	{
		PortraitCollection.RemoveAll(delegate(CharGenPortraitSelectorItemVM item)
		{
			PortraitData portraitData = item.PortraitData;
			return (portraitData == null) ? (!item.CustomPortraitCreatorItem) : (!portraitData.EnsureImages());
		});
	}

	public void RemoveCustomItems()
	{
		PortraitCollection.RemoveAll((CharGenPortraitSelectorItemVM item) => item.PortraitData?.IsCustom ?? (!item.CustomPortraitCreatorItem));
	}

	public void Clear()
	{
		PortraitCollection.Clear();
	}

	public CharGenPortraitSelectorItemVM GetByIdOrFirstValid(string id)
	{
		return PortraitCollection.FirstOrDefault((CharGenPortraitSelectorItemVM i) => i.PortraitData?.CustomId == id) ?? PortraitCollection.FirstOrDefault((CharGenPortraitSelectorItemVM i) => !i.CustomPortraitCreatorItem);
	}

	protected override void DisposeImplementation()
	{
	}
}
