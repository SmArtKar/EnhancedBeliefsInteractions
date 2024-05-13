using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EnhancedBeliefsInteractions
{
    public class ThoughtWorker_Precept_Research_Social : ThoughtWorker_Precept_Social
    {
        public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {
            if (otherPawn.skills == null)
            {
                return ThoughtState.Inactive;
            }

            int topLevel = Math.Abs(def.stages[0].baseOpinionOffset) <= 1 ? 10 : 5;
            int skill = otherPawn.skills.GetSkill(SkillDefOf.Intellectual).GetLevel();

            if (skill > topLevel)
            {
                return ThoughtState.ActiveDefault;
            }

            if (skill < 5)
            {
                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}
