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
    public class ThoughtWorker_Precept_Zealots : ThoughtWorker_Precept
    {
        public int HereticsInJail(Pawn p)
        {
            List<Pawn> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony;
            int heretics = 0;

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];

                if (pawn.Ideo != null && pawn.Ideo != p.Ideo && pawn.Faction == p.Faction)
                {
                    heretics += 1;
                }
            }

            return heretics;
        }

        public override string PostProcessLabel(Pawn p, string label)
        {
            int num = Mathf.RoundToInt(MoodMultiplier(p));
            if (num <= 1)
            {
                return base.PostProcessLabel(p, label);
            }
            return base.PostProcessLabel(p, label) + " x" + num;
        }

        public override float MoodMultiplier(Pawn p)
        {
            if (p.Ideo == null)
            {
                return 1f;
            }

            return Mathf.Min(def.stackLimit, HereticsInJail(p));
        }

        public override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (HereticsInJail(p) == 0)
            {
                return ThoughtState.Inactive;
            }

            return ThoughtState.ActiveDefault;
        }
    }
}
