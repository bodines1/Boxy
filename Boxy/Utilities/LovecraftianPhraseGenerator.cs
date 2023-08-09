using System;
using System.Collections.Generic;

namespace CardMimic.Utilities
{
    public static class LovecraftianPhraseGenerator
    {
        private static readonly Random _random = new Random();

        private static List<string> OngoingActions { get; } = new List<string>
        {
            "Deciphering archaic poems",
            "Summoning unknowable horrors",
            "Transcribing grave misdeeds",
            "Performing diabolical prestidigitations",
            "Opening the primordial cradle",
            "Unveiling, by sinister alchemies",
            "Finalizing loathsome changes",
            "Investigating faint miasmal odour",
            "Lo, a detestably sticky noise",
            "Acting on studied malevolence",
            "Avoiding the undulating coils",
            "Malfeasant susurrations rising",
            "Plunging blindly into the abyss",
            "Resisting the madness of the infinite",
            "Devising oily temptations",
        };

        public static string RandomPhrase()
        {
            int maxVal = OngoingActions.Count;
            var index = (int)(_random.NextDouble() * maxVal);
            return OngoingActions[index];
        }
    }
}
