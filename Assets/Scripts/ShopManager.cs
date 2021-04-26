using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class ShopManager : MonoBehaviour
    {
        public MainSceneController scene;

        // Currency
        public int currencyBone = 200;
        [HideInInspector]
        public int currencyPlastic = 200;
        [HideInInspector]
        public int currencyGlue = 100;
        [HideInInspector]
        public int currencyFood = 200;

        // Prices
        [HideInInspector]
        public readonly int costRoomBone = 20;
        [HideInInspector]
        public readonly int costRoomPlastic = 50;

        [HideInInspector]
        public readonly int costHarpoonBone = 50;
        [HideInInspector]
        public readonly int costHarpoonPlastic = 20;

        [HideInInspector]
        public readonly int costFixPlastic = 1;
        [HideInInspector]
        public readonly int costFixGlue = 0;

        private float timeBetweenBites = 2f;
        private float lastBite = 0f;

        private void Start()
        {
            currencyBone = 200;
            currencyPlastic = 200;
            currencyGlue = 100;
            currencyFood = 200;
            scene.overlay.UpdateShopCosts();
        }

        private void Update()
        {
            // eat food over time

            if (!scene.GameplayPaused)
            {
                DoEatFood();
            }
        }

        private void DoEatFood()
        {
            int mouths = scene.citizens.citizens.Count;

            if ((scene.gametime - lastBite) > timeBetweenBites)
            {
                currencyFood -= mouths;

                // update UI
                CurrencyUpdated();

                lastBite = scene.gametime;
            }

            if (currencyFood <= 0) {
                scene.LoseConditionStarve();
            }
        }

        public void CurrencyUpdated()
        {
            scene.overlay.UpdateShop();
        }

        public bool BuyRoom()
        {
            if (currencyBone < costRoomBone)
            {
                return false;
            }
            if (currencyPlastic < costRoomPlastic)
            {
                return false;
            }

            currencyBone -= costRoomBone;
            currencyPlastic -= costRoomPlastic;

            CurrencyUpdated();

            return true;
        }
        public bool BuyHarpoon()
        {
            if (currencyBone < costHarpoonBone)
            {
                return false;
            }
            if (currencyPlastic < costHarpoonPlastic)
            {
                return false;
            }

            currencyBone -= costHarpoonBone;
            currencyPlastic -= costHarpoonPlastic;

            CurrencyUpdated();

            return true;
        }

        // returns the amount fixed
        public int FixModule(int desiredAmount)
        {
            if (currencyPlastic < costFixPlastic)
            {
                return 0;
            }
            if (currencyGlue < costFixGlue)
            {
                return 0;
            }

            int purchaseAmount = desiredAmount;

            int totalCostFixPlastic = desiredAmount * costFixPlastic;
            int amountPurchaseablePlastic = currencyPlastic / costFixPlastic;
            if (amountPurchaseablePlastic < purchaseAmount)
            {
                purchaseAmount = amountPurchaseablePlastic;
            }

            if (costFixGlue > 0)
            {
                int totalCostFixGlue = desiredAmount * costFixGlue;
                int amountPurchaseableGlue = currencyGlue / costFixGlue;
                if (amountPurchaseableGlue < purchaseAmount)
                {
                    purchaseAmount = amountPurchaseableGlue;
                }
            }


            currencyPlastic -= costFixPlastic * purchaseAmount;
            currencyGlue -= costFixGlue * purchaseAmount;

            CurrencyUpdated();

            return purchaseAmount;
        }
    }
}