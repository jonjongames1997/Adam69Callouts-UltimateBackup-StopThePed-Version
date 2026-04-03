using CalloutInterfaceAPI;
using System;
using System.Drawing;

namespace Adam69Callouts.Callouts
{

    [CalloutInterface("[Adam69 Callouts] Drugs Found [Beta]", CalloutProbability.Medium, "Drugs found reported by a citizen", "Code 2", "LEO")]
    public class DrugsFound : Callout
    {
        private static readonly Random rng = new();

        private static readonly string[] drugList = new string[] { "sf_prop_sf_bag_weed_01b", "bkr_prop_weed_bigbag_open_01a", "m24_1_prop_m41_weed_bigbag_01a", "sf_prop_sf_bag_weed_open_01a", "m25_1_prop_m51_box_weed_01a", "m25_1_prop_m51_box_weed_02a", "m25_1_prop_m51_bag_weed_01a" };

        private Vector3 spawnpoint;
        public Rage.Object theDrugs;
        private Ped theCaller;
        private Vector3 callerSpawn;
        private Blip drugBlip;
        private Blip callerBlip;
        private int counter;
        private string malefemale;
        private static readonly string[] backupList = new string[] { "s_m_y_cop_01", "s_f_y_cop_01", "csb_cop", "s_f_y_sheriff_01", "s_m_y_sheriff_01" };
        private static readonly string[] backupVehicle = new string[] { "police", "police2", "police3", "police4", "fbi", "fbi2", "sheriff", "sheriff2", "policeb", "policeb2" };
        private Ped theCop;
        private Vector3 copSpawn;
        private Vector3 leoVehicleSpawn;
        private Blip theCopBlip;
        private Blip policeCarBlip;
        private Vehicle policeVehicle;
        private string copGender;
        private bool isCollected;
        private float callerHeading;
        private float copHeading;

        public static bool IsDlcInstalled(uint dlcHash)
        {
            return NativeFunction.CallByName<bool>("IS_DLC_PRESENT", dlcHash);
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!DLCManager.AreRequiredDLCsInstalled())
            {
                string missing = DLCManager.GetMissingDLC();
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~DLC Missing", "~w~Adam69 Callouts", $"Required DLC '{missing}' is not installed. This callout will not function properly.");
                Game.LogTrivial("Adam69 Callouts [LOG]: Required DLC '" + missing + "' is not installed. This callout will not function properly.");
                LoggingManager.Log("Adam69 Callouts [LOG]: Required DLC '" + missing + "' is not installed. This callout will not function properly.");
                return false;
            }

            spawnpoint = new Vector3(979.11f, -1957.23f, 30.77f);
            callerSpawn = new Vector3(989.72f, -1945.02f, 30.99f);
            callerHeading = 199.09f;
            copSpawn = new Vector3(1000.36f, -1952.72f, 30.91f);
            copHeading = 74.75f;
            leoVehicleSpawn = new Vector3(1000.69f, -1958.26f, 30.86f);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of illegal drugs found by a nearby citizen.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("A_CIVILIAN_REQUIRING_ASSISTANCE_01", spawnpoint);
            CalloutMessage = "Illegal drugs found";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Drugs Found callout has been accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Drugs Found", "~b~Dispatch~w~: The caller has been located. Respond ~r~Code 2~w~.");
            LoggingManager.Log("Adam69 Callouts [LOG]: Drugs Found callout has been accepted!");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            theCaller = new Ped(callerSpawn) { IsPersistent = true, BlockPermanentEvents = true };
            theCaller.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);

            theDrugs = new Rage.Object("m25_1_prop_m51_bag_weed_01a", spawnpoint) { IsPersistent = true };
            // Add pickup blip via native
            NativeFunction.Natives.ADD_BLIP_FOR_PICKUP(theDrugs);

            callerBlip = theCaller.AttachBlip();
            callerBlip.Color = Color.Orange;
            callerBlip.Alpha = 0.5f;

            drugBlip = theDrugs.AttachBlip();
            drugBlip.Color = Color.Purple;
            drugBlip.IsRouteEnabled = true;

            theCop = new Ped(backupList[rng.Next(backupList.Length)], copSpawn, 0f) { IsPersistent = true, BlockPermanentEvents = true };

            theCopBlip = theCop.AttachBlip();
            theCopBlip.Color = Color.LightBlue;

            policeVehicle = new Vehicle(backupVehicle[rng.Next(backupVehicle.Length)], leoVehicleSpawn, 0f) { IsPersistent = true };

