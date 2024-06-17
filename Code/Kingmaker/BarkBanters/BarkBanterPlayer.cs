using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.BarkBanters;

public class BarkBanterPlayer
{
	private class Entry
	{
		internal readonly Entity Speaker;

		internal readonly LocalizedString Text;

		public Entry(Entity speaker, LocalizedString text)
		{
			Speaker = speaker;
			Text = text;
		}

		[CanBeNull]
		public IBarkHandle Play()
		{
			return BarkPlayer.Bark(Speaker, Text, UIUtility.GetBarkDuration(Text), playVoiceOver: true);
		}

		[CanBeNull]
		public IBarkHandle Play(string encyclopediaLink)
		{
			return BarkPlayer.Bark(Speaker, Text, encyclopediaLink, UIUtility.GetBarkDuration(Text), playVoiceOver: true);
		}
	}

	private const float InitialDelay = 1f;

	private const float ReplyDelay = 1f;

	private float m_Delay = 1f;

	[CanBeNull]
	private IBarkHandle m_CurrentBark;

	[NotNull]
	private readonly List<Entry> m_Entries = new List<Entry>();

	private int m_NextEntryIndex;

	private BlueprintAstropathBrief m_Brief;

	public bool Finished { get; private set; }

	public void AddEntry(Entity speaker, LocalizedString text)
	{
		m_Entries.Add(new Entry(speaker, text));
	}

	public void AddBrief(BlueprintAstropathBrief brief)
	{
		m_Brief = brief;
		Game.Instance.Player.StarSystemsState.AstropathBriefs.Add(new AstropathBriefInfo
		{
			AstropathBrief = brief,
			MessageLocation = Game.Instance.CurrentlyLoadedArea.Name,
			MessageDate = string.Empty
		});
	}

	public void Tick()
	{
		if (m_CurrentBark != null && m_CurrentBark.IsPlayingBark())
		{
			return;
		}
		if (m_Delay > 0f)
		{
			float deltaTime = Game.Instance.TimeController.DeltaTime;
			m_Delay -= deltaTime;
			return;
		}
		Entry entry = m_Entries.Get(m_NextEntryIndex);
		if (entry != null)
		{
			m_CurrentBark = ((m_Brief == null) ? entry.Play() : entry.Play(m_Brief.EncyclopediaLink));
			m_NextEntryIndex++;
			m_Delay = 1f;
		}
		else
		{
			m_CurrentBark = null;
			Finished = true;
		}
	}

	public void InterruptBark()
	{
		m_CurrentBark?.InterruptBark();
		m_CurrentBark = null;
	}
}
