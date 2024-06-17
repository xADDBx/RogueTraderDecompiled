using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public interface INavigationVectorDirectionHandler : IConsoleEntity
{
	bool HandleVector(Vector2 vector);
}
