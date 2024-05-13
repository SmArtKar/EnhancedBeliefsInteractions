using EnhancedBeliefs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EnhancedBeliefsInteractions
{
    public class Thought_DiversitySocial : Thought_SituationalSocial
    {
        public override float OpinionOffset()
        {
            float offset = base.OpinionOffset();
            GameComponent_EnhancedBeliefs comp = Current.Game.GetComponent<GameComponent_EnhancedBeliefs>();

            if (pawn.Ideo == null || otherPawn.Ideo == null || !comp.pawnTrackerData.ContainsKey(pawn))
            {
                return offset;
            }

            float[] opinionKeys = comp.pawnTrackerData[pawn].DetailedIdeoOpinion(otherPawn.Ideo, true);
            float opinion = opinionKeys[0] * opinionKeys[1];

            if (offset > 0)
            {
                offset *= opinion * 2f;
            }
            else
            {
                offset *= (1f - opinion) * 2f;
            }

            return offset;
        }
    }
}
