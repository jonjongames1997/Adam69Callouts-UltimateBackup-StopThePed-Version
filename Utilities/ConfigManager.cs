using Rage;
using System.IO;

namespace Adam69Callouts.Utilities
{
    public static class ConfigManager
    {
        private static readonly string ConfigFilePath = "plugins\\LSPDFR\\Adam69Callouts\\Adam69Callouts.ini";
        private static InitializationFile config;

        private static bool EnableLogs { get; set; }
        private static bool VehicleBlockingDriveway { get; set; }
        private static bool AbandonedVehicle { get; set; }
        private static bool BicycleBlockingRoadway { get; set; }
        private static bool VehicleBlockingSidewalk { get; set; }
        private static bool BicyclePursuit { get; set; }
        private static bool PersonCarryingAConcealedWeapon { get; set; }
        private static bool Loitering { get; set; }
        private static bool SuspiciousVehicle { get; set; }
        private static bool DrugsFound { get; set; }
        private static bool SuspiciousPerson { get; set; }
        private static bool OfficerDown { get; set; }
        private static bool DerangedDrunkenFeller { get; set; }
        private static bool DeadBirdOnTheRoad { get; set; }
        private static bool KnifeAttack { get; set; }
        private static bool HelpMessages { get; set; }
        private static bool MissionMessages { get; set; }
        private static Keys EndCall { get; set; }
        private static Keys Dialog { get; set; }
        private static Keys PickUp { get; set; }
        private static Keys CallAnimalControlKey { get; set; }
        private static Keys CallAmbulanceKey { get; set; }
        private static Keys RequestVehicleInfo { get; set; }
        private static Keys RequestTowTruck { get; set; }
        private static Keys CallFireDepartmentKey { get; set; }

