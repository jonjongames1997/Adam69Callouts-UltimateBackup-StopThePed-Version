using CalloutInterfaceAPI;
using Adam69Callouts.Common;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] LSIA Airport Incident", CalloutProbability.Medium, "Incident reported at LSIA", "Code 2", "LSPD")]
    public class LSIAAirportIncident : Callout
    {
        private static Ped suspect;
        private static Ped victim;
        private static Ped witness;
        private static Vehicle suspectVehicle;
        private static Blip suspectBlip;
        private static Blip victimBlip;
        private static Blip witnessBlip;
        private static Blip vehicleBlip;
        private static Vector3 spawnpoint;
        private static Vector3 victimSpawn;
        private static Vector3 witnessSpawn;
        private static Vector3 vehicleSpawn;
        private static float suspectHeading;
        private static float victimHeading;
        private static float witnessHeading;
        private static float vehicleHeading;
        private static int counter;
        private static int witnessCounter;
        private static string malefemale;
        private static string victimGender;
        private static string witnessGender;
        private static bool scenarioTriggered;
        private static bool pursuitCreated;
        private static bool witnessInteractionComplete;
        private static LHandle pursuit;
        private static readonly string[] weaponList = new string[] { "WEAPON_PISTOL", "WEAPON_COMBATPISTOL", "WEAPON_KNIFE", "WEAPON_BAT", "WEAPON_PUMPSHOTGUN" };

        private enum AirportScenario
        {
            CargoTheft,
            DomesticDispute,
            TerroristThreat,
            DrunkPassenger,
            SmugglingAttempt
        }
        private static AirportScenario scenario;
        private static readonly Random random = new Random();

        public override bool OnBeforeCalloutDisplayed()
        {
            List<Vector3> airportLocations = new()
            {
                new(-1042.48f, -2746.84f, 13.87f),  // Main terminal entrance
                new(-1336.72f, -3044.30f, 13.94f),  // Cargo area
                new(-1271.77f, -3380.63f, 13.94f),  // Hangar area
                new(-1115.91f, -2883.68f, 13.95f),  // Parking lot
                new(-989.24f, -2954.71f, 13.95f)    // Security checkpoint area
            };

            spawnpoint = LocationChooser.ChooseNearestLocation(airportLocations);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);

            // Randomly select scenario
            scenario = (AirportScenario)random.Next(Enum.GetValues(typeof(AirportScenario)).Length);

            switch (scenario)
            {
                case AirportScenario.CargoTheft:
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of cargo theft in progress at LSIA");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_GRAND_THEFT_AUTO_02", spawnpoint);
                    CalloutMessage = "Cargo Theft at LSIA";
                    break;
                case AirportScenario.DomesticDispute:
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Domestic dispute reported at LSIA terminal");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_ASSAULT_ON_A_CIVILIAN", spawnpoint);
                    CalloutMessage = "Domestic Dispute at LSIA";
                    break;
                case AirportScenario.TerroristThreat:
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Possible terrorist threat at LSIA - Code 3 response");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_BRANDISHING_WEAPON_03", spawnpoint);
                    CalloutMessage = "Terrorist Threat at LSIA";
                    break;
                case AirportScenario.DrunkPassenger:
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Intoxicated passenger causing disturbance at LSIA");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_DISTURBING_THE_PEACE_01", spawnpoint);
                    CalloutMessage = "Drunk Passenger at LSIA";
                    break;
                case AirportScenario.SmugglingAttempt:
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Suspected smuggling operation at LSIA cargo area");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_SUSPICIOUS_ACTIVITY_01", spawnpoint);
                    CalloutMessage = "Smuggling Attempt at LSIA";
                    break;
            }

            CalloutPosition = spawnpoint;
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: LSIA Airport Incident callout accepted! Scenario: " + scenario.ToString());
                LoggingManager.Log("Adam69 Callouts [LOG]: LSIA Airport Incident callout accepted! Scenario: " + scenario.ToString());
            }

            // Adjust spawn points based on scenario
            SetScenarioSpawnPoints();

            // Determine code response
            string responseCode = scenario == AirportScenario.TerroristThreat ? "~r~Code 3~w~" : "~y~Code 2~w~";
            string audioCode = scenario == AirportScenario.TerroristThreat ? "Adam69Callouts_Respond_Code_3_Audio" : "Adam69Callouts_Respond_Code_2_Audio";

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~LSIA Airport Incident", $"~b~Dispatch~w~: Incident confirmed at Los Santos International Airport. Respond {responseCode}.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(audioCode);

            // Spawn entities based on scenario
            SpawnScenarioEntities();

            counter = 0;
            witnessCounter = 0;
            scenarioTriggered = false;
            pursuitCreated = false;
            witnessInteractionComplete = false;

            return base.OnCalloutAccepted();
        }

        private void SetScenarioSpawnPoints()
        {
            switch (scenario)
            {
                case AirportScenario.CargoTheft:
                    spawnpoint = new(-1336.72f, -3044.30f, 13.94f);
                    suspectHeading = 60.0f;
                    vehicleSpawn = new(-1340.12f, -3050.45f, 13.94f);
                    vehicleHeading = 58.23f;
                    witnessSpawn = new(-1330.24f, -3038.76f, 13.94f);
                    witnessHeading = 240.0f;
                    break;
                case AirportScenario.DomesticDispute:
                    spawnpoint = new(-1042.48f, -2746.84f, 13.87f);
                    suspectHeading = 120.0f;
                    victimSpawn = new(-1038.32f, -2749.12f, 13.87f);
                    victimHeading = 300.0f;
                    witnessSpawn = new(-1045.67f, -2743.21f, 13.87f);
                    witnessHeading = 180.0f;
                    break;
                case AirportScenario.TerroristThreat:
                    spawnpoint = new(-989.24f, -2954.71f, 13.95f);
                    suspectHeading = 270.0f;
                    vehicleSpawn = new(-995.43f, -2957.89f, 13.95f);
                    vehicleHeading = 268.12f;
                    break;
                case AirportScenario.DrunkPassenger:
                    spawnpoint = new(-1115.91f, -2883.68f, 13.95f);
                    suspectHeading = 45.0f;
                    witnessSpawn = new(-1112.34f, -2880.12f, 13.95f);
                    witnessHeading = 225.0f;
                    break;
                case AirportScenario.SmugglingAttempt:
                    spawnpoint = new(-1271.77f, -3380.63f, 13.94f);
                    suspectHeading = 150.0f;
                    vehicleSpawn = new(-1275.23f, -3385.12f, 13.94f);
                    vehicleHeading = 148.76f;
                    witnessSpawn = new(-1268.45f, -3375.89f, 13.94f);
                    witnessHeading = 330.0f;
                    break;
            }
        }

        private void SpawnScenarioEntities()
        {
            // Spawn suspect
            suspect = new Ped(spawnpoint)
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

                // Set initial behavior based on scenario
                switch (scenario)
                {
                    case AirportScenario.CargoTheft:
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@prop_human_bum_bin@base"), "base", -1f, AnimationFlags.Loop);
                        break;
                    case AirportScenario.DomesticDispute:
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_02", -1f, AnimationFlags.Loop);
                        break;
                    case AirportScenario.TerroristThreat:
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@heists@fleeca_bank@ig_7_jetski_owner"), "owner_idle", -1f, AnimationFlags.Loop);
                        break;
                    case AirportScenario.DrunkPassenger:
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                        break;
                    case AirportScenario.SmugglingAttempt:
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", -1f, AnimationFlags.Loop);
                        break;
                }
            }

            // Spawn victim for domestic dispute
            if (scenario == AirportScenario.DomesticDispute)
            {
                victim = new Ped(victimSpawn)
                {
                    IsPersistent = true,
                    BlockPermanentEvents = true
                };

                if (victim != null && victim.Exists() && victim.IsValid())
                {
                    victim.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                    victimBlip = victim.AttachBlip();
                    victimBlip.Color = System.Drawing.Color.Blue;
                    victimBlip.Alpha = 0.75f;
                    victimGender = victim.IsMale ? "Sir" : "Ma'am";
                }
            }

            // Spawn witness for applicable scenarios
            if (scenario == AirportScenario.CargoTheft || scenario == AirportScenario.DrunkPassenger || scenario == AirportScenario.SmugglingAttempt || scenario == AirportScenario.DomesticDispute)
            {
                witness = new Ped(witnessSpawn)
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
            }

            // Spawn vehicle for applicable scenarios
            if (scenario == AirportScenario.CargoTheft || scenario == AirportScenario.TerroristThreat || scenario == AirportScenario.SmugglingAttempt)
            {
                suspectVehicle = new Vehicle(vehicleSpawn)
                {
                    IsPersistent = true,
                    IsStolen = true
                };

                if (suspectVehicle != null && suspectVehicle.Exists() && suspectVehicle.IsValid())
                {
                    vehicleBlip = suspectVehicle.AttachBlip();
                    vehicleBlip.Color = System.Drawing.Color.Orange;
                    vehicleBlip.Alpha = 0.75f;
                }
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

            // Witness interaction
            if (witness != null && witness.Exists() && witness.IsValid() && !witnessInteractionComplete)
            {
                if (MainPlayer.DistanceTo(witness) <= 12f)
                {
                    if (Settings.HelpMessages && witnessCounter == 0)
                    {
                        Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to speak with the witness. Press ~g~T~w~ to interact with suspect.");
                    }

                    if (Game.IsKeyDown(System.Windows.Forms.Keys.T))
                    {
                        witnessCounter++;
                        HandleWitnessInteraction();
                    }
                }
            }

            if (suspect != null && suspect.Exists() && suspect.IsValid())
            {
                if (MainPlayer.DistanceTo(suspect) <= 15f && !scenarioTriggered)
                {
                    if (Settings.HelpMessages && counter == 0 && witnessCounter == 0)
                    {
                        Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to interact with the suspect.");
                    }

                    if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    {
                        counter++;
                        HandleScenarioInteraction();
                    }
                }

                // Auto-trigger for terrorist threat
                if (scenario == AirportScenario.TerroristThreat && MainPlayer.DistanceTo(suspect) <= 10f && !scenarioTriggered)
                {
                    scenarioTriggered = true;
                    TriggerTerroristThreat();
                }
            }

            // Request backup hotkey
            if (Game.IsKeyDown(System.Windows.Forms.Keys.B) && !Game.IsKeyDownRightNow(System.Windows.Forms.Keys.B))
            {
                PolicingRedefined.API.BackupDispatchAPI.RequestCode3Backup();
                Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Dispatch:", "Additional units en route to LSIA.");
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
            switch (scenario)
            {
                case AirportScenario.CargoTheft:
                    HandleWitnessCargoTheft();
                    break;
                case AirportScenario.DomesticDispute:
                    HandleWitnessDomesticDispute();
                    break;
                case AirportScenario.DrunkPassenger:
                    HandleWitnessDrunkPassenger();
                    break;
                case AirportScenario.SmugglingAttempt:
                    HandleWitnessSmuggling();
                    break;
            }
        }

        private void HandleWitnessCargoTheft()
        {
            if (witness == null || !witness.Exists() || !witness.IsValid()) return;

            switch (witnessCounter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(witness, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {witnessGender}, did you call about suspicious activity?");
                    break;
                case 2:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("gestures@m@standing@casual"), "gesture_point", -1f, AnimationFlags.None);
                    Game.DisplaySubtitle($"~g~Witness~w~: Yes, officer! That {malefemale} over there has been loading boxes into that vehicle for the past hour.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Did you see what was in the boxes?");
                    break;
                case 4:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~g~Witness~w~: No, but they're not wearing any airport employee uniform, and they looked nervous when I walked by.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: Thank you. Please stay here while I investigate.");
                    GameFiber.Sleep(1500);
                    Game.DisplaySubtitle("~g~Witness statement recorded. Confront the suspect.");
                    witnessInteractionComplete = true;
                    if (witnessBlip != null && witnessBlip.Exists())
                        witnessBlip.Color = System.Drawing.Color.Gray;
                    break;
            }
        }

        private void HandleWitnessDomesticDispute()
        {
            if (witness == null || !witness.Exists() || !witness.IsValid()) return;

            switch (witnessCounter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(witness, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {witnessGender}, what happened here?");
                    break;
                case 2:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("gestures@m@standing@casual"), "gesture_point", -1f, AnimationFlags.None);
                    Game.DisplaySubtitle($"~g~Witness~w~: Officer, those two have been arguing for at least 20 minutes. It started getting really heated.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Did either of them get physical?");
                    break;
                case 4:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle($"~g~Witness~w~: The {malefemale} tried to grab the other person's arm, but they pulled away. I thought it was going to escalate so I called you.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: Did you hear what they were arguing about?");
                    break;
                case 6:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle($"~g~Witness~w~: Something about a relationship and a vacation. The {malefemale} seemed really angry and kept saying they were cheated on.");
                    break;
                case 7:
                    Game.DisplaySubtitle("~b~You~w~: Alright, thank you. Please wait here while I sort this out.");
                    GameFiber.Sleep(1500);
                    Game.DisplaySubtitle("~g~Witness statement recorded. Speak with both parties.");
                    witnessInteractionComplete = true;
                    if (witnessBlip != null && witnessBlip.Exists())
                        witnessBlip.Color = System.Drawing.Color.Gray;
                    break;
            }
        }

        private void HandleWitnessDrunkPassenger()
        {
            if (witness == null || !witness.Exists() || !witness.IsValid()) return;

            switch (witnessCounter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(witness, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {witnessGender}, I understand you reported a disturbance?");
                    break;
                case 2:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("gestures@m@standing@casual"), "gesture_point", -1f, AnimationFlags.None);
                    Game.DisplaySubtitle($"~g~Witness~w~: Yes! That {malefemale} is completely wasted. They were stumbling around, almost knocked over a child.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Did they say anything to you?");
                    break;
                case 4:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~g~Witness~w~: They were slurring and yelling nonsense. I think they missed their flight and have been drinking at the bar.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: Understood. Stay back, I'll handle this.");
                    GameFiber.Sleep(1500);
                    Game.DisplaySubtitle("~g~Witness statement recorded. Deal with the intoxicated passenger.");
                    witnessInteractionComplete = true;
                    if (witnessBlip != null && witnessBlip.Exists())
                        witnessBlip.Color = System.Drawing.Color.Gray;
                    break;
            }
        }

        private void HandleWitnessSmuggling()
        {
            if (witness == null || !witness.Exists() || !witness.IsValid()) return;

            switch (witnessCounter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(witness, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {witnessGender}, what did you see?");
                    break;
                case 2:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("gestures@m@standing@casual"), "gesture_point", -1f, AnimationFlags.None);
                    Game.DisplaySubtitle($"~g~Witness~w~: That {malefemale} has been hanging around this restricted area for hours. They keep looking around like they're waiting for someone.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Did you see them with any cargo or packages?");
                    break;
                case 4:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~g~Witness~w~: I saw them put something in the trunk of that vehicle earlier. It looked heavy, and they were trying to hide it with blankets.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: Did you see what it was?");
                    break;
                case 6:
                    witness.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~g~Witness~w~: No, but I work here and I know they shouldn't be in this area. Something's definitely wrong.");
                    break;
                case 7:
                    Game.DisplaySubtitle("~b~You~w~: Thank you for the information. Please step back to a safe distance.");
                    GameFiber.Sleep(1500);
                    Game.DisplaySubtitle("~g~Witness statement recorded. Investigate the suspect and vehicle.");
                    witnessInteractionComplete = true;
                    if (witnessBlip != null && witnessBlip.Exists())
                        witnessBlip.Color = System.Drawing.Color.Gray;
                    break;
            }
        }

        private void HandleScenarioInteraction()
        {
            switch (scenario)
            {
                case AirportScenario.CargoTheft:
                    HandleCargoTheftInteraction();
                    break;
                case AirportScenario.DomesticDispute:
                    HandleDomesticDisputeInteraction();
                    break;
                case AirportScenario.TerroristThreat:
                    HandleTerroristThreatInteraction();
                    break;
                case AirportScenario.DrunkPassenger:
                    HandleDrunkPassengerInteraction();
                    break;
                case AirportScenario.SmugglingAttempt:
                    HandleSmugglingInteraction();
                    break;
            }
        }

        private void HandleCargoTheftInteraction()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle("~b~You~w~: LSPD! Step away from the cargo!");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: I work here! I'm just doing my job!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Can I see your employee ID?");
                    break;
                case 4:
                    int cargoOutcome = random.Next(1, 4);
                    if (cargoOutcome == 1)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: I... uh... forgot it at home!");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~r~Suspect~w~: Screw this!");
                        suspect.Tasks.ReactAndFlee(MainPlayer);
                        if (suspectBlip != null && suspectBlip.Exists())
                            suspectBlip.Color = System.Drawing.Color.Yellow;
                        scenarioTriggered = true;
                    }
                    else if (cargoOutcome == 2)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: You want my ID? Come get it, cop!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.FightAgainst(MainPlayer);
                        if (suspect != null && suspect.Exists() && suspect.IsValid())
                            SafeInventory.SafeGiveWeapon(suspect, weaponList[random.Next(weaponList.Length)], 500, true);
                        scenarioTriggered = true;
                    }
                    else
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: Here you go, officer. Sorry for the confusion.");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~g~The suspect's ID checks out. They're a legitimate employee.");
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        private void HandleDomesticDisputeInteraction()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    if (victim != null && victim.Exists() && victim.IsValid())
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(victim, MainPlayer, -1);
                    Game.DisplaySubtitle("~b~You~w~: Alright, break it up! What's going on here?");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_02", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle($"~r~Suspect~w~: This {victimGender} cheated on me! We were supposed to go on vacation together!");
                    break;
                case 3:
                    if (victim != null && victim.Exists() && victim.IsValid())
                    {
                        victim.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle($"~p~Victim~w~: That's not true! We broke up weeks ago! This {malefemale} is stalking me!");
                    }
                    break;
                case 4:
                    Game.DisplaySubtitle($"~b~You~w~: Calm down, both of you. {malefemale}, I need you to step back and calm down.");
                    break;
                case 5:
                    int disputeOutcome = random.Next(1, 4);
                    if (disputeOutcome == 1)
                    {
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~r~Suspect~w~: You're right, officer. I'm sorry. I'll leave.");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~g~The suspect agrees to leave peacefully.");
                        scenarioTriggered = true;
                    }
                    else if (disputeOutcome == 2)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: No! I'm not going anywhere!");
                        GameFiber.Sleep(1000);
                        if (victim != null && victim.Exists() && victim.IsValid())
                        {
                            suspect.Tasks.FightAgainst(victim);
                            PolicingRedefined.API.PedAPI.IsPedResistingRightNow(suspect);
                        }
                        scenarioTriggered = true;
                    }
                    else
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: Screw you, cop! You don't understand!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.FightAgainst(MainPlayer);
                        if (suspect != null && suspect.Exists() && suspect.IsValid())
                            SafeInventory.SafeGiveWeapon(suspect, "WEAPON_KNIFE", 0, true);
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        private void HandleTerroristThreatInteraction()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle("~b~You~w~: LSPD! Show me your hands!");
                    break;
                case 2:
                    Game.DisplaySubtitle("~r~Suspect~w~: You'll never take me alive, infidel!");
                    GameFiber.Sleep(1500);
                    TriggerTerroristThreat();
                    break;
            }
        }

        private void TriggerTerroristThreat()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            scenarioTriggered = true;
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~r~ALERT", "~w~Terrorist Threat", "~r~SHOTS FIRED! REQUEST BACKUP!");

            suspect.Tasks.FightAgainst(MainPlayer);
            suspect.Armor = 2500;

            if (suspect != null && suspect.Exists() && suspect.IsValid())
            {
                SafeInventory.SafeGiveWeapon(suspect, "WEAPON_COMBATPISTOL", 500, true);
            }

            PolicingRedefined.API.BackupDispatchAPI.RequestPanicBackup();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("OFFICERS_UNDER_FIRE");
        }

        private void HandleDrunkPassengerInteraction()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: {malefemale}, have you been drinking?");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: *slurring* Noooo, occifer! I'm totally... totally fine!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: You're clearly intoxicated. I'm going to need you to come with me.");
                    break;
                case 4:
                    int drunkOutcome = random.Next(1, 4);
                    if (drunkOutcome == 1)
                    {
                        suspect.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~r~Suspect~w~: *slurring* Okaaay... I'll go quietly...");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~g~The suspect complies. Take them into custody.");
                        scenarioTriggered = true;
                    }
                    else if (drunkOutcome == 2)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: *slurring* You can't tell me what to do!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.ReactAndFlee(MainPlayer);
                        if (suspectBlip != null && suspectBlip.Exists())
                            suspectBlip.Color = System.Drawing.Color.Yellow;
                        scenarioTriggered = true;
                    }
                    else
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: *aggressive* Get away from me!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.FightAgainst(MainPlayer);
                        PolicingRedefined.API.PedAPI.IsPedResistingRightNow(suspect);
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        private void HandleSmugglingInteraction()
        {
            if (suspect == null || !suspect.Exists() || !suspect.IsValid()) return;

            switch (counter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: LSPD. {malefemale}, what are you doing out here?");
                    break;
                case 2:
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Suspect~w~: Just... waiting for a friend. Is that a crime?");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: In a restricted cargo area? I'm going to need to search that vehicle.");
                    break;
                case 4:
                    int smugglingOutcome = random.Next(1, 4);
                    if (smugglingOutcome == 1)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: Go ahead. You won't find anything.");
                        GameFiber.Sleep(2000);
                        Game.DisplaySubtitle("~g~The vehicle is clean. The suspect checks out.");
                        scenarioTriggered = true;
                    }
                    else if (smugglingOutcome == 2)
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: I don't think so! I'm out of here!");
                        GameFiber.Sleep(1000);
                        if (suspectVehicle != null && suspectVehicle.Exists() && suspectVehicle.IsValid())
                        {
                            suspect.Tasks.EnterVehicle(suspectVehicle, -1, EnterVehicleFlags.AllowJacking);
                            GameFiber.Sleep(3000);
                            if (!pursuitCreated && suspect.IsInVehicle(suspectVehicle, false))
                            {
                                pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, suspect);
                                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                PolicingRedefined.API.BackupDispatchAPI.RequestPursuitBackup();
                                pursuitCreated = true;
                            }
                        }
                        scenarioTriggered = true;
                    }
                    else
                    {
                        Game.DisplaySubtitle("~r~Suspect~w~: You'll have to catch me first!");
                        GameFiber.Sleep(1000);
                        suspect.Tasks.FightAgainst(MainPlayer);
                        if (suspect != null && suspect.Exists() && suspect.IsValid())
                            SafeInventory.SafeGiveWeapon(suspect, weaponList[random.Next(weaponList.Length)], 500, true);
                        scenarioTriggered = true;
                    }
                    break;
            }
        }

        public override void End()
        {
            CleanupEntities();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~LSIA Airport Incident", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "LSIA is secure. You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: LSIA Airport Incident callout is CODE 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: LSIA Airport Incident callout is CODE 4!");
            }
        }

        private void CleanupEntities()
        {
            if (suspect != null && suspect.Exists()) suspect.Dismiss();
            if (victim != null && victim.Exists()) victim.Dismiss();
            if (witness != null && witness.Exists()) witness.Dismiss();
            if (suspectVehicle != null && suspectVehicle.Exists()) suspectVehicle.Dismiss();
            if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();
            if (victimBlip != null && victimBlip.Exists()) victimBlip.Delete();
            if (witnessBlip != null && witnessBlip.Exists()) witnessBlip.Delete();
            if (vehicleBlip != null && vehicleBlip.Exists()) vehicleBlip.Delete();
        }
    }
}