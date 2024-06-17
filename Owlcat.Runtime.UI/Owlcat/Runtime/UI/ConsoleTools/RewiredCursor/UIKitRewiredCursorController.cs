using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;

public static class UIKitRewiredCursorController
{
	private static IRewiredCursorController s_RewiredCursorController;

	public static bool HasController => s_RewiredCursorController != null;

	public static bool HasCursor => s_RewiredCursorController?.Cursor != null;

	public static bool Enabled
	{
		get
		{
			return s_RewiredCursorController?.Enabled ?? false;
		}
		set
		{
			if (s_RewiredCursorController != null)
			{
				s_RewiredCursorController.Enabled = value;
			}
		}
	}

	public static GameObject Cursor
	{
		get
		{
			if (!HasCursor)
			{
				return null;
			}
			return s_RewiredCursorController.Cursor;
		}
	}

	public static void SetRewiredCursorController(IRewiredCursorController rewiredCursorController)
	{
		s_RewiredCursorController = rewiredCursorController;
	}
}
