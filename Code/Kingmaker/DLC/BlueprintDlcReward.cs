using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Code.GameCore.Mics;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("1c03a731ff4c4a31be5e33517f35eed6")]
public class BlueprintDlcReward : BlueprintScriptableObject, IBlueprintDlcReward
{
	[Serializable]
	public struct AssetPath
	{
		[SerializeField]
		private string m_Path;

		private Regex Regex;

		public string Path => m_Path;

		public bool Check(string path)
		{
			Regex = Regex ?? new Regex("^" + Path.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")
				.Replace("[", "\\[")
				.Replace("]", "\\]")
				.Replace("{", "\\{")
				.Replace("}", "\\}")
				.Replace("|", "\\|")
				.Replace("+", "\\+")
				.Replace("^", "\\^")
				.Replace("$", "\\$")
				.Replace(".", "\\.")
				.Replace("*", ".+")
				.Replace("?", ".") + "$");
			return Regex.IsMatch(path);
		}
	}

	public LocalizedString Description;

	[SerializeField]
	private AssetPath[] m_IncludeAssetPaths;

	[SerializeField]
	private UnityEngine.Object[] m_IncludeObjects;

	[Tooltip("After the reward is used the further saves won't load if no DLC containing this reward is presented.")]
	public bool IsRequiredInSaves;

	private List<IBlueprintDlc> m_Dlcs;

	public IEnumerable<AssetPath> IncludeAssetPaths => m_IncludeAssetPaths;

	public IEnumerable<UnityEngine.Object> IncludeObjects => m_IncludeObjects;

	public List<IBlueprintDlc> Dlcs => m_Dlcs ?? (m_Dlcs = InterfaceServiceLocator.GetService<IDlcRootService>().Dlcs?.Where((IBlueprintDlc _dlc) => _dlc.Rewards.Contains(this)).ToList());

	public bool IsAvailable => GetIsAvailable();

	private bool GetIsAvailable()
	{
		return Dlcs.Any((IBlueprintDlc _dlc) => _dlc.IsAvailable);
	}

	public virtual void RecheckAvailability()
	{
	}
}
