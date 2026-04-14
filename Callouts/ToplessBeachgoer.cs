using Adam69Callouts.Common;
using CalloutInterfaceAPI;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Topless Beachgoer", CalloutProbability.Medium, "Report of topless beachgoer", "Code 2", "LSPD")]
    public class ToplessBeachgoer : Callout
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

        private enum BeachScenario
        {
            ForeignTourist,          // Tourist from country where topless beaches are legal
            Influencer,              // Social media influencer doing photoshoot
            Sunbather,               // Sunbathing topless, unaware it's illegal
            CompliantApologetic,     // Immediately covers up and apologizes
            DefiantRefusal           // Refuses to cover up, argues about rights
        }
        private static BeachScenario scenario;

        // Female ped models for beach scenarios
        private static readonly string[] touristPeds = new string[]
        {
            "a_f_y_tourist_01",         // Young tourist female
            "a_f_y_tourist_02",         // Tourist female variant
            "a_f_m_tourist_01",         // Middle-aged tourist
            "a_f_y_beach_01",           // Beach female
            "a_f_y_bevhills_01",        // Beverly Hills female (well-off tourist)
            "a_f_y_bevhills_02"         // Beverly Hills female variant
        };

        private static readonly string[] influencerPeds = new string[]
        {
            "a_f_y_bevhills_01",        // Beverly Hills female (influencer type)
            "a_f_y_bevhills_02",        // Beverly Hills female variant
            "a_f_y_fitness_01",         // Fitness female (fit influencer)
            "a_f_y_fitness_02",         // Fitness female variant
            "a_f_y_beach_01",           // Beach female
            "a_f_y_hipster_01",         // Hipster female
            "a_f_y_hipster_02"          // Hipster female variant
        };

        private static readonly string[] sunbatherPeds = new string[]
        {
            "a_f_y_beach_01",           // Beach female
            "a_f_m_beach_01",           // Middle-aged beach female
            "a_f_y_tourist_01",         // Tourist female
            "a_f_y_fitness_01",         // Fitness female
            "a_f_m_bodybuild_01",       // Bodybuilder female
            "a_f_y_bikerhipster_01"     // Biker hipster female
        };

        private static readonly string[] compliantPeds = new string[]
        {
            "a_f_y_business_01",        // Business female (professional)
            "a_f_m_business_02",        // Middle-aged business female
            "a_f_y_beach_01",           // Beach female
            "a_f_y_tourist_01",         // Tourist female
            "a_f_m_bevhills_01",        // Beverly Hills female
            "a_f_m_tourist_01"          // Middle-aged tourist
        };

        private static readonly string[] defiantPeds = new string[]
        {
            "a_f_y_hipster_01",         // Hipster female (rebellious)
            "a_f_y_hipster_02",         // Hipster female variant
            "a_f_y_hippie_01",          // Hippie female
            "a_f_y_beach_01",           // Beach female
            "a_f_m_bodybuild_01",       // Bodybuilder female (confident)
            "a_f_y_bevhills_04"         // Beverly Hills female (entitled)
        };

        private static readonly string[] witnessPeds = new string[]
        {
            "a_f_m_tourist_01",         // Tourist female (concerned witness)
            "a_f_y_tourist_01",         // Young tourist female
            "a_m_m_tourist_01",         // Tourist male
            "a_f_m_bodybuild_01",       // Bodybuilder female
            "a_f_y_business_01",        // Business female (professional witness)
            "a_m_m_farmer_01",          // Farmer (conservative citizen)
            "a_f_m_eastsa_01",          // East SA female
            "a_m_m_eastsa_01",          // East SA male
            "a_f_y_fitness_01",         // Fitness female
            "a_m_y_cyclist_01",         // Cyclist
            "a_m_m_beach_01",           // Beach male
            "a_f_m_beach_01"            // Beach female
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            // Vespucci Beach locations
            List<Vector3> beachLocations = new()
            {
                new Vector3(-1279.82f, -1501.65f, 4.37f),    // Vespucci Beach - main beach area
                new Vector3(-1316.45f, -1523.89f, 4.38f),    // Vespucci Beach - near lifeguard tower
                new Vector3(-1238.77f, -1572.34f, 4.35f),    // Vespucci Beach - south section
                new Vector3(-1355.23f, -1490.12f, 4.39f),    // Vespucci Beach - north section
                new Vector3(-1298.56f, -1545.67f, 4.36f)     // Vespucci Beach - volleyball court area
            };

            spawnpoint = LocationChooser.ChooseNearestLocation(beachLocations);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);

            // Randomly select scenario
            scenario = (BeachScenario)random.Next(Enum.GetValues(typeof(BeachScenario)).Length);

            CalloutInterfaceAPI.Functions.SendMessage(this, "Report of topless beachgoer at Vespucci Beach");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_DISTURBING_THE_PEACE_01", spawnpoint);
            CalloutMessage = "Topless Beachgoer Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Topless Beachgoer callout accepted! Scenario: " + scenario.ToString());
                LoggingManager.Log("Adam69 Callouts [LOG]: Topless Beachgoer callout accepted! Scenario: " + scenario.ToString());
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Topless Beachgoer", "~b~Dispatch~w~: Report of a topless female sunbathing at Vespucci Beach. Multiple complaints received. Respond ~y~Code 2~w~.");

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

                malefemale = "Ma'am"; // Always female for this callout

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
            witnessInteractionComplete = false;

            return base.OnCalloutAccepted();
        }

        private string GetSuspectModelForScenario()
        {
            switch (scenario)
            {
                case BeachScenario.ForeignTourist:
                    return touristPeds[random.Next(touristPeds.Length)];
                case BeachScenario.Influencer:
                    return influencerPeds[random.Next(influencerPeds.Length)];
                case BeachScenario.Sunbather:
                    return sunbatherPeds[random.Next(sunbatherPeds.Length)];
                case BeachScenario.CompliantApologetic:
                    return compliantPeds[random.Next(compliantPeds.Length)];
                case BeachScenario.DefiantRefusal:
                    return defiantPeds[random.Next(defiantPeds.Length)];
                default:
                    return "a_f_y_beach_01"; // Fallback
            }
        }

        private void SetInitialSuspectBehavior()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (scenario)
            {
                case BeachScenario.ForeignTourist:
                case BeachScenario.Sunbather:
                    // Laying down sunbathing
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_sunbathe@female@back@base"), "base", -1f, AnimationFlags.Loop);
                    break;

                case BeachScenario.Influencer:
                    // Taking selfies/posing
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_text_read_base", -1f, AnimationFlags.Loop);
                    break;

                case BeachScenario.CompliantApologetic:
                    // Sitting nervously
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", -1f, AnimationFlags.Loop);
                    break;

                case BeachScenario.DefiantRefusal:
                    // Standing confidently
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", -1f, AnimationFlags.Loop);
                    break;
            }
        }

        private void SetScenarioSpawnPoints()
        {
            switch (scenario)
            {
                case BeachScenario.ForeignTourist:
                    suspectHeading = 180.0f;
                    witnessSpawn = spawnpoint.Around2D(8.0f);
                    witnessHeading = 0.0f;
                    break;
                case BeachScenario.Influencer:
                    suspectHeading = 90.0f;
                    witnessSpawn = spawnpoint.Around2D(10.0f);
                    witnessHeading = 270.0f;
                    break;
                case BeachScenario.Sunbather:
                    suspectHeading = 45.0f;
                    witnessSpawn = spawnpoint.Around2D(7.0f);
                    witnessHeading = 225.0f;
                    break;
                case BeachScenario.CompliantApologetic:
                    suspectHeading = 135.0f;
                    witnessSpawn = spawnpoint.Around2D(9.0f);
                    witnessHeading = 315.0f;
                    break;
                case BeachScenario.DefiantRefusal:
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

            // Witness interaction
            if (!witnessInteractionComplete && witness != null && witness.Exists() && witness.IsValid())
            {
                if (MainPlayer.DistanceTo(witness) <= 10f)
                {
                    if (Settings.HelpMessages && counter == 0)
                    {
                        Game.DisplayHelp("Press ~y~H~w~ to speak with the witness.");
                    }

                    if (Game.IsKeyDown(System.Windows.Forms.Keys.H))
                    {
                        HandleWitnessInteraction();
                    }
                }
            }

            // Suspect interaction
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

        private void HandleWitnessInteraction()
        {
            if (witness == null || !witness.Exists() || !witness.IsValid()) return;

            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(witness, MainPlayer, -1);
            Game.DisplaySubtitle($"~g~Witness~w~: Officer, thank you for responding! That woman over there has been sunbathing topless for over an hour. There are families with children here!");
            GameFiber.Sleep(4000);
            Game.DisplaySubtitle($"~b~You~w~: Thank you for the report, {witnessGender}. I'll handle this. Please step back.");
            GameFiber.Sleep(3000);

            if (witnessBlip != null && witnessBlip.Exists())
            {
                witnessBlip.Delete();
            }

            witnessInteractionComplete = true;
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
                        case BeachScenario.ForeignTourist:
                        case BeachScenario.Sunbather:
                            PerformSunbathingBehavior();
                            break;

                        case BeachScenario.Influencer:
                            PerformInfluencerBehavior();
                            break;

                        case BeachScenario.CompliantApologetic:
                            PerformCompliantBehavior();
                            break;

                        case BeachScenario.DefiantRefusal:
                            PerformDefiantBehavior();
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

        private void PerformSunbathingBehavior()
        {
            // Relaxed sunbathing, occasionally adjusting position
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(8000, 15000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 4);
                switch (action)
                {
                    case 1:
                        // Sunbathe on back
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_sunbathe@female@back@base"), "base", 10.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Sunbathe on front
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_sunbathe@female@front@base"), "base", 10.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Sitting up, drinking water
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", 8.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformInfluencerBehavior()
        {
            // Taking photos, posing, checking phone
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(5000, 10000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 5);
                switch (action)
                {
                    case 1:
                        // Taking selfie
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_text_read_base", 8.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Posing for camera
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", 6.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Checking photos on phone
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_call_listen_base", 8.0f, AnimationFlags.None);
                        break;
                    case 4:
                        // Adjusting hair/appearance
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_safehousevic_01@"), "vic_ig_4_idle_a", 6.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformCompliantBehavior()
        {
            // Nervous, covering self with towel or hands
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(4000, 7000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 3);
                switch (action)
                {
                    case 1:
                        // Sitting nervously
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", 8.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Looking around nervously
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@idle_a"), "idle_a", 6.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void PerformDefiantBehavior()
        {
            // Confident stance, possibly confrontational
            while (!scenarioTriggered && suspect != null && suspect.Exists() && suspect.IsValid())
            {
                GameFiber.Sleep(random.Next(5000, 9000));

                if (suspect == null || !suspect.Exists() || !suspect.IsValid()) break;

                int action = random.Next(1, 4);
                switch (action)
                {
                    case 1:
                        // Standing confident
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", 8.0f, AnimationFlags.None);
                        break;
                    case 2:
                        // Arms crossed defiant
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_hang_out_street@female_arms_crossed@base"), "base", 8.0f, AnimationFlags.None);
                        break;
                    case 3:
                        // Sunbathing defiantly
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_sunbathe@female@back@base"), "base", 8.0f, AnimationFlags.None);
                        break;
                }
            }
        }

        private void HandleScenarioInteraction()
        {
            switch (scenario)
            {
                case BeachScenario.ForeignTourist:
                    HandleForeignTourist();
                    break;
                case BeachScenario.Influencer:
                    HandleInfluencer();
                    break;
                case BeachScenario.Sunbather:
                    HandleSunbather();
                    break;
                case BeachScenario.CompliantApologetic:
                    HandleCompliantApologetic();
                    break;
                case BeachScenario.DefiantRefusal:
                    HandleDefiantRefusal();
                    break;
            }
        }

        private void HandleForeignTourist()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~You~w~: Ma'am, LSPD. I need you to cover up, please.");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_confused", 3.0f, AnimationFlags.None);
                    Game.DisplaySubtitle("~r~Suspect~w~: *with foreign accent* Cover? But... this is normal beach in my country. What is problem?");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: In California, you can't sunbathe topless. It's against the law. You need to put your top back on.");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_oh_shit", 3.0f, AnimationFlags.None);
                    Game.DisplaySubtitle("~r~Suspect~w~: Oh! I am so sorry! I did not know this! In Europe, this is completely normal!");
                    break;
                case 5:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I will cover up right away. Please forgive me, officer. I truly did not know.");
                    GameFiber.Sleep(2000);
                    Game.DisplaySubtitle("~g~The tourist appears genuinely unaware and cooperative. Consider issuing a warning.");
                    GameFiber.Sleep(2000);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Cultural Misunderstanding", "~b~Issue warning and educate on local laws. No citation necessary.");
                    scenarioTriggered = true;
                    break;
            }
        }

        private void HandleInfluencer()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_text_read_base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~You~w~: Ma'am, put the phone down. You need to cover yourself immediately.");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_point", 3.0f, AnimationFlags.None);
                    Game.DisplaySubtitle("~r~Suspect~w~: Hold on, I'm getting like thousands of likes! This is going viral! #FreeTheNipple!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: I don't care about your social media. Cover up now or you'll be cited for indecent exposure.");
                    break;
                case 4:
                    int influencerOutcome = random.Next(1, 3);
                    if (influencerOutcome == 1)
                    {
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_why", 3.0f, AnimationFlags.None);
                        Game.DisplaySubtitle("~r~Suspect~w~: Ugh, fine! You're killing my engagement rate! But whatever!");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~g~The influencer reluctantly complies. Consider citation for public disturbance.");
                        UltimateBackup.API.Functions.callCode2Backup(suspect);
                        scenarioTriggered = true;
                    }
                    else
                    {
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_text_read_base", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~r~Suspect~w~: You can't stop me! This is art! I'm live streaming this police harassment!");
                        GameFiber.Sleep(1000);
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspect Non-Compliant", "~r~Subject refusing lawful orders!");
                        UltimateBackup.API.Functions.callCode2Backup(suspect);
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        private void HandleSunbather()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~You~w~: Ma'am, I need you to cover your chest. We've received multiple complaints.");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_shrug_hard", 3.0f, AnimationFlags.None);
                    Game.DisplaySubtitle("~r~Suspect~w~: Oh my god, seriously? I'm just trying to avoid tan lines! Nobody's even around!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: There are families here. This is a public beach. You need to follow the law.");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: *sighs* Fine. I'll put my top back on. This is ridiculous though.");
                    GameFiber.Sleep(2000);
                    Game.DisplaySubtitle("~g~Subject is complying. Issue warning for indecent exposure.");
                    GameFiber.Sleep(2000);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Subject Compliant", "~b~Issue verbal warning. Educate on beach regulations.");
                    scenarioTriggered = true;
                    break;
            }
        }

        private void HandleCompliantApologetic()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_picnic@female@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~You~w~: Ma'am, I need to speak with you about your attire.");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_oh_shit", 3.0f, AnimationFlags.None);
                    Game.DisplaySubtitle("~r~Suspect~w~: Oh god, officer, I'm so sorry! I already covered up when I saw people complaining!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Good. But I need to ask - why were you sunbathing topless in the first place?");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I wasn't thinking. I've been so stressed and just wanted to relax. I should have known better.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: I appreciate your cooperation and honesty. I'm going to issue you a warning this time.");
                    GameFiber.Sleep(2000);
                    Game.DisplaySubtitle("~r~Suspect~w~: Thank you so much, officer. It won't happen again, I promise.");
                    GameFiber.Sleep(2000);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Verbal Warning Issued", "~b~Subject is cooperative and remorseful. No citation needed.");
                    scenarioTriggered = true;
                    break;
            }
        }

        private void HandleDefiantRefusal()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_hang_out_street@female_arms_crossed@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~You~w~: Ma'am, you need to cover yourself. You're in violation of indecent exposure laws.");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("gestures@f@standing@casual"), "gesture_no_way", 3.0f, AnimationFlags.None);
                    Game.DisplaySubtitle("~r~Suspect~w~: This is my body! I have the right to be comfortable! This law is sexist and oppressive!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: I understand your perspective, but the law is the law. Cover up or you'll be cited.");
                    break;
                case 4:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_02", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: No! Men can be shirtless, why can't women? I'm not covering up! Arrest me if you want!");
                    break;
                case 5:
                    int defiantOutcome = random.Next(1, 3);
                    if (defiantOutcome == 1)
                    {
                        Game.DisplaySubtitle("~g~Subject is refusing lawful orders. Prepare citation for indecent exposure.");
                        GameFiber.Sleep(2000);
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Citation Required", "~r~Subject refusing compliance. Issue citation for PC 314.");
                        UltimateBackup.API.Functions.callCode2Backup(suspect);
                        scenarioTriggered = true;
                    }
                    else
                    {
                        suspect.Tasks.ReactAndFlee(MainPlayer);
                        if (suspectBlip != null && suspectBlip.Exists())
                            suspectBlip.Color = System.Drawing.Color.Yellow;
                        Game.DisplaySubtitle("~r~Suspect~w~: You'll never take me alive! *runs away*");
                        GameFiber.Sleep(1000);
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspect Fleeing", "~r~Subject is fleeing the scene!");
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        public override void End()
        {
            base.End();
            CleanupEntities();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Topless Beachgoer callout ended.");
                LoggingManager.Log("Adam69 Callouts [LOG]: Topless Beachgoer callout ended.");
            }
        }

        private void CleanupEntities()
        {
            if (suspectBlip != null && suspectBlip.Exists())
                suspectBlip.Delete();

            if (witnessBlip != null && witnessBlip.Exists())
                witnessBlip.Delete();

            if (suspect != null && suspect.Exists())
                suspect.Dismiss();

            if (witness != null && witness.Exists())
                witness.Dismiss();

            if (pursuit != null)
                LSPD_First_Response.Mod.API.Functions.ForceEndPursuit(pursuit);
        }
    }
}