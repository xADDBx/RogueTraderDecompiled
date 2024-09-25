using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public interface IFloatConsoleNavigationEntity : IConsoleNavigationEntity, IConsoleEntity
{
	Vector2 GetPosition();

	List<IFloatConsoleNavigationEntity> GetNeighbours();
}
