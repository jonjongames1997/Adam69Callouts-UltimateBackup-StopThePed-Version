using System;
using Rage;
using Rage.Native;

namespace Adam69Callouts.Common
{
    /// <summary>
    /// Defensive helpers for giving/equipping/checking weapons using native functions.
    /// Uses weapon name or numeric hash to avoid depending on a WeaponHash enum in the SDK.
    /// </summary>
    public static class SafeInventory
    {
        public static bool TryPedHasWeapon(Ped ped, int weaponHash)
        {
            if (ped == null || !ped.Exists() || !ped.IsValid()) return false;

            try
            {
                return (bool)NativeFunction.Natives.HAS_PED_GOT_WEAPON(ped, weaponHash);
            }
            catch
            {
                return false;
            }
        }

        public static bool TryPedHasWeapon(Ped ped, string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return false;
            if (int.TryParse(weaponName, out var parsed)) return TryPedHasWeapon(ped, parsed);

            var hash = (int)Game.GetHashKey(weaponName);
            return TryPedHasWeapon(ped, hash);
        }

        public static void SafeGiveWeapon(Ped ped, int weaponHash, int ammo = 0, bool equip = true)
        {
            if (ped == null || !ped.Exists() || !ped.IsValid()) return;

            try
            {
                NativeFunction.Natives.GIVE_WEAPON_TO_PED(ped, weaponHash, ammo, false, equip);
            }
            catch
            {
                // best-effort - swallow
            }
        }

        public static void SafeGiveWeapon(Ped ped, string weaponName, int ammo = 0, bool equip = true)
        {
            if (string.IsNullOrEmpty(weaponName)) return;
            if (int.TryParse(weaponName, out var parsed)) { SafeGiveWeapon(ped, parsed, ammo, equip); return; }

            var hash = (int)Game.GetHashKey(weaponName);
            SafeGiveWeapon(ped, hash, ammo, equip);
        }

        public static void SafeEquipWeapon(Ped ped, int weaponHash)
        {
            if (ped == null || !ped.Exists() || !ped.IsValid()) return;

            try
            {
                NativeFunction.Natives.SET_CURRENT_PED_WEAPON(ped, weaponHash, true);
            }
            catch
            {
                // best-effort - swallow
            }
        }

        public static void SafeEquipWeapon(Ped ped, string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return;
            if (int.TryParse(weaponName, out var parsed)) { SafeEquipWeapon(ped, parsed); return; }

            var hash = (int)Game.GetHashKey(weaponName);
            SafeEquipWeapon(ped, hash);
        }
    }
}