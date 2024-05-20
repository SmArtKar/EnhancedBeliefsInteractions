using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedBeliefsInteractions
{
    public class GoodwillSituationWorker_SupremacyMeme : GoodwillSituationWorker_MemeCompatibility
    {
        public override int GetNaturalGoodwillOffset(Faction other)
        {
            if (!Applies(other))
            {
                return 0;
            }

            return (int)(def.naturalGoodwillOffset * Mathf.Clamp01(1f - 0.2f * SharedMemes(other)));
        }

        public int SharedMemes(Faction other)
        {
            Ideo primaryIdeo1 = Faction.OfPlayer.ideos.PrimaryIdeo;
            Ideo primaryIdeo2 = other.ideos.PrimaryIdeo;

            if (primaryIdeo1 == null || primaryIdeo2 == null)
            {
                return 0;
            }

            return primaryIdeo1.memes.Intersect(primaryIdeo2.memes).Count();
        }
    }
}
