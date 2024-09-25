using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class ItemsStrings : StringsContainer
{
	[Header("CopyItem Context Actions")]
	public LocalizedString CopyItem;

	public LocalizedString CopyScroll;

	public LocalizedString CopyRecipe;

	[Header("Item Name Modifiers")]
	public LocalizedString PotionPrefix;

	public LocalizedString FlaskPrefix;

	public LocalizedString ScrollPrefix;

	public LocalizedString WandPrefix;

	public LocalizedString FlaskDescriptionPrefix;

	[Header("Identify")]
	public LocalizedString NotIdentified;

	public LocalizedString NotIdentifiedSuffix;
}
