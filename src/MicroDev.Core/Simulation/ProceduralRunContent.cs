namespace MicroDev.Core.Simulation;

public static class ProceduralRunContent
{
    private static readonly string[] CandidateNames =
    [
        "Alex",
        "Jordan",
        "Sam",
        "Morgan",
        "Taylor",
        "Riley",
        "Casey",
        "Avery",
        "Jamie",
        "Quinn",
        "Harper",
        "Rowan",
    ];

    private static readonly string[] ShowTitles =
    [
        "Midnight Refactor",
        "Space Diner",
        "Patch Notes",
        "Cozy Detectives",
        "Neon Borough",
        "After Hours Arcade",
        "Ghost Cursor",
        "Merge Conflict Manor",
    ];

    private static readonly string[] BurgerNames =
    [
        "Break Burger",
        "Night Melt",
        "Stack Burger",
        "Deadline Double",
    ];

    private static readonly string[] BurritoNames =
    [
        "Deploy Wrap",
        "Merge Wrap",
        "Patch Burrito",
        "Sprint Wrap",
    ];

    private static readonly string[] PizzaNames =
    [
        "Checkpoint Pie",
        "Hotfix Slice",
        "Overtime Pie",
        "Merge Pizza",
    ];

    private static readonly string[] DumplingNames =
    [
        "Debug Dumplings",
        "Merge Gyoza",
        "Desk Dumplings",
        "Cache Dumplings",
    ];

    private static readonly string[] PastaNames =
    [
        "Skillet Pasta",
        "Garlic Noodles",
        "Tomato Orzo",
        "Pan Sauce Pasta",
    ];

    private static readonly string[] ChiliNames =
    [
        "Meal Prep Chili",
        "Slow Simmer Chili",
        "Weeknight Chili Pot",
        "Curry Bean Chili",
    ];

    public static string GetRelationshipCandidateName(int runSeed)
    {
        return Pick(runSeed, "relationship:name", CandidateNames);
    }

    public static int GetRelationshipCandidateCompatibility(int runSeed)
    {
        return 62 + (CreateSeed(runSeed, "relationship:compatibility") % 37);
    }

    public static string GetShowTitle(int runSeed, string key)
    {
        return Pick(runSeed, key, ShowTitles);
    }

