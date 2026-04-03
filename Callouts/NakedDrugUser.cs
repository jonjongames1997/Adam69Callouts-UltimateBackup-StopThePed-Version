using CalloutInterfaceAPI;
using System;
using System.Drawing;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Naked Drug User", CalloutProbability.Medium, "Naked person on drugs causing a disturbance", "Code 2", "LEO")]
    public class NakedDrugUser : Callout
    {
        private static readonly Random rng = new Random();

        private static readonly string[] malePedModels = new string[] 
        { 
            "a_m_y_hipster_01", "a_m_m_beach_01", "a_m_y_beach_01", "a_m_y_runner_01", 
            "a_m_y_surfer_01", "a_m_y_skater_01", "a_m_m_runner_01" 
        };

        private static readonly string[] femalePedModels = new string[] 
        { 
            "a_f_y_beach_01", "a_f_y_runner_01", "a_f_y_fitness_01", 
            "a_f_m_beach_01", "a_f_y_yoga_01" 
        };

        private static readonly string[] crazyAnimations = new string[]
        {
            "move_m@drunk@verydrunk", 
            "move_f@drunk@verydrunk",
            "move_m@drunk@slightlydrunk",
            "move_f@drunk@slightlydrunk"
        };

        private Vector3 spawnpoint;
        private Ped nakedPed;
        private Blip pedBlip;
        private bool hasBeenArrested;
        private bool hasRanAway;
        private bool isChasing;
        private int conversationCounter;
        private string pedGender;
        private LHandle pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Get random spawn location near player but not too close
            spawnpoint = MainPlayer.Position.Around2D(150f, 300f);

            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of a naked person on drugs causing a public disturbance.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_DISTURBING_THE_PEACE_01 IN_OR_ON_POSITION", spawnpoint);
            CalloutMessage = "Naked Drug User";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Naked Drug User callout has been accepted!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Naked Drug User callout has been accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Naked Drug User", "~b~Dispatch~w~: Reports of a naked person acting erratically. Approach with caution. Respond ~r~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            // Randomly select male or female
            bool isMale = rng.Next(2) == 0;
            string modelName = isMale ? malePedModels[rng.Next(malePedModels.Length)] : femalePedModels[rng.Next(femalePedModels.Length)];

            nakedPed = new Ped(modelName, spawnpoint, (float)rng.Next(360))
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            // Strip the ped naked
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 3, 15, 0, 0); // Torso
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 4, 61, 0, 0); // Legs
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 5, 0, 0, 0);  // Hands
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 6, 34, 0, 0); // Feet
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 7, 0, 0, 0);  // Accessories
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 8, 15, 0, 0); // Undershirt
            NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(nakedPed, 11, 15, 0, 0); // Torso 2

            // Make ped act drunk/high
            string animSet = isMale ? "move_m@drunk@verydrunk" : "move_f@drunk@verydrunk";
            NativeFunction.Natives.REQUEST_ANIM_SET(animSet);
            while (!NativeFunction.Natives.HAS_ANIM_SET_LOADED<bool>(animSet))
            {
                GameFiber.Yield();
            }
            NativeFunction.Natives.SET_PED_MOVEMENT_CLIPSET(nakedPed, animSet, 1.0f);

            // Make them wander around
            nakedPed.Tasks.Wander();

            pedBlip = nakedPed.AttachBlip();
            pedBlip.Color = Color.Yellow;
            pedBlip.IsRouteEnabled = true;

            pedGender = isMale ? "Sir" : "Ma'am";
            conversationCounter = 0;
            hasBeenArrested = false;
            hasRanAway = false;
            isChasing = false;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            SafeDeleteBlip(pedBlip);
            SafeDelete(nakedPed);

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            try
            {
                // Check if ped is dead
                if (nakedPed != null && nakedPed.Exists() && nakedPed.IsDead)
                {
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Naked Drug User", "~r~The suspect has died. Code 4.");
                    End();
                    return;
                }

                // Check if player is dead
                if (MainPlayer.IsDead)
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("Callout Failed!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                    }
                    End();
                    return;
                }

                // Check if ped has been arrested
                if (nakedPed != null && nakedPed.Exists())
                {
                    if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(nakedPed) && !hasBeenArrested)
                    {
                        hasBeenArrested = true;
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Naked Drug User", "~g~Suspect arrested! Good work. Code 4.");

                        if (Settings.MissionMessages)
                        {
                            BigMessageThread bigMessage = new BigMessageThread();
                            bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Suspect in custody!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                        }

                        GameFiber.Wait(3000);
                        End();
                        return;
                    }

                    // Random chance ped runs away when player gets close
                    if (!isChasing && !hasRanAway && MainPlayer.DistanceTo(nakedPed) <= 15f)
                    {
                        int runChance = rng.Next(100);
                        if (runChance < 40) // 40% chance to run
                        {
                            hasRanAway = true;
                            isChasing = true;
                            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Naked Drug User", "~r~The suspect is fleeing!");

                            // Make ped flee
                            nakedPed.BlockPermanentEvents = false;
                            nakedPed.Tasks.Clear();
                            NativeFunction.Natives.TASK_SMART_FLEE_PED(nakedPed, MainPlayer, 500f, -1, false, false);

                            // Change blip color to red
                            if (pedBlip != null && pedBlip.Exists())
                            {
                                pedBlip.Color = Color.Red;
                            }
                        }
                    }

                    // Handle conversation when close
                    if (!hasRanAway && MainPlayer.DistanceTo(nakedPed) <= 5f)
                    {
                        if (Settings.HelpMessages && conversationCounter == 0)
                        {
                            Game.DisplayHelp("Press ~y~Y~w~ to talk to the suspect.");
                        }

                        if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                        {
                            conversationCounter++;
                            HandleConversation();
                        }
                    }
                }

                // End call key
                if (Game.IsKeyDown(Settings.EndCall))
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                    }
                    End();
                    return;
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Error in Naked Drug User callout. Error: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.StackTrace);
                }
            }

            base.Process();
        }

        private void HandleConversation()
        {
            if (nakedPed == null || !nakedPed.Exists()) return;

            switch (conversationCounter)
            {
                case 1:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(nakedPed, MainPlayer, -1);
                    nakedPed.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_drug_dealer_hard@male@base"), "base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~b~You~w~: " + pedGender + ", are you okay? Where are your clothes?");
                    GameFiber.Wait(1000);
                    break;
                case 2:
                    nakedPed.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_fall_over", -1f, AnimationFlags.StayInEndFrame);
                    Game.DisplaySubtitle("~r~Naked Suspect~w~: MAAAAN! The aliens took my clothes! They wanted to study human fashion!");
                    GameFiber.Wait(1000);
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: What have you taken tonight?");
                    GameFiber.Wait(1000);
                    break;
                case 4:
                    nakedPed.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@nightclub@peds@"), "rcmme_amanda1_stand_loop_cop", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~r~Naked Suspect~w~: I don't know man, some dude gave me these purple pills at the beach! Now I can taste colors!");
                    GameFiber.Wait(1000);
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: I need you to calm down. You're being detained for public intoxication and indecent exposure.");
                    GameFiber.Wait(1000);
                    break;
                case 6:
                    nakedPed.Tasks.PlayAnimation(new AnimationDictionary("misscarsteal3"), "confusion_back", -1f, AnimationFlags.StayInEndFrame);
                    Game.DisplaySubtitle("~r~Naked Suspect~w~: BUT THE LIZARD PEOPLE NEED ME! I'M THE CHOSEN ONE!");
                    GameFiber.Wait(1000);
                    break;
                case 7:
                    Game.DisplaySubtitle("~w~Conversation ended. You may arrest the suspect or let them go.");

                    // Random chance they try to run after conversation
                    if (rng.Next(100) < 30) // 30% chance
                    {
                        GameFiber.Wait(2000);
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Naked Drug User", "~r~The suspect is trying to escape!");
                        hasRanAway = true;
                        isChasing = true;
                        nakedPed.BlockPermanentEvents = false;
                        nakedPed.Tasks.Clear();
                        NativeFunction.Natives.TASK_SMART_FLEE_PED(nakedPed, MainPlayer, 500f, -1, false, false);

                        if (pedBlip != null && pedBlip.Exists())
                        {
                            pedBlip.Color = Color.Red;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override void End()
        {
            SafeDeleteBlip(pedBlip);
            SafeDismiss(nakedPed);

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Naked Drug User", "~b~You~w~: Dispatch, we are ~g~Code 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Return to patrol!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Naked Drug User callout is code 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Naked Drug User callout is code 4!");
            }
        }

        #region Helpers
        private void SafeDelete(Ped ped)
        {
            if (ped != null && ped.Exists()) ped.Delete();
        }

        private void SafeDismiss(Ped ped)
        {
            if (ped != null && ped.Exists()) ped.Dismiss();
        }

        private void SafeDeleteBlip(Blip blip)
        {
            if (blip != null && blip.Exists()) blip.Delete();
        }
        #endregion
    }
}