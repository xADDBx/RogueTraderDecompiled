using Kingmaker.Blueprints.Encyclopedia.Blocks;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockImageVM : EncyclopediaPageBlockVM
{
	public Sprite Image => (m_Block as BlueprintEncyclopediaBlockImage)?.Image;

	public EncyclopediaPageBlockImageVM(BlueprintEncyclopediaBlockImage block)
		: base(block)
	{
	}
}
