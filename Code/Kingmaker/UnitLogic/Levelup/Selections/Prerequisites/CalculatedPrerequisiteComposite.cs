using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;

public class CalculatedPrerequisiteComposite : CalculatedPrerequisite
{
	private const string HeaderTemplate = "{0}:\n";

	private const string InnerPrerequisiteTemplate = "\t<color=#{0}>{1}</color>\n";

	private readonly CalculatedPrerequisite[] m_Prerequisites;

	public FeaturePrerequisiteComposition Composition { get; }

	public ReadonlyList<CalculatedPrerequisite> Prerequisites => m_Prerequisites;

	private static CalculatedPrerequisiteStrings Strings => LocalizedTexts.Instance.CalculatedPrerequisites;

	public CalculatedPrerequisiteComposite(bool value, FeaturePrerequisiteComposition composition, bool not, [NotNull] IEnumerable<CalculatedPrerequisite> prerequisites)
		: base(value, not)
	{
		m_Prerequisites = prerequisites.ToArray();
		Composition = composition;
	}

	protected override string GetDescriptionInternal()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		string arg = ((Composition != 0) ? (base.Not ? Strings.CompositeOrFalse.Text : Strings.CompositeOrTrue.Text) : (base.Not ? Strings.CompositeAndFalse.Text : Strings.CompositeAndTrue.Text));
		builder.Append($"{arg}:\n");
		foreach (CalculatedPrerequisite prerequisite in Prerequisites)
		{
			Color32 color = (prerequisite.Value ? UIConfig.Instance.TooltipColors.Bonus : UIConfig.Instance.TooltipColors.Penaty);
			builder.Append($"\t<color=#{ColorUtility.ToHtmlStringRGB(color)}>{prerequisite.Description}</color>\n");
		}
		return builder.ToString();
	}
}
