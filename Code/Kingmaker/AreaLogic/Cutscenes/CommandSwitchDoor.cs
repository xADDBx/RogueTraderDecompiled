using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("22c096e9da5a20444aa206de7156cd6a")]
public class CommandSwitchDoor : CommandBase
{
	public class PlayerData : ContextData<PlayerData>
	{
		public CutscenePlayerData Player { get; private set; }

		public PlayerData Setup(CutscenePlayerData player)
		{
			Player = player;
			return this;
		}

		protected override void Reset()
		{
			Player = null;
		}
	}

	private bool m_Finished;

	private Vector3 m_UnitOffset = Vector3.zero;

	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	[SerializeReference]
	public MapObjectEvaluator Door;

	public bool UnlockIfLocked;

	public bool CloseIfAlreadyOpen;

	public bool OpenIfAlreadyClosed;

	public bool WaitUntilAnimationEnds;

	public bool SyncUnitRotation;

	[SerializeReference]
	[ShowIf("SyncUnitRotation")]
	public AbstractUnitEvaluator Unit;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		using (ContextData<PlayerData>.Request().Setup(player))
		{
			InteractionDoorPart optional = Door.GetValue().GetOptional<InteractionDoorPart>();
			if ((bool)optional)
			{
				if (UnlockIfLocked)
				{
					optional.AlreadyUnlocked = true;
				}
				if (optional.GetState())
				{
					if (CloseIfAlreadyOpen)
					{
						optional.Open();
					}
				}
				else if (OpenIfAlreadyClosed)
				{
					optional.Open();
				}
			}
			m_Finished = (skipping && !WaitUntilAnimationEnds) || !WaitUntilAnimationEnds;
			if (SyncUnitRotation && Unit?.GetValue() != null && Door?.GetValue().View.transform.GetChild(0) != null)
			{
				m_UnitOffset = Unit.GetValue().Position - Door.GetValue().View.transform.position;
				m_UnitOffset.y = 0f;
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (m_Finished)
		{
			return true;
		}
		return Door.GetValue().GetOptional<InteractionDoorPart>().IsAnimationFinished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (SyncUnitRotation && Unit?.GetValue() != null && Door?.GetValue().View.transform.GetChild(0) != null)
		{
			Transform transform = Door.GetValue().View.transform.GetChild(0).transform;
			Unit.GetValue().Position = transform.position + transform.rotation * m_UnitOffset;
			Unit.GetValue().DesiredOrientation = transform.rotation.eulerAngles.y;
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		m_Finished = !WaitUntilAnimationEnds;
	}

	public override string GetCaption()
	{
		return $"<b>Open door</b> {Door}";
	}
}
