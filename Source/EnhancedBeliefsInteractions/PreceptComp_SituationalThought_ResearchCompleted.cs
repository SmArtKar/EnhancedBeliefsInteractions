using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EnhancedBeliefsInteractions
{
    public class PreceptComp_SituationalThought_ResearchCompleted : PreceptComp_SituationalThought
    {
        public void ResearchCompleted(Pawn pawn, Precept precept, float points)
        {
            Thought_Memory memory = ThoughtMaker.MakeThought(thought, precept);
            memory.SetForcedStage(points > 2000 ? 2 : points > 1000 ? 1 : 0);
            pawn.needs.mood.thoughts.memories.TryGainMemory(memory);
        }
    }
}
