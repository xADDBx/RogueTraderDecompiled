using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;

public interface IRewiredCursorController
{
	bool Enabled { get; set; }

	GameObject Cursor { get; }
}
