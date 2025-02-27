//
// Script created by Valter Lindecrantz at the Game Development Program, MAU, 2022.
//

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// DimensionManager description
/// </summary>
public class DimensionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PostProcessingManager postProcessingManager;

    [Header("Lighting")]
    [SerializeField] private GameObject trueLighting;
    [SerializeField] private GameObject mirrorLighting;

    [Header("Skybox")]
    [SerializeField] private Material trueSkybox;
    [SerializeField] private Material mirrorSkybox;

    [Header("Music")]
    [SerializeField] private MusicManager musicManager;

    [Header("Changeable")]
    [SerializeField] private List<ChangeableObject> changeableObjects;

    [Header("Ability")]
    //[SerializeField] private GameObject chargeBar;
    [SerializeField] private int maximumCharges;
    [SerializeField] private int currentCharges;

    [Header("Read Only")]
    [ReadOnly][SerializeField] private PlayerStatSystem statSystem;

    private static Dimension currentDimension;
    WeightedRandomList<GameObject> LootDropList;
    EnemyStatSystem enemyStatSystem;

    public static UnityEvent DimensionChanged = new UnityEvent();

    // Properties

    public static Dimension CurrentDimension => currentDimension;

    /// <summary>
    /// Returns true if the current dimension is True
    /// </summary>
    public static bool True => currentDimension == Dimension.True;

    /// <summary>
    /// Returns true if the current dimension is Mirror
    /// </summary>
    public static bool Mirror => currentDimension == Dimension.Mirror;

    private void Awake()
    {
        musicManager = GameObject.Find("Music Manager").GetComponent<MusicManager>();
        currentDimension = Dimension.True;
    }

    /// <summary>
    /// Swaps dimension if fully charged. Returns whether or not the swap was successful
    /// </summary>
    public bool TrySwap()
    {
        if (!CanSwap())
            return false;

        if (True)
        {
            LootDropList.SetWeight(3, 4);
            LootDropList.SetWeight(2, 0);
        }
        else if (Mirror)
        {
            LootDropList.SetWeight(2, 4);
            LootDropList.SetWeight(3, 0);
        }

        //enemyStatSystem.ApplyNewStats(True);

        ForcedSwap();
        ResetCharges();
        return true;
    }

    /// <summary>
    /// Swaps dimension without affecting charges.
    /// </summary>
    public void ForcedSwap()
    {
        SetDimension(Mirror ? Dimension.True : Dimension.Mirror);
    }

    /// <summary>
    /// Sets the current dimension to Mirror regardless of current charges
    /// </summary>
    public void SetDimension(Dimension dimension)
    {
        currentDimension = dimension;

        if (True)
            postProcessingManager.UseTrueProfile();
        else
            postProcessingManager.UseMirrorProfile();

        musicManager.SwapMusicScore(dimension);

        trueLighting.SetActive(True);
        mirrorLighting.SetActive(Mirror);

        RenderSettings.skybox = True ? trueSkybox : mirrorSkybox;
        RenderSettings.fog = Mirror;

        foreach (ChangeableObject changeableObject in changeableObjects)
            changeableObject.UpdateMesh();

        DimensionChanged.Invoke();
    }

    public void AddChangeableObject(ChangeableObject newObject)
    {
        changeableObjects.Add(newObject);
    }
    
    public bool CanSwap()
    {
        return currentCharges >= maximumCharges;
    }

    public void SetMaxCharges(int newCharges)
    {
        maximumCharges = newCharges;
    }

    public int GetCurrentCharges()
    {
        return currentCharges;
    }

    public int GetMaxCharges()
    {
        return maximumCharges;
    }
    public void GainCharges(int addCharges)
    {
        currentCharges = Mathf.Clamp(currentCharges + addCharges, 0, maximumCharges);
        if(currentCharges >= GetMaxCharges())
        {
            LootDropList.SetWeight(4, 0);
        }
    }

    public void ResetCharges()
    {
        currentCharges = 0;
        LootDropList.SetWeight(4, 2);
    }

    public void SetStatSystem(PlayerStatSystem newStatSystem)
    {
        statSystem = newStatSystem;
    }

    public void FindSystems()
    {
        LootDropList = GameObject.Find("LootPoolManager").GetComponent<LootPoolManager>().GetCollectablePool();
        enemyStatSystem = GameObject.Find("EnemyStatSystem").GetComponent<EnemyStatSystem>();
    }
}