using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics;

public class MechanicGroupView<T> : EntityViewBase where T : MechanicEntityView
{
	[HashNoGenerate]
	public class MechanicGroupData : SimpleEntity
	{
		[CanBeNull]
		public new MechanicGroupView<T> View => (MechanicGroupView<T>)base.View;

		public T[] ChildrenViews
		{
			get
			{
				if (!View.ComponentsParent)
				{
					return Array.Empty<T>();
				}
				return View.ComponentsParent.GetComponentsInChildren<T>(includeInactive: true);
			}
		}

		public MechanicGroupData(EntityViewBase view)
			: base(view)
		{
		}

		protected MechanicGroupData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected override void OnIsInGameChanged()
		{
			base.OnIsInGameChanged();
			if (base.IsInGame)
			{
				TryCreateViews();
			}
			SetViewInGame(base.IsInGame);
			if (base.IsInGame)
			{
				View.OnActivate();
			}
			else
			{
				View.OnDeactivate();
			}
		}

		private void SetViewInGame(bool flag)
		{
			if (View.ComponentsParent != null)
			{
				View.ComponentsParent.SetActive(flag);
			}
			T[] childrenViews = ChildrenViews;
			foreach (T val in childrenViews)
			{
				if (!(val == null) && val.Data != null)
				{
					val.Data.IsInGame = flag;
					val.SetVisible(flag);
				}
			}
		}

		private void TryCreateViews()
		{
			T[] childrenViews = ChildrenViews;
			foreach (T val in childrenViews)
			{
				if (!(val == null) && val.Data == null)
				{
					try
					{
						Game.Instance.EntitySpawner.SpawnEntityWithView(val, Game.Instance.LoadedAreaState.MainState, moveView: false);
					}
					catch (Exception ex)
					{
						PFLog.SceneLoader.Error($"Exception when creating data for {val} in {val.gameObject.scene.name}, {ex.Message}");
					}
				}
			}
		}

		protected override void OnViewDidAttach()
		{
			base.OnViewDidAttach();
			SetViewInGame(base.IsInGame);
		}

		protected override void OnViewWillDetach()
		{
			base.OnViewWillDetach();
			SetViewInGame(flag: false);
		}
	}

	public GameObject ComponentsParent;

	public override bool CreatesDataOnLoad => true;

	public new MechanicGroupData Data => (MechanicGroupData)base.Data;

	public virtual void Activate(bool flag)
	{
		Data.IsInGame = flag;
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MechanicGroupData(this));
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnDeactivate()
	{
	}
}
