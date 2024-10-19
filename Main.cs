global using static UnlimitedFuel.Logger;
using System.Reflection;
using System.IO;
using HarmonyLib;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;
using VTOLAPI;


namespace UnlimitedFuel;

public class UnlimitedFuelComponent : MonoBehaviour {
    private GameObject playerGameObject;
    private VehicleMaster vm;
    private FuelTank[] fuelTanks;
    private double fuelValue = 760.0;

    private void TryToSetFields() {
        if (playerGameObject && fuelTanks != null) return;
        if (fuelTanks is { Length: > 0 }) return;
        
        playerGameObject = VTAPI.GetPlayersVehicleGameObject();
        if (playerGameObject == null) return;
        vm = playerGameObject.GetComponent<VehicleMaster>();
        if (vm == null) {
            LogWarn("VehicleMaster not found");
            return;
        }
        
        fuelTanks = vm.fuelTanks;

        foreach (var fuelTank in fuelTanks) {
            if (!fuelTank) continue;
            if (fuelTank.currentFuel < 100.0) {
                fuelTank.currentFuel = 760.0;
                break;
            }
            fuelValue = fuelTank.currentFuel;
            break;
        }
    }

    private void Awake() {
        TryToSetFields();
    }
    
    private void Update() {
        if (fuelTanks == null) {
            TryToSetFields();
        }
        
        foreach (var fuelTank in fuelTanks ?? []) {
            var currentFuelField = AccessTools.Field(typeof(FuelTank), "currentFuel");
            if (currentFuelField == null) {
                LogError("currentFuelField is null");
                return;
            }
            currentFuelField.SetValue(fuelTank, fuelValue);
        }
    }
}

[ItemId("gabrielkaszewski.UnlimitedFuel")] // Harmony ID for your mod, make sure this is unique
public class Main : VtolMod {
    public string ModFolder;
    private GameObject playerGameObject;
    
    private void Awake() {
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        VTAPI.SceneLoaded += SceneLoaded;
        VTAPI.MissionReloaded += MissionReloaded;
    }

    private void MissionReloaded() {
        playerGameObject = VTAPI.GetPlayersVehicleGameObject();
        if (playerGameObject == null) return;
        playerGameObject.AddComponent<UnlimitedFuelComponent>();
    }

    public override void UnLoad() {
        // Destroy any objects
        Destroy(playerGameObject.GetComponent<UnlimitedFuelComponent>());
    }

    private void SceneLoaded(VTScenes scenes) {
        switch (scenes) {
            case VTScenes.CustomMapBase:
            case VTScenes.Akutan:
            case VTScenes.OpenWater:
            case VTScenes.MeshTerrain:
            case VTScenes.CustomMapBase_OverCloud:
                playerGameObject = VTAPI.GetPlayersVehicleGameObject();
                if (playerGameObject == null) return;
                playerGameObject.AddComponent<UnlimitedFuelComponent>();
                break;
        }
    }
}