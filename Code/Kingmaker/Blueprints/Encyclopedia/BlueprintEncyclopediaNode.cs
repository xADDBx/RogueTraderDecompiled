using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("8d4270da5e193cb45ae875b54f1400a2")]
public abstract class BlueprintEncyclopediaNode : BlueprintScriptableObject, INode
{
	[NotNull]
	public LocalizedString Title;

	public bool Expanded;

	public List<BlueprintEncyclopediaPageReference> ChildPages = new List<BlueprintEncyclopediaPageReference>();

	public bool FirstExpanded => Expanded;

	public virtual List<IPage> GetChilds()
	{
		List<IPage> pages = new List<IPage>();
		ChildPages.ForEach(delegate(BlueprintEncyclopediaPageReference x)
		{
			pages.Add(x.Get());
		});
		return pages;
	}

	public bool IsChilds()
	{
		return ChildPages.Any();
	}

	public virtual string GetTitle()
	{
		return Title;
	}
}
