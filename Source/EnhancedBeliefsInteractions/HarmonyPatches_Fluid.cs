using EnhancedBeliefs;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EnhancedBeliefsInteractions
{
    /*
    [HarmonyPatch(typeof(Dialog_ChooseMemes), nameof(Dialog_ChooseMemes.CanUseMeme))]
    public static class DialogMemes_CanUseMeme
    {
        public static int lastCacheTick = 0;
        public static Ideo lastCacheIdeo;
        public static List<MemeDef> validMemes = new List<MemeDef>();

        public static void Postfix(Dialog_ChooseMemes __instance, MemeDef meme, bool __result)
        {
            if (!__result || !__instance.ReformingFluidIdeo || __instance.ideo.HasMeme(meme))
            {
                return;
            }

            if (lastCacheIdeo != __instance.ideo || Find.TickManager.TicksGame - lastCacheTick > 250)
            {
                lastCacheIdeo = __instance.ideo;
                lastCacheTick = Find.TickManager.TicksGame;
                validMemes.Clear();

                // Filtering out all already invalid memes so players don't get left out with 0 choices

                for (int i = 0; i < DefDatabase<MemeDef>.AllDefsListForReading.Count; i++)
                {
                    MemeDef specificMeme = DefDatabase<MemeDef>.AllDefsListForReading[i];
                    if (specificMeme.hiddenInChooseMemes || __instance.ideo.HasMeme(specificMeme))
                    {
                        continue;
                    }

                    bool valid = true;

                    foreach (Faction allFaction in Find.FactionManager.AllFactions)
                    {
                        if (!allFaction.def.hidden && !allFaction.def.isPlayer && allFaction.ideos != null && (allFaction.ideos.IsPrimary(__instance.ideo) || allFaction.ideos.IsMinor(__instance.ideo)) && !IdeoUtility.IsMemeAllowedFor(specificMeme, allFaction.def))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        validMemes.Add(specificMeme);
                    }
                }
            }

            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
            Dictionary<MemeDef, float> memeOpinions = validMemes.ToDictionary((MemeDef x) => x, (MemeDef x) => 0f);
            List<MemeDef> validMemes = memeOpinions.Keys.ToList();

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];

                if (pawn.Ideo != __instance.ideo)
                {
                    continue;
                }

                IdeoTrackerData tracker = comp.pawnTrackerData[pawn];

                for (int j = 0; j < memeOpinions.Count; j++)
                {
                    memeOpinions[validMemes[j]] += tracker.TrueMemeOpinion(validMemes[j]);
                }
            }

            List<KeyValuePair<MemeDef, float>> memeOpinionsSorted = memeOpinions.ToList();
            memeOpinionsSorted.SortBy((KeyValuePair<MemeDef, float> pair) => pair.Value);
            List<MemeDef> memes = memeOpinionsSorted.Select(pair => pair.Key).ToList();
            EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();

            if (!memes.Contains(meme) || memes.IndexOf(meme) < memes.Count - 1 - settings.reformMemeChoices)
            {
                __result = false;
            }
        }
    }
    */

    [HarmonyPatch(typeof(Dialog_ChooseMemes), nameof(Dialog_ChooseMemes.DoNormalMemeSelector))]
    public static class DialogMemes_MemeSelector
    {
        public static int lastCacheTick = 0;
        public static Ideo lastCacheIdeo;
        public static List<MemeDef> selectableMemes = new List<MemeDef>();

        public static void Prefix(Dialog_ChooseMemes __instance, Rect viewRect, ref float curY, List<MemeDef> memes)
        {
            if (!__instance.ReformingFluidIdeo)
            {
                return;
            }

            if (lastCacheIdeo != __instance.ideo || Find.TickManager.TicksGame - lastCacheTick > 250)
            {
                lastCacheIdeo = __instance.ideo;
                lastCacheTick = Find.TickManager.TicksGame;
                List<MemeDef> validMemes = new List<MemeDef>();

                // Filtering out all already invalid memes so players don't get left out with 0 choices

                for (int i = 0; i < DefDatabase<MemeDef>.AllDefsListForReading.Count; i++)
                {
                    MemeDef specificMeme = DefDatabase<MemeDef>.AllDefsListForReading[i];
                    if (specificMeme.hiddenInChooseMemes || __instance.ideo.HasMeme(specificMeme) || specificMeme.category == MemeCategory.Structure)
                    {
                        continue;
                    }

                    bool valid = true;

                    foreach (Faction allFaction in Find.FactionManager.AllFactions)
                    {
                        if (!allFaction.def.hidden && !allFaction.def.isPlayer && allFaction.ideos != null && (allFaction.ideos.IsPrimary(__instance.ideo) || allFaction.ideos.IsMinor(__instance.ideo)) && !IdeoUtility.IsMemeAllowedFor(specificMeme, allFaction.def))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        validMemes.Add(specificMeme);
                    }
                }

                GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
                List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
                Dictionary<MemeDef, float> memeOpinions = validMemes.ToDictionary((MemeDef x) => x, (MemeDef x) => 0f);

                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];

                    if (pawn.Ideo != __instance.ideo)
                    {
                        continue;
                    }

                    IdeoTrackerData tracker = comp.pawnTrackerData[pawn];

                    for (int j = 0; j < memeOpinions.Count; j++)
                    {
                        memeOpinions[validMemes[j]] += tracker.TrueMemeOpinion(validMemes[j]);
                    }

                    List<Ideo> pawnIdeos = tracker.baseIdeoOpinions.Keys.ToList();

                    for (int j = 0; j < pawnIdeos.Count; j++)
                    {
                        Ideo ideo = pawnIdeos[j];

                        for (int n = 0; n < ideo.memes.Count; n++)
                        {
                            if (memeOpinions.ContainsKey(ideo.memes[n]))
                            {
                                memeOpinions[ideo.memes[n]] += (tracker.IdeoOpinion(ideo) - 0.2f) * 20f;
                            }
                        }
                    }
                }

                List<KeyValuePair<MemeDef, float>> memeOpinionsSorted = memeOpinions.ToList();
                memeOpinionsSorted.SortBy((KeyValuePair<MemeDef, float> pair) => pair.Value);
                EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();
                selectableMemes = memeOpinionsSorted.Select(pair => pair.Key).Reverse().Take(settings.reformMemeChoices).ToList();
            }

            for (int i = memes.Count - 1; i >= 0; i--)
            {
                MemeDef meme = memes[i];

                if (__instance.ideo.HasMeme(meme) || meme.category == MemeCategory.Structure)
                {
                    continue;
                }

                if (!selectableMemes.Contains(meme))
                {
                    memes.Remove(meme);
                }
            }
        }
    }

    [HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.CanListPrecept))]
    public static class IdeoUtility_CanListPrecept
    {
        public static int lastCacheTick = 0;
        public static Ideo lastCacheIdeo;
        public static List<PreceptDef> selectablePrecepts = new List<PreceptDef>();

        public static void Postfix(Ideo ideo, PreceptDef precept, IdeoEditMode editMode, ref AcceptanceReport __result)
        {
            if (!__result || editMode != IdeoEditMode.Reform || ideo.HasPrecept(precept))
            {
                return;
            }

            if (lastCacheIdeo != ideo || Find.TickManager.TicksGame - lastCacheTick > 250)
            {
                lastCacheIdeo = ideo;
                lastCacheTick = Find.TickManager.TicksGame;
                selectablePrecepts.Clear();
                List<PreceptDef> validPrecepts = new List<PreceptDef>();

                // Filtering out all already invalid memes so players don't get left out with 0 choices

                for (int i = 0; i < DefDatabase<PreceptDef>.AllDefsListForReading.Count; i++)
                {
                    PreceptDef specificPrecept = DefDatabase<PreceptDef>.AllDefsListForReading[i];
                    bool dontAdd = false;

                    for (int j = 0; j < ideo.memes.Count; j++)
                    {
                        MemeDef meme = ideo.memes[j];

                        // Always allow meme-specific precepts to give players some leeway
                        // Also allow all non-precept stuff that's under precepts for some reason

                        if (specificPrecept.requiredMemes.Contains(meme) || specificPrecept.preceptClass != typeof(Precept))
                        {
                            selectablePrecepts.Add(specificPrecept);
                            dontAdd = true;
                            break;
                        }

                        if (meme.requireOne != null && meme.requireOne.Any((List<PreceptDef> x) => x.Contains(specificPrecept)))
                        {
                            selectablePrecepts.Add(specificPrecept);
                            dontAdd = true;
                            break;
                        }

                        if (specificPrecept.conflictingMemes.Contains(meme))
                        {
                            dontAdd = true;
                            break;
                        }
                    }

                    if (!dontAdd && specificPrecept.visible)
                    {
                        validPrecepts.Add(specificPrecept);
                    }
                }

                GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
                List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
                Dictionary<PreceptDef, float> preceptOpinions = validPrecepts.ToDictionary((PreceptDef x) => x, (PreceptDef x) => 0f);

                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];

                    if (pawn.Ideo != ideo)
                    {
                        continue;
                    }

                    IdeoTrackerData tracker = comp.pawnTrackerData[pawn];

                    for (int j = 0; j < validPrecepts.Count; j++)
                    {
                        preceptOpinions[validPrecepts[j]] += tracker.TruePreceptOpinion(validPrecepts[j]);
                    }

                    List<Ideo> pawnIdeos = tracker.baseIdeoOpinions.Keys.ToList();

                    for (int j = 0; j < pawnIdeos.Count; j++)
                    {
                        Ideo ideo2 = pawnIdeos[j];

                        for (int n = 0; n < ideo2.precepts.Count; n++)
                        {
                            if (preceptOpinions.ContainsKey(ideo2.precepts[n].def))
                            {
                                preceptOpinions[ideo2.precepts[n].def] += (tracker.IdeoOpinion(ideo2) - 0.2f) * 20f;
                            }
                        }
                    }
                }

                Dictionary<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float>> issues = new Dictionary<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float>>();

                for (int i = 0; i < validPrecepts.Count; i++)
                {
                    PreceptDef keyPrecept = validPrecepts[i];

                    if (!issues.ContainsKey(keyPrecept.issue))
                    {
                        issues[keyPrecept.issue] = new Pair<List<KeyValuePair<PreceptDef, float>>, float>(new List<KeyValuePair<PreceptDef, float>>(), 0f);
                    }

                    issues[keyPrecept.issue].First.Add(new KeyValuePair<PreceptDef, float>(keyPrecept, preceptOpinions[keyPrecept]));
                }

                List<IssueDef> issueList = issues.Keys.ToList();

                for (int i = 0; i < issueList.Count; i++)
                {
                    IssueDef issue = issueList[i];

                    issues[issue].First.SortBy((KeyValuePair<PreceptDef, float> x) => -x.Value);

                    PreceptDef ideoPrecept = null;

                    for (int j = 0; j < ideo.precepts.Count; j++)
                    {
                        ideoPrecept = ideo.precepts[j].def;

                        if (ideoPrecept.issue == issue)
                        {
                            break;
                        }
                    }

                    float deductOpinion = 0f;

                    if (ideoPrecept != null && preceptOpinions.ContainsKey(ideoPrecept))
                    {
                        deductOpinion = preceptOpinions[ideoPrecept];
                    }

                    issues[issue] = new Pair<List<KeyValuePair<PreceptDef, float>>, float>(issues[issue].First, issues[issue].First[0].Value - deductOpinion);
                }

                EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();
                List<KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float>>> sortedIssues = issues.ToList();
                sortedIssues.SortBy((KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float>> x) => -x.Value.Second);

                for (int i = 0; i < Mathf.Min(settings.reformIssueChoices, sortedIssues.Count); i++)
                {
                    for (int j = 0; j < Mathf.Min(settings.reformPreceptsInIssue, sortedIssues[i].Value.First.Count); j++)
                    {
                        if (!selectablePrecepts.Contains(sortedIssues[i].Value.First[j].Key))
                        {
                            selectablePrecepts.Add(sortedIssues[i].Value.First[j].Key);
                        }
                    }
                }
            }

            if (!selectablePrecepts.Contains(precept))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(IdeoDevelopmentTracker), nameof(IdeoDevelopmentTracker.TryAddDevelopmentPoints))]
    public static class FluidIdeoTracker_AddPoints
    {
        public static void Postfix(IdeoDevelopmentTracker __instance, int pointsToAdd)
        {
            if (!__instance.CanReformNow)
            {
                return;
            }

            EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();

            if (!settings.colonyChoiceMode)
            {
                return;
            }

            Ideo newIdeo = IdeoGenerator.MakeIdeo(__instance.ideo.foundation.def);
            __instance.ideo.CopyTo(newIdeo);

            List<MemeDef> validMemes = new List<MemeDef>();

            for (int i = 0; i < DefDatabase<MemeDef>.AllDefsListForReading.Count; i++)
            {
                MemeDef specificMeme = DefDatabase<MemeDef>.AllDefsListForReading[i];

                if (specificMeme.hiddenInChooseMemes || specificMeme.category == MemeCategory.Structure)
                {
                    continue;
                }

                bool valid = true;

                foreach (Faction allFaction in Find.FactionManager.AllFactions)
                {
                    if (!allFaction.def.hidden && !allFaction.def.isPlayer && allFaction.ideos != null && (allFaction.ideos.IsPrimary(__instance.ideo) || allFaction.ideos.IsMinor(__instance.ideo)) && !IdeoUtility.IsMemeAllowedFor(specificMeme, allFaction.def))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    validMemes.Add(specificMeme);
                }
            }

            List<PreceptDef> validPrecepts = new List<PreceptDef>();

            for (int i = 0; i < DefDatabase<PreceptDef>.AllDefsListForReading.Count; i++)
            {
                PreceptDef specificPrecept = DefDatabase<PreceptDef>.AllDefsListForReading[i];
                bool dontAdd = false;

                for (int j = 0; j < __instance.ideo.memes.Count; j++)
                {
                    MemeDef meme = __instance.ideo.memes[j];

                    if (specificPrecept.conflictingMemes.Contains(meme))
                    {
                        dontAdd = true;
                        break;
                    }
                }

                if (!dontAdd && specificPrecept.visible && __instance.ideo.CanAddPreceptAllFactions(specificPrecept) && specificPrecept.preceptClass == typeof(Precept))
                {
                    validPrecepts.Add(specificPrecept);
                }
            }

            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
            Dictionary<MemeDef, float> memeOpinions = validMemes.ToDictionary((MemeDef x) => x, (MemeDef x) => 0f);
            Dictionary<PreceptDef, float> preceptOpinions = validPrecepts.ToDictionary((PreceptDef x) => x, (PreceptDef x) => 0f);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];

                if (pawn.Ideo != __instance.ideo)
                {
                    continue;
                }

                IdeoTrackerData tracker = comp.pawnTrackerData[pawn];

                for (int j = 0; j < memeOpinions.Count; j++)
                {
                    memeOpinions[validMemes[j]] += tracker.TrueMemeOpinion(validMemes[j]);
                }

                for (int j = 0; j < validPrecepts.Count; j++)
                {
                    preceptOpinions[validPrecepts[j]] += tracker.TruePreceptOpinion(validPrecepts[j]);
                }

                List<Ideo> pawnIdeos = tracker.baseIdeoOpinions.Keys.ToList();

                for (int j = 0; j < pawnIdeos.Count; j++)
                {
                    Ideo ideo = pawnIdeos[j];

                    for (int n = 0; n < ideo.memes.Count; n++)
                    {
                        if (memeOpinions.ContainsKey(ideo.memes[n]))
                        {
                            memeOpinions[ideo.memes[n]] += (tracker.IdeoOpinion(ideo) - 0.2f) * 20f;
                        }
                    }

                    for (int n = 0; n < ideo.precepts.Count; n++)
                    {
                        if (preceptOpinions.ContainsKey(ideo.precepts[n].def))
                        {
                            preceptOpinions[ideo.precepts[n].def] += (tracker.IdeoOpinion(ideo) - 0.2f) * 20f;
                        }
                    }
                }
            }

            float lowestOpinion = float.PositiveInfinity;
            MemeDef lowestOpinionMeme = null;

            for (int i = 0; i < __instance.ideo.memes.Count; i++)
            {
                MemeDef meme = __instance.ideo.memes[i];

                if (memeOpinions.ContainsKey(meme))
                {
                    if (memeOpinions[meme] < lowestOpinion)
                    {
                        lowestOpinion = memeOpinions[meme];
                        lowestOpinionMeme = meme;
                    }

                    memeOpinions.Remove(meme);
                }
            }

            // Amount of memes we're aiming for, with a 70% chance to potentially replace one of our existing ones if we're at 3 memes.
            int desiredMemeCount = Rand.Value > 0.7f ? 4 : 3;

            // If we have free space and don't hate any memes, don't add memes we violently dislike
            if (newIdeo.memes.Count < desiredMemeCount)
            {
                lowestOpinion = Math.Min(lowestOpinion, 0);
            }

            // Filtering out memes that we cannot slot because we like all of our current ones more or because we hate them too much
            memeOpinions = memeOpinions.Where((KeyValuePair<MemeDef, float> x) => x.Value > lowestOpinion).ToDictionary((KeyValuePair<MemeDef, float> x) => x.Key, (KeyValuePair<MemeDef, float> x) => x.Value);

            if (memeOpinions.Count > 0)
            {
                MemeDef chosenMeme = memeOpinions.RandomElementByWeight((KeyValuePair<MemeDef, float> x) => (x.Value + (lowestOpinion < 0 ? -lowestOpinion + 0.01f : 0)) / x.Key.impact).Key;

                if (newIdeo.memes.Count > desiredMemeCount)
                {
                    MemeDef toRemove = newIdeo.memes.Where((MemeDef x) => memeOpinions.ContainsKey(x) && memeOpinions[x] < memeOpinions[chosenMeme]).RandomElementByWeight((MemeDef x) => 1 / memeOpinions[x] + (lowestOpinion < 0 ? -lowestOpinion + 0.01f : 0));

                    newIdeo.memes.Remove(toRemove);
                }

                newIdeo.memes.Add(chosenMeme);
            }

            Dictionary<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> issues = new Dictionary<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>>();

            for (int i = 0; i < validPrecepts.Count; i++)
            {
                PreceptDef keyPrecept = validPrecepts[i];

                if (!issues.ContainsKey(keyPrecept.issue))
                {
                    issues[keyPrecept.issue] = new Pair<List<KeyValuePair<PreceptDef, float>>, float[]>(new List<KeyValuePair<PreceptDef, float>>(), [0f, 0f]);
                }

                issues[keyPrecept.issue].First.Add(new KeyValuePair<PreceptDef, float>(keyPrecept, preceptOpinions[keyPrecept]));
            }

            List<IssueDef> issueList = issues.Keys.ToList();

            for (int i = 0; i < issueList.Count; i++)
            {
                IssueDef issue = issueList[i];

                issues[issue].First.SortBy((KeyValuePair<PreceptDef, float> x) => -x.Value);

                PreceptDef ideoPrecept = null;

                for (int j = 0; j < __instance.ideo.precepts.Count; j++)
                {
                    if (__instance.ideo.precepts[j].def.issue == issue)
                    {
                        ideoPrecept = __instance.ideo.precepts[j].def;
                    }
                }

                float deductOpinion = 0f;

                if (ideoPrecept != null && preceptOpinions.ContainsKey(ideoPrecept))
                {
                    deductOpinion = preceptOpinions[ideoPrecept];
                }

                float impactWeight = 0f;
                float weightTotal = 0f;

                for (int j = 0; j < issues[issue].First.Count; j++)
                {
                    PreceptDef issuePrecept = issues[issue].First[j].Key;
                    impactWeight += Math.Max(0f, issues[issue].First[j].Value + (deductOpinion < 0 ? -deductOpinion : 0)) * (issuePrecept.impact == PreceptImpact.High ? 5f : issuePrecept.impact == PreceptImpact.Medium ? 3f : 1f);
                    weightTotal += Math.Max(0f, issues[issue].First[j].Value + (deductOpinion < 0 ? -deductOpinion : 0));
                }

                float impactDivider = Math.Max((impactWeight / weightTotal), 1f);

                if (float.IsNaN(impactDivider))
                {
                    impactDivider = 1f;
                }

                issues[issue] = new Pair<List<KeyValuePair<PreceptDef, float>>, float[]>(issues[issue].First, [(issues[issue].First[0].Value - deductOpinion) / impactDivider, deductOpinion]);
            }

            List<KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>>> sortedIssues = issues.Where((KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> x) => x.Value.Second[0] > 0f).ToList();
            sortedIssues.SortBy((KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> x) => -x.Value.Second[0]);

            int issuesTaken = Math.Min(sortedIssues.Count, Rand.RangeInclusive(Math.Max(3, settings.reformIssueChoices), Math.Min(7, settings.reformIssueChoices)));

            for (int i = 0; i < issuesTaken; i++)
            {
                KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> pickedIssue = sortedIssues.RandomElementByWeight((KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> x) => x.Value.Second[0]);
                List<KeyValuePair<PreceptDef, float>> possiblePrecepts = pickedIssue.Value.First.Where((KeyValuePair<PreceptDef, float> x) => x.Value > pickedIssue.Value.Second[1]).ToList();
                PreceptDef newPrecept = possiblePrecepts.RandomElementByWeight((KeyValuePair<PreceptDef, float> x) => x.Value + (pickedIssue.Value.Second[1] < 0 ? -pickedIssue.Value.Second[1] : 0)).Key;

                bool samePrecept = false;

                for (int j = 0; j < newIdeo.precepts.Count; j++)
                {
                    Precept precept2 = newIdeo.precepts[j];

                    if (precept2.def == newPrecept)
                    {
                        samePrecept = true;
                        break;
                    }

                    if (precept2.def.issue == pickedIssue.Key)
                    {
                        newIdeo.RemovePrecept(precept2, true);
                        break;
                    }
                }

                if (samePrecept)
                {
                    continue;
                }

                newIdeo.AddPrecept(PreceptMaker.MakePrecept(newPrecept), init: true);
                newIdeo.anyPreceptEdited = true;
            }

            newIdeo.SortMemesInDisplayOrder();
            newIdeo.foundation.EnsurePreceptsCompatibleWithMemes(__instance.ideo.memes, newIdeo.memes, new IdeoGenerationParms(IdeoUIUtility.FactionForRandomization(newIdeo)));
            newIdeo.RecachePrecepts();

            IdeoDevelopmentUtility.ApplyChangesToIdeo(__instance.ideo, newIdeo);
        }
    }
}
