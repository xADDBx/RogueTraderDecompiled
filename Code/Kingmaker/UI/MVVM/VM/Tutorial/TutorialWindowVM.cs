using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Tutorial;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tutorial;

public abstract class TutorialWindowVM : VMBase
{
	public readonly TutorialData Data;

	private readonly Action m_CallbackHide;

	public readonly BoolReactiveProperty EncyclopediaLinkExist = new BoolReactiveProperty();

	public List<TutorialData.Page> Pages => Data?.Pages;

	public TutorialTag? TutorialTag => Data?.Blueprint.Tag;

	public bool BanTutorInsteadOfTag => Data?.Trigger != null;

	public bool CanBeBanned
	{
		get
		{
			if (Data?.Trigger == null)
			{
				return TutorialTag != Kingmaker.Tutorial.TutorialTag.NoTag;
			}
			return true;
		}
	}

	private INode EncyclopediaReference => Data.Blueprint.EncyclopediaReference.Get();

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	protected TutorialWindowVM(TutorialData data, Action callbackHide)
	{
		Data = data;
		m_CallbackHide = callbackHide;
		EncyclopediaLinkExist.Value = data.Blueprint.EncyclopediaReference.Get() != null;
	}

	protected override void DisposeImplementation()
	{
	}

	public void BanTutor()
	{
		if (BanTutorInsteadOfTag)
		{
			Game.Instance.Player.Tutorial.Ban(Data.Blueprint);
		}
		else
		{
			Game.Instance.Player.Tutorial.BanTag(Data.Blueprint.Tag);
		}
	}

	public void Hide()
	{
		m_CallbackHide?.Invoke();
		EventBus.RaiseEvent(delegate(ITutorialWindowClosedHandler i)
		{
			i.HandleHideTutorial(Game.Instance.Player.Tutorial.ShowingData);
		});
		Game.Instance.Player.Tutorial.ShowingData = null;
	}

	public void TemporarilyHide()
	{
		m_CallbackHide?.Invoke();
	}

	public void GoToEncyclopedia()
	{
		TemporarilyHide();
		UIUtility.EntityLinkActions.ShowEncyclopediaPage(EncyclopediaReference);
	}
}
