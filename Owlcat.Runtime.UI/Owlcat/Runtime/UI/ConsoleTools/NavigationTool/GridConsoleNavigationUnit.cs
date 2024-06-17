using System.Collections.Generic;
using UniRx;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public class GridConsoleNavigationUnit : IConsoleNavigationEntity, IConsoleEntity
{
	private ReactiveProperty<bool> IsSelected;

	public GridConsoleNavigationBehaviour Behaviour;

	public IConsoleEntity DeepestSelectedEntity
	{
		get
		{
			if (Behaviour.CurrentEntity is GridConsoleNavigationUnit gridConsoleNavigationUnit)
			{
				return gridConsoleNavigationUnit.DeepestSelectedEntity;
			}
			return Behaviour.CurrentEntity;
		}
	}

	public GridConsoleNavigationUnit(List<List<IConsoleEntity>> entities, IConsoleNavigationOwner owner, ReactiveProperty<bool> isSelected = null)
	{
		IsSelected = isSelected;
		Behaviour = new GridConsoleNavigationBehaviour(entities, owner);
	}

	public bool IsValid()
	{
		return Behaviour.IsValid();
	}

	public virtual void SetFocus(bool value)
	{
		Behaviour.SetFocus(value);
		if (IsSelected != null)
		{
			IsSelected.Value = value;
		}
	}
}
