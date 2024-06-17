using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Stories;

public class CharInfoStoriesVM : CharInfoComponentVM
{
	public readonly AutoDisposingList<CharInfoCompanionStoryVM> Stories = new AutoDisposingList<CharInfoCompanionStoryVM>();

	public CharInfoStoriesVM(IReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		Stories.Clear();
		foreach (BlueprintCompanionStory item in Game.Instance.Player.CompanionStories.Get(Unit.Value).ToList())
		{
			CharInfoCompanionStoryVM charInfoCompanionStoryVM = new CharInfoCompanionStoryVM(item);
			AddDisposable(charInfoCompanionStoryVM);
			Stories.Add(charInfoCompanionStoryVM);
		}
	}
}
