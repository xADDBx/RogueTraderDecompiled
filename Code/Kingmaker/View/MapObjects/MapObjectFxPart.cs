using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class MapObjectFxPart : ViewBasedPart<MapObjectFxSettings>, IAreaHandler, ISubscriber, IHashable
{
	private GameObject m_FxInstance;

	private bool m_IsLoaded;

	[JsonProperty]
	public bool FxActive { get; private set; }

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_IsLoaded = true;
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		if (!m_IsLoaded)
		{
			FxActive = base.Settings.StartActive;
		}
	}

	public void SetFxActive(bool active)
	{
		if (FxActive != active)
		{
			if ((bool)m_FxInstance)
			{
				FxHelper.Destroy(m_FxInstance);
				m_FxInstance = null;
			}
			if (active)
			{
				m_FxInstance = FxHelper.SpawnFxOnGameObject(base.Settings.FxPrefab, base.Settings.FxRoot ? base.Settings.FxRoot.gameObject : ((EntityViewBase)base.View).Or(null)?.gameObject);
			}
			FxActive = active;
		}
	}

	public void OnAreaBeginUnloading()
	{
		if ((bool)m_FxInstance)
		{
			FxHelper.Destroy(m_FxInstance);
			m_FxInstance = null;
		}
	}

	public void OnAreaDidLoad()
	{
		if (FxActive)
		{
			m_FxInstance = FxHelper.SpawnFxOnGameObject(base.Settings.FxPrefab, base.Settings.FxRoot ? base.Settings.FxRoot.gameObject : ((EntityViewBase)base.View).Or(null)?.gameObject);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = FxActive;
		result.Append(ref val2);
		return result;
	}
}
