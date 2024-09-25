using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPortraitAndName : ITooltipBrick
{
	private readonly Sprite m_Icon;

	private readonly string m_Line;

	private readonly TooltipBrickTitle m_BrickTitle;

	private readonly int m_Difficulty;

	private readonly bool m_IsUsedSubtypeIcon;

	private readonly bool m_IsEnemy;

	private readonly bool m_IsFriend;

	public TooltipBrickPortraitAndName(Sprite icon, string line, TooltipBrickTitle brickTitle = null, int difficulty = 0, bool isUsedSubtypeIcon = false, bool isEnemy = false, bool isFriend = false)
	{
		m_Icon = icon;
		m_Line = line;
		m_BrickTitle = brickTitle;
		m_Difficulty = difficulty;
		m_IsUsedSubtypeIcon = isUsedSubtypeIcon;
		m_IsEnemy = isEnemy;
		m_IsFriend = isFriend;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPortraitAndNameVM(m_Icon, m_Line, m_BrickTitle, m_Difficulty, m_IsUsedSubtypeIcon, m_IsEnemy, m_IsFriend);
	}
}
