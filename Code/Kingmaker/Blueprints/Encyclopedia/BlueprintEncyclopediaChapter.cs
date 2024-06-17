using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("a2f5cc0482d9e184d8edde8490dd7d90")]
public class BlueprintEncyclopediaChapter : BlueprintEncyclopediaPage
{
	[Tooltip("Stay true when you want hide it in encyclopedia, but to use it for glossary tooltips")]
	public bool HiddenInEncyclopedia;
}
