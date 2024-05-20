using EnhancedBeliefs;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EnhancedBeliefsInteractions
{
    public static class IdeoSchismUtility
    {
        public static void TryCauseSchism(Ideo ideo)
        {
            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();
            List<Pawn> believers = comp.ideoPawnsList[ideo].Where((Pawn x) => x.IsFreeColonist).ToList();

            if (believers.Count < settings.minColonistCount)
            {
                return;
            }

            List<Pawn> schismPawns = PickSchismPawns(ideo, believers);

            if (schismPawns.Count < 3)
            {
                return;
            }

            schismPawns = schismPawns.InRandomOrder().Take(Rand.RangeInclusive((int)Math.Max(3f, schismPawns.Count / 2f), (int)(believers.Count * settings.maxSplitSize))).ToList();

            Ideo newIdeo = IdeoGenerator.MakeIdeo(ideo.foundation.def);
            ideo.CopyTo(newIdeo);
            IdeoGenerationParms parms = new IdeoGenerationParms(Faction.OfPlayer.def);
            newIdeo.foundation.GenerateTextSymbols();
            newIdeo.foundation.GenerateLeaderTitle();
            newIdeo.foundation.RandomizeIcon();

            GenerateMemesAndPrecepts(newIdeo, ideo, schismPawns, out StringBuilder ideoChanges);
            newIdeo.foundation.InitPrecepts(parms);
            newIdeo.RecachePrecepts();

            newIdeo.foundation.ideo.RegenerateDescription(force: true);
            newIdeo.thingStyleCategories = new List<ThingStyleCategoryWithPriority>(ideo.thingStyleCategories);
            Find.IdeoManager.Add(newIdeo);

            if (ideo.Fluid)
            {
                newIdeo.Fluid = true;
                ideo.development.CopyTo(newIdeo.development);
            }

            for (int i = 0; i < schismPawns.Count; i++)
            {
                schismPawns[i].ideo.SetIdeo(newIdeo);
            }

            Find.FactionManager.OfPlayer.ideos.RecalculateIdeosBasedOnPlayerPawns();

            Find.WindowStack.Add(new Window_SchismReport
            {
                newIdeo = newIdeo,
                newMembers = schismPawns,
                ideoChanges = ideoChanges
            });
        }

        public static List<Pawn> PickSchismPawns(Ideo ideo, List<Pawn> believers)
        {
            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            EBInteractionsSettings settings = LoadedModManager.GetMod<EBInteractionsMod>().GetSettings<EBInteractionsSettings>();
            Pawn leader = null;

            for (int i = 0; i < believers.Count; i++)
            {
                if (ideo.GetRole(believers[i])?.def == PreceptDefOf.IdeoRole_Leader)
                {
                    leader = believers[i];
                    break;
                }
            }

            if (leader == null)
            {
                leader = believers.RandomElementByWeight((Pawn x) => x.ideo.Certainty + (ideo.GetRole(x) != null ? 10 : 0));
            }

            List<Pawn> oldPawns = believers.Where((Pawn x) => x.ideo.Certainty > settings.schismSafeCertainty || ideo.GetRole(x) != null).Union(new List<Pawn> { leader }).ToList();
            List<Pawn> schismPawns = believers.Except(oldPawns).ToList();
            Dictionary<Pawn, float> opinions = schismPawns.ToDictionary((Pawn x) => x, (Pawn x) => 0f);

            for (int i = 0; i < oldPawns.Count; i++)
            {
                ApplyOpinions(oldPawns[i], comp.pawnTrackerData[leader], oldPawns, ref opinions);
            }

            while (schismPawns.Count > believers.Count * settings.maxSplitSize)
            {
                KeyValuePair<Pawn, float> bestFit = opinions.MaxBy((KeyValuePair<Pawn, float> x) => x.Value);

                if (bestFit.Value < 0f)
                {
                    break;
                }

                Pawn bestPawn = bestFit.Key;
                schismPawns.Remove(bestPawn);
                opinions.Remove(bestPawn);
                ApplyOpinions(bestPawn, comp.pawnTrackerData[bestPawn], oldPawns, ref opinions);
                oldPawns.Add(bestPawn);
            }

            return schismPawns;
        }

        public static float GetIdeologicalCompatibility(Pawn pawn1, Pawn pawn2)
        {
            float compatibility = 0f;
            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            IdeoTrackerData tracker1 = comp.pawnTrackerData[pawn1];
            IdeoTrackerData tracker2 = comp.pawnTrackerData[pawn2];
            List<MemeDef> memes = tracker1.memeOpinions.Keys.Union(tracker2.memeOpinions.Keys).ToList();
            List<PreceptDef> precepts = tracker1.preceptOpinions.Keys.Union(tracker2.preceptOpinions.Keys).ToList();

            for (int i = 0; i < memes.Count; i++)
            {
                float opinion1 = tracker1.TrueMemeOpinion(memes[i]);
                float opinion2 = tracker2.TrueMemeOpinion(memes[i]);
                compatibility -= Math.Abs(opinion1 - opinion2) / Math.Max(opinion1, opinion2) - 1f;
            }

            for (int i = 0; i < precepts.Count; i++)
            {
                float opinion1 = tracker1.TruePreceptOpinion(precepts[i]);
                float opinion2 = tracker2.TruePreceptOpinion(precepts[i]);
                compatibility -= (Math.Abs(opinion1 - opinion2) / Math.Max(opinion1, opinion2) - 1f) * 0.5f;
            }

            return compatibility;
        }

        public static void ApplyOpinions(Pawn pawn, IdeoTrackerData tracker, List<Pawn> pawns, ref Dictionary<Pawn, float> opinions)
        {
            List<Pawn> otherPawns = opinions.Keys.ToList();

            for (int i = 0; i <  otherPawns.Count; i++)
            {
                Pawn otherPawn = otherPawns[i];
                opinions[otherPawn] = (opinions[otherPawn] * pawns.Count + tracker.cachedRelationships.TryGetValue(otherPawn) + GetIdeologicalCompatibility(pawn, otherPawn) * 10f) / (pawns.Count + 1);
            }
        }

        [DebugAction("General", "Schism Primary Ideo")]
        public static void TriggerIdeoSchism()
        {
            TryCauseSchism(Faction.OfPlayer.ideos.PrimaryIdeo);
        }

        public static void GenerateMemesAndPrecepts(Ideo ideo, Ideo oldIdeo, List<Pawn> pawns, out StringBuilder ideoChanges)
        {
            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();
            ideoChanges = new StringBuilder();

            List<MemeDef> validMemes = new List<MemeDef>();
            List<PreceptDef> validPrecepts = new List<PreceptDef>();

            for (int i = 0; i < DefDatabase<MemeDef>.AllDefsListForReading.Count; i++)
            {
                MemeDef specificMeme = DefDatabase<MemeDef>.AllDefsListForReading[i];

                if (!specificMeme.hiddenInChooseMemes && specificMeme.category == MemeCategory.Normal)
                {
                    validMemes.Add(specificMeme);
                }
            }

            for (int i = 0; i < DefDatabase<PreceptDef>.AllDefsListForReading.Count; i++)
            {
                PreceptDef specificPrecept = DefDatabase<PreceptDef>.AllDefsListForReading[i];
                bool dontAdd = false;

                for (int j = 0; j < ideo.memes.Count; j++)
                {
                    MemeDef meme = ideo.memes[j];

                    if (specificPrecept.conflictingMemes.Contains(meme))
                    {
                        dontAdd = true;
                        break;
                    }
                }

                if (!dontAdd && specificPrecept.visible && ideo.CanAddPreceptAllFactions(specificPrecept) && specificPrecept.preceptClass == typeof(Precept))
                {
                    validPrecepts.Add(specificPrecept);
                }
            }

            Dictionary<MemeDef, float> memeOpinions = validMemes.ToDictionary((MemeDef x) => x, (MemeDef x) => 0f);
            Dictionary<PreceptDef, float> preceptOpinions = validPrecepts.ToDictionary((PreceptDef x) => x, (PreceptDef x) => 0f);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                IdeoTrackerData tracker = comp.pawnTrackerData[pawn];

                for (int j = 0; j < validMemes.Count; j++)
                {
                    memeOpinions[validMemes[i]] += tracker.TrueMemeOpinion(validMemes[i]);
                }

                for (int j = 0; j < validPrecepts.Count; j++)
                {
                    preceptOpinions[validPrecepts[i]] += tracker.TruePreceptOpinion(validPrecepts[i]);
                }
            }

            MemeDef bestMeme = memeOpinions.Where((KeyValuePair<MemeDef, float> x) => !ideo.memes.Contains(x.Key)).MaxBy((KeyValuePair<MemeDef, float> x) => x.Value).Key;

            if (bestMeme != null)
            {
                if (ideo.memes.Count >= 3)
                {
                    MemeDef toRemove = ideo.memes.Where((MemeDef x) => memeOpinions.ContainsKey(x)).MaxBy((MemeDef x) => -memeOpinions[x]);

                    if (memeOpinions[toRemove] < memeOpinions[bestMeme])
                    {
                        ideoChanges.AppendLine("Meme changed: {0} -> {1}".Formatted(toRemove.LabelCap, bestMeme.LabelCap));
                        ideo.memes.Remove(toRemove);
                        ideo.memes.Add(bestMeme);
                    }
                }
                else
                {
                    ideoChanges.AppendLine("New meme added: " + bestMeme.LabelCap);
                    ideo.memes.Add(bestMeme);
                }
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

                for (int j = 0; j < ideo.precepts.Count; j++)
                {
                    if (ideo.precepts[j].def.issue == issue)
                    {
                        ideoPrecept = ideo.precepts[j].def;
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

            int issuesTaken = Math.Min(sortedIssues.Count, Rand.RangeInclusive(3, 7));

            for (int i = 0; i < issuesTaken; i++)
            {
                KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> pickedIssue = sortedIssues.RandomElementByWeight((KeyValuePair<IssueDef, Pair<List<KeyValuePair<PreceptDef, float>>, float[]>> x) => x.Value.Second[0]);
                List<KeyValuePair<PreceptDef, float>> possiblePrecepts = pickedIssue.Value.First.Where((KeyValuePair<PreceptDef, float> x) => x.Value > pickedIssue.Value.Second[1]).ToList();
                PreceptDef newPrecept = possiblePrecepts.RandomElementByWeight((KeyValuePair<PreceptDef, float> x) => x.Value + (pickedIssue.Value.Second[1] < 0 ? -pickedIssue.Value.Second[1] : 0)).Key;

                bool samePrecept = false;
                string removedPrecept = null;

                for (int j = 0; j < ideo.precepts.Count; j++)
                {
                    Precept precept2 = ideo.precepts[j];

                    if (precept2.def == newPrecept)
                    {
                        samePrecept = true;
                        break;
                    }

                    if (precept2.def.issue == pickedIssue.Key)
                    {
                        removedPrecept = precept2.TipLabel;
                        ideo.RemovePrecept(precept2, true);
                        break;
                    }
                }

                if (samePrecept)
                {
                    continue;
                }

                Precept precept = PreceptMaker.MakePrecept(newPrecept);

                if (removedPrecept != null)
                {
                    ideoChanges.AppendLine("Precept changed: {0} -> {1}".Formatted(removedPrecept, precept.TipLabel));
                }
                else
                {
                    ideoChanges.AppendLine("New precept added: " + precept.TipLabel);
                }

                ideo.AddPrecept(precept, init: true);
                ideo.anyPreceptEdited = true;
            }

            ideo.SortMemesInDisplayOrder();
            ideo.foundation.EnsurePreceptsCompatibleWithMemes(oldIdeo.memes, ideo.memes, new IdeoGenerationParms(IdeoUIUtility.FactionForRandomization(ideo)));
        }
    }
}
