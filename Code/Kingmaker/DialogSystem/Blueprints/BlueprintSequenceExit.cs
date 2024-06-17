using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("6091ddca960a4d1459ac23e917c21d7d")]
public class BlueprintSequenceExit : BlueprintScriptableObject
{
	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	public CueSelection Continue;
}
