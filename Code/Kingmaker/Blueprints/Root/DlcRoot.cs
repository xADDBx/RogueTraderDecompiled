using System;
using System.Collections.Generic;
using Code.GameCore.Mics;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DlcRoot : IDlcRootService, IDlcRoot, InterfaceService
{
	[SerializeField]
	private BlueprintDlcReference[] m_Dlcs;

	public IEnumerable<IBlueprintDlc> Dlcs => m_Dlcs?.Dereference();
}
