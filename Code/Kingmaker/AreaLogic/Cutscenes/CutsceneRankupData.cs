using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutsceneRankupData
{
	public class ContextData : ContextData<ContextData>, IDialogNotificationHandler, ISubscriber, IItemsCollectionHandler, IPartyGainExperienceHandler
	{
		public CutsceneRankupData Context { get; private set; }

		public ContextData Setup([NotNull] CutsceneRankupData context)
		{
			EventBus.Subscribe(this);
			Context = context;
			return this;
		}

		protected override void Reset()
		{
			Context = null;
			EventBus.Unsubscribe(this);
		}

		public void AddCustomNotification(string line)
		{
			Context.CustomNotifications.Add(line);
		}

		public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
		{
			Context.ItemsChanged.Add(item.Name, count);
		}

		public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
		{
			Context.ItemsChanged.Add(item.Name, -count);
		}

		public void HandlePartyGainExperience(int gained, bool isExperienceForDeath)
		{
			Context.XpGaineds.Add(gained);
		}
	}

	public List<string> RevealedLocationNames = new List<string>();

	public Dictionary<string, int> ItemsChanged = new Dictionary<string, int>();

	public List<int> XpGaineds = new List<int>();

	public List<string> CustomNotifications = new List<string>();

	public IDisposable RequestContextData()
	{
		return ContextData<ContextData>.Request().Setup(this);
	}
}
