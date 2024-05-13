using EnhancedBeliefs;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EnhancedBeliefsInteractions
{
    [HarmonyPatch(typeof(GameComponent_EnhancedBeliefs), nameof(GameComponent_EnhancedBeliefs.GameComponentTick))]
    public static class GameComp_Tick
    {
        public static void Postfix(GameComponent_EnhancedBeliefs __instance)
        {
            ResearchManager_ResearchPerformed.lastResearchTicks[(Find.TickManager.TicksGame + 1) % 31] = 0f;

            // Every 30 seconds
            if (Find.TickManager.TicksGame % 1800 != 0)
            {
                return;
            }

            Ideo primary = Faction.OfPlayer.ideos.PrimaryIdeo;

            if (primary != null && primary.HasMeme(EBIDefOf.EBI_Zealots))
            {
                List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];

                    if (pawn.Ideo != primary && pawn.guilt != null && pawn.guilt.TicksUntilInnocent < 1801)
                    {
                        pawn.guilt.Notify_Guilty(1801);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.SocialFightChance))]
    public static class PawnInteractions_FightChance
    {
        public static void Postfix(Pawn_InteractionsTracker __instance, InteractionDef interaction, Pawn initiator, ref float __result)
        {
            if (__instance.pawn.Ideo == null || initiator.Ideo == null || __instance.pawn.Ideo == initiator.Ideo || !__instance.pawn.Ideo.HasMeme(EBIDefOf.EBI_Zealots))
            {
                return;
            }

            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            IdeoTrackerData tracker = comp.pawnTrackerData[__instance.pawn];
            __result *= 1f + (1f - tracker.IdeoOpinion(initiator.Ideo)) * 3f;
        }
    }

    [HarmonyPatch(typeof(GameComponent_EnhancedBeliefs), nameof(GameComponent_EnhancedBeliefs.ConversionFactor))]
    public static class EBGameComp_ConversionFactor
    {
        private static readonly SimpleCurve FactorFromMood = new SimpleCurve
        {
            new CurvePoint(0f, 1.3f),
            new CurvePoint(0.5f, 1f),
            new CurvePoint(1f, 0.7f)
        };

        private static readonly SimpleCurve FactorFromOpinion = new SimpleCurve
        {
            new CurvePoint(-100f, 1.3f),
            new CurvePoint(0f, 1f),
            new CurvePoint(100f, 0.7f)
        };

        private static readonly SimpleCurve FactorFromTerror = new SimpleCurve
        {
            new CurvePoint(0f, 0.8f),
            new CurvePoint(30f, 1f),
            new CurvePoint(60f, 1.2f),
            new CurvePoint(100f, 1.5f)
        };

        public static void Postfix(GameComponent_EnhancedBeliefs __instance, Pawn initiator, Pawn recipient, ref float __result)
        {
            if (initiator.Ideo == null || !initiator.Ideo.HasMeme(EBIDefOf.EBI_Zealots) || !recipient.IsPrisonerOfColony || !recipient.IsSlaveOfColony)
            {
                return;
            }

            __result *= FactorFromMood.Evaluate((recipient.needs.mood == null) ? 1f : recipient.needs.mood.CurInstantLevelPercentage) * FactorFromOpinion.Evaluate(recipient.relations?.OpinionOf(initiator) ?? 0) *
                FactorFromTerror.Evaluate(recipient.GetStatValue(StatDefOf.Terror)) * (0.8f + ((recipient.needs.TryGetNeed<Need_Suppression>() == null) ? 0.4f : recipient.needs.TryGetNeed<Need_Suppression>().CurInstantLevelPercentage) / 2f);
        }
    }

    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResearchPerformed))]
    public static class ResearchManager_ResearchPerformed
    {
        public static List<float> lastResearchTicks = new List<float>();
        public static bool recachingArray = true;

        static ResearchManager_ResearchPerformed()
        {
            for (int i = 0; i < 31; i++)
            {
                lastResearchTicks.Add(0f);
            }
        }

        public static void Postfix(ResearchManager __instance, float amount, Pawn researcher)
        {
            lastResearchTicks[Find.TickManager.TicksGame % 31] += amount;
        }

        public static float AverageResearch()
        {
            return lastResearchTicks.Sum() / 30f;
        }
    }

    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
    public static class ResearchManager_FinishProject
    {
        public static void Postfix(ResearchManager __instance, ResearchProjectDef proj)
        {
            List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];

                if (pawn.Ideo == null)
                {
                    continue;
                }

                for (int j = 0; j < pawn.Ideo.precepts.Count; j++)
                {
                    Precept precept = pawn.Ideo.precepts[j];
                    List<PreceptComp_SituationalThought_ResearchCompleted> comps = precept.TryGetComps<PreceptComp_SituationalThought_ResearchCompleted>();

                    for (int k = 0; k < comps.Count; k++)
                    {
                        comps[k].ResearchCompleted(pawn, precept, proj.baseCost);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(IdeoGenerator), nameof(IdeoGenerator.MakeIdeo))]
    public static class IdeoGenerator_MakeIdeo
    {
        public static void Postfix(Ideo __result)
        {
            EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();

            if (settings.fluidMode)
            {
                __result.Fluid = true;
            }
        }
    }
}
