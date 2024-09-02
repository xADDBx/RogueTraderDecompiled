using System;
using System.Collections.Generic;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class UtilityRoot
{
	public List<BlueprintEtudeReference> SkipCheckingEtudeHierarchy = new List<BlueprintEtudeReference>();
}
