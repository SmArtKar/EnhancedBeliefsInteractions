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
    public class ThoughtWorker_Precept_PawnResearching : ThoughtWorker_Precept
    {
        public int researchersCache = 0;
        public int lastRecacheTick = 0;

        public override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!p.IsColonist && !p.IsSlaveOfColony)
            {
                return ThoughtState.Inactive;
            }

           return ActiveResearchers() == 0 ? ThoughtState.Inactive : ThoughtState.ActiveDefault;
        }

        public override float MoodMultiplier(Pawn p)
        {
            return Mathf.Min(def.stackLimit, ActiveResearchers());
        }

        public int ActiveResearchers()
        {
            if (Find.TickManager.TicksGame -  lastRecacheTick < 250)
            {
                return researchersCache;
            }

            researchersCache = 0;
            List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonistsSpawned;

            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].CurJobDef == JobDefOf.Research)
                {
                    researchersCache += 1;
                }
            }

            return researchersCache;
        }
    }
}
