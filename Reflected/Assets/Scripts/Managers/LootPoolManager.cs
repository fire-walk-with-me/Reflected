using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootPoolManager : MonoBehaviour
{
    [SerializeField] WeightedRandomList<GameObject> powerupPool;
    [SerializeField] WeightedRandomList<GameObject> truePowerupPool;
    [SerializeField] WeightedRandomList<GameObject> mirrorPowerupPool;
    [SerializeField] WeightedRandomList<GameObject> collectablePool;
    [SerializeField] WeightedRandomList<GameObject> shopCollectables;
    [SerializeField] WeightedRandomList<GameObject> weaponPowerupPool;
    [SerializeField] WeightedRandomList<Rarity> rarityTiers;
    [SerializeField] Dictionary<PowerUpEffect, int> powerupPickAmount;
    int nrOfLegendaries = 0;

    private void Start()
    {
        powerupPickAmount = new Dictionary<PowerUpEffect, int>();
        foreach (var pair in powerupPool.list)
        {
            powerupPickAmount.Add(pair.item.GetComponent<InteractablePowerUp>().powerUpEffect, 0);
        }
    }

    public void SetRarityTiers(int commonWeight, int rareWeight, int epicWeight)
    {
        rarityTiers.SetWeight(0, commonWeight);
        rarityTiers.SetWeight(1, rareWeight);
        rarityTiers.SetWeight(2, epicWeight);
    }

    public void IncreaseRarity()
    {
        rarityTiers.IncreaseWeight(1, 1);
        rarityTiers.IncreaseWeight(2, 2);
        if(rarityTiers.list[3].weight <= 8 && rarityTiers.list[3].weight > 0)
            rarityTiers.IncreaseWeight(3, 1);
    }

    public void SetHealthItemWeighs(bool enableHealthItems)
    {
        if (enableHealthItems)
        {
            collectablePool.SetWeight(1, 3);
            collectablePool.SetWeight(5, 2);
            collectablePool.SetWeight(6, 1);
            
        }
        else
        {
            foreach (var pair in collectablePool.list)
            {
                if (pair.item.GetComponent<Health>())
                {
                    pair.weight = 0;
                }
            }
        }
    }

    private void OnEnable()
    {
        InteractablePowerUp.OnPowerUPCollected += AddPowerupPickRate;
        Player.OnHealthMaxed += SetHealthItemWeighs;
    }

    private void OnDisable()
    {
        InteractablePowerUp.OnPowerUPCollected -= AddPowerupPickRate;
        Player.OnHealthMaxed -= SetHealthItemWeighs;
    }

    private void AddPowerupPickRate(PowerUpEffect powerupEffectData)
    {
        powerupPickAmount[powerupEffectData] += 1;        
        if(powerupEffectData.type == "weapon")
        {
            nrOfLegendaries++;
            if(nrOfLegendaries >= 2)
            {
                rarityTiers.list[3].SetWeight(0);
            }

            foreach (var pair in weaponPowerupPool.list)
            {
                if (powerupEffectData == pair.item.GetComponent<InteractablePowerUp>().powerUpEffect)
                {
                    pair.SetWeight(0);
                }
            }
        }
        else
        {
            float average = AveragePickRate();
            if (powerupPickAmount[powerupEffectData] >= average)
            {
                foreach (var pair in powerupPool.list) //For shop
                {
                    if (powerupEffectData == pair.item.GetComponent<InteractablePowerUp>().powerUpEffect)
                    {
                        pair.SetWeight(pair.weight / 2);
                    }
                }

                if(powerupEffectData.type == "offensive") //For chests
                {
                    foreach (var pair in truePowerupPool.list)
                    {
                        if (powerupEffectData == pair.item.GetComponent<InteractablePowerUp>().powerUpEffect)
                        {
                            pair.SetWeight(pair.weight / 2);
                        }
                    }
                }
                else if(powerupEffectData.type == "defensive")
                {
                    foreach (var pair in mirrorPowerupPool.list)
                    {
                        if (powerupEffectData == pair.item.GetComponent<InteractablePowerUp>().powerUpEffect)
                        {
                            pair.SetWeight(pair.weight / 2);
                        }
                    }
                }
            }
        }      
       
    }

    public WeightedRandomList<GameObject> GetPowerupPool(bool dimension)
    {
        if (dimension) return truePowerupPool;
        else return mirrorPowerupPool;
    }

    public WeightedRandomList<GameObject> GetPowerupPool()
    {
        return powerupPool;
    }

    public WeightedRandomList<GameObject> GetCollectablePool()
    {
        return collectablePool;
    }

    public WeightedRandomList<GameObject> GetWeaponPowerupPool()
    {
        return weaponPowerupPool;
    }

    public WeightedRandomList<GameObject> GetShopCollectables()
    {
        return shopCollectables;
    }

    public WeightedRandomList<Rarity> GetRarityList()
    {
        return rarityTiers;
    }

    public Rarity GetRandomRarity()
    {
        return rarityTiers.GetRandom();
    }

    public int GetAmountPicked(PowerUpEffect powerupEffectData)
    {
        return powerupPickAmount[powerupEffectData];
    }

    private float AveragePickRate()
    {
        float sum = 0;
        foreach (var item in powerupPickAmount)
        {
            sum += item.Value;
        }

        return sum / powerupPickAmount.Count;        
    }
}
