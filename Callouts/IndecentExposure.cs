using CalloutInterfaceAPI;
using Adam69Callouts.Common;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Indecent Exposure", CalloutProbability.Medium, "Reports of indecent exposure", "Code 2", "LSPD")]
    public class IndecentExposure : Callout
    {
        private static Ped suspect;
        private static Ped witness;
        private static Blip suspectBlip;
        private static Blip witnessBlip;
        private static Vector3 spawnpoint;
        private static Vector3 witnessSpawn;
        private static float suspectHeading;
        private static float witnessHeading;
        private static int counter;
        private static string malefemale;
        private static string witnessGender;
        private static bool scenarioTriggered;
        private static bool pursuitCreated;
        private static bool behaviorStarted;
        private static bool witnessInteractionComplete;
        private static LHandle pursuit;
        private static readonly Random random = new Random();

        private enum ExposureScenario
        {
            MentalHealthCrisis,      // Disrobed due to mental health episode
            Exhibitionist,           // Intentional indecent exposure
            IntoxicatedDisrobed,     // Removed clothing while intoxicated
            CompliantRemorseful,     // Caught and immediately compliant
            FleeWhenConfronted       // Flees when police arrive
        }
        private static ExposureScenario scenario;

        // Ped models for each scenario type
        private static readonly string[] mentalHealthPeds = new string[]
        {
            "a_m_m_tramp_01",           // Homeless male - disheveled appearance
            "a_m_o_tramp_01",           // Older homeless male
            "a_m_m_skater_01",          // Young disheveled male
            "a_f_m_tramp_01",           // Homeless female
            "a_m_m_beach_01",           // Beach male (often associated with erratic behavior)
            "a_m_y_beach_03"            // Young beach male
        };

        private static readonly string[] exhibitionistPeds = new string[]
        {
            "a_m_y_hipster_01",         // Young hipster male
            "a_m_y_hipster_02",         // Hipster male variant
            "a_m_m_beach_01",           // Beach male
            "a_m_y_beach_01",           // Young beach male
            "a_f_y_hippie_01",          // Hippie female
            "a_m_y_hippy_01",           // Hippie male
            "a_m_y_musclbeac_01",       // Muscle beach male (exhibitionist type)
            "a_m_y_musclbeac_02"        // Muscle beach male variant
        };

        private static readonly string[] intoxicatedPeds = new string[]
        {
            "a_m_y_clubcust_01",        // Club customer male (party/drunk type)
            "a_m_y_clubcust_02",        // Club customer variant
            "a_m_y_clubcust_03",        // Club customer variant
            "a_f_y_clubcust_01",        // Club customer female
            "a_m_m_beach_02",           // Beach male (party type)
            "a_m_y_stbla_01",           // Street male (party type)
            "a_m_y_stwhi_01",           // Street white male (party type)
            "a_f_y_beach_01"            // Beach female (party type)
        };

        private static readonly string[] compliantPeds = new string[]
        {
            "a_m_m_business_01",        // Business male (respectable citizen)
            "a_m_y_business_01",        // Young business male
            "a_m_y_business_02",        // Business male variant
            "a_f_m_business_02",        // Business female
            "a_m_m_genfat_01",          // General fat male (average citizen)
            "a_m_m_genfat_02",          // General fat male variant
            "a_m_y_genstreet_01",       // General street male
            "a_f_m_fatcult_01"          // Average female
        };

        private static readonly string[] fleePeds = new string[]
        {
            "a_m_y_runner_01",          // Runner male (athletic/can flee fast)
            "a_m_y_runner_02",          // Runner variant
            "a_m_y_skater_01",          // Skater male (young, agile)
            "a_m_y_skater_02",          // Skater variant
            "a_m_y_stwhi_02",           // Street white male (young, agile)
            "a_m_y_stbla_02",           // Street black male (young, agile)
            "a_m_y_beach_02",           // Beach male (athletic)
            "a_f_y_runner_01"           // Female runner
        };

        private static readonly string[] witnessPeds = new string[]
        {
            "a_f_m_tourist_01",         // Tourist female (shocked witness)
            "a_f_y_tourist_01",         // Young tourist female
            "a_m_m_tourist_01",         // Tourist male
            "a_f_m_bodybuild_01",       // Bodybuilder female (confident witness)
            "a_f_y_business_01",        // Business female (professional witness)
            "a_m_m_farmer_01",          // Farmer (concerned citizen)
            "a_f_m_eastsa_01",          // East SA female (neighborhood resident)
            "a_m_m_eastsa_01",          // East SA male (neighborhood resident)
            "a_f_y_fitness_01",         // Fitness female (morning jogger witness)
            "a_m_y_cyclist_01"          // Cyclist (passing witness)
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            List<Vector3> exposureLocations = new()
            {
                new(291.45f, -1782.34f, 28.05f),     // Public park
                new(-265.78f, -977.42f, 31.22f),     // Downtown area
                new(1137.89f, -982.45f, 45.67f),     // Residential area
                new(-1486.23f, -378.56f, 40.23f),    // Beach area
                new(-1042.48f, -2746.84f, 13.87f)    // LSIA terminal area
            };

            spawnpoint = LocationChooser.ChooseNearestLocation(exposureLocations);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);

            // Randomly select scenario
            scenario = (ExposureScenario)random.Next(Enum.GetValues(typeof(ExposureScenario)).Length);

            CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of indecent exposure in a public area");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_DISTURBING_THE_PEACE_01", spawnpoint);
            CalloutMessage = "Indecent Exposure Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Indecent Exposure callout accepted! Scenario: " + scenario.ToString());
                LoggingManager.Log("Adam69 Callouts [LOG]: Indecent Exposure callout accepted! Scenario: " + scenario.ToString());
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Indecent Exposure", "~b~Dispatch~w~: Suspect reported exposing themselves in public. Respond ~y~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            // Set spawn points based on scenario
            SetScenarioSpawnPoints();

            // Spawn suspect with appropriate ped model based on scenario
            string suspectModel = GetSuspectModelForScenario();
            suspect = new Ped(suspectModel, spawnpoint, suspectHeading)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            if (suspect != null && suspect.Exists() && suspect.IsValid())
            {
                suspectBlip = suspect.AttachBlip();
                suspectBlip.Color = System.Drawing.Color.Red;
                suspectBlip.IsRouteEnabled = true;
                suspectBlip.Alpha = 0.75f;

                malefemale = suspect.IsMale ? "Sir" : "Ma'am";

                // Set initial suspect behavior
                SetInitialSuspectBehavior();
            }

            // Spawn witness with random witness ped model
            string witnessModel = witnessPeds[random.Next(witnessPeds.Length)];
            witness = new Ped(witnessModel, witnessSpawn, witnessHeading)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            if (witness != null && witness.Exists() && witness.IsValid())
            {
                witness.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                witnessBlip = witness.AttachBlip();
                witnessBlip.Color = System.Drawing.Color.Green;
                witnessBlip.Alpha = 0.5f;
                witnessGender = witness.IsMale ? "Sir" : "Ma'am";
            }

            counter = 0;
            scenarioTriggered = false;
            pursuitCreated = false;
            behaviorStarted = false;

            return base.OnCalloutAccepted();
        }

        private string GetSuspectModelForScenario()
        {
            switch (scenario)
            {
                case ExposureScenario.MentalHealthCrisis:
                    return mentalHealthPeds[random.Next(mentalHealthPeds.Length)];
                case ExposureScenario.Exhibitionist:
                    return exhibitionistPeds[random.Next(exhibitionistPeds.Length)];
                case ExposureScenario.IntoxicatedDisrobed:
                    return intoxicatedPeds[random.Next(intoxicatedPeds.Length)];
                case ExposureScenario.CompliantRemorseful:
                    return compliantPeds[random.Next(compliantPeds.Length)];
                case ExposureScenario.FleeWhenConfronted:
                    return fleePeds[random.Next(fleePeds.Length)];
                default:
                    return "a_m_m_skater_01"; // Fallback
            }
        }

        private void SetInitialSuspectBehavior()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (scenario)
            {
                case ExposureScenario.MentalHealthCrisis:
                    // Erratic pacing, talking to themselves
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    break;

                case ExposureScenario.Exhibitionist:
                    // Standing confidently, possibly waving at people
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", -1f, AnimationFlags.Loop);
                    break;

                case ExposureScenario.IntoxicatedDisrobed:
                    // Stumbling, swaying, clearly drunk
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                    break;

                case ExposureScenario.CompliantRemorseful:
                    // Standing nervously, looking down
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    break;

                case ExposureScenario.FleeWhenConfronted:
                    // Looking around nervously, ready to bolt
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@heists@fleeca_bank@ig_7_jetski_owner"), "owner_idle", -1f, AnimationFlags.Loop);
                    break;
            }
        }

        private void SetScenarioSpawnPoints()
        {
            switch (scenario)
            {
                case ExposureScenario.MentalHealthCrisis:
                    suspectHeading = 45.0f;
                    witnessSpawn = spawnpoint.Around2D(8.0f);
                    witnessHeading = 225.0f;
                    break;
                case ExposureScenario.Exhibitionist:
                    suspectHeading = 90.0f;
                    witnessSpawn = spawnpoint.Around2D(10.0f);
                    witnessHeading = 270.0f;
                    break;
                case ExposureScenario.IntoxicatedDisrobed:
                    suspectHeading = 180.0f;
                    witnessSpawn = spawnpoint.Around2D(7.0f);
                    witnessHeading = 0.0f;
                    break;
                case ExposureScenario.CompliantRemorseful:
                    suspectHeading = 135.0f;
                    witnessSpawn = spawnpoint.Around2D(9.0f);
                    witnessHeading = 315.0f;
                    break;
                case ExposureScenario.FleeWhenConfronted:
                    suspectHeading = 270.0f;
                    witnessSpawn = spawnpoint.Around2D(12.0f);
                    witnessHeading = 90.0f;
                    break;
            }
        }

        public override void OnCalloutNotAccepted()
        {
            CleanupEntities();
            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            // Start realistic behaviors when player is approaching
            if (!behaviorStarted && suspect != null && suspect.Exists() && suspect.IsValid() && MainPlayer.DistanceTo(suspect) <= 30f)
            {
                behaviorStarted = true;
                StartRealisticBehaviors();
            }

            if (suspect != null && suspect.Exists() && suspect.IsValid())
            {
                if (MainPlayer.DistanceTo(suspect) <= 15f && !scenarioTriggered)
                {
                    if (Settings.HelpMessages && counter == 0)
                    {
                        Game.DisplayHelp("Press ~y~Y~w~ to interact with the suspect.");
                    }

                    if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    {
                        counter++;
                        HandleScenarioInteraction();
                    }
                }
            }

            // Request backup hotkey
            if (Game.IsKeyDown(System.Windows.Forms.Keys.B) && !Game.IsKeyDownRightNow(System.Windows.Forms.Keys.B))
            {
                UltimateBackup.API.Functions.callCode2Backup();
                Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Dispatch:", "Additional units en route to your location.");
            }

            // End callout
            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                End();
            }

            if (MainPlayer.IsDead)
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("MISSION FAILED!", "You'll get 'em next time!", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }
                End();
            }
        }

        private void StartRealisticBehaviors()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            GameFiber.StartNew(() =>
            {
                try
                {
                    switch (scenario)
                    {
                        case ExposureScenario.MentalHealthCrisis:
                            PerformMentalHealthBehavior();
                            break;

                        case ExposureScenario.Exhibitionist:
                            PerformExhibitionistBehavior();
                            break;

                        case ExposureScenario.IntoxicatedDisrobed:
                            PerformIntoxicatedBehavior();
                            break;

                        case ExposureScenario.CompliantRemorseful:
                            PerformCompliantBehavior();
                            break;

                        case ExposureScenario.FleeWhenConfronted:
                            PerformNervousBehavior();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (Settings.EnableLogs)
                    {
                        Game.LogTrivial("[Adam69 Callouts LOG]: Error in StartRealisticBehaviors: " + ex.Message);
                        LoggingManager.Log("Adam69 Callouts [LOG]: Error in StartRealisticBehaviors: " + ex.Message);
                    }
                }
            });
        }

        private void PerformMentalHealthBehavior()
        {
            // Pacing, talking to themselves, paranoid movements
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(3000, 6000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 5);
                switch (action)
                {
                    case 1:
                        // Pace around nervously
                        Vector3 wanderPoint = suspect.Position.Around2D(3.0f);
                        suspect.Tasks.FollowNavigationMeshToPosition(wanderPoint, suspect.Heading, 1.0f, 0.5f);
                        break;
                    case 2:
                        // Look around paranoid
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_c", 5.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Cover face/head with hands
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@mp_player_intcelebrationmale@face_palm"), "face_palm", 5.0f, AnimationFlags.None);
                        break;
                    case 4:
                        // Stand still looking down
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", 5.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformExhibitionistBehavior()
        {
            // Confident stance, waving at pedestrians, attention-seeking
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(4000, 7000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 4);
                switch (action)
                {
                    case 1:
                        // Wave at people/cars
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("friends@frj@ig_1"), "wave_a", 5.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Flex/show off
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_muscle_flex@arms_at_side@base"), "base", 5.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Stand confident
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", 5.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformIntoxicatedBehavior()
        {
            // Stumbling, swaying, occasional sitting/falling
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(3000, 6000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 5);
                switch (action)
                {
                    case 1:
                        // Drunk stumble
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", 5.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Nearly fall over
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_fall_over", 5.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Lean against wall (if near one) or just sway
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@drunk@idle_a"), "idle_a", 5.0f, AnimationFlags.None);
                        break;
                    case 4:
                        // Vomiting animation
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("timetable@tracy@ig_7@base"), "base", 5.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformCompliantBehavior()
        {
            // Nervous, ashamed body language, avoiding eye contact
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(4000, 7000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 4);
                switch (action)
                {
                    case 1:
                        // Look down ashamed
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", 5.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Nervous fidgeting
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@idle_a"), "idle_a", 5.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Cover face in shame
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@mp_player_intcelebrationmale@face_palm"), "face_palm", 5.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformNervousBehavior()
        {
            // Constantly looking around, ready to run
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(2000, 4000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 4);
                switch (action)
                {
                    case 1:
                        // Look around nervously
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@heists@fleeca_bank@ig_7_jetski_owner"), "owner_idle", 3.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Pace nervously
                        Vector3 pacePoint = suspect.Position.Around2D(2.0f);
                        suspect.Tasks.FollowNavigationMeshToPosition(pacePoint, suspect.Heading, 2.0f, 0.5f);
                        break;
                    case 3:
                        // Check surroundings
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", 3.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void HandleScenarioInteraction()
        {
            switch (scenario)
            {
                case ExposureScenario.MentalHealthCrisis:
                    HandleMentalHealthCrisis();
                    break;
                case ExposureScenario.Exhibitionist:
                    HandleExhibitionist();
                    break;
                case ExposureScenario.IntoxicatedDisrobed:
                    HandleIntoxicatedDisrobed();
                    break;
                case ExposureScenario.CompliantRemorseful:
                    HandleCompliantRemorseful();
                    break;
                case ExposureScenario.FleeWhenConfronted:
                    HandleFleeWhenConfronted();
                    break;
            }
        }

        private void HandleMentalHealthCrisis()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {malefemale}, we need to talk. Can you put some clothes on?");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_02", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: The government put chips in the clothes! They're tracking me!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Nobody is tracking you. I'm here to help. Do you have family I can call?");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I... I don't know... I'm scared...");
                    break;
                case 5:
                    Game.DisplaySubtitle("~g~The suspect appears to be experiencing a severe mental health crisis. Consider calling for mental health crisis intervention team.");
                    GameFiber.Sleep(2000);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Mental Health Crisis", "~b~Recommend 5150 hold and psychiatric evaluation.");
                    scenarioTriggered = true;
                    break;
            }
        }

        private void HandleExhibitionist()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD! {malefemale}, you need to cover yourself immediately!");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I have the right to express myself! This is freedom!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: No, you're exposing yourself to the public. That's illegal. Put your clothes on now.");
                    break;
                case 4:
                    int exhibitionistOutcome = random.Next(1, 4);
                    if (exhibitionistOutcome == 1)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: Fine! You're oppressing me but whatever!");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~g~The suspect reluctantly complies. Process them for indecent exposure.");
                        UltimateBackup.API.Functions.callCode2Backup(suspect);
                        scenarioTriggered = true;
                    }
                    else if (exhibitionistOutcome == 2)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: Catch me if you can, pig!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.ReactAndFlee(MainPlayer);
                        if (suspectBlip != null && suspectBlip.Exists())
                            suspectBlip.Color = System.Drawing.Color.Yellow;
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspect Fleeing", "~r~Suspect is fleeing on foot!");
                        scenarioTriggered = true;
                    }
                    else
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: You can't tell me what to do!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.FightAgainst(MainPlayer);
                        UltimateBackup.API.Functions.callCode3Backup(suspect);
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspect Resisting", "~r~Suspect is physically resisting!");
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        private void HandleIntoxicatedDisrobed()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: {malefemale}, you're naked in public. Are you okay?");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: *slurring* It was sooo hot... I just... took 'em off...");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: You're clearly intoxicated. Where are your clothes?");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: *slurring* I dunno... somewhere... Can I go home now?");
                    break;
                case 5:
                    Game.DisplaySubtitle("~g~The suspect is severely intoxicated and disoriented. Consider protective custody and medical evaluation.");
                    GameFiber.Sleep(2000);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Public Intoxication", "~b~Subject requires medical evaluation for alcohol poisoning.");
                    scenarioTriggered = true;
                    break;
            }
        }

        private void HandleCompliantRemorseful()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {malefemale}, we received multiple complaints about your behavior.");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I know, officer. I'm so sorry. I don't know what I was thinking.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: What you did is a serious crime. You exposed yourself to the public, including potentially children.");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I understand. I'm ready to cooperate fully. I'll accept whatever consequences come.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~g~The suspect is fully compliant and remorseful. Process them for indecent exposure.");
                    GameFiber.Sleep(2000);
                    UltimateBackup.API.Functions.callCode2Backup(suspect);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Indecent Exposure", "~b~Subject is cooperative. Run warrant check and process accordingly.");
                    scenarioTriggered = true;
                    break;
            }
        }

        private void HandleFleeWhenConfronted()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD! {malefemale}, don't move!");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_02", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: Oh crap! The cops!");
                    GameFiber.Sleep(1000);
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Stop right there! You're under arrest!");
                    suspect.Tasks.ReactAndFlee(MainPlayer);
                    if (suspectBlip != null && suspectBlip.Exists())
                        suspectBlip.Color = System.Drawing.Color.Yellow;
                    GameFiber.Sleep(2000);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspect Fleeing", "~r~Suspect is fleeing on foot from the scene!");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("SUSPECT_LAST_SEEN_ON_FOOT");
                    scenarioTriggered = true;
                    break;
            }
        }

        public override void End()
        {
            CleanupEntities();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Indecent Exposure", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Indecent Exposure callout is CODE 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Indecent Exposure callout is CODE 4!");
            }
        }

        private void CleanupEntities()
        {
            if (suspect != null && suspect.Exists()) suspect.Dismiss();
            if (witness != null && witness.Exists()) witness.Dismiss();
            if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();
            if (witnessBlip != null && witnessBlip.Exists()) witnessBlip.Delete();
        }
    }
}