        private static void LoadConfig()
        {
            Directory.CreateDirectory("Plugins\\LSPDFR\\Adam69Callouts");

            if (!File.Exists(ConfigFilePath))
            {
                Game.LogTrivial("Adam69 Callouts: Config file not found, creating default config.");
                LoggingManager.Log("Adam69 Callouts: Config file not found, creating default config.");

                File.WriteAllText(ConfigFilePath,
                    @"[General Settings]
                    [Keys]
                    Dialog = Y
                    EndCall = End
                    PickUp = E
                    CallAnimalControlKey = NumPad1 // this key option only works for certain callout scenarios. Will expand to existing callouts if needed. //
                    CallAmbulanceKey = K
                    RequestVehicleInfo = P // This only works with Suspicious Vehicle, Vehicle Blocking Sidewalk and Vehicle Blocking Crosswalk Callouts. 
                    RequestTowTruck = L // Only works in certain callouts that require a tow truck. //

                    If the P key interferes with the pause menu, go to mods/update/update.rpf/x64/data/control, 
                    you can remove this item line code in your default.meta:
                    ""<Item>
	                    <Input>INPUT_FRONTEND_PAUSE</Input>
	                    <Source>IOMS_KEYBOARD</Source>
	                    <Parameters>
	                    <Item>KEY_P</Item>
	                    </Parameters>
                    </Item>""
                    This will not affect your ELS Panel UI. It only affects the pause menu bind to the P key.
                    
                    [Settings]

                    HelpMessages = true

                    MissionMessages = true // Show mission messages on screen. set to false to disable, set true to enable.

                    EnableLogs = false // Set to true to enable logs. Useful for debugging callouts.

                    -------------------------

                    // Set these to true or false //
                    // true - Callout will be active //
                    // false - Callout will be not active //

                    [Callouts]

                    VehicleParkingInHandicap = true
                    VehicleBlockingSidewalk = true
                    BicyclePursuit = true
                    PersonCarryingAConcealedWeapon = true
                    Loitering = true
                    VehicleBlockingCrosswalk = true
                    BicycleBlockingRoadway = true
                    SuspiciousVehicle = true
                    AbandonedVehicle = true
                    DrugsFound = true
                    SuspiciousPerson = false // Suspicious Person is currently disabled for a complete rewrite. It will be back in a future update. //
                    OfficerDown = true
                    DerangedDrunkenFeller = false // Disabled for a complete rewrite
                    DeadBirdOnTheRoad = true
                    KnifeAttack = true

                    ------------------------------

                    [Recommended Mods To Use with this callout pack]

                    Basic Parking Enforcement by ThruZZd: https://www.lcpdfr.com/downloads/gta5mods/scripts/33307-basicparkingenforcement-go-get-em-paydisplay-tickets-more/
                    Signs & Barricades DLC by PNWParksFan (HIGHLY Recommended): https://www.lcpdfr.com/downloads/gta5mods/misc/31234-traffic-signs-and-barricades-pack-lml-or-dlc-fivem-ready/
                    PoliceTape by PNWParksFan (HIGHLY Recommended): https://www.lcpdfr.com/downloads/gta5mods/scripts/19488-police-tape/
                    AI: Respond by VeteranFighter: https://www.lcpdfr.com/downloads/gta5mods/scripts/44660-airespond/
                    Vehicle Push by Faya: https://www.lcpdfr.com/downloads/gta5mods/scripts/39798-vehicle-push-push-any-vehicle-at-any-time/

                    ----------------------------

                    [Socials] 

                     //Twitch: https://www.twitch.tv/jonjongamesyt
                    //YouTube: https://www.youtube.com/@DiverseGamerHubOfficial
                    //GTA Mods: https://www.gta5-mods.com/users/JonJonGames
                    //Discord: https://discord.gg/N9KgZx4KUn
                    //LSPDFR Profile: https://www.lcpdfr.com/profile/534047-jonjongamesofficial/ 
                    ");


            }
        }

        private static void CreateConfig()
        {
            config = new InitializationFile(ConfigFilePath);
            config.Create();
            EnableLogs = config.ReadBoolean("Settings", "EnableLogs", false);
            VehicleBlockingSidewalk = config.ReadBoolean("Callouts", "VehicleBlockingSidewalk", true);
            BicyclePursuit = config.ReadBoolean("Callouts", "BicyclePursuit", true);
            PersonCarryingAConcealedWeapon = config.ReadBoolean("Callouts", "PersonCarryingAConcealedWeapon", true);
            Loitering = config.ReadBoolean("Callouts", "Loitering", true);
            VehicleBlockingDriveway = config.ReadBoolean("Callouts", "VehicleParkingInHandicap", true);
            BicycleBlockingRoadway = config.ReadBoolean("Callouts", "BicycleBlockingRoadway", true);
            SuspiciousVehicle = config.ReadBoolean("Callouts", "SuspiciousVehicle", true);
            AbandonedVehicle = config.ReadBoolean("Callouts", "AbandonedVehicle", true);
            DrugsFound = config.ReadBoolean("Callouts", "DrugsFound", true);
            SuspiciousPerson = config.ReadBoolean("Callouts", "SuspiciousPerson", false);
            OfficerDown = config.ReadBoolean("Callouts", "OfficerDown", true);
            DerangedDrunkenFeller = config.ReadBoolean("Callouts", "DerangedDrunkenFeller", false);
            DeadBirdOnTheRoad = config.ReadBoolean("Callouts", "DeadBirdOnTheRoad", true);
            KnifeAttack = config.ReadBoolean("Callouts", "KnifeAttack", true);
            HelpMessages = config.ReadBoolean("Settings", "HelpMessages", true);
            MissionMessages = config.ReadBoolean("Settings", "MissionMessages", true);
            EndCall = config.ReadEnum<Keys>("Keys", "EndCall", Keys.End);
            Dialog = config.ReadEnum<Keys>("Keys", "Dialog", Keys.Y);
            PickUp = config.ReadEnum<Keys>("Keys", "PickUp", Keys.E);
        }

    }
}
