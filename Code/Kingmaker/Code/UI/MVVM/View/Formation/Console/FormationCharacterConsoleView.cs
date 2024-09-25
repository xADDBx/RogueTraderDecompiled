using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Formation.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Formation.Console;

public class FormationCharacterConsoleView : FormationCharacterBaseView, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity
{
	private Vector2 m_Position;

	public void MoveCharacter(Vector2 vec, bool moveFreely)
	{
		m_Position += vec;
		m_Position = m_FormationCharacterDragComponent.ClampPosition(m_Position);
		Vector2 position = m_Position;
		if (!moveFreely)
		{
			position.x -= position.x % 23f;
			position.y -= position.y % 23f;
		}
		base.transform.localPosition = position;
	}

	protected override void SetupPosition()
	{
		base.SetupPosition();
		m_Position = base.transform.localPosition;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}

	public Vector2 GetPosition()
	{
		return base.ViewModel.GetLocalPosition();
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
