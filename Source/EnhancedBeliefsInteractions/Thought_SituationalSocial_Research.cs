using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedBeliefsInteractions
{
    public class Thought_SituationalSocial_Research : Thought_SituationalSocial
    {
        public override int CurStageIndex
        {
            get
            {
                if (otherPawn.skills == null)
                {
                    return base.CurStageIndex;
                }

                int topLevel = Math.Abs(def.stages[0].baseOpinionOffset) <= 1 ? 10 : 5;
                int skill = otherPawn.skills.GetSkill(SkillDefOf.Intellectual).GetLevel();

                if (skill > topLevel)
                {
                    return 2;
                }

                if (skill < 5)
                {
                    return 0;
                }

                return 1;
            }
        }

        public override float OpinionOffset()
        {
            if (otherPawn.skills == null)
            {
                return base.OpinionOffset();
            }

            int topLevel = Math.Abs(def.stages[0].baseOpinionOffset) <= 1 ? 10 : 5;
            int skill = otherPawn.skills.GetSkill(SkillDefOf.Intellectual).GetLevel();

            if (skill > topLevel)
            {
                return (skill - topLevel) * CurStage.baseOpinionOffset;
            }

            if (skill < 5)
            {
                return (5f - skill) * CurStage.baseOpinionOffset;
            }

            return 0f;
        }
    }
}
