using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class MainSceneController : MonoBehaviour
    {
        public OverlayController overlay;
        public GridController grid;
        public CitizenManager citizens;
        public SeaCreatureManager creatures;
        public ShopManager shop;

        public GameObject ground;

        public bool GameplayPaused = false;
        public float gametime = 0;

        public CitizenController selectedCitizen = null;
        public ModuleComponent selectedModule = null;
        public float lastCreatureSpawn = 0;
        public float lastCastawaySpawn = 0;
        public int castawaySpawnTotal = 0;

        public readonly Vector2Int defaultModule = new Vector2Int(5, 0);
        public readonly Vector3 defaultCastawayWaitingPosition = new Vector3(7.5f, 1.15f, 0.25f);

        private void Start()
        {
            ground.SetActive(false);

            GridModule initialModule = new GridModule();
            initialModule.type = ModuleType.Room;
            grid.SetModule(defaultModule, initialModule);

            /*initialModule = new GridModule();
            initialModule.type = ModuleType.Room;
            grid.SetModule(new Vector2Int(2, 2), initialModule);

            initialModule = new GridModule();
            initialModule.type = ModuleType.Room;
            grid.SetModule(new Vector2Int(3, 2), initialModule);*/
            

            /*for (int x = 0; x < 10; x++)
            {
                initialModule = new GridModule();
                initialModule.type = ModuleType.Room;
                grid.SetModule(new Vector2Int(x, x), initialModule);
            }*/


            Citizen citizen = new Citizen();
            citizen.name = "Citizen 1";
            citizens.AddCitizen(citizen);

            citizen = new Citizen();
            citizen.name = "Citizen 2";
            citizens.AddCitizen(citizen);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoMouseClick();
            }

            if (!GameplayPaused)
            {
                gametime += Time.deltaTime;
                SpawnCreatures();
                SpawnCastaways();
            }
        }

        public void LoseConditionStarve()
        {
            GameplayPaused = true;
            overlay.MessageError("Oh no! You all starved to death!\n\nThanks for playing!");
        }

        public void WinCondition()
        {
            ground.SetActive(true);
            GameplayPaused = true;
            overlay.MessageError("It exists! There's a bottom! We'll all be rich!\n\nThanks for playing!");
        }

        private void SpawnCreatures()
        {
            int creatureCount = creatures.creatures.Count;
            int desiredCreatureCount = 9;
            float minTimeBetweenSpawns = 3f;

            if (creatureCount < desiredCreatureCount && (gametime - lastCreatureSpawn) > minTimeBetweenSpawns)
            {
                // spawn just one at a random depth
                int depth = UnityEngine.Random.Range(0, 4);

                SeaCreature creature = new SeaCreature();
                creature.type = (SeaCreatureType)depth;
                creatures.AddSeaCreature(creature);

                lastCreatureSpawn = gametime;
            }
        }

        private void SpawnCastaways()
        {
            int castawayCount = citizens.castaways.Count;
            int desiredCount = 1;
            float minTimeBetweenSpawns = 30f;

            if (castawayCount < desiredCount && (gametime - lastCastawaySpawn) > minTimeBetweenSpawns)
            {
                Castaway castaway = new Castaway();
                castaway.name = "Castaway " + ++castawaySpawnTotal;
                citizens.AddCastaway(castaway);

                lastCastawaySpawn = gametime;
            }
        }

        private void DoMouseClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //int mask = LayerMask.GetMask("Structure");

            //if (Physics.Raycast(ray, out hit, 100f, mask))
            if (Physics.Raycast(ray, out hit, 100f))
            {
                AddModuleComponent addModuleComponent = hit.collider.gameObject.GetComponentInParent<AddModuleComponent>();
                if (addModuleComponent != null)
                {
                    // Buying a room
                    if (addModuleComponent.type == ModuleType.Room &&!shop.BuyRoom())
                    {
                        // Can't afford the room. Present the user with a message.
                        overlay.MessageError("You need more materials to build this!");
                        return;
                    }
                    // Buying a harpoon
                    else if (addModuleComponent.type == ModuleType.Harpoon && !shop.BuyHarpoon())
                    {
                        // Can't afford the room. Present the user with a message.
                        overlay.MessageError("You need more materials to build this!");
                        return;
                    }

                    // Paid for. Add it.
                    grid.AddModule(addModuleComponent.Position, addModuleComponent);
                    grid.HideAvailableModules();
                    overlay.ExitEditModeClicked();
                    return;
                }

                CitizenController citizenController = hit.collider.gameObject.GetComponentInParent<CitizenController>();
                if (citizenController != null)
                {
                    SelectCharacter(citizenController);
                    return;
                }

                if (selectedCitizen != null)
                {
                    ModuleComponent moduleComponent = hit.collider.gameObject.GetComponent<ModuleComponent>();
                    if (moduleComponent != null)
                    {
                        moduleComponent.ShowSelection();
                        selectedModule = moduleComponent;

                        // assign the citizen to the module
                        selectedCitizen.AssignTask(moduleComponent);
                        return;
                    }
                }

                // Clear all stuff on click
                ClearSelections();
            }
            else
            {
                if (selectedCitizen != null && selectedModule != null)
                {
                    // The citizen started the task
                    if (selectedCitizen.pendingTask == null)
                    {
                        ClearSelections();
                    }
                }
            }
        }

        public void SelectCharacter(CitizenController citizenController)
        {
            citizens.ClearAllCitizenHighlights();

            citizenController.ShowCharacterSelection();
            selectedCitizen = citizenController;
        }

        private void ClearSelections()
        {
            if (selectedCitizen != null)
            {
                selectedCitizen.HideCharacterSelection();
                selectedCitizen = null;
            }
            if (selectedModule != null)
            {
                selectedModule.HideSelection();
                selectedModule = null;
            }
        }
    }
}