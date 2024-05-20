using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

[DefOf]
public static class EBIDefOf
{
    public static MemeDef EBI_Zealots;

    static EBIDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EBIDefOf));
    }
}

namespace EnhancedBeliefsInteractions
{
    public class EBInteractionsSettings : ModSettings
    {
        public int reformMemeChoices = 5;
        public int reformIssueChoices = 7;
        public int reformPreceptsInIssue = 3;
        public bool colonyChoiceMode = false;
        public bool fluidMode = false;

        public bool schismsEnabled = false;
        public float schismMTBDays = 30f;
        public int minColonistCount = 11;
        public float maxSplitSize = 0.5f;
        public float schismSafeCertainty = 0.9f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref reformMemeChoices, "reformMemeChoices");
            Scribe_Values.Look(ref reformIssueChoices, "reformPreceptChoices");
            Scribe_Values.Look(ref reformPreceptsInIssue, "reformPreceptsInIssue");
            Scribe_Values.Look(ref colonyChoiceMode, "colonyChoiceMode");
            Scribe_Values.Look(ref fluidMode, "fluidMode");

            Scribe_Values.Look(ref schismsEnabled, "schismsEnabled");
            Scribe_Values.Look(ref schismMTBDays, "schismMTBDays");
            Scribe_Values.Look(ref minColonistCount, "minColonistCount");
            Scribe_Values.Look(ref maxSplitSize, "maxSplitSize");
            Scribe_Values.Look(ref schismSafeCertainty, "schismSafeCertainty");
        }
    }

    public class EBInteractionsMod : Mod
    {
        public Harmony harmonyInstance;
        public EBInteractionsSettings Settings => modSettings as EBInteractionsSettings;

        public EBInteractionsMod(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony(id: "rimworld.smartkar.enhancedbeliefsinteractions.main");
            harmonyInstance.PatchAll();
            modSettings = GetSettings<EBInteractionsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Widgets.DrawMenuSection(inRect);

            Rect leftRect = inRect.ContractedBy(15).Rounded();

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(leftRect);

            listing.CheckboxLabeled("Colony's Choice mode", ref Settings.colonyChoiceMode, tooltip: "Pawns will pick memes and precepts when developing fluid ideologies and schisms by themselves (Default: off)");
            listing.CheckboxLabeled("Fluid mode", ref Settings.fluidMode, tooltip: "All generated ideologies will be fluid (Default: off)");
            listing.Label("Amount of memes to choose between when reforming an ideology (Default: 5, current value: {0})".Formatted(Settings.reformMemeChoices));
            Settings.reformMemeChoices = (int)Widgets.HorizontalSlider(listing.GetRect(15f), Settings.reformMemeChoices, 1, 15, roundTo: 1);
            listing.Label("Amount of issues to give options to alter when reforming an ideology (Default: 7, current value: {0})".Formatted(Settings.reformIssueChoices));
            Settings.reformIssueChoices = (int)Widgets.HorizontalSlider(listing.GetRect(15f), Settings.reformIssueChoices, 1, 30, roundTo: 1);
            listing.Label("Amount of precepts per issue to give as options when reforming an ideology (Default: 3, current value: {0})".Formatted(Settings.reformPreceptsInIssue));
            Settings.reformPreceptsInIssue = (int)Widgets.HorizontalSlider(listing.GetRect(15f), Settings.reformPreceptsInIssue, 1, 10, roundTo: 1);

            listing.CheckboxLabeled("Schisms enabled", ref Settings.schismsEnabled, tooltip: "Whenever schisms should happen in large colonies (Default: off)");
            listing.Label("Average amount of days between schism checks (Default: 30, current value: {0})".Formatted(Settings.schismMTBDays));
            Settings.schismMTBDays = Widgets.HorizontalSlider(listing.GetRect(15f), Settings.schismMTBDays, 1, 120, roundTo: 0.1f);
            listing.Label("Minimum amount of colonist in an ideology for a schisms to occur (Default: 11, current value: {0})".Formatted(Settings.minColonistCount));
            Settings.minColonistCount = (int)Widgets.HorizontalSlider(listing.GetRect(15f), Settings.minColonistCount, 1, 30, roundTo: 1);
            listing.Label("Maximum amount of pawns in the new ideology after a schism (Default: 50%, current value: {0})".Formatted(Settings.maxSplitSize.ToStringPercent()));
            Settings.maxSplitSize = Widgets.HorizontalSlider(listing.GetRect(15f), Settings.maxSplitSize, 0.05f, 1f, roundTo: 0.05f);
            listing.Label("Certainty above which pawns cannot participate in a schism (Default: 90%, current value: {0})".Formatted(Settings.schismSafeCertainty.ToStringPercent()));
            Settings.schismSafeCertainty = Widgets.HorizontalSlider(listing.GetRect(15f), Settings.schismSafeCertainty, 0.05f, 1f, roundTo: 0.05f);

            listing.End();
            GUI.EndGroup();
        }

        public override string SettingsCategory()
        {
            return "Enhanced Beliefs - Interactions";
        }
    }
}
