using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD48
{
    public class ModuleComponent : MonoBehaviour
    {
        public MainSceneController scene;
        public GridModule gridModule;
        public GameObject pathTarget;
        public Image healthBar;
        public GameObject healthBarContainer;

        public GameObject selectionHighlight;

        public int health = 100;
        //public readonly float healthModifier = 1f;

        private void Awake()
        {
            health = 100;
            UpdateHealth(0);
        }

        public void UpdateHealth(int change)
        {
            health += change;

            healthBar.fillAmount = health / 100f;


            if (health >= 100)
            {
                healthBarContainer.SetActive(false);
            }
            else
            {
                healthBarContainer.SetActive(true);
            }

            if (health <= 0)
            {
                scene.overlay.SetFloatingText("Crack! Kachunk! (it broke)", transform.position);

                scene.grid.RemoveModule(gridModule);

                // maybe animate / make sounds / something
            }
        }

        public void ShowSelection()
        {
            // disable for now
            //selectionHighlight.SetActive(true);
        }
        public void HideSelection()
        {
            selectionHighlight.SetActive(false);
        }
    }

}