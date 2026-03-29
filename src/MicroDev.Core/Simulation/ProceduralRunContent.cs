namespace MicroDev.Core.Simulation;

public static class ProceduralRunContent
{
    private static readonly string[] CandidateFirstNames =
    [
        "Alex",
        "Avery",
        "Bailey",
        "Cameron",
        "Casey",
        "Devon",
        "Elliot",
        "Emerson",
        "Harper",
        "Jamie",
        "Jordan",
        "Kai",
        "Logan",
        "Morgan",
        "Parker",
        "Quinn",
        "Remy",
        "Riley",
        "Rowan",
        "Sage",
        "Sam",
        "Taylor",
        "Wren",
        "Zion",
    ];

    private static readonly string[] CandidateLastNames =
    [
        "Alden",
        "Barlow",
        "Bennett",
        "Calloway",
        "Cross",
        "Dawes",
        "Ellis",
        "Fletcher",
        "Graves",
        "Hale",
        "Keaton",
        "Lane",
        "Mercer",
        "Monroe",
        "Nash",
        "Pierce",
        "Quill",
        "Rhodes",
        "Sawyer",
        "Vale",
        "Wilder",
        "York",
    ];

    private static readonly string[] BossFirstNames =
    [
        "Clara",
        "Dominic",
        "Elena",
        "Graham",
        "Imani",
        "Jonah",
        "Leah",
        "Marcos",
        "Naomi",
        "Priya",
        "Soren",
        "Talia",
    ];

    private static readonly string[] BossLastNames =
    [
        "Bishop",
        "Carr",
        "Dalton",
        "Faraday",
        "Huxley",
        "Iverson",
        "Kerr",
        "Maddox",
        "Prescott",
        "Rosales",
        "Sterling",
        "Vargas",
    ];

