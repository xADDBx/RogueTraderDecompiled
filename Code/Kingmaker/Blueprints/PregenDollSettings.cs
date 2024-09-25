using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintPortrait))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("02852e8357c14ddda3acc4f0a1f6f651")]
public class PregenDollSettings : BlueprintComponent
{
	[Serializable]
	public class Entry
	{
		[SerializeField]
		[ValidateNotNull]
		private BlueprintRaceVisualPresetReference m_RacePreset;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Head;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Scar;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Warpaint;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo2;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo3;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo4;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Tattoo5;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Port;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Port2;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Eyebrows;

		[SerializeField]
		[ValidateNotNull]
		public EquipmentEntityLink Hair;

		[SerializeField]
		public EquipmentEntityLink Beard;

		[SerializeField]
		public EquipmentEntityLink Horn;

		[SerializeField]
		public EquipmentEntityLink NavigatorMutation;

		public int HairRampIndex = -1;

		public int WarpaintRampIndex = -1;

		public int SkinRampIndex = -1;

		public int TattooRampIndex = -1;

		public int HornsRampIndex = -1;

		public int EyebrowsColorRampIndex = -1;

		public int BeardColorRampIndex = -1;

		public int EquipmentRampIndex = -1;

		public int EquipmentRampIndexSecondary = -1;

		public int EyesColorRampIndex = -1;

		public BlueprintRaceVisualPreset RacePreset => m_RacePreset;
	}

	public Entry Default;
}
