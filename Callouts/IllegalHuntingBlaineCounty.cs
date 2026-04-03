using CalloutInterfaceAPI;
using Adam69Callouts.Common;

namespace Adam69Callouts.Callouts
{

    [CalloutInterface("[Adam69 Callouts] Illegal Hunting", CalloutProbability.Medium, "Reports of Illegal Hunting of Endangered Species", "Code 2", "SASPR")]

    public class IllegalHuntingBlaineCounty : Callout
    {

        private static Ped suspect;
        private static Ped theAnimal;
        private static Vehicle susVehicle;
        private static Blip susBlip;
        private static Blip rangerBlip;
        private static Vector3 spawnpoint;
        private static Vector3 animalSpawn;
        private static Ped ParkOfficer;
        private static Vehicle rangerVehicle;
        private static int counter;
        private static string malefemale;
        private static bool hasWeapon;
        private static bool willFlee;
        private static bool animalKilled;
        private static readonly string[] hunterVehicles = new string[] { "rebel", "bison", "bodhi", "dloader", "sandking" };
        private static readonly string[] endangeredAnimals = new string[] { "a_c_deer", "a_c_boar", "a_c_coyote", "a_c_mtlion" };
        private static readonly string[] huntingWeapons = new string[] { "weapon_sniperrifle", "weapon_assaultrifle", "weapon_carbinerifle" };
        private static LHandle pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Spawn in rural Blaine County areas
            spawnpoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(400f, 700f));
            
