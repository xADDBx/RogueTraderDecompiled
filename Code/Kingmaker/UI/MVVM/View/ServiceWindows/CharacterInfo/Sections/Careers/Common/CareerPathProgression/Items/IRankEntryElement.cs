using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public interface IRankEntryElement
{
	MonoBehaviour MonoBehaviour { get; }

	void SetRotation(float angle, bool hasArrow);

	void StartHighlight(string key);

	void StopHighlight();
}