            // Turn on police lights - guard with null/valid check
            try
            {
                if (policeVehicle != null && policeVehicle.IsValid())
                {
                    policeVehicle.IsSirenOn = true;
                    policeVehicle.IsSirenSilent = true;
                    LoggingManager.Log("Adam69 Callouts [LOG]: Police vehicle siren/lights enabled for Drugs Found.");
                }
                else
                {
                    Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~Error", "~w~Drugs Found", "Could not find police vehicle to enable emergency lights.");
                    if (Settings.EnableLogs)
                    {
                        Game.LogTrivial("ERR: policeVehicle is null or invalid. Cannot enable emergency lights.");
                        LoggingManager.Log("Adam69 Callouts [LOG]: policeVehicle is null or invalid. Cannot enable emergency lights.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Exception while setting up police vehicle emergency lights: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: " + ex.StackTrace);
                }
            }

            if (policeVehicle != null)
            {
                policeCarBlip = policeVehicle.AttachBlip();
                policeCarBlip.Color = Color.DarkBlue;
            }

            malefemale = theCaller != null && theCaller.IsMale ? "Sir" : "Ma'am";
            copGender = MainPlayer.IsMale ? "Sir" : "Ma'am";

            counter = 0;
            isCollected = false;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            SafeDelete(theDrugs);
            SafeDeleteBlip(drugBlip);
            SafeDeleteBlip(callerBlip);
            SafeDeleteBlip(theCopBlip);
            SafeDelete(policeVehicle);
            SafeDelete(theCaller);
            SafeDelete(theCop);

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            // keep exception handling localized so we don't swallow normal flow
            try
            {
                HandleConversation();
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Error in conversation handling: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR:" + ex.StackTrace);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR:" + ex.Message);
                }
            }

            try
            {
                HandlePickupAndInputs();
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Error in pickup/inputs handling: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR:" + ex.StackTrace);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR:" + ex.Message);
                }
            }

            // Player death check
            if (MainPlayer.IsDead)
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("Callout Failed!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }

                End();
                base.Process();
                return;
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
                base.Process();
                return;
            }

            base.Process();
        }

        private void HandleConversation()
        {
            if (theCaller == null || !theCaller.Exists()) return;

            if (MainPlayer.DistanceTo(theCaller) <= 5f && Game.IsKeyDown(System.Windows.Forms.Keys.Y))
            {
                counter++;

                switch (counter)
                {
                    case 1:
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(theCaller, MainPlayer, -1);
                        if (theCop != null) theCop.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_cop_idles@male@idle_b"), "idle_e", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~b~You~w~: Hello there, " + malefemale + ". Are you the caller?");
                        break;
                    case 2:
                        theCaller.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~o~The Caller~w~: Yes I am, " + copGender + ".");
                        break;
                    case 3:
                        theCaller.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~b~You~w~: Can you explain how did you find the drugs?");
                        break;
                    case 4:
                        theCaller.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~o~The Caller~w~: I was going for a walk then I spotted this opened bag of weed from LD Organics. I didn't know who owns that bag of weed. That's why I called.");
                        break;
                    case 5:
                        theCaller.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~b~You~w~: We really appreciate that you reported it. I'll investigate this. I just need to see your ID so I know who I'm talking to.");
                        break;
                    case 6:
                        theCaller.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~o~The Caller~w~: Ok, no problem. I'm really in a hurry. I got to go watch Kansas City Chiefs vs Buffalo Bills game.");
                        break;
                    default:
                        Game.DisplaySubtitle("Convo Ended. Deal with the situation you may see fit.");
                        theCaller.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                        break;
                }
            }
        }

        private void HandlePickupAndInputs()
        {
            if (theDrugs != null && theDrugs.Exists() && MainPlayer.DistanceTo(theDrugs) <= 5f)
            {
                if (Settings.HelpMessages)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.PickUp.ToString() + "~w~ to pick up the drugs.");
                }

                if (Game.IsKeyDown(System.Windows.Forms.Keys.E))
                {
                    isCollected = true;
                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("anim@move_m@trash"), "pickup", -1f, AnimationFlags.UpperBodyOnly);
                    theDrugs.AttachTo(MainPlayer, 6286, Vector3.RelativeRight, Rotator.Zero);
                    GameFiber.Yield();
                    SafeDelete(theDrugs);
                }
            }
        }

        public override void End()
        {
            SafeDelete(theDrugs);
            SafeDismiss(theCaller);
            SafeDeleteBlip(drugBlip);
            SafeDeleteBlip(callerBlip);
            SafeDismiss(theCop);
            SafeDeleteBlip(theCopBlip);
            SafeDelete(policeVehicle);
            SafeDeleteBlip(policeCarBlip);

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Drugs Found", "~b~You~w~: Dispatch, we are ~g~Code 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Return to patrol!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Drugs Found callout is code 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Drugs Found callout is code 4!");
            }
        }

        #region Helpers
        private void SafeDelete(Rage.Object obj)
        {
            if (obj != null && obj.Exists()) obj.Delete();
        }

        private void SafeDelete(Ped ped)
        {
            if (ped != null && ped.Exists()) ped.Delete();
        }

        private void SafeDelete(Vehicle veh)
        {
            if (veh != null && veh.Exists()) veh.Delete();
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