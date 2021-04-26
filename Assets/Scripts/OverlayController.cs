using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD48
{
    public class OverlayController : MonoBehaviour
    {
        public MainSceneController scene;

        public Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        public Dictionary<string, Text> texts = new Dictionary<string, Text>();
        public Dictionary<string, GameObject> tutorials = new Dictionary<string, GameObject>();

        public VerticalLayoutGroup citizenListPanel;
        public GameObject tutorialPanel;
        public GameObject floatingTextContainer;

        // Prefabs
        public GameObject citizenListItemPrefab;
        public GameObject floatingTextPrefab;


        private void Awake()
        {
            // Index the buttons
            Button[] buttonComponents = GetComponentsInChildren<Button>();
            foreach (Button buttonComponent in buttonComponents) {
                if (!buttons.ContainsKey(buttonComponent.name))
                {
                    buttons.Add(buttonComponent.name, buttonComponent);
                }
            }

            // Index the text
            Text[] textComponents = GetComponentsInChildren<Text>();
            foreach (Text textComponent in textComponents)
            {
                if (!texts.ContainsKey(textComponent.name))
                {
                    texts.Add(textComponent.name, textComponent);
                }
            }

            // Index the tutorials
            tutorialPanel.SetActive(true);
            for (int i = 0; i < tutorialPanel.transform.childCount; i++)
            {
                GameObject tutorialGo = tutorialPanel.transform.GetChild(i).gameObject;
                tutorials.Add(tutorialGo.name, tutorialGo);

                // Add event to the ok button
                Button tutorialButton = tutorialGo.GetComponentInChildren<Button>();
                tutorialButton.onClick.AddListener(() => TutorialOkClicked(tutorialGo));

                tutorialGo.SetActive(false);
            }
           

            // Disable buttons that need disabling
            buttons["ExitEditModeButton"].gameObject.SetActive(false);

            // Add Events
            buttons["BuyRoomButton"].onClick.AddListener(() => BuyRoomClicked());
            buttons["BuyHarpoonButton"].onClick.AddListener(() => BuyHarpoonClicked());

            buttons["ExitEditModeButton"].onClick.AddListener(() => ExitEditModeClicked());

            buttons["InviteCitizenButton"].onClick.AddListener(() => InviteCastawayClicked());

            buttons["ShowTutorial"].onClick.AddListener(() => ShowTutorial());
            

            // Initialize shop
            UpdateShop();
        }

        private void Start()
        {
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            // Force the vertical layout groups to update
            foreach (var layoutGroup in GetComponentsInChildren<LayoutGroup>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }
        }

        public void UpdateShop()
        {
            texts["Food"].text = "Food: " + scene.shop.currencyFood;
            texts["Bone"].text = "Bone: " + scene.shop.currencyBone;
            texts["Plastic"].text = "Plastic: " + scene.shop.currencyPlastic;
        }
        public void UpdateShopCosts()
        {
            texts["BuyRoomText"].text = "Room: " + scene.shop.costRoomBone + " bone, " + scene.shop.costRoomPlastic + " plastic";
            texts["BuyHarpoonText"].text = "Harpoon: " + scene.shop.costHarpoonBone + " bone, " + scene.shop.costHarpoonPlastic + " plastic";
            texts["FixStationText"].text = "Fix (per 1% health): " + scene.shop.costFixPlastic + " plastic";
        }

        public void BuyRoomClicked()
        {
            scene.grid.ShowAvailableModules(ModuleType.Room);
            buttons["BuyRoomButton"].gameObject.SetActive(false);
            buttons["BuyHarpoonButton"].gameObject.SetActive(false);
            buttons["ExitEditModeButton"].gameObject.SetActive(true);
            RefreshLayout();
        }
        public void ExitEditModeClicked()
        {
            scene.grid.HideAvailableModules();
            buttons["BuyRoomButton"].gameObject.SetActive(true);
            buttons["BuyHarpoonButton"].gameObject.SetActive(true);
            buttons["ExitEditModeButton"].gameObject.SetActive(false);
            RefreshLayout();
        }

        public void BuyHarpoonClicked()
        {
            scene.grid.ShowAvailableModules(ModuleType.Harpoon);
            buttons["BuyHarpoonButton"].gameObject.SetActive(false);
            buttons["BuyRoomButton"].gameObject.SetActive(false);
            buttons["ExitEditModeButton"].gameObject.SetActive(true);
            RefreshLayout();
        }

        public void InviteCastawayClicked()
        {
            Castaway castaway = scene.citizens.InviteCastaway();
            if (castaway != null)
            {
                // can announce him
            }
            else
            {
                // no castaway to invite
            }
        }

        public void AddCitizen(Citizen citizen)
        {
            GameObject listItemGo = Instantiate(citizenListItemPrefab);
            CitizenListItemController controller = listItemGo.GetComponent<CitizenListItemController>();
            controller.overlay = this;
            controller.Citizen = citizen;
            listItemGo.transform.SetParent(citizenListPanel.transform);
            listItemGo.transform.localScale = Vector3.one;
        }
        public void CitizenClicked(CitizenListItemController citizenListItemController)
        {
            scene.SelectCharacter(citizenListItemController.Citizen.controller);
        }


        public void ShowTutorial()
        {
            tutorials["Tut0"].SetActive(true);

            scene.GameplayPaused = true;
        }

        public void TutorialOkClicked(GameObject tutorialPanel)
        {
            if (tutorialPanel.name == "Tut0")
            {
                tutorialPanel.SetActive(false);
                tutorials["Tut1"].SetActive(true);
            }
            else if (tutorialPanel.name == "Tut1")
            {
                tutorialPanel.SetActive(false);
                tutorials["Tut2"].SetActive(true);
            }
            else if (tutorialPanel.name == "Tut2")
            {
                tutorialPanel.SetActive(false);
                tutorials["Tut3"].SetActive(true);
            }
            else if (tutorialPanel.name == "Tut3")
            {
                tutorialPanel.SetActive(false);

                scene.GameplayPaused = false;
            }
            else // messages
            {
                tutorialPanel.SetActive(false);
                scene.GameplayPaused = false;
            }
        }

        public void MessageError(string message)
        {
            texts["MessageShortText"].text = message;
            tutorials["MessageShort"].SetActive(true);
            scene.GameplayPaused = true;
        }

        public void SetFloatingText(string message, Vector3 position)
        {
            GameObject floatingTextGo = Instantiate(floatingTextPrefab);
            FloatingTextController controller = floatingTextGo.GetComponent<FloatingTextController>();
            controller.SetText(message);

            floatingTextGo.transform.SetParent(floatingTextContainer.transform);
            //floatingTextGo.transform.localScale = Vector3.one;

            position.z -= 0.5f;
            floatingTextGo.transform.position = position;
        }
    }
}