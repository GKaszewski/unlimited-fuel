global using static UnlimitedFuel.Logger;
using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using VTOLAPI;


namespace UnlimitedFuel;

[ItemId("gabrielkaszewski.UnlimitedFuel")] // Harmony ID for your mod, make sure this is unique
public class Main : VtolMod {
    public string ModFolder;
    
    private void Awake() {
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Log($"Awake at {ModFolder}");
        
        VTAPI.SceneLoaded += SceneLoaded;
    }

    public override void UnLoad() {
        // Destroy any objects
    }

    private void SceneLoaded(VTScenes scenes) {
        switch (scenes) {
            case VTScenes.CustomMapBase:
            case VTScenes.Akutan:
            case VTScenes.OpenWater:
            case VTScenes.MeshTerrain:
            case VTScenes.CustomMapBase_OverCloud:
                break;
        }
    }
}

[HarmonyPatch(typeof(FuelTank))]
public class FuelTankPatch {
    
    [HarmonyPatch(nameof(FuelTank.RequestFuel), new Type[] { typeof(double) })]
    [HarmonyPostfix]
    private static void Postfix(double deltaFuel, FuelTank __instance) {
        var playerGameObject = VTAPI.GetPlayersVehicleGameObject();
        if (playerGameObject == null) return;
        var vm = playerGameObject.GetComponent<VehicleMaster>();
        if (vm == null) {
            LogWarn("VehicleMaster not found");
            return;
        }

        var fuelTanks = vm.fuelTanks;
        var isAtLeastOneFuelTankPlayers = false;
        foreach (var fuelTank in fuelTanks) {
            if (fuelTank == __instance) {
                isAtLeastOneFuelTankPlayers = true;
                break;
            }
        }
        
        if (!isAtLeastOneFuelTankPlayers) return;
        __instance.currentFuel = 760.0;
    }

    static bool Prepare() => true;
}