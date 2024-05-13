using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace EnhancedBeliefsInteractions
{
    public class ThoughtWorker_Precept_AllResearchFinished : ThoughtWorker_Precept
    {
        public override ThoughtState ShouldHaveThought(Pawn p)
        {
            return Find.ResearchManager.progress.Count == DefDatabase<ResearchProjectDef>.DefCount ? ThoughtState.ActiveDefault : ThoughtState.Inactive;
        }
    }
}
