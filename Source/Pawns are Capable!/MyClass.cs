using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace PawnsAreCapable {

	[StaticConstructorOnStartup]
	internal static class PawnsAreCapable_Initializer {
		public static List<WorkTypeDef> AnimalsJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> ArtisticJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> CaringJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> CleaningJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> CookingJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> CraftingJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> FirefightingJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> HaulingJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> IntellectualJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> ManualDumbJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> ManualSkilledJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> MiningJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> PlantWorkJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> SocialJobs = new List<WorkTypeDef>();
		public static List<WorkTypeDef> ViolentJobs = new List<WorkTypeDef>();
		static PawnsAreCapable_Initializer() {
			LongEventHandler.QueueLongEvent(new Action(PawnsAreCapable_Initializer.Setup), "LibraryStartup", false, null);
		}
		public static void Setup() {
			AnimalsJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Animals) != 0)
			  .ToList();
			ArtisticJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Artistic) != 0)
			  .ToList();
			CaringJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Caring) != 0)
			  .ToList();
			CleaningJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Cleaning) != 0)
			  .ToList();
			CookingJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Cooking) != 0)
			  .ToList();
			CraftingJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Crafting) != 0)
			  .ToList();
			FirefightingJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Firefighting) != 0)
			  .ToList();
			HaulingJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Hauling) != 0)
			  .ToList();
			IntellectualJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Intellectual) != 0)
			  .ToList();
			ManualDumbJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.ManualDumb) != 0)
			  .ToList();
			ManualSkilledJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.ManualSkilled) != 0)
			  .ToList();
			MiningJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Mining) != 0)
			  .ToList();
			PlantWorkJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.PlantWork) != 0)
			  .ToList();
			SocialJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Social) != 0)
			  .ToList();
			ViolentJobs = DefDatabase<WorkTypeDef>.AllDefsListForReading
			  .Where(wtd => (wtd.workTags & WorkTags.Violent) != 0)
			  .ToList();
			foreach(KeyValuePair<string, Backstory> kvp in BackstoryDatabase.allBackstories) {
				if (kvp.Value.forcedTraits.NullOrEmpty()) {
					kvp.Value.forcedTraits = new List<TraitEntry>();
				}
				if ((WorkTags.ManualDumb & kvp.Value.workDisables) != WorkTags.None) {
					kvp.Value.forcedTraits.Add(new TraitEntry(TraitDef.Named("HatesDumbLabor"), 0));
				}
				else if ((WorkTags.Cleaning & kvp.Value.workDisables) != WorkTags.None) {
					kvp.Value.forcedTraits.Add(new TraitEntry(TraitDef.Named("HatesDumbLabor"), 0));	
				}
				else if ((WorkTags.Hauling & kvp.Value.workDisables) != WorkTags.None) {
					kvp.Value.forcedTraits.Add(new TraitEntry(TraitDef.Named("HatesDumbLabor"), 0));	
				}
			}
			foreach (WorkTypeDef workType in ViolentJobs) {
				if (workType.defName == "FinishingOff") { 
					ViolentJobs.Remove(workType); 
					break;
				}
			}
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("Force Do Job"))) {
				Log.Error("You should remove \"Force Do Job\" from your mod list, as \"Pawns are Capable!\" incorporates its functionality, and if both mods are running, you may encounter odd behavior.");
			}
		}
	}
 
	public class Controller: Mod {
		public static Settings Settings;
		public override string SettingsCategory() { return "PAC.PawnsAreCapable".Translate(); }
		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		public Controller(ModContentPack content) : base(content) {
			var harmony = HarmonyInstance.Create( "rainbeau.pawnsAreCapable" );
			harmony.PatchAll( Assembly.GetExecutingAssembly() );
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public bool allowFDJ = true;
		public void DoWindowContents(Rect canvas) {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap();
			list.CheckboxLabeled( "PAC.allowFDJ".Translate(), ref allowFDJ, "PAC.allowFDJTip".Translate() );
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref allowFDJ, "allowFDJ", true);
		}
	}
	
	// ------------------------------------- //
	// ---------- HARMONY PATCHES ---------- //
	// ------------------------------------- //

	[HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard", new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) })]
	public static class CharacterCardUtility_DrawCharacterCard {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++) {
				var strOperand = codes[i].operand as String;
				if (strOperand == "IncapableOf") {
					codes[i].operand = "PAC.IncapableOf";
				}
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders", null)]
	public static class FloatMenuMakerMap_AddHumanlikeOrders {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var codes = new List<CodeInstruction>(instructions);
			for (int i=875; i < 901; i++) {
				codes[i].opcode = OpCodes.Nop;
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders", null)]
	public static class FloatMenuMakerMap_AddUndraftedOrders {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++) {
				var strOperand = codes[i].operand as String;
				if (strOperand == "CannotPrioritizeWorkGiverDisabled") {
					codes[i].operand = "PAC.CannotPrioritizeWorkGiverDisabled";
				}
				if (strOperand == "CannotPrioritizeNotAssignedToWorkType") {
					codes[i].operand = "PAC.CannotPrioritizeNotAssignedToWorkType";
				}
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor", null)]
	public static class FloatMenuMakerMap_ChoicesAtFor {
		[HarmonyPriority(Priority.VeryLow)]
		static void Prefix(Pawn pawn, ref Dictionary<WorkTypeDef, int> __state) {
			__state = new Dictionary<WorkTypeDef, int>();
			if (pawn.workSettings != null && Controller.Settings.allowFDJ.Equals(true)) {
				foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefsListForReading) {
					if (!pawn.story.DisabledWorkTypes.Contains(def) && (pawn.workSettings.GetPriority(def) == 0)) {
						__state.Add(def, pawn.workSettings.GetPriority(def));
						pawn.workSettings.SetPriority(def, 3);
					}
				}
			}
		}
		[HarmonyPriority(Priority.VeryHigh)]
		static void Postfix(Pawn pawn, ref Dictionary<WorkTypeDef, int> __state) {
			if (pawn.workSettings != null && Controller.Settings.allowFDJ.Equals(true)) {
				foreach (KeyValuePair<WorkTypeDef, int> kv in __state) {
					pawn.workSettings.SetPriority(kv.Key, kv.Value);
				}
				__state.Clear();
			}
		}
	}

	[HarmonyPatch(typeof(FloatMenuUtility), "GetMeleeAttackAction", null)]
	public static class FloatMenuUtility_GetMeleeAttackAction {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++) {
				var strOperand = codes[i].operand as String;
				if (strOperand == "IsIncapableOfViolenceLower") {
					codes[i].operand = "PAC.IsIncapableOfViolenceLower";
				}
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(GameInitData), "PrepForMapGen", null)]
	public static class GameInitData_PrepForMapGen {
		private static void Postfix(ref GameInitData __instance) {
			var GID = Traverse.Create(__instance);
			List<Pawn> pawns = GID.Field("startingAndOptionalPawns").GetValue<List<Pawn>>();
			foreach (Pawn startingPawn in pawns) {
				ResetWork.ResetHatedWorkTypes(startingPawn);
			}
		}
	}
	
	[HarmonyPatch(typeof(PawnGenerator), "GenerateSkills", null)]
	public static class PawnGenerator_GenerateSkills {
		private static void Postfix(Pawn pawn) {
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Animals) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Animals).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Artistic) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Artistic).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Caring) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Medicine).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Cooking) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Cooking).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Crafting) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Crafting).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Intellectual) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Intellectual).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Construction).passion = Passion.None;
				pawn.skills.GetSkill(SkillDefOf.Cooking).passion = Passion.None;
				pawn.skills.GetSkill(SkillDefOf.Crafting).passion = Passion.None;
				pawn.skills.GetSkill(SkillDefOf.Plants).passion = Passion.None;
				pawn.skills.GetSkill(SkillDefOf.Mining).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Mining) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Mining).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.PlantWork) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Plants).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Social) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Social).passion = Passion.None;
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Violent) != 0) {
				pawn.skills.GetSkill(SkillDefOf.Melee).passion = Passion.None;
				pawn.skills.GetSkill(SkillDefOf.Shooting).passion = Passion.None;
			}
		}
	}
		
	[HarmonyPatch(typeof(Pawn_StoryTracker), "OneOfWorkTypesIsDisabled", null)]
	public static class Pawn_StoryTracker_OneOfWorkTypesIsDisabled {
		public static bool Prefix(List<WorkTypeDef> wts, ref Pawn_StoryTracker __instance, ref bool __result) {
			var PST = Traverse.Create(__instance);
			Pawn pawn = PST.Field("pawn").GetValue<Pawn>();
			for (int i = 0; i < wts.Count; i++) {
				if (pawn.story.DisabledWorkTypes.Contains(wts[i])) {
					__result = true;
					return false;
				}
			}
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(Pawn_StoryTracker), "WorkTagIsDisabled",  null)]
	public static class Pawn_StoryTracker_WorkTagIsDisabled {
		public static bool Prefix(WorkTags w, ref Pawn_StoryTracker __instance, ref bool __result) {
			var PST = Traverse.Create(__instance);
			Pawn pawn = PST.Field("pawn").GetValue<Pawn>();
			if (w == WorkTags.Violent) {
				if ((pawn.equipment.Primary != null) && WeaponCheck.HasWeapon(pawn)) {
					__result = false;
					return false;
				}
				return true;
			}
			__result = false;
			return false;
		}
	}
		
	[HarmonyPatch(typeof(Pawn_StoryTracker), "WorkTypeIsDisabled",  null)]
	public static class Pawn_StoryTracker_WorkTypeIsDisabled {
		public static bool Prefix(WorkTypeDef w, ref bool __result) {
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(Pawn_WorkSettings), "EnableAndInitialize", null)]
	public static class Pawn_WorkSettings_EnableAndInitialize {
		private static void Postfix(ref Pawn_WorkSettings __instance) {
			var PWS = Traverse.Create(__instance);
			Pawn pawn = PWS.Field("pawn").GetValue<Pawn>();
			ResetWork.ResetHatedWorkTypes(pawn);
		}
	}
	
	[HarmonyPatch(typeof(SkillRecord), "CalculateTotallyDisabled", null)]
	public static class SkillRecord_CalculateTotallyDisabled {
		public static bool Prefix(ref bool __result) {
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(WidgetsWork), "DrawWorkBoxBackground", null)]
	public static class WidgetsWork_DrawWorkBoxBackground {
		private static bool Prefix(Rect rect, Pawn p, WorkTypeDef workDef) {
			Texture2D workBoxBGTexAwful;
			Texture2D workBoxBGTexBad;
			float single;
			float single1 = p.skills.AverageOfRelevantSkillsFor(workDef);
			if (single1 < 4f) {
				workBoxBGTexAwful = WidgetsWork.WorkBoxBGTex_Awful;
				workBoxBGTexBad = WidgetsWork.WorkBoxBGTex_Bad;
				single = single1 / 4f;
			}
			else if (single1 > 14f) {
				workBoxBGTexAwful = WidgetsWork.WorkBoxBGTex_Mid;
				workBoxBGTexBad = WidgetsWork.WorkBoxBGTex_Excellent;
				single = (single1 - 14f) / 6f;
			}
			else {
				workBoxBGTexAwful = WidgetsWork.WorkBoxBGTex_Bad;
				workBoxBGTexBad = WidgetsWork.WorkBoxBGTex_Mid;
				single = (single1 - 4f) / 10f;
			}
			bool badWork = false;
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Animals) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.AnimalsJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Artistic) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ArtisticJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Caring) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CaringJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Cleaning) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CleaningJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Cooking) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Crafting) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Firefighting) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.FirefightingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Hauling) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.HaulingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Intellectual) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.IntellectualJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.ManualDumb) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ManualDumbJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CleaningJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.HaulingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ManualSkilledJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Mining) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.PlantWork) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.PlantWorkJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Social) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.SocialJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Violent) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ViolentJobs) {
					if (workDef.Equals(workType)) { badWork = true; }
				}
			}
			if (badWork.Equals(true)) {
				workBoxBGTexAwful = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Despised", true);
				workBoxBGTexBad = ContentFinder<Texture2D>.Get("UI/Widgets/WorkBoxBG_Despised", true);
			}
			GUI.DrawTexture(rect, workBoxBGTexAwful);
			float single2 = GUI.color.r;
			float single3 = GUI.color.g;
			Color color = GUI.color;
			GUI.color = new Color(single2, single3, color.b, single);
			GUI.DrawTexture(rect, workBoxBGTexBad);
			if (workDef.relevantSkills.Any<SkillDef>() && single1 <= 2f && p.workSettings.WorkIsActive(workDef)) {
				GUI.color = Color.white;
				GUI.DrawTexture(rect.ContractedBy(-2f), WidgetsWork.WorkBoxOverlay_Warning);
			}
			Passion passion = p.skills.MaxPassionOfRelevantSkillsFor(workDef);
			if (passion > Passion.None && badWork.Equals(false)) {
				GUI.color = new Color(1f, 1f, 1f, 0.4f);
				Rect rect1 = rect;
				rect1.xMin = rect.center.x;
				rect1.yMin = rect.center.y;
				if (passion == Passion.Minor) {
					GUI.DrawTexture(rect1, WidgetsWork.PassionWorkboxMinorIcon);
				}
				else if (passion == Passion.Major) {
					GUI.DrawTexture(rect1, WidgetsWork.PassionWorkboxMajorIcon);
				}
			}
			GUI.color = Color.white;
			return false;
		}
	}
	
	[HarmonyPatch(typeof(WidgetsWork), "TipForPawnWorker", new Type[] { typeof(Pawn), typeof(WorkTypeDef), typeof(bool) })]
	public static class WidgetsWork_TipForPawnWorker {
		public static bool Prefix(Pawn p, WorkTypeDef wDef, bool incapableBecauseOfCapacities, ref string __result) {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(wDef.gerundLabel.CapitalizeFirst());
			if (p.story.DisabledWorkTypes.Contains(wDef)) {
				stringBuilder.Append("PAC.CannotDoThisWork".Translate(p.LabelShort));
				__result = stringBuilder.ToString();
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(WITab_Caravan_Gear), "TryEquipDraggedItem", null)]
	public static class WITab_Caravan_Gear_TryEquipDraggedItem {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var codes = new List<CodeInstruction>(instructions);
			for (int i=16; i < 32; i++) {
				codes[i].opcode = OpCodes.Nop;
			}
			return codes.AsEnumerable();
		}
	}	
		
	public static class ResetWork {
		public static void ResetHatedWorkTypes(Pawn pawn) {
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Animals) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.AnimalsJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Artistic) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ArtisticJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Caring) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CaringJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Cleaning) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CleaningJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Cooking) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Crafting) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Firefighting) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.FirefightingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Hauling) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.HaulingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Intellectual) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.IntellectualJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.ManualDumb) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ManualDumbJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CleaningJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.HaulingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ManualSkilledJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Mining) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.PlantWork) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.PlantWorkJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Social) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.SocialJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
			if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Violent) != 0) {
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ViolentJobs) {
					pawn.workSettings.SetPriority(workType, 0);
				}
			}
		}
	}
	
	[HarmonyPatch]
	static class WorkTab_Pawn_Extensions_AllowedToDo {
		static MethodBase target;
		static bool Prepare() {
			var mod = LoadedModManager.RunningMods.FirstOrDefault(m => m.Name == "Work Tab");
			if (mod == null) {
				return false;
			}
			var type = mod.assemblies.loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "WorkTab").GetType("WorkTab.Pawn_Extensions");
			if (type == null) {
				Log.Warning("PAC can't patch WorkTab; no Pawn_Extensions found!");
				return false;
			}
			target = AccessTools.DeclaredMethod(type, "AllowedToDo");
			if (target == null) {
				Log.Warning("PAC can't patch WorkTab; no Pawn_Extensions.AllowedToDo found!");
				return false;
			}
			return true;
		}
		static MethodBase TargetMethod() {
			return target;
		}
		static void Postfix(ref bool __result) {
			__result = true;
		}
	}

	// -------------------------- //
	// ---------- DEFS ---------- //
	// -------------------------- //
	
	[DefOf]
	public static class WorkTypeDefOf {
		public static WorkTypeDef Art;
		public static WorkTypeDef Cleaning;
		public static WorkTypeDef Construction;
		public static WorkTypeDef Cooking;
		public static WorkTypeDef Crafting;
		public static WorkTypeDef Doctor;
		public static WorkTypeDef Firefighter;
		public static WorkTypeDef Growing;
		public static WorkTypeDef Handling;
		public static WorkTypeDef Hauling;
		public static WorkTypeDef Hunting;
		public static WorkTypeDef Mining;
		public static WorkTypeDef PlantCutting;
		public static WorkTypeDef Research;
		public static WorkTypeDef Smithing;
		public static WorkTypeDef Tailoring;
		public static WorkTypeDef Warden;
	}

	// ---------------------------------- //
	// ---------- WEAPON CHECK ---------- //
	// ---------------------------------- //
	
	public static class WeaponCheck {
		public static bool HasWeapon (Pawn pawn) {
			if (pawn.equipment.Primary.def.IsWeapon) {
				if (pawn.equipment.Primary.def.equippedStatOffsets != null) {
					for (int i = 0; i < pawn.equipment.Primary.def.equippedStatOffsets.Count; i++) {
						if (pawn.equipment.Primary.def.equippedStatOffsets[i].stat.defName == "MeleeHitChance") {
							return true;
						}
					}
					return false;
				}
				return true;
			}
			else {
				return false;
			}
		}
	}
	
	// ----------------------------- //
	// ---------- HEDIFFS ---------- //
	// ----------------------------- //

	public class Hediff_AssignedToArt : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToCleaning : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToConstruction : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToCooking : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToCrafting : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToDoctor : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToFirefighter : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToGrowing : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToHandling : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToHauling : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToHunting : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToMining : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToPlantCutting : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToResearch : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToSmithing : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToTailoring : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedToWarden : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	public class Hediff_AssignedAWeapon : HediffWithComps {
		private int doneWhining = 0;
		public int DoneWhining {
			get { 
				if (doneWhining == 0) {
					int duration = ageTicks/2;
					if (duration < 2500) { duration = 2500; }
					if (duration > 60000) { duration = 60000; }
					doneWhining = ageTicks + duration;
				}
				return doneWhining;
			}
		}
		public override void ExposeData () {
			base.ExposeData ();
			Scribe_Values.Look<int> (ref doneWhining, "doneWhining");
		}
		public override bool Visible {
			get {
				if (base.Severity < 0.05) { return false; }
				return true;
			}
		}
	}

	// ---------------------------------- //
	// ---------- HEDIFFGIVERS ---------- //
	// ---------------------------------- //
	
	public class HediffGiver_AssignedToArt : HediffGiver {
		public HediffGiver_AssignedToArt() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Artistic) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ArtisticJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToArt"))) {
							Hediff_AssignedToArt firstHediffOfDef = (Hediff_AssignedToArt)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToArt"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToCleaning : HediffGiver {
		public HediffGiver_AssignedToCleaning() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Cleaning) != 0 || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualDumb) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CleaningJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCleaning"))) {
							Hediff_AssignedToCleaning firstHediffOfDef = (Hediff_AssignedToCleaning)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCleaning"), false);
							firstHediffOfDef.Severity = 0.01f;
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToConstruction : HediffGiver {
		public HediffGiver_AssignedToConstruction() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Construction) && (pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Construction) || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0)) {
					if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToConstruction"))) {
						Hediff_AssignedToConstruction firstHediffOfDef = (Hediff_AssignedToConstruction)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToConstruction"), false);
						if (firstHediffOfDef.ageTicks > 120000) {
							firstHediffOfDef.Severity = 0.4f;
						}
						else if (firstHediffOfDef.ageTicks > 60000) {
							firstHediffOfDef.Severity = 0.3f;
						}
						else if (firstHediffOfDef.ageTicks > 30000) {
							firstHediffOfDef.Severity = 0.2f;
						}
						else if (firstHediffOfDef.ageTicks > 15000) {
							firstHediffOfDef.Severity = 0.1f;
						}
						else {
							firstHediffOfDef.Severity = 0.01f;
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToCooking : HediffGiver {
		public HediffGiver_AssignedToCooking() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Cooking) != 0 || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCooking"))) {
							Hediff_AssignedToCooking firstHediffOfDef = (Hediff_AssignedToCooking)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCooking"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToCrafting : HediffGiver {
		public HediffGiver_AssignedToCrafting() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Crafting) != 0 || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
						if (workType == WorkTypeDefOf.Smithing || workType == WorkTypeDefOf.Tailoring) { }
						else if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCrafting"))) {
							Hediff_AssignedToCrafting firstHediffOfDef = (Hediff_AssignedToCrafting)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCrafting"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToDoctor : HediffGiver {
		public HediffGiver_AssignedToDoctor() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Caring) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CaringJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToDoctor"))) {
							Hediff_AssignedToDoctor firstHediffOfDef = (Hediff_AssignedToDoctor)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToDoctor"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToFirefighter : HediffGiver {
		public HediffGiver_AssignedToFirefighter() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Firefighting) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.FirefightingJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToFirefighter"))) {
							Hediff_AssignedToFirefighter firstHediffOfDef = (Hediff_AssignedToFirefighter)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToFirefighter"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToGrowing : HediffGiver {
		public HediffGiver_AssignedToGrowing() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.PlantWork) != 0 || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.PlantWorkJobs) {
						if (workType == WorkTypeDefOf.PlantCutting) { }
						else if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToGrowing"))) {
							Hediff_AssignedToGrowing firstHediffOfDef = (Hediff_AssignedToGrowing)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToGrowing"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToHandling : HediffGiver {
		public HediffGiver_AssignedToHandling() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Animals) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.AnimalsJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHandling"))) {
							Hediff_AssignedToHandling firstHediffOfDef = (Hediff_AssignedToHandling)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHandling"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToHauling : HediffGiver {
		public HediffGiver_AssignedToHauling() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Hauling) != 0 || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualDumb) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.HaulingJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHauling"))) {
							Hediff_AssignedToHauling firstHediffOfDef = (Hediff_AssignedToHauling)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHauling"), false);
							firstHediffOfDef.Severity = 0.01f;
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToHunting : HediffGiver {
		public HediffGiver_AssignedToHunting() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Violent) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ViolentJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHunting"))) {
							Hediff_AssignedToHunting firstHediffOfDef = (Hediff_AssignedToHunting)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHunting"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToMining : HediffGiver {
		public HediffGiver_AssignedToMining() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Mining) != 0 || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToMining"))) {
							Hediff_AssignedToMining firstHediffOfDef = (Hediff_AssignedToMining)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToMining"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToPlantCutting : HediffGiver {
		public HediffGiver_AssignedToPlantCutting() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.PlantCutting) && (pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.PlantCutting))) {
					if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToPlantCutting"))) {
						Hediff_AssignedToPlantCutting firstHediffOfDef = (Hediff_AssignedToPlantCutting)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToPlantCutting"), false);
						firstHediffOfDef.Severity = 0.01f;
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToResearch : HediffGiver {
		public HediffGiver_AssignedToResearch() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Intellectual) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.IntellectualJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToResearch"))) {
							Hediff_AssignedToResearch firstHediffOfDef = (Hediff_AssignedToResearch)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToResearch"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToSmithing : HediffGiver {
		public HediffGiver_AssignedToSmithing() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Smithing) && (pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Smithing) || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0)) {
					if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToSmithing"))) {
						Hediff_AssignedToSmithing firstHediffOfDef = (Hediff_AssignedToSmithing)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToSmithing"), false);
						if (firstHediffOfDef.ageTicks > 120000) {
							firstHediffOfDef.Severity = 0.4f;
						}
						else if (firstHediffOfDef.ageTicks > 60000) {
							firstHediffOfDef.Severity = 0.3f;
						}
						else if (firstHediffOfDef.ageTicks > 30000) {
							firstHediffOfDef.Severity = 0.2f;
						}
						else if (firstHediffOfDef.ageTicks > 15000) {
							firstHediffOfDef.Severity = 0.1f;
						}
						else {
							firstHediffOfDef.Severity = 0.01f;
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToTailoring : HediffGiver {
		public HediffGiver_AssignedToTailoring() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if (pawn.workSettings.WorkIsActive(WorkTypeDefOf.Tailoring) && (pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Tailoring) || (pawn.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0)) {
					if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToTailoring"))) {
						Hediff_AssignedToTailoring firstHediffOfDef = (Hediff_AssignedToTailoring)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToTailoring"), false);
						if (firstHediffOfDef.ageTicks > 120000) {
							firstHediffOfDef.Severity = 0.4f;
						}
						else if (firstHediffOfDef.ageTicks > 60000) {
							firstHediffOfDef.Severity = 0.3f;
						}
						else if (firstHediffOfDef.ageTicks > 30000) {
							firstHediffOfDef.Severity = 0.2f;
						}
						else if (firstHediffOfDef.ageTicks > 15000) {
							firstHediffOfDef.Severity = 0.1f;
						}
						else {
							firstHediffOfDef.Severity = 0.01f;
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedToWarden : HediffGiver {
		public HediffGiver_AssignedToWarden() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if ((pawn.story.CombinedDisabledWorkTags & WorkTags.Social) != 0) {
					bool check = false;
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.SocialJobs) {
						if (pawn.workSettings.WorkIsActive(workType)) { check = true; }
					}
					if (check.Equals(true)) {
						if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedToWarden"))) {
							Hediff_AssignedToWarden firstHediffOfDef = (Hediff_AssignedToWarden)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToWarden"), false);
							if (firstHediffOfDef.ageTicks > 120000) {
								firstHediffOfDef.Severity = 0.4f;
							}
							else if (firstHediffOfDef.ageTicks > 60000) {
								firstHediffOfDef.Severity = 0.3f;
							}
							else if (firstHediffOfDef.ageTicks > 30000) {
								firstHediffOfDef.Severity = 0.2f;
							}
							else if (firstHediffOfDef.ageTicks > 15000) {
								firstHediffOfDef.Severity = 0.1f;
							}
							else {
								firstHediffOfDef.Severity = 0.01f;
							}
						}
					}
				}
			}
		}
	}
	
	public class HediffGiver_AssignedAWeapon : HediffGiver {
		public HediffGiver_AssignedAWeapon() { }
		public override void OnIntervalPassed(Pawn pawn, Hediff cause) {
			if (pawn.IsColonist) {
				if (((pawn.equipment.Primary != null) && WeaponCheck.HasWeapon(pawn)) && pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting)) {
					if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AssignedAWeapon"))) {
						Hediff_AssignedAWeapon firstHediffOfDef = (Hediff_AssignedAWeapon)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedAWeapon"), false);
						if (firstHediffOfDef.ageTicks > 120000) {
							firstHediffOfDef.Severity = 0.4f;
						}
						else if (firstHediffOfDef.ageTicks > 60000) {
							firstHediffOfDef.Severity = 0.3f;
						}
						else if (firstHediffOfDef.ageTicks > 30000) {
							firstHediffOfDef.Severity = 0.2f;
						}
						else if (firstHediffOfDef.ageTicks > 15000) {
							firstHediffOfDef.Severity = 0.1f;
						}
						else {
							firstHediffOfDef.Severity = 0.01f;
						}
					}
				}
			}
		}
	}

	// ------------------------------------- //
	// ---------- THOUGHT WORKERS ---------- //
	// ------------------------------------- //

	public class ThoughtWorker_AssignedToArt : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Artistic) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ArtisticJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToArt"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToArt"));
					}
					Hediff_AssignedToArt firstHediffOfDef = (Hediff_AssignedToArt)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToArt"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToArt"))) {
				Hediff_AssignedToArt firstHediffOfDef = (Hediff_AssignedToArt)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToArt"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}
	
	public class ThoughtWorker_AssignedToCleaning : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Cleaning) != 0 || (p.story.CombinedDisabledWorkTags & WorkTags.ManualDumb) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CleaningJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCleaning"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToCleaning"));
					}
					Hediff_AssignedToCleaning firstHediffOfDef = (Hediff_AssignedToCleaning)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCleaning"), false);
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCleaning"))) {
				Hediff_AssignedToCleaning firstHediffOfDef = (Hediff_AssignedToCleaning)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCleaning"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(1);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToConstruction : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (p.workSettings.WorkIsActive(WorkTypeDefOf.Construction) && (p.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Construction) || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0)) {
				if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToConstruction"))) {
					p.health.AddHediff(HediffDef.Named("AssignedToConstruction"));
				}
				Hediff_AssignedToConstruction firstHediffOfDef = (Hediff_AssignedToConstruction)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToConstruction"), false);
				if (firstHediffOfDef.ageTicks > 120000) {
					return ThoughtState.ActiveAtStage(4);
				}
				if (firstHediffOfDef.ageTicks > 60000) {
					return ThoughtState.ActiveAtStage(3);
				}
				if (firstHediffOfDef.ageTicks > 30000) {
					return ThoughtState.ActiveAtStage(2);
				}
				if (firstHediffOfDef.ageTicks > 15000) {
					return ThoughtState.ActiveAtStage(1);
				}
				return ThoughtState.ActiveAtStage(0);
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToConstruction"))) {
				Hediff_AssignedToConstruction firstHediffOfDef = (Hediff_AssignedToConstruction)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToConstruction"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToCooking : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Cooking) != 0 || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCooking"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToCooking"));
					}
					Hediff_AssignedToCooking firstHediffOfDef = (Hediff_AssignedToCooking)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCooking"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCooking"))) {
				Hediff_AssignedToCooking firstHediffOfDef = (Hediff_AssignedToCooking)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCooking"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToCrafting : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Crafting) != 0 || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
					if (workType == WorkTypeDefOf.Smithing || workType == WorkTypeDefOf.Tailoring) { }
					else if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCrafting"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToCrafting"));
					}
					Hediff_AssignedToCrafting firstHediffOfDef = (Hediff_AssignedToCrafting)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCrafting"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToCrafting"))) {
				Hediff_AssignedToCrafting firstHediffOfDef = (Hediff_AssignedToCrafting)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToCrafting"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToDoctor : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Caring) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CaringJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToDoctor"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToDoctor"));
					}
					Hediff_AssignedToDoctor firstHediffOfDef = (Hediff_AssignedToDoctor)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToDoctor"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToDoctor"))) {
				Hediff_AssignedToDoctor firstHediffOfDef = (Hediff_AssignedToDoctor)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToDoctor"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToFirefighter : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Firefighting) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.FirefightingJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToFirefighter"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToFirefighter"));
					}
					Hediff_AssignedToFirefighter firstHediffOfDef = (Hediff_AssignedToFirefighter)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToFirefighter"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToFirefighter"))) {
				Hediff_AssignedToFirefighter firstHediffOfDef = (Hediff_AssignedToFirefighter)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToFirefighter"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToGrowing : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.PlantWork) != 0 || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.PlantWorkJobs) {
					if (workType == WorkTypeDefOf.PlantCutting) { }
					else if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToGrowing"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToGrowing"));
					}
					Hediff_AssignedToGrowing firstHediffOfDef = (Hediff_AssignedToGrowing)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToGrowing"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToGrowing"))) {
				Hediff_AssignedToGrowing firstHediffOfDef = (Hediff_AssignedToGrowing)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToGrowing"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToHandling : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Animals) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.AnimalsJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHandling"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToHandling"));
					}
					Hediff_AssignedToHandling firstHediffOfDef = (Hediff_AssignedToHandling)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHandling"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHandling"))) {
				Hediff_AssignedToHandling firstHediffOfDef = (Hediff_AssignedToHandling)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHandling"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}
	
	public class ThoughtWorker_AssignedToHauling : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Hauling) != 0 || (p.story.CombinedDisabledWorkTags & WorkTags.ManualDumb) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.HaulingJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHauling"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToHauling"));
					}
					Hediff_AssignedToHauling firstHediffOfDef = (Hediff_AssignedToHauling)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHauling"), false);
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHauling"))) {
				Hediff_AssignedToHauling firstHediffOfDef = (Hediff_AssignedToHauling)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHauling"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(1);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToHunting : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Violent) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ViolentJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHunting"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToHunting"));
					}
					Hediff_AssignedToHunting firstHediffOfDef = (Hediff_AssignedToHunting)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHunting"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToHunting"))) {
				Hediff_AssignedToHunting firstHediffOfDef = (Hediff_AssignedToHunting)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToHunting"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToMining : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Mining) != 0 || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToMining"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToMining"));
					}
					Hediff_AssignedToMining firstHediffOfDef = (Hediff_AssignedToMining)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToMining"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToMining"))) {
				Hediff_AssignedToMining firstHediffOfDef = (Hediff_AssignedToMining)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToMining"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToPlantCutting : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (p.workSettings.WorkIsActive(WorkTypeDefOf.PlantCutting) && (p.story.DisabledWorkTypes.Contains(WorkTypeDefOf.PlantCutting))) {
				if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToPlantCutting"))) {
					p.health.AddHediff(HediffDef.Named("AssignedToPlantCutting"));
				}
				Hediff_AssignedToPlantCutting firstHediffOfDef = (Hediff_AssignedToPlantCutting)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToPlantCutting"), false);
				return ThoughtState.ActiveAtStage(0);
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToPlantCutting"))) {
				Hediff_AssignedToPlantCutting firstHediffOfDef = (Hediff_AssignedToPlantCutting)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToPlantCutting"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(1);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToResearch : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Intellectual) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.IntellectualJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToResearch"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToResearch"));
					}
					Hediff_AssignedToResearch firstHediffOfDef = (Hediff_AssignedToResearch)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToResearch"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToResearch"))) {
				Hediff_AssignedToResearch firstHediffOfDef = (Hediff_AssignedToResearch)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToResearch"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToSmithing : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (p.workSettings.WorkIsActive(WorkTypeDefOf.Smithing) && (p.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Smithing) || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0)) {
				if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToSmithing"))) {
					p.health.AddHediff(HediffDef.Named("AssignedToSmithing"));
				}
				Hediff_AssignedToSmithing firstHediffOfDef = (Hediff_AssignedToSmithing)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToSmithing"), false);
				if (firstHediffOfDef.ageTicks > 120000) {
					return ThoughtState.ActiveAtStage(4);
				}
				if (firstHediffOfDef.ageTicks > 60000) {
					return ThoughtState.ActiveAtStage(3);
				}
				if (firstHediffOfDef.ageTicks > 30000) {
					return ThoughtState.ActiveAtStage(2);
				}
				if (firstHediffOfDef.ageTicks > 15000) {
					return ThoughtState.ActiveAtStage(1);
				}
				return ThoughtState.ActiveAtStage(0);
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToSmithing"))) {
				Hediff_AssignedToSmithing firstHediffOfDef = (Hediff_AssignedToSmithing)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToSmithing"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToTailoring : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (p.workSettings.WorkIsActive(WorkTypeDefOf.Tailoring) && (p.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Tailoring) || (p.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0)) {
				if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToTailoring"))) {
					p.health.AddHediff(HediffDef.Named("AssignedToTailoring"));
				}
				Hediff_AssignedToTailoring firstHediffOfDef = (Hediff_AssignedToTailoring)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToTailoring"), false);
				if (firstHediffOfDef.ageTicks > 120000) {
					return ThoughtState.ActiveAtStage(4);
				}
				if (firstHediffOfDef.ageTicks > 60000) {
					return ThoughtState.ActiveAtStage(3);
				}
				if (firstHediffOfDef.ageTicks > 30000) {
					return ThoughtState.ActiveAtStage(2);
				}
				if (firstHediffOfDef.ageTicks > 15000) {
					return ThoughtState.ActiveAtStage(1);
				}
				return ThoughtState.ActiveAtStage(0);
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToTailoring"))) {
				Hediff_AssignedToTailoring firstHediffOfDef = (Hediff_AssignedToTailoring)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToTailoring"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedToWarden : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if ((p.story.CombinedDisabledWorkTags & WorkTags.Social) != 0) {
				bool check = false;
				foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.SocialJobs) {
					if (p.workSettings.WorkIsActive(workType)) { check = true; }
				}
				if (check.Equals(true)) {
					if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToWarden"))) {
						p.health.AddHediff(HediffDef.Named("AssignedToWarden"));
					}
					Hediff_AssignedToWarden firstHediffOfDef = (Hediff_AssignedToWarden)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToWarden"), false);
					if (firstHediffOfDef.ageTicks > 120000) {
						return ThoughtState.ActiveAtStage(4);
					}
					if (firstHediffOfDef.ageTicks > 60000) {
						return ThoughtState.ActiveAtStage(3);
					}
					if (firstHediffOfDef.ageTicks > 30000) {
						return ThoughtState.ActiveAtStage(2);
					}
					if (firstHediffOfDef.ageTicks > 15000) {
						return ThoughtState.ActiveAtStage(1);
					}
					return ThoughtState.ActiveAtStage(0);
				}
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedToWarden"))) {
				Hediff_AssignedToWarden firstHediffOfDef = (Hediff_AssignedToWarden)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedToWarden"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}

	public class ThoughtWorker_AssignedAWeapon : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (((p.equipment.Primary != null) && WeaponCheck.HasWeapon(p)) && p.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting)) {
				if (!p.health.hediffSet.HasHediff(HediffDef.Named("AssignedAWeapon"))) {
					p.health.AddHediff(HediffDef.Named("AssignedAWeapon"));
				}
				Hediff_AssignedAWeapon firstHediffOfDef = (Hediff_AssignedAWeapon)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedAWeapon"), false);
				if (firstHediffOfDef.ageTicks > 120000) {
					return ThoughtState.ActiveAtStage(4);
				}
				if (firstHediffOfDef.ageTicks > 60000) {
					return ThoughtState.ActiveAtStage(3);
				}
				if (firstHediffOfDef.ageTicks > 30000) {
					return ThoughtState.ActiveAtStage(2);
				}
				if (firstHediffOfDef.ageTicks > 15000) {
					return ThoughtState.ActiveAtStage(1);
				}
				return ThoughtState.ActiveAtStage(0);
			}
			if (p.health.hediffSet.HasHediff(HediffDef.Named("AssignedAWeapon"))) {
				Hediff_AssignedAWeapon firstHediffOfDef = (Hediff_AssignedAWeapon)p.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("AssignedAWeapon"), false);
				if (firstHediffOfDef.ageTicks < firstHediffOfDef.DoneWhining) {
					return ThoughtState.ActiveAtStage(5);
				}
				p.health.RemoveHediff(firstHediffOfDef);
			}
			return false;
		}
	}
	
	// ---------------------------- //	
	// ---------- ALERTS ---------- //
	// ---------------------------- //	

	public class Alert_BadWorkAssignment : Alert {
		public Alert_BadWorkAssignment() {
			this.defaultLabel = "PAC.BadWorkAssignment.Label".Translate();
			this.defaultExplanation = "PAC.BadWorkAssignment.Explanation".Translate();
			this.defaultPriority = AlertPriority.High;
		}
		public override AlertReport GetReport() {
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned) {
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Artistic) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ArtisticJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if (current.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Construction)
				  && current.workSettings.WorkIsActive(WorkTypeDefOf.Construction)) { 
					return current;
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Cooking) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Crafting) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Caring) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CaringJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Firefighting) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.FirefightingJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.PlantWork) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.PlantWorkJobs) {
						if (workType == WorkTypeDefOf.PlantCutting) { }
						else if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Animals) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.AnimalsJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Violent) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ViolentJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Mining) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Intellectual) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.IntellectualJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.Social) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.SocialJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
				if ((current.story.CombinedDisabledWorkTags & WorkTags.ManualSkilled) != 0) {
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.ManualSkilledJobs) {
						if (workType == WorkTypeDefOf.PlantCutting) { }
						else if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CookingJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.CraftingJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
					foreach (WorkTypeDef workType in PawnsAreCapable_Initializer.MiningJobs) {
						if (current.workSettings.WorkIsActive(workType)) { return current; }
					}
				}
			}
			return false;
		}
	}

	public class Alert_AssignedAWeapon : Alert {
		public Alert_AssignedAWeapon() {
			this.defaultLabel = "PAC.AssignedAWeapon.Label".Translate();
			this.defaultExplanation = "PAC.AssignedAWeapon.Explanation".Translate();
			this.defaultPriority = AlertPriority.High;
		}
		public override AlertReport GetReport() {
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned) {
				if (current.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting)
				  && ((current.equipment.Primary != null) && WeaponCheck.HasWeapon(current))) {
					return current;
				}
			}
			return false;
		}
	}

}
