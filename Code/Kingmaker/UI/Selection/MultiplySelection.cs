using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.Selection;

public class MultiplySelection : MonoBehaviour
{
	[SerializeField]
	private SelectFrameController m_SelectFrameController;

	private bool m_HasActiveBox;

	private bool m_StartDrag;

	private Vector3 m_Anchor;

	private Vector3 m_Outer;

	private const float SizeDelta = 32f;

	private readonly HashSet<UnitEntityView> m_UnitsInFrame = new HashSet<UnitEntityView>();

	public static MultiplySelection Instance { get; private set; }

	public bool ShouldMultiSelect => !Game.Instance.Player.Group.IsInCombat;

	public bool HasActiveBox => m_HasActiveBox;

	public void Initialize()
	{
		Instance = this;
		m_SelectFrameController.Initialize();
	}

	public void CreateBoxSelection(Vector2 point)
	{
		m_StartDrag = true;
		m_Anchor = point;
		m_Outer = point;
		m_SelectFrameController.StartSelectBox(m_Anchor);
	}

	public void DragBoxSelection()
	{
		if (!m_StartDrag)
		{
			return;
		}
		m_Outer = Input.mousePosition;
		if (!m_HasActiveBox)
		{
			Vector2 size = m_SelectFrameController.GetSize();
			if (size.x >= 32f || size.y >= 32f)
			{
				m_HasActiveBox = true;
				m_SelectFrameController.Show();
			}
		}
		m_SelectFrameController.DragSelectBox(m_Outer);
		if (m_HasActiveBox)
		{
			UpdateUnitsInside();
		}
	}

	private void UpdateUnitsInside()
	{
		CameraRig instance = CameraRig.Instance;
		Vector3 vector = instance.transform.right.To2D().normalized.To3D();
		Vector3 vector2 = instance.transform.forward.To2D().normalized.To3D();
		Rect viewportBounds = SelectUtil.GetViewportBounds(instance.Camera, m_Anchor, m_Outer);
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit.IsDirectlyControllable())
			{
				Vector3 position = allBaseAwakeUnit.Position;
				Vector2 cameraOrientedBoundsSize = allBaseAwakeUnit.View.CameraOrientedBoundsSize;
				position -= vector2 * cameraOrientedBoundsSize.x / 4f;
				Vector2 lhs = instance.WorldToViewport(position - vector * cameraOrientedBoundsSize.x / 2f);
				Vector2 rhs = instance.WorldToViewport(position + vector * cameraOrientedBoundsSize.x / 2f + Vector3.up * cameraOrientedBoundsSize.y);
				Vector2 vector3 = Vector2.Min(lhs, rhs);
				Vector2 vector4 = Vector2.Max(lhs, rhs);
				Rect other = new Rect(vector3, vector4 - vector3);
				bool flag = viewportBounds.Overlaps(other);
				if (m_UnitsInFrame.Contains(allBaseAwakeUnit.View) && !flag)
				{
					m_UnitsInFrame.Remove(allBaseAwakeUnit.View);
				}
				if (!m_UnitsInFrame.Contains(allBaseAwakeUnit.View) && flag)
				{
					m_UnitsInFrame.Add(allBaseAwakeUnit.View);
				}
			}
		}
	}

	public void SelectEntities()
	{
		if (m_UnitsInFrame.Count > 0)
		{
			(UIAccess.SelectionManager as SelectionManagerPC)?.MultiSelect(m_UnitsInFrame);
		}
		Cancel();
	}

	public void Cancel()
	{
		m_StartDrag = false;
		m_HasActiveBox = false;
		m_SelectFrameController.ClearBox();
		foreach (UnitEntityView item in m_UnitsInFrame)
		{
			_ = item;
		}
		m_UnitsInFrame.Clear();
	}
}