            // Ensure we're in a rural area (Blaine County)
            if (spawnpoint.DistanceTo(new Vector3(0f, 0f, 0f)) < 2000f)
            {
                spawnpoint = new Vector3(1500f + MathHelper.GetRandomInteger(1000), 3500f + MathHelper.GetRandomInteger(1000), 100f);
            }

            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of illegal hunting of endangered species in Blaine County. Park Ranger requesting assistance.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_BRANDISHING_WEAPON_02", spawnpoint);
            CalloutMessage = "Illegal Hunting Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Illegal Hunting callout has been accepted!");
                LoggingManager.Log("Adam69 Callouts: Illegal Hunting callout accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Illegal Hunting", "~b~Dispatch~w~: Park Ranger on scene. Suspect armed with hunting rifle. Respond ~r~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            // Randomize scenario
            hasWeapon = MathHelper.GetRandomInteger(10) > 3; // 70% chance has weapon
            willFlee = MathHelper.GetRandomInteger(10) > 5; // 50% chance will flee
            animalKilled = MathHelper.GetRandomInteger(10) > 4; // 60% chance animal is dead

            // Spawn suspect vehicle
            susVehicle = new Vehicle(hunterVehicles[MathHelper.GetRandomInteger(hunterVehicles.Length)], spawnpoint);
            if (susVehicle.Exists())
            {
                susVehicle.IsPersistent = true;
                susVehicle.IsEngineOn = false;
            }

            // Spawn suspect
            suspect = susVehicle.CreateRandomDriver();
            if (suspect.Exists())
            {
                suspect.IsPersistent = true;
                suspect.BlockPermanentEvents = true;
                
                if (suspect.IsMale)
                    malefemale = "Sir";
                else
                    malefemale = "Ma'am";

                // Give hunting weapon
                if (hasWeapon)
                {
                    SafeInventory.SafeGiveWeapon(suspect, huntingWeapons[MathHelper.GetRandomInteger(huntingWeapons.Length)], 30, false);
                }

                // Create blip
                susBlip = suspect.AttachBlip();
                susBlip.Color = System.Drawing.Color.Yellow;
                susBlip.IsRouteEnabled = true;
            }

            // Spawn animal
            animalSpawn = spawnpoint.Around2D(8f, 15f);
            theAnimal = new Ped(endangeredAnimals[MathHelper.GetRandomInteger(endangeredAnimals.Length)], animalSpawn, 0f);
            if (theAnimal.Exists())
            {
                theAnimal.IsPersistent = true;
                theAnimal.BlockPermanentEvents = true;
                
                if (animalKilled)
                {
                    theAnimal.Kill();
                    theAnimal.IsRagdoll = true;
                }
                else
                {
                    // Animal is wounded but alive
                    theAnimal.Health = theAnimal.MaxHealth / 4;
                    theAnimal.Tasks.Cower(-1);
                }
            }

            // Spawn Park Ranger
            Vector3 rangerSpawn = spawnpoint.Around2D(20f, 30f);
            rangerVehicle = new Vehicle("pranger", rangerSpawn);
            if (rangerVehicle.Exists())
            {
                rangerVehicle.IsPersistent = true;
                rangerVehicle.IsEngineOn = false;
            }

            ParkOfficer = rangerVehicle.CreateRandomDriver();
            if (ParkOfficer.Exists())
            {
                ParkOfficer.IsPersistent = true;
                ParkOfficer.BlockPermanentEvents = true;
                ParkOfficer.Tasks.LeaveVehicle(rangerVehicle, LeaveVehicleFlags.None);
                
                rangerBlip = ParkOfficer.AttachBlip();
                rangerBlip.Color = System.Drawing.Color.Green;
                rangerBlip.Scale = 0.8f;
            }

            counter = 0;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (suspect != null && suspect.Exists()) suspect.Delete();
            if (theAnimal != null && theAnimal.Exists()) theAnimal.Delete();
            if (susVehicle != null && susVehicle.Exists()) susVehicle.Delete();
            if (ParkOfficer != null && ParkOfficer.Exists()) ParkOfficer.Delete();
            if (rangerVehicle != null && rangerVehicle.Exists()) rangerVehicle.Delete();
            if (susBlip != null && susBlip.Exists()) susBlip.Delete();
            if (rangerBlip != null && rangerBlip.Exists()) rangerBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            // Talk to Park Ranger first
            if (ParkOfficer != null && ParkOfficer.Exists() && MainPlayer.DistanceTo(ParkOfficer) <= 15f && counter == 0)
            {
                if (Settings.HelpMessages)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to speak with the Park Ranger.");
                }

                if (Game.IsKeyDown(Settings.Dialog))
                {
                    GameFiber.Sleep(500);
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ParkOfficer, MainPlayer, -1);
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(MainPlayer, ParkOfficer, -1);
                    Game.DisplaySubtitle("~g~Park Ranger~w~: Thank god you're here! That hunter just shot " + (animalKilled ? "and killed" : "a wounded") + " an endangered animal. They have a hunting rifle!");
                    GameFiber.Sleep(4000);
                    Game.DisplaySubtitle("~b~You~w~: Copy that. Stay back, I'll handle this.");
                    GameFiber.Sleep(3000);
                    counter++;
                }
            }

            // Interact with suspect
            if (suspect != null && suspect.Exists() && MainPlayer.DistanceTo(suspect) <= 25f && counter >= 1)
            {
                if (Settings.HelpMessages && counter == 1)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to question the suspect.");
                }

                if (Game.IsKeyDown(Settings.Dialog))
                {
                    GameFiber.Sleep(500);
                    counter++;

                    try
                    {
                        if (counter == 2)
                        {
                            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                            Game.DisplaySubtitle("~b~You~w~: " + malefemale + ", drop your weapon and put your hands up! You're under investigation for illegal hunting!");
                            GameFiber.Sleep(4000);
                        }
                        else if (counter == 3)
                        {
                            suspect.Tasks.PlayAnimation(new AnimationDictionary("missminuteman_1ig_2"), "handsup_base", 2f, AnimationFlags.Loop);
                            Game.DisplaySubtitle("~r~Suspect~w~: Wait, wait! I didn't know this was a protected area! I have a hunting license!");
                            GameFiber.Sleep(4000);
                        }
                        else if (counter == 4)
                        {
                            Game.DisplaySubtitle("~b~You~w~: This entire area is a wildlife preserve. All hunting is prohibited. That's an endangered species you shot!");
                            GameFiber.Sleep(4000);
                        }
                        else if (counter == 5)
                        {
                            if (willFlee)
                            {
                                Game.DisplaySubtitle("~r~Suspect~w~: I'm not going to jail! *Runs to vehicle*");
                                GameFiber.Sleep(2000);
                                
                                // Suspect flees
                                suspect.Tasks.ClearImmediately();
                                suspect.Tasks.EnterVehicle(susVehicle, -1).WaitForCompletion(5000);
                                
                                if (suspect.IsInVehicle(susVehicle, false))
                                {
                                    pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                                    LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, suspect);
                                    LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                    Game.DisplayNotification("~r~Suspect is fleeing! Pursue the vehicle!");
                                    
                                    if (susBlip.Exists())
                                    {
                                        susBlip.Color = System.Drawing.Color.Red;
                                    }
                                }
                            }
                            else
                            {
                                Game.DisplaySubtitle("~r~Suspect~w~: *Sighs* You're right, officer. I'll come quietly. I'm sorry about the animal.");
                                GameFiber.Sleep(3000);
                                Game.DisplaySubtitle("~b~You~w~: Good choice. You're under arrest for illegal hunting and poaching of an endangered species.");
                                GameFiber.Sleep(3000);
                                suspect.Tasks.PutHandsUp(-1, MainPlayer);
                                
                                if (Settings.HelpMessages)
                                {
                                    Game.DisplayHelp("Arrest the suspect and call animal control with ~y~" + Settings.CallAnimalControlKey.ToString() + "~w~ for the wounded animal.");
                                }
                            }
                            counter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Settings.EnableLogs)
                        {
                            Game.LogTrivial("Adam69 Callouts [ERROR]: Exception in Illegal Hunting dialogue: " + ex.Message);
                            LoggingManager.Log("Adam69 Callouts: Exception in Illegal Hunting dialogue: " + ex.Message);
                        }
                    }
                }
            }

            // Call animal control
            if (theAnimal != null && theAnimal.Exists() && MainPlayer.DistanceTo(theAnimal) <= 20f)
            {
                if (Game.IsKeyDown(Settings.CallAnimalControlKey))
                {
                    GameFiber.Sleep(500);
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Animal Control", "~b~Dispatch~w~: Animal Control is en route to handle the " + (animalKilled ? "deceased" : "wounded") + " animal.");
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");
                    
                    GameFiber.Sleep(5000);
                    if (theAnimal.Exists()) theAnimal.Dismiss();
                }
            }

            // Check if suspect arrested or killed
            if (suspect != null && suspect.Exists())
            {
                if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(suspect) || LSPD_First_Response.Mod.API.Functions.IsPedStoppedByPlayer(suspect))
                {
                    Game.DisplayHelp("The suspect has been detained. End the callout with ~y~" + Settings.EndCall.ToString() + "~w~.");
                }

                if (suspect.IsDead)
                {
                    Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Illegal Hunting", "~r~Suspect is deceased. Notify dispatch.");
                }
            }

            // Player death check
            if (MainPlayer.IsDead)
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("MISSION FAILED!", "You have fallen in the line of duty.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }

                if (Settings.EnableLogs)
                {
                    LoggingManager.Log("Adam69 Callouts: Illegal Hunting callout failed - Player died!");
                }

                End();
            }

            // End callout
            if (Game.IsKeyDown(Settings.EndCall))
            {
                End();
            }
        }

        public override void End()
        {
            if (suspect != null && suspect.Exists()) suspect.Dismiss();
            if (theAnimal != null && theAnimal.Exists()) theAnimal.Dismiss();
            if (susVehicle != null && susVehicle.Exists()) susVehicle.Dismiss();
            if (ParkOfficer != null && ParkOfficer.Exists()) ParkOfficer.Dismiss();
            if (rangerVehicle != null && rangerVehicle.Exists()) rangerVehicle.Dismiss();
            if (susBlip != null && susBlip.Exists()) susBlip.Delete();
            if (rangerBlip != null && rangerBlip.Exists()) rangerBlip.Delete();

            if (pursuit != null)
            {
                LSPD_First_Response.Mod.API.Functions.ForceEndPursuit(pursuit);
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Illegal Hunting", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Completed!", "Wildlife protected. You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Illegal Hunting callout is CODE 4!");
                LoggingManager.Log("Adam69 Callouts: Illegal Hunting callout is CODE 4!");
            }
        }
    }
}
