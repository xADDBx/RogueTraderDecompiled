using Owlcat.Runtime.UI.ConsoleTools;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public interface IConsoleNavigationScroll
{
	bool CanFocusEntity(IConsoleEntity entity);

	void ScrollLeft();

	void ScrollRight();

	void ScrollUp();

	void ScrollDown();

	void ScrollInDirection(Vector2 direction);

	void ForceScrollToEntity(IConsoleEntity entity);
}
