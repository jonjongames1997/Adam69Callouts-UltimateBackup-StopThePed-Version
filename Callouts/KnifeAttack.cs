using CalloutInterfaceAPI;

namespace Adam69Callouts.Callouts
{

    [CalloutInterface("[Adam69 Callouts] Knife Attack", CalloutProbability.Medium, "Knife attack reported", "Code 3", "LSPD")]
    public class KnifeAttack : Callout
    {

        // General variables
        private static Ped suspect;
        private static Ped victim;
        private static float suspectHeading;
        private static float victimHeading;
        private static Vector3 spawnpoint;
        private static Vector3 victimSpawn;
        private static Blip suspectBlip;
        private static Blip victimBlip;
        private static int _scenario;
        private static bool hasBegunAttacking;
        private static bool isArmed;
        private static bool hasPursuitBegun;
        private static bool hasSpoke;
        private static bool ispursuitCreated = false;
        private static LHandle pursuit;


        // Helper: safely detect if a ped has a weapon (guards against invalid PedInventory)
        private static bool TryPedHasWeapon(Ped ped, WeaponHash weapon)
        {
            if (ped == null || !ped.Exists() || !ped.IsValid()) return false;

            try
            {
                // Access Inventory safely; this can throw if PedInventory is invalid
                return ped.Inventory != null && ped.Inventory.Weapons.Contains(weapon);
            }
            catch
            {
                // Fallback to native check if Inventory is invalid
                try
                {
                    return (bool)NativeFunction.Natives.HAS_PED_GOT_WEAPON(ped, (int)weapon);
                }
                catch
                {
                    return false;
                }
            }
        }

        // Helper: safely give a weapon to a ped, falling back to native if Inventory is invalid
        private static void SafeGiveWeapon(Ped ped, WeaponHash weapon)
        {
            if (ped == null || !ped.Exists() || !ped.IsValid()) return;

            try
            {
                ped.Inventory.GiveNewWeapon(weapon, 0, true);
            }
            catch
            {
                // Fallback native: give weapon and equip it
                try
                {
                    NativeFunction.Natives.GIVE_WEAPON_TO_PED(ped, (int)weapon, 0, false, true);
                }
                catch
                {
                    // swallow - best effort
                }
            }
        }

