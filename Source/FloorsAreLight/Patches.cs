using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FloorsAreLight;

[StaticConstructorOnStartup]
public static class Patches
{
	static Patches()
	{
		Harmony harmony = new Harmony("lurmey.floorsarelight");
		harmony.Patch(typeof(GenConstruct).GetMethod("CanBuildOnTerrain"), null, new HarmonyMethod(typeof(Patches).GetMethod("CanBuildOnTerrain_Postfix")));
		harmony.Patch(typeof(GenConstruct).GetMethod("CanPlaceBlueprintAt"), null, new HarmonyMethod(typeof(Patches).GetMethod("CanPlaceBlueprintAt_Postfix")));
	}

	public static void CanBuildOnTerrain_Postfix(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, ref bool __result, Thing thingToIgnore = null, ThingDef stuffDef = null)
	{
		if (entDef is TerrainDef && !c.GetTerrain(map).changeable)
		{
			return;
		}
		TerrainAffordanceDef terrainAffordanceNeed = entDef.GetTerrainAffordanceNeed(stuffDef);
		if (terrainAffordanceNeed == null)
		{
			return;
		}
		CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
		cellRect.ClipInsideMap(map);
		using CellRect.Enumerator enumerator = cellRect.GetEnumerator();
		if (enumerator.MoveNext())
		{
			IntVec3 current = enumerator.Current;
			if (map.terrainGrid.TerrainAt(current).affordances.Contains(terrainAffordanceNeed) || (map.terrainGrid.UnderTerrainAt(current) != null && map.terrainGrid.UnderTerrainAt(current).affordances.Contains(terrainAffordanceNeed)))
			{
				__result = true;
			}
		}
	}

	public static void CanPlaceBlueprintAt_Postfix(ref AcceptanceReport __result, BuildableDef entDef, IntVec3 center, Map map)
	{
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(center);
		if (entDef is TerrainDef && !entDef.HasModExtension<FloorsAreLightModExt>() && !terrainDef.HasModExtension<FloorsAreLightModExt>())
		{
			TerrainAffordanceDef terrainAffordanceNeeded = terrainDef.terrainAffordanceNeeded;
			TerrainAffordanceDef terrainAffordanceDef = DefDatabase<TerrainAffordanceDef>.AllDefs.FirstOrDefault((TerrainAffordanceDef x) => x.defName == "BridgeableDeep");
			if (terrainAffordanceDef == null && terrainAffordanceNeeded == TerrainAffordanceDefOf.Bridgeable)
			{
				__result = new AcceptanceReport("NoFloorsOnBridges".Translate());
			}
			else if (terrainAffordanceDef != null && (terrainAffordanceNeeded == TerrainAffordanceDefOf.Bridgeable || terrainAffordanceNeeded == terrainAffordanceDef))
			{
				__result = new AcceptanceReport("NoFloorsOnBridges".Translate());
			}
		}
	}
}
