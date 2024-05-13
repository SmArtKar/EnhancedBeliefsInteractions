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
    public class ThoughtWorker_Precept_ResearchTooSlow : ThoughtWorker_Precept
    {
        public override ThoughtState ShouldHaveThought(Pawn p)
        {
            return ResearchManager_ResearchPerformed.AverageResearch() < def.stages[0].baseMoodEffect ? ThoughtState.ActiveDefault : ThoughtState.Inactive;
        }

        public override float MoodMultiplier(Pawn p)
        {
            return Mathf.Clamp((float)Math.Round(def.stages[0].baseMoodEffect / ResearchManager_ResearchPerformed.AverageResearch(), MidpointRounding.ToEven), 1f, 3f);
        }
    }
}