        // Helper: safely equip a weapon on a ped, falling back to native if Inventory is invalid
        private static void SafeEquipWeapon(Ped ped, WeaponHash weapon)
        {
            if (ped == null || !ped.Exists() || !ped.IsValid()) return;

            try
            {
                ped.Inventory.EquippedWeapon = weapon;
            }
            catch
            {
                try
                {
                    NativeFunction.Natives.SET_CURRENT_PED_WEAPON(ped, (int)weapon, true);
                }
                catch
                {
                    // swallow - best effort
                }
            }
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = new(-315.15f, 2786.42f, 59.56f);
            suspectHeading = 254.36f;
            victimSpawn = new(-311.28f, 2789.56f, 59.52f);
            victimHeading = 284.96f;
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of a knife attack in progress");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_ASSAULT_ON_A_CIVILIAN", spawnpoint);
            CalloutMessage = "Knife Attack Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Knife Attack callout accepted!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Knife Attack callout accepted!");
            }
            else
            {
                Settings.EnableLogs = false;
            }


            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Knife Attack", "~b~Dispatch~w~: The suspect has been located. Respond ~r~Code 3~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_3_Audio");

            suspect = new Ped(spawnpoint)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };
            suspect.IsValid();
            suspect.Exists();

            victim = new Ped(victimSpawn)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            victim.IsValid();
            victim.Exists();
            victim.Kill();
            NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(victim, "TD_KNIFE_FRONT", 1f, 1f);

            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = System.Drawing.Color.Red;
            suspectBlip.IsRouteEnabled = true;
            suspectBlip.Alpha = 0.75f;

            victimBlip = victim.AttachBlip();
            victimBlip.Color = System.Drawing.Color.Blue;
            victimBlip.IsRouteEnabled = true;
            victimBlip.Alpha = 0.75f;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (suspect != null && suspect.Exists())
            {
                suspect.Delete();
            }
            if (victim != null && victim.Exists())
            {
                victim.Delete();
            }
            if (suspectBlip != null && suspectBlip.Exists())
            {
                suspectBlip.Delete();
            }
            if (victimBlip != null && victimBlip.Exists())
            {
                victimBlip.Delete();
            }
            base.OnCalloutNotAccepted();

        }

        public override void Process()
        {
            // Ensure suspect is valid before accessing Inventory or other members to avoid invalid PedInventory exceptions
            if (suspect != null && suspect.Exists() && suspect.IsValid())
            {
                // Use safe helpers instead of direct Inventory access which can throw
                if (!TryPedHasWeapon(suspect, WeaponHash.Knife) && suspect.DistanceTo(MainPlayer.GetOffsetPosition(Vector3.RelativeFront)) < 20f)
                {
                    SafeGiveWeapon(suspect, WeaponHash.Knife);
                    isArmed = true;
                }
                else if (!isArmed && TryPedHasWeapon(suspect, WeaponHash.Knife) && suspect.DistanceTo(MainPlayer.GetOffsetPosition(Vector3.RelativeFront)) < 20f)
                {
                    SafeEquipWeapon(suspect, WeaponHash.Knife);
                    isArmed = true;
                }
            }

            if (!hasBegunAttacking && suspect != null && suspect.Exists() && suspect.IsValid() && suspect.DistanceTo(MainPlayer.GetOffsetPosition(Vector3.RelativeFront)) < 20f)
            {
                hasBegunAttacking = true;
                GameFiber.StartNew(() =>
                {
                    switch (_scenario)
                    {
                        case > 50:
                            suspect.KeepTasks = true;
                            suspect.Tasks.FightAgainst(MainPlayer);
                            switch (Rndm.Next(1, 4))
                            {
                                case 1:
                                    Game.DisplaySubtitle("~r~Suspect~w~: I'm going to stab you!", 5000);
                                    PolicingRedefined.API.PedAPI.IsPedResistingRightNow(suspect);
                                    hasSpoke = true;
                                    break;

                                case 2:
                                    Game.DisplaySubtitle("~r~Suspect~w~: You picked the wrong person to mess with!", 5000);
                                    PolicingRedefined.API.PedAPI.IsPedResistingRightNow(suspect);
                                    hasSpoke = true;
                                    break;

                                case 3:
                                    Game.DisplaySubtitle("~r~Suspect~w~: I'll cut you!", 5000);
                                    PolicingRedefined.API.PedAPI.GetPedResistanceAction(suspect);
                                    PolicingRedefined.API.BackupDispatchAPI.RequestPanicBackup();
                                    hasSpoke = true;
                                    break;
                            }
                            GameFiber.Wait(5000);
                            break;

                        default:
                            if (!hasPursuitBegun)
                            {
                                ispursuitCreated = true;
                                pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                                LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, suspect);
                                LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                PolicingRedefined.API.BackupDispatchAPI.RequestPursuitBackup();
                                hasPursuitBegun = true;
                            }

                            break;

                    }

                });

            }
            base.Process();

            if (Game.IsKeyDown(System.Windows.Forms.Keys.K))
            {
                PolicingRedefined.API.BackupDispatchAPI.RequestEMSCode3Backup();
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Call_Ambulance_Audio");
                Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Dispatch:", "An Ambulance has been called to the scene.");
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("Suspect Neutralized!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }
                else
                {
                    Settings.MissionMessages = false;
                }

                this.End();
            }
        }

        public override void End()
        {
            if (suspect != null && suspect.Exists()) suspect.Dismiss();
            if (victim != null && victim.Exists()) victim.Dismiss();
            if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();
            if (victimBlip != null && victimBlip.Exists()) victimBlip.Delete();

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Person With A Knife", "~w~Dispatch: The scene is now ~r~CODE 4~w~.");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
            }
            else
            {
                Settings.MissionMessages = false;
            }
            base.End();

            if (Settings.EnableLogs)
            {

                Game.LogTrivial("Adam69 Callouts [LOG]: Knife Attack callout is CODE 4!");

                LoggingManager.Log("Adam69 Callouts [LOG]: Knife Attack callout is CODE 4!");
            }
            else
            {
                Settings.EnableLogs = false;
            }
        }
    }
}
