using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using HotSwap;

namespace EnhancedBeliefsInteractions
{
    [HotSwappable]
    public class Window_SchismReport : Window
    {
        public Ideo newIdeo;
        public List<Pawn> newMembers;
        public StringBuilder ideoChanges;
        private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

        public override Vector2 InitialSize => new Vector2(600f, 750f);

        public Window_SchismReport()
        {
            forcePause = true;
            doCloseX = false;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 32f), "Ideological Schism: " + newIdeo);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            StringBuilder believers = new StringBuilder();
            for (int i = 0; i < newMembers.Count; i++)
            {
                believers.AppendLine(" - " + newMembers[i].LabelShort);
            }

            Widgets.Label(new Rect(inRect.x, inRect.y + 47f, inRect.width, inRect.height - 102f), "Following changes happened to the ideology:\n{0}\n\nThe following colonists have converted to {1}:\n{2}".Formatted(ideoChanges.ToString().TrimEndNewlines(), newIdeo, believers.ToString().TrimEndNewlines()));

            if (Widgets.ButtonText(new Rect(inRect.width / 2f - BottomButtonSize.x / 2f, inRect.yMax - 55f, BottomButtonSize.x, BottomButtonSize.y), "Close".Translate()))
            {
                Close();
            }
        }
    }
}
