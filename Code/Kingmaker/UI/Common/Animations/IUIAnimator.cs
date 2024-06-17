using JetBrains.Annotations;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public interface IUIAnimator
{
	void Initialize();

	void AppearAnimation([CanBeNull] UnityAction action = null);

	void DisappearAnimation([CanBeNull] UnityAction action = null);
}