    private static readonly string[] BossTitles =
    [
        "Engineering Manager",
        "Studio Director",
        "Product Lead",
        "Technical Director",
        "Delivery Manager",
        "Creative Ops Lead",
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

    private static readonly string[] CorporateCompanyPrefixes =
    [
        "Atlas",
        "Blue Harbor",
        "Granite",
        "Iron Ledger",
        "Northline",
        "Red Signal",
        "Summit",
        "Vector",
    ];

    private static readonly string[] CorporateCompanySuffixes =
    [
        "Dynamics",
        "Labs",
        "Softworks",
        "Systems",
        "Works",
        "Interactive",
    ];

    private static readonly string[] IndieStudioPrefixes =
    [
        "After Hours",
        "Campfire",
        "Greenlight",
        "Little Signal",
        "Patchwork",
        "Soft Lantern",
        "Tiny Yard",
        "Wishbone",
    ];

    private static readonly string[] IndieStudioSuffixes =
    [
        "Collective",
        "Games",
        "Studio",
        "Works",
        "Workshop",
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

    private static readonly string[] RamenNames =
    [
        "Night Shift Ramen",
        "Focus Broth Bowl",
        "Late Commit Ramen",
        "Debug Noodle Cup",
    ];

    private static readonly string[] RiceBowlNames =
    [
        "Deploy Rice Bowl",
        "Workday Grain Bowl",
        "Patch Bowl",
        "Balanced Builder Bowl",
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

    private static readonly string[] AppThemes =
    [
        "Automation",
        "Creator Tools",
        "Delivery",
        "Finance",
        "Health",
        "Productivity",
        "Social",
        "Studio Tools",
        "Wellness",
        "Workflow",
    ];

    private static readonly string[] GameThemes =
    [
        "Deckbuilder",
        "Life Sim",
        "Management",
        "Narrative",
        "Puzzle",
        "Roguelite",
        "Tactics",
        "Tycoon",
    ];

    private static readonly string[] ProjectTones =
    [
        "Cozy",
        "Grounded",
        "Lo-fi",
        "Neon",
        "Offbeat",
        "Retro",
        "Sharp",
        "Wholesome",
    ];

    private static readonly string[] ProjectPlatforms =
    [
        "Desktop",
        "Web",
        "Mobile",
        "Cross-Platform",
    ];

    private static readonly string[] AppBusinessModels =
    [
        "Contract",
        "Freemium",
        "Premium",
        "Subscription",
    ];

    private static readonly string[] GameBusinessModels =
    [
        "DLC Roadmap",
        "Early Access",
        "Free-to-Play",
        "Premium",
    ];

    private static readonly string[] AppTitlePrefixes =
    [
        "Build",
        "Desk",
        "Flow",
        "Frame",
        "Merge",
        "Minute",
        "Pocket",
        "Signal",
    ];

    private static readonly string[] AppTitleSuffixes =
    [
        "Board",
        "Forge",
        "Ledger",
        "Loop",
        "Pilot",
        "Pulse",
        "Stack",
        "Studio",
    ];

    private static readonly string[] GameTitlePrefixes =
    [
        "Garage",
        "Greenlight",
        "Medium",
        "Night",
        "Patchwork",
        "Signal",
        "Tiny",
        "Wish",
    ];

    private static readonly string[] GameTitleSuffixes =
    [
        "Arcade",
        "Story",
        "Sprint",
        "Tycoon",
        "Valley",
        "Workshop",
        "World",
        "Yard",
    ];

    private static readonly string[] CommitVerbs =
    [
        "balance",
        "clean up",
        "document",
        "finish",
        "patch",
        "polish",
        "stabilize",
        "untangle",
    ];

    private static readonly string[] CommitObjects =
    [
        "build flow",
        "collision pass",
        "economy tuning",
        "input handling",
        "save state",
        "shipping hooks",
        "tooltip layout",
        "wishlist sync",
    ];

    private static readonly string[] FreelanceClientPrefixes =
    [
        "Blue Hour",
        "Corner",
        "Field Note",
        "Northstar",
        "Patch Bay",
        "Quiet Signal",
        "Sidecar",
        "Tiny Harbor",
    ];

    private static readonly string[] FreelanceClientSuffixes =
    [
        "Apps",
        "Collective",
        "Games",
        "Group",
        "Lab",
        "Studio",
        "Works",
    ];

    public static string GetRelationshipCandidateName(int runSeed)
    {
        var firstName = Pick(runSeed, "relationship:first-name", CandidateFirstNames);
        var lastName = Pick(runSeed, "relationship:last-name", CandidateLastNames);
        return $"{firstName} {lastName}";
    }

    public static int GetRelationshipCandidateCompatibility(int runSeed)
    {
        return 58 + (CreateSeed(runSeed, "relationship:compatibility") % 41);
    }

    public static string GetSocialContactName(int runSeed, string key)
    {
        var firstName = Pick(runSeed, $"{key}:contact:first-name", CandidateFirstNames);
        var lastName = Pick(runSeed, $"{key}:contact:last-name", CandidateLastNames);
        return $"{firstName} {lastName}";
    }

    public static string GetCommunicationPrompt(int runSeed, string contactId, string name, SocialContactRole role, int bondProgress, bool isPartner)
    {
        string[] prompts = (role, isPartner, bondProgress) switch
        {
            (SocialContactRole.Date, true, >= 6) =>
            [
                $"{name} wants to hear something real about the day, not another edited status update.",
                $"{name} asks if tonight can belong to both of you for at least a little while.",
                $"{name} sends a warm check-in that makes the whole apartment-or-house dream feel less abstract.",
                $"{name} is looking for something more grounded than another drive-by update between commits.",
            ],
            (SocialContactRole.Date, true, _) =>
            [
                $"{name} is part of the run now. A quick message keeps the line alive and a call makes it feel lived in.",
                $"{name} reaches in from outside the backlog and reminds you there is more here than the next file.",
                $"{name} is around, but the relationship still needs actual attention instead of momentum alone.",
                $"{name} is not asking for perfection, just proof that the run still has room for an actual person inside it.",
            ],
            (SocialContactRole.Date, false, >= 3) =>
            [
                $"{name} is still leaning in. A thoughtful text keeps the thread warm and a call could turn it into a real plan.",
                $"{name} answers fast enough that it feels like something could actually happen if you make room for it.",
                $"{name} has become part of the week instead of just a distraction from it.",
                $"{name} is right on the edge of feeling real. A call would push it further than another vague reply.",
            ],
            (SocialContactRole.Date, false, _) =>
            [
                $"{name} is still mostly possibility. A message keeps the conversation moving, while a call risks making it real.",
                $"{name} is somewhere between a match and an actual person in your life, depending on what you do next.",
                $"{name} keeps surfacing between tasks. It is early, but not imaginary anymore.",
                $"{name} still lives in the maybe-space, which means a little attention goes a long way.",
            ],
            (SocialContactRole.Mentor, _, >= 4) =>
            [
                $"{name} has become a real mentor voice. A text gets a sharp note back, and a call can turn into an actual strategy reset.",
                $"{name} already knows your weak spots. Reaching out now usually comes back as something useful instead of generic advice.",
                $"{name} is the kind of contact who remembers what you are trying to become, not just what shipped today.",
                $"{name} has enough context now that a short call can save you from a very expensive wrong turn.",
            ],
            (SocialContactRole.Mentor, _, _) =>
            [
                $"{name} is good for blunt advice and cleaner stories when the work starts blurring together.",
                $"{name} usually answers with something annoyingly practical, which is exactly why they help.",
                $"{name} is not a rescue button, but they can still sharpen the way you talk about the work.",
                $"{name} is still a lighter-touch mentor contact, but even a small reply tends to clean up your thinking.",
            ],
            (_, _, >= 4) =>
            [
                $"{name} knows your rhythms well enough that a quick message actually lands as support instead of noise.",
                $"{name} has become part of the week. A text steadies things and a call can buy back some of the day.",
                $"{name} is the kind of friend who notices when the sprint is starting to eat you alive.",
                $"{name} has seen enough of the pattern to tell when you are about to grind yourself into a wall again.",
            ],
            _ =>
            [
                $"{name} is around and reachable. A text keeps the connection moving; a call costs more, but feels more real.",
                $"{name} is one of the few people outside the editor who still cuts through the static.",
                $"{name} could turn into a real support line if you keep showing up consistently.",
                $"{name} is not deep in the orbit yet, but this is exactly how the orbit gets built.",
            ],
        };

        return Pick(runSeed, $"contact:{contactId}:{bondProgress}:{(isPartner ? 1 : 0)}", prompts);
    }

    public static string GetBossName(int runSeed)
    {
        var firstName = Pick(runSeed, "boss:first-name", BossFirstNames);
        var lastName = Pick(runSeed, "boss:last-name", BossLastNames);
        return $"{firstName} {lastName}";
    }

    public static string GetBossTitle(int runSeed)
    {
        return Pick(runSeed, "boss:title", BossTitles);
    }

    public static BossDisposition GetBossDisposition(int runSeed, GameplayLoopMode gameplayMode = GameplayLoopMode.Interview)
    {
        var disposition = (BossDisposition)(CreateSeed(runSeed, "boss:disposition") % 4);
        if (gameplayMode != GameplayLoopMode.Corporate)
        {
            return disposition;
        }

        return disposition switch
        {
            BossDisposition.Supportive => BossDisposition.Mean,
            BossDisposition.Nice => BossDisposition.Micromanager,
            _ => disposition,
        };
    }

    public static string GetBossDispositionLabel(BossDisposition disposition)
    {
        return disposition switch
        {
            BossDisposition.Supportive => "Supportive",
            BossDisposition.Nice => "Nice",
            BossDisposition.Mean => "Mean",
            _ => "Micromanager",
        };
    }

    public static string GetBossFlavor(BossDisposition disposition, string bossName)
    {
        return disposition switch
        {
            BossDisposition.Supportive => $"{bossName} actually clears blockers when they say they will.",
            BossDisposition.Nice => $"{bossName} is warm in tone, but still expects the work to land clean.",
            BossDisposition.Mean => $"{bossName} frames everything like a pressure test and rarely lets the desk breathe.",
            _ => $"{bossName} turns every feature into a status ritual, a sync, and one more tiny approval gate.",
        };
    }

    public static string GetCoworkerName(int runSeed, string key)
    {
        var firstName = Pick(runSeed, $"{key}:coworker:first", CandidateFirstNames);
        var lastName = Pick(runSeed, $"{key}:coworker:last", CandidateLastNames);
        return $"{firstName} {lastName}";
    }

    public static string GetCompanyName(int runSeed, string key, GameplayLoopMode offerMode)
    {
        return offerMode == GameplayLoopMode.Indie
            ? $"{Pick(runSeed, $"{key}:indie:prefix", IndieStudioPrefixes)} {Pick(runSeed, $"{key}:indie:suffix", IndieStudioSuffixes)}"
            : $"{Pick(runSeed, $"{key}:corp:prefix", CorporateCompanyPrefixes)} {Pick(runSeed, $"{key}:corp:suffix", CorporateCompanySuffixes)}";
    }

    public static string[] GetFounderStudioNameChoices(int runSeed)
    {
        return
        [
            GetCompanyName(runSeed, "founder:studio:0", GameplayLoopMode.Indie),
            GetCompanyName(runSeed, "founder:studio:1", GameplayLoopMode.Indie),
            GetCompanyName(runSeed, "founder:studio:2", GameplayLoopMode.Indie),
        ];
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
            FoodChoice.Ramen => Pick(runSeed, "food:ramen:name", RamenNames),
            FoodChoice.RiceBowl => Pick(runSeed, "food:rice-bowl:name", RiceBowlNames),
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
            FoodChoice.Ramen => "A warm middle ground: quicker comfort than a home-cooked meal, steadier than pizza, and forgiving if the day is already fraying.",
            FoodChoice.RiceBowl => "Balanced fuel that trades a little raw focus for steadier recovery. Better when you need the next coding block to stay clean.",
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
            FoodChoice.Ramen =>
            [
                new(FoodOrderModifier.NoCheese, "Keep It Mild", "Too much heat turns comfort into a distraction.", true),
                new(FoodOrderModifier.SauceOnSide, "Broth On Side", "Protects the desk from becoming a noodle disaster.", true),
                new(FoodOrderModifier.SkipSoda, "Add Water", "Optional, but it helps avoid the salty crash.", false),
            ],
            FoodChoice.RiceBowl =>
            [
                new(FoodOrderModifier.NoCheese, "Light Sauce", "Keeps the bowl steady instead of turning it into a nap.", true),
                new(FoodOrderModifier.SauceOnSide, "Crunch Separate", "Prevents the whole bowl from going soggy before it lands.", true),
                new(FoodOrderModifier.SkipSoda, "Skip Soda", "Optional, but it helps the balanced meal stay balanced.", false),
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

    public static string GetFreelanceClientName(int runSeed, FreelanceGigType type)
    {
        return $"{Pick(runSeed, $"freelance:{type}:client-prefix", FreelanceClientPrefixes)} {Pick(runSeed, $"freelance:{type}:client-suffix", FreelanceClientSuffixes)}";
    }

    public static (string Title, string Brief, string FileName, IReadOnlyList<string> CodeLines) CreateFreelanceAssignment(
        int runSeed,
        ProjectBlueprint blueprint,
        FreelanceGigType type)
    {
        var themeRoot = BuildCodeLabel(blueprint.Theme, "Project");
        var toneRoot = BuildCodeLabel(blueprint.Tone, "Tone");
        var productLabel = blueprint.ProductType == ProjectProductType.Game ? "Game" : "App";

        return type switch
        {
            FreelanceGigType.UIPolishPass => CreateUiPolishAssignment(blueprint, themeRoot, toneRoot, productLabel),
            FreelanceGigType.GameplayTunePass => CreateGameplayTuneAssignment(blueprint, themeRoot, toneRoot, productLabel),
            FreelanceGigType.DataMigration => CreateDataMigrationAssignment(blueprint, themeRoot, toneRoot, productLabel),
            FreelanceGigType.PipelineRescue => CreatePipelineRescueAssignment(blueprint, themeRoot, toneRoot, productLabel),
            _ => CreateQuickBugfixAssignment(blueprint, themeRoot, toneRoot, productLabel),
        };
    }

    public static ProjectBlueprint CreateProjectBlueprint(int runSeed, GameplayLoopMode gameplayMode)
    {
        var preferredType = gameplayMode switch
        {
            GameplayLoopMode.Corporate => ProjectProductType.App,
            GameplayLoopMode.Indie => ProjectProductType.Game,
            _ => (ProjectProductType)(CreateSeed(runSeed, "project:type") % 2),
        };

        var theme = Pick(runSeed, "project:theme", GetProjectThemes(preferredType));
        var tone = Pick(runSeed, "project:tone", ProjectTones);
        var platform = Pick(runSeed, "project:platform", ProjectPlatforms);
        var businessModel = Pick(runSeed, "project:business", GetBusinessModels(preferredType));
        return BuildProjectBlueprint(runSeed, gameplayMode, preferredType, theme, tone, platform, businessModel, variantSeedOffset: 0);
    }

    public static ProjectBlueprint AdvanceProjectBlueprint(
        int runSeed,
        GameplayLoopMode gameplayMode,
        ProjectBlueprint current,
        ProjectPlanField field)
    {
        var productType = current.ProductType;
        var theme = current.Theme;
        var tone = current.Tone;
        var platform = current.Platform;
        var businessModel = current.BusinessModel;

        switch (field)
        {
            case ProjectPlanField.ProductType:
                productType = productType == ProjectProductType.App
                    ? ProjectProductType.Game
                    : ProjectProductType.App;
                theme = CycleValue(GetProjectThemes(productType), theme, fallbackIndex: CreateSeed(runSeed, $"project:switch-theme:{current.VariantSeedOffset}") % GetProjectThemes(productType).Count);
                businessModel = CycleValue(GetBusinessModels(productType), businessModel, fallbackIndex: CreateSeed(runSeed, $"project:switch-business:{current.VariantSeedOffset}") % GetBusinessModels(productType).Count);
                break;

            case ProjectPlanField.Theme:
                theme = CycleValue(GetProjectThemes(productType), theme);
                break;

            case ProjectPlanField.Tone:
                tone = CycleValue(ProjectTones, tone);
                break;

            case ProjectPlanField.Platform:
                platform = CycleValue(ProjectPlatforms, platform);
                break;

            case ProjectPlanField.BusinessModel:
                businessModel = CycleValue(GetBusinessModels(productType), businessModel);
                break;
        }

        return BuildProjectBlueprint(runSeed, gameplayMode, productType, theme, tone, platform, businessModel, current.VariantSeedOffset);
    }

    public static ProjectBlueprint RerollProjectBlueprint(int runSeed, GameplayLoopMode gameplayMode, ProjectBlueprint current)
    {
        return BuildProjectBlueprint(
            runSeed,
            gameplayMode,
            current.ProductType,
            current.Theme,
            current.Tone,
            current.Platform,
            current.BusinessModel,
            current.VariantSeedOffset + 1);
    }

    public static IReadOnlyList<string> GetProjectThemes(ProjectProductType productType)
    {
        return productType == ProjectProductType.Game
            ? GameThemes
            : AppThemes;
    }

    public static IReadOnlyList<string> GetProjectTones()
    {
        return ProjectTones;
    }

    public static IReadOnlyList<string> GetProjectPlatforms()
    {
        return ProjectPlatforms;
    }

    public static IReadOnlyList<string> GetBusinessModels(ProjectProductType productType)
    {
        return productType == ProjectProductType.Game
            ? GameBusinessModels
            : AppBusinessModels;
    }

    public static string GetPublishedAppName(int runSeed, int releaseNumber, string currentProjectName)
    {
        var stem = string.IsNullOrWhiteSpace(currentProjectName)
            ? Pick(runSeed, "publish:label", ["Patchlight", "Overnight Build", "Desk Commit", "Late Shift"])
            : currentProjectName;
        return $"{stem} Release {releaseNumber}";
    }

    public static string GetVersionControlBranchName(int runSeed, ProjectBlueprint blueprint, int branchSerial)
    {
        var baseLabel = blueprint.Theme.Replace(' ', '-').ToLowerInvariant();
        var toneLabel = blueprint.Tone.Replace(' ', '-').ToLowerInvariant();
        var suffix = 1 + (CreateSeed(runSeed, $"branch:{branchSerial}:{blueprint.Signature}") % 9);
        return $"feature/{baseLabel}-{toneLabel}-{suffix}";
    }

    public static string GetVersionControlCommitSummary(int runSeed, ProjectBlueprint blueprint, int commitCount)
    {
        var verb = Pick(runSeed, $"commit:{commitCount}:verb:{blueprint.Signature}", CommitVerbs);
        var obj = Pick(runSeed, $"commit:{commitCount}:object:{blueprint.Signature}", CommitObjects);
        return $"{verb} {obj}";
    }

    public static ActiveMergeConflict CreateMergeConflict(int runSeed, ProjectBlueprint blueprint, string branchName, int conflictCount)
    {
        var fileStem = blueprint.Theme.Replace(" ", string.Empty, StringComparison.Ordinal);
        var suffix = blueprint.ProductType == ProjectProductType.Game ? "System" : "Service";
        var fileName = $"{fileStem}{suffix}.cs";
        var summary = blueprint.ProductType == ProjectProductType.Game
            ? $"{branchName} and main both rewired the game loop around {blueprint.Theme.ToLowerInvariant()} pacing."
            : $"{branchName} and main both touched the {blueprint.Theme.ToLowerInvariant()} flow from different product directions.";

        return new ActiveMergeConflict
        {
            FileName = fileName,
            Summary = summary,
            IncomingBranchName = branchName,
            OptimalResolutionOptionIndex = CreateSeed(runSeed, $"conflict:{conflictCount}:{blueprint.Signature}") % 3,
            Severity = 1 + (CreateSeed(runSeed, $"conflict:severity:{conflictCount}:{blueprint.Signature}") % 3),
        };
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

    public static string GetIndieFundingLine(int runSeed, string incidentId, ProjectBlueprint blueprint, bool positive)
    {
        var positiveLines = new[]
        {
            $"{blueprint.Title} catches a tiny algorithm bump and a few extra people finally find it.",
            $"A small creator shout-out sends a little traffic into {blueprint.Title}.",
            $"A quiet wish-list spike gives {blueprint.Title} a rare good day.",
        };
        var negativeLines = new[]
        {
            $"Traffic falls off and {blueprint.Title} stops paying the week by itself. Freelance work suddenly matters again.",
            $"No funding arrives and {blueprint.Title} goes quiet. The runway tightens and contract work looks smarter.",
            $"{blueprint.Title} hits a slow patch. The storefront cools off and survival leans back toward gigs.",
        };

        return positive
            ? Pick(runSeed, $"{incidentId}:funding:up", positiveLines)
            : Pick(runSeed, $"{incidentId}:funding:down", negativeLines);
    }

    public static string GetIncidentDescription(int runSeed, string incidentId, IncidentType incidentType)
    {
        string[] choices = incidentType switch
        {
            IncidentType.CatInterruption =>
            [
                "A desk distraction slams into the session and immediately starts competing with the keyboard.",
                "Something in the room decides your workflow is optional and your focus is not.",
                "The run picks up a fresh real-world distraction right on top of the editor.",
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
            IncidentType.BossCheckIn =>
            [
                "Corporate life pulls you into one more status ritual.",
                "Your boss wants an update now, not when the work is actually ready.",
                "A management sync appears and turns the sprint into theater.",
            ],
            IncidentType.CoworkerInterruption =>
            [
                "A co-worker suddenly needs context, help, or emotional labor right in the middle of your block.",
                "Office hours turn into hallway-drive-by work and the schedule bends around someone else's urgency.",
                "A co-worker interruption lands and turns a clean coding stretch into people management.",
            ],
            IncidentType.IndieFundingSwing =>
            [
                "The storefront graph twitches and the whole indie plan suddenly feels volatile again.",
                "Funding chatter shifts and your runway now depends on whether the next wobble is up or down.",
                "An indie revenue beat lands and reminds you how unstable self-funded work can be.",
            ],
            IncidentType.FounderNaming =>
            [
                "If you are going to bootstrap a studio from this desk, it needs a name before it needs a logo.",
                "The company exists as soon as you decide what to call it.",
                "The founder route starts with picking the name that will sit on the pitch deck, storefront page, and tax headache.",
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

    private static ProjectBlueprint BuildProjectBlueprint(
        int runSeed,
        GameplayLoopMode gameplayMode,
        ProjectProductType productType,
        string theme,
        string tone,
        string platform,
        string businessModel,
        int variantSeedOffset)
    {
        var keyBase = $"{productType}:{theme}:{tone}:{platform}:{businessModel}:{variantSeedOffset}";
        var prefixes = productType == ProjectProductType.Game ? GameTitlePrefixes : AppTitlePrefixes;
        var suffixes = productType == ProjectProductType.Game ? GameTitleSuffixes : AppTitleSuffixes;
        var prefix = Pick(runSeed, $"title:prefix:{keyBase}", prefixes);
        var suffix = Pick(runSeed, $"title:suffix:{keyBase}", suffixes);
        var title = $"{tone} {prefix} {suffix}";
        var publishMultiplier = productType == ProjectProductType.Game ? 1.06 : 0.98;
        var saleMultiplier = productType == ProjectProductType.Game ? 1.08 : 0.99;

        if (theme.Contains("Tycoon", StringComparison.Ordinal) ||
            title.Contains("Garage", StringComparison.Ordinal) ||
            title.Contains("Greenlight", StringComparison.Ordinal) ||
            title.Contains("Medium", StringComparison.Ordinal))
        {
            publishMultiplier += 0.03;
            saleMultiplier += 0.04;
        }

        if (businessModel.Contains("Subscription", StringComparison.Ordinal) ||
            businessModel.Contains("DLC", StringComparison.Ordinal) ||
            businessModel.Contains("Free-to-Play", StringComparison.Ordinal))
        {
            saleMultiplier += 0.04;
        }

        if (businessModel.Contains("Premium", StringComparison.Ordinal))
        {
            publishMultiplier += 0.04;
        }

        if (gameplayMode == GameplayLoopMode.Corporate && productType == ProjectProductType.App)
        {
            publishMultiplier += 0.03;
        }

        if (gameplayMode == GameplayLoopMode.Indie && productType == ProjectProductType.Game)
        {
            saleMultiplier += 0.05;
        }

        if (gameplayMode == GameplayLoopMode.Founder)
        {
            publishMultiplier += productType == ProjectProductType.App ? 0.02 : 0.04;
            saleMultiplier += 0.06;
        }

        return new ProjectBlueprint
        {
            ProductType = productType,
            Theme = theme,
            Tone = tone,
            Platform = platform,
            BusinessModel = businessModel,
            VariantSeedOffset = variantSeedOffset,
            Title = title,
            Pitch = productType == ProjectProductType.Game
                ? $"{tone} {theme.ToLowerInvariant()} game for {platform.ToLowerInvariant()} with a {businessModel.ToLowerInvariant()} plan."
                : $"{tone} {theme.ToLowerInvariant()} app for {platform.ToLowerInvariant()} built around a {businessModel.ToLowerInvariant()} model.",
            PublishIncomeMultiplier = publishMultiplier,
            SaleIncomeMultiplier = saleMultiplier,
        };
    }

    private static string CycleValue(IReadOnlyList<string> values, string current, int? fallbackIndex = null)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        var index = IndexOf(values, current);
        if (index < 0)
        {
            var resolvedFallback = Math.Clamp(fallbackIndex ?? 0, 0, values.Count - 1);
            return values[resolvedFallback];
        }

        return values[(index + 1) % values.Count];
    }

    private static int IndexOf(IReadOnlyList<string> values, string current)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (string.Equals(values[index], current, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
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

    private static (string Title, string Brief, string FileName, IReadOnlyList<string> CodeLines) CreateQuickBugfixAssignment(
        ProjectBlueprint blueprint,
        string themeRoot,
        string toneRoot,
        string productLabel)
    {
        var className = $"{toneRoot}{themeRoot}BugfixPatch";
        return (
            blueprint.ProductType == ProjectProductType.Game
                ? $"{blueprint.Theme} interaction bugfix"
                : $"{blueprint.Theme} workflow bugfix",
            blueprint.ProductType == ProjectProductType.Game
                ? $"A client build keeps dropping state during a {blueprint.Theme.ToLowerInvariant()} interaction. Patch the hot path without widening the bug."
                : $"A client flow keeps losing state inside a {blueprint.Theme.ToLowerInvariant()} tool. Clean up the edge case without rewriting the whole feature.",
            $"{className}.cs",
            [
                "using System;",
                "",
                $"namespace Freelance.Contracts.{productLabel};",
                "",
                $"public static class {className}",
                "{",
                "    public static int ClampCursor(int index, int count)",
                "    {",
                "        if (count <= 0)",
                "        {",
                "            return 0;",
                "        }",
                "",
                "        if (index < 0)",
                "        {",
                "            return 0;",
                "        }",
                "",
                "        return Math.Min(index, count - 1);",
                "    }",
                "",
                "    public static bool ShouldKeepSelection(bool isVisible, bool wasExplicitlyChosen)",
                "    {",
                "        return isVisible || wasExplicitlyChosen;",
                "    }",
                "}",
            ]);
    }

    private static (string Title, string Brief, string FileName, IReadOnlyList<string> CodeLines) CreateUiPolishAssignment(
        ProjectBlueprint blueprint,
        string themeRoot,
        string toneRoot,
        string productLabel)
    {
        var className = $"{toneRoot}{themeRoot}LayoutRules";
        var panelLabel = blueprint.ProductType == ProjectProductType.Game ? "HUD" : "dashboard";
        return (
            $"{blueprint.Theme} UI polish pass",
            $"The client wants the {panelLabel} to stop crowding itself on narrow layouts. Tighten the spacing rules without flattening the whole look.",
            $"{className}.cs",
            [
                "using Microsoft.Xna.Framework;",
                "",
                $"namespace Freelance.Contracts.{productLabel};",
                "",
                $"public static class {className}",
                "{",
                "    public static Rectangle Inset(Rectangle bounds, int padding)",
                "    {",
                "        return new Rectangle(",
                "            bounds.X + padding,",
                "            bounds.Y + padding,",
                "            Math.Max(0, bounds.Width - (padding * 2)),",
                "            Math.Max(0, bounds.Height - (padding * 2)));",
                "    }",
                "",
                "    public static int GetGap(int width)",
                "    {",
                "        return width < 960 ? 10 : 16;",
                "    }",
                "}",
            ]);
    }

    private static (string Title, string Brief, string FileName, IReadOnlyList<string> CodeLines) CreateGameplayTuneAssignment(
        ProjectBlueprint blueprint,
        string themeRoot,
        string toneRoot,
        string productLabel)
    {
        var className = $"{toneRoot}{themeRoot}PacingProfile";
        var actorLabel = blueprint.ProductType == ProjectProductType.Game ? "encounter" : "workflow";
        return (
            blueprint.ProductType == ProjectProductType.Game
                ? $"{blueprint.Theme} encounter tuning"
                : $"{blueprint.Theme} interaction tuning",
            $"The client wants a steadier {actorLabel} curve. Smooth out the pacing without stripping away the identity of the feature.",
            $"{className}.cs",
            [
                "using System;",
                "",
                $"namespace Freelance.Contracts.{productLabel};",
                "",
                $"public sealed class {className}",
                "{",
                "    public int MinimumBeatDelayMs { get; init; } = 220;",
                "",
                "    public int MaximumBeatDelayMs { get; init; } = 540;",
                "",
                "    public int ResolveDelay(double intensity)",
                "    {",
                "        var clamped = Math.Clamp(intensity, 0d, 1d);",
                "        return (int)Math.Round(MaximumBeatDelayMs - ((MaximumBeatDelayMs - MinimumBeatDelayMs) * clamped));",
                "    }",
                "}",
            ]);
    }

    private static (string Title, string Brief, string FileName, IReadOnlyList<string> CodeLines) CreateDataMigrationAssignment(
        ProjectBlueprint blueprint,
        string themeRoot,
        string toneRoot,
        string productLabel)
    {
        var className = $"{toneRoot}{themeRoot}MigrationMap";
        return (
            $"{blueprint.Theme} save migration",
            "A small production data shift is blocking rollout. Wire the legacy keys to the new shape before support tickets pile up.",
            $"{className}.cs",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Freelance.Contracts.{productLabel};",
                "",
                $"public static class {className}",
                "{",
                "    public static IReadOnlyDictionary<string, string> Create()",
                "    {",
                "        return new Dictionary<string, string>",
                "        {",
                "            [\"theme.mode\"] = \"project.mode\",",
                "            [\"theme.seed\"] = \"project.seed\",",
                "            [\"theme.profile\"] = \"project.profile\",",
                "        };",
                "    }",
                "}",
            ]);
    }

    private static (string Title, string Brief, string FileName, IReadOnlyList<string> CodeLines) CreatePipelineRescueAssignment(
        ProjectBlueprint blueprint,
        string themeRoot,
        string toneRoot,
        string productLabel)
    {
        var className = $"{toneRoot}{themeRoot}BuildManifest";
        return (
            $"{blueprint.Theme} pipeline rescue",
            "The release process is shedding artifacts and nobody trusts the deploy lane. Rebuild the manifest logic before the whole contract turns into an incident room.",
            $"{className}.cs",
            [
                "using System.Collections.Generic;",
                "",
                $"namespace Freelance.Contracts.{productLabel};",
                "",
                $"public sealed class {className}",
                "{",
                "    private readonly List<string> _artifacts = [];",
                "",
                "    public void Add(string artifactName)",
                "    {",
                "        if (!_artifacts.Contains(artifactName))",
                "        {",
                "            _artifacts.Add(artifactName);",
                "        }",
                "    }",
                "",
                "    public string[] Snapshot()",
                "    {",
                "        return _artifacts.ToArray();",
                "    }",
                "}",
            ]);
    }

    private static string BuildCodeLabel(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var parts = value
            .Split([' ', '-', '+', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static part =>
            {
                if (part.Length == 0)
                {
                    return string.Empty;
                }

                return part.Length == 1
                    ? part.ToUpperInvariant()
                    : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant();
            })
            .Where(static part => part.Length > 0);

        var joined = string.Concat(parts);
        return string.IsNullOrWhiteSpace(joined) ? fallback : joined;
    }
}