    public static string GetFoodName(int runSeed, FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => Pick(runSeed, "food:burrito:name", BurritoNames),
            FoodChoice.Pizza => Pick(runSeed, "food:pizza:name", PizzaNames),
            FoodChoice.Dumplings => Pick(runSeed, "food:dumplings:name", DumplingNames),
            FoodChoice.SkilletPasta => Pick(runSeed, "food:pasta:name", PastaNames),
            FoodChoice.MealPrepChili => Pick(runSeed, "food:chili:name", ChiliNames),
            _ => Pick(runSeed, "food:burger:name", BurgerNames),
        };
    }

    public static string GetFoodDescription(int runSeed, FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito => "Cheap, filling, and steadier than the greasiest options if you bother to lock the details in before the order flies.",
            FoodChoice.Pizza => "Big focus spike, but the post-meal drag can absolutely flatten the rest of the coding block if you let the order get sloppy.",
            FoodChoice.Dumplings => "Comfort food that helps sanity more than raw typing energy. Better for stabilizing a rough day than brute-forcing more clicks.",
            FoodChoice.SkilletPasta => "A faster home-cooked option. Cheaper than delivery, calmer on the stomach, and only worth it if you can wait for the pan to finish.",
            FoodChoice.MealPrepChili => "The long route. It ties the meal loop to real planning, but the sanity payoff is strong and the recovery is cleaner than takeout.",
            _ => "Reliable focus recovery at a fair price, but a messy order can still turn the next session sluggish.",
        };
    }

    public static IReadOnlyList<FoodOrderModifierOption> GetFoodModifiers(int runSeed, FoodChoice choice)
    {
        return choice switch
        {
            FoodChoice.Burrito =>
            [
                new(FoodOrderModifier.NoCheese, "No Cheese", "Avoids the heavy lunch spiral that wrecks the next coding block.", true),
                new(FoodOrderModifier.SauceOnSide, "Sauce On Side", "Keeps the wrap from becoming a total desk disaster.", true),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Cuts the sugar crash, but this one is optional.", false),
            ],
            FoodChoice.Pizza =>
            [
                new(FoodOrderModifier.NoCheese, "Light Cheese", "Tones down the greasy post-slice slowdown.", true),
                new(FoodOrderModifier.SauceOnSide, "Hold Ranch", "Skip the extra heaviness unless you really need the comfort.", false),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Avoids stacking a sugar crash on top of the pizza drag.", true),
            ],
            FoodChoice.Dumplings =>
            [
                new(FoodOrderModifier.NoCheese, "No Extra Chili", "Keeps the comfort food from becoming a focus tax.", true),
                new(FoodOrderModifier.SauceOnSide, "Sauce On Side", "Lets you control how messy the whole break becomes.", true),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Optional. Helpful, but not the core issue here.", false),
            ],
            FoodChoice.SkilletPasta =>
            [
                new(FoodOrderModifier.NoCheese, "Salt The Water", "Skipping the basics turns cheap pasta into a morale hit.", true),
                new(FoodOrderModifier.SauceOnSide, "Prep Before Heat", "Get everything ready first so cooking does not unravel into chaos.", true),
                new(FoodOrderModifier.SkipSoda, "Set A Timer", "Optional, but it protects you from drifting back into work and scorching the pan.", false),
            ],
            FoodChoice.MealPrepChili =>
            [
                new(FoodOrderModifier.NoCheese, "Chop Veg First", "Front-loading prep keeps the whole cook from feeling endless.", true),
                new(FoodOrderModifier.SauceOnSide, "Low Simmer", "Patience matters more than heat if you want the night to stay stable.", true),
                new(FoodOrderModifier.SkipSoda, "Portion It Out", "Optional, but future-you will absolutely thank you.", false),
            ],
            _ =>
            [
                new(FoodOrderModifier.NoCheese, "No Cheese", "The classic burger mistake. Leaving it on slows the whole desk down.", true),
                new(FoodOrderModifier.SauceOnSide, "Sauce On Side", "Optional, but it keeps the keyboard and your brain cleaner.", false),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Avoids the greasy-food sugar crash combo.", true),
            ],
        };
    }

    public static string GetPublishedAppName(int runSeed, int releaseNumber, string currentProjectName)
    {
        var stem = string.IsNullOrWhiteSpace(currentProjectName)
            ? Pick(runSeed, "publish:label", ["Patchlight", "Overnight Build", "Desk Commit", "Late Shift"])
            : currentProjectName;
        return $"{stem} Release {releaseNumber}";
    }

    public static string GetPartnerPrompt(int runSeed, string partnerName, int relationshipProgress)
    {
        string[] prompts = relationshipProgress switch
        {
            >= 6 =>
            [
                $"{partnerName} pings with a tiny check-in that somehow matters more than half the apps on your task bar.",
                $"{partnerName} asks whether tonight is turning into another vanish-into-the-editor kind of night.",
            ],
            >= 3 =>
            [
                $"{partnerName} sends a steady little message right as the desk starts feeling too loud.",
                $"{partnerName} checks in and you can feel the tension between the work and the rest of your life.",
            ],
            _ =>
            [
                $"{partnerName} keeps the conversation alive, but it needs actual attention instead of another vague reply.",
                $"{partnerName} surfaces between tasks again, and it feels like a real fork in the night.",
            ],
        };

        return Pick(runSeed, $"partner:{partnerName}:{relationshipProgress}", prompts);
    }

    public static string GetIncidentDescription(int runSeed, string incidentId, IncidentType incidentType)
    {
        string[] choices = incidentType switch
        {
            IncidentType.CatInterruption =>
            [
                "The cat launches onto the desk and claims the keyboard.",
                "A full cat interruption lands right on top of the editor.",
                "The cat decides your workflow is optional and your keyboard is not.",
            ],
            IncidentType.TechDebtBug =>
            [
                "A stubborn regression starts eating your momentum.",
                "A quiet bug slips into the work and starts poisoning trust in the build.",
                "A nasty little defect surfaces right as the session was stabilizing.",
            ],
            IncidentType.JobListing =>
            [
                "A fresh C# / .NET listing pops up in the inbox.",
                "A recruiter follow-up lands with a role that actually fits the stack.",
                "A new opening appears before the week can close around you.",
            ],
            IncidentType.DeepWorkWindow =>
            [
                "A clean deep-work pocket opens up. The desk finally clicks into place.",
                "For once the noise drops and the next block looks writable.",
                "Everything aligns for a little while and the session finally breathes.",
            ],
            IncidentType.ContextSwitch =>
            [
                "Pings, tabs, and context switching shred the next block of focus.",
                "The day splinters into notifications and tiny expensive interruptions.",
                "Three unrelated tabs steal the next chunk of your brain.",
            ],
            IncidentType.CoffeeBounce =>
            [
                "You find one last good coffee. The next stretch suddenly feels survivable again.",
                "A rare clean caffeine bounce lands exactly when you need it.",
                "The mug is still warm and the desk gets a little less hostile.",
            ],
            IncidentType.MentorNudge =>
            [
                "A mentor message lands with a sharp note about what recruiters actually notice.",
                "A thoughtful senior dev drops advice that cuts right through the panic.",
                "A quiet mentor nudge lands in the inbox and reframes the whole task.",
            ],
            IncidentType.ExpenseSpike =>
            [
                "A surprise expense hits the account and the whole desk gets tighter.",
                "A sudden bill appears and turns the rest of the night into triage.",
                "One annoying real-world expense immediately changes the mood of the run.",
            ],
            IncidentType.RubberDuckInsight =>
            [
                "You explain the problem out loud and the shape of the fix suddenly clicks.",
                "The rubber duck gets the full rant and somehow hands back an answer.",
                "Talking through the bug untangles a part of the work you were overcomplicating.",
            ],
            IncidentType.MicroSale =>
            [
                "A tiny payout from older work lands out of nowhere and buys back breathing room.",
                "An old storefront trickle clears and the desk loosens up for a second.",
                "A forgotten little sale lands and quietly keeps the week alive.",
            ],
            IncidentType.PublishedAppSale =>
            [
                "A storefront payout from your shipped apps finally clears.",
                "One of your live releases kicks back another sale.",
                "Published work keeps humming in the background and sends over a payout.",
            ],
            IncidentType.DoomscrollSpiral =>
            [
                "One stray tab turns into a doomscroll spiral and drains the next block of energy.",
                "The browser steals the next stretch of your attention before you realize it.",
                "You lose the lane for a while and the scroll never gives anything back.",
            ],
            IncidentType.ComputerFreeze =>
            [
                "The whole machine locks up right as you hit a flow state.",
                "The computer hard-freezes and your plan for the next hour vanishes.",
                "The desktop stalls at the worst possible moment.",
            ],
            IncidentType.StreamingBinge =>
            [
                "Autoplay is already teeing up the next episode.",
                "A comfort show lines itself up and makes the night dangerously easy to lose.",
                "The next episode is one click away and the timing is terrible.",
            ],
            IncidentType.PartnerCheckIn =>
            [
                "Someone on the other side of the grind wants a little part of the night too.",
                "A relationship check-in lands right in the middle of the backlog.",
                "The run is not just code anymore, and the timing proves it.",
            ],
            _ =>
            [
                "A new match shows up online and real life suddenly competes with the backlog.",
                "A promising match shows up while you're half-looking for a distraction.",
                "A real human possibility breaks through the routine for a minute.",
            ],
        };

        return Pick(runSeed, $"{incidentId}:{incidentType}", choices);
    }

    private static string Pick(int runSeed, string key, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        var index = CreateSeed(runSeed, key) % values.Count;
        return values[index];
    }

    private static int CreateSeed(int runSeed, string key)
    {
        unchecked
        {
            var seed = runSeed == 0 ? 17 : runSeed;
            foreach (var character in key)
            {
                seed = (seed * 31) + character;
            }

            return seed & int.MaxValue;
        }
    }
}
