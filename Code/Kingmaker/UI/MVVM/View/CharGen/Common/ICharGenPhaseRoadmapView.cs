using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common;

public interface ICharGenPhaseRoadmapView : IInitializable
{
	RectTransform ViewRectTransform { get; }

	void SetParentTransform(Transform parent, int siblingIndex = 0);

	CharGenPhaseBaseVM GetPhaseBaseVM();
}
