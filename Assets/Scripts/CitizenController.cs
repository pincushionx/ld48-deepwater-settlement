using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class CitizenController : MonoBehaviour
    {
        public MainSceneController scene;
        public Citizen citizen;

        public GameObject selectionHighlight;
        public GameObject aoeHighlight;


        public ModuleComponent pendingTask = null;
        public ModuleComponent currentTask = null;

        private float walkSpeed = 1;
        private float targetDistanceTolerance = 0.1f;
        public LinkedList<Vector3> path = null;

        public float harpoonRange = 2.5f; // world coords

        public float reelSpeed = 3f;
        private SeaCreatureController reelingCreature = null;
        private float aoeRotation = 0f;
        private float aoeRotationSpeed = 0.1f;
        private float maxAoeRotation = 20;
        private float aoeRotationDirection = 1f;

        private void Start()
        {
            scene.overlay.SetFloatingText("Hi, I'm " + citizen.name, transform.position);
        }

        private void Update()
        {
            if (!scene.GameplayPaused)
            {
                DoPath();
                DoHarpoon();
                DoFixModule();
            }
        }

        private void DoFixModule()
        {
            if (currentTask != null)
            {
                // This works for any module type
                if (currentTask.gridModule.controller.health < 100)
                {
                    int fixedAmount = scene.shop.FixModule(100 - currentTask.gridModule.controller.health);
                    scene.shop.CurrencyUpdated();
                    currentTask.UpdateHealth(fixedAmount);

                    if (fixedAmount == 0)
                    {
                        scene.overlay.SetFloatingText("I need more plastic to fix this.", transform.position);
                    }
                    else
                    {
                        scene.overlay.SetFloatingText(fixedAmount + "% Fixed", transform.position);
                    }
                }
            }
        }

        private void DoHarpoon()
        {
            if (currentTask != null && currentTask.gridModule.type == ModuleType.Harpoon)
            {
                if (reelingCreature != null)
                {




                    float delta = Time.deltaTime;
                    Vector3 previous = reelingCreature.transform.localPosition;
                    Vector3 destination = transform.localPosition;


                    if (Vector3.Distance(previous, destination) <= targetDistanceTolerance)
                    {
                        // Got em

                        // Get the currency
                        scene.shop.currencyFood += reelingCreature.creature.currencyFood;
                        scene.shop.currencyBone += reelingCreature.creature.currencyBone;
                        scene.shop.currencyPlastic += reelingCreature.creature.currencyPlastic;
                        scene.shop.CurrencyUpdated();

                        scene.overlay.SetFloatingText("+" + reelingCreature.creature.currencyFood + " Food\n" +
                            "+" + reelingCreature.creature.currencyBone + " Bone\n" +
                            "+" + reelingCreature.creature.currencyPlastic + " Plastic",  transform.position);

                        // remove the GO
                        scene.creatures.RemoveSeaCreature(reelingCreature.creature);

                        // reset harpooning
                        aoeHighlight.SetActive(true);

                        // maybe a chaching sound effect here
                        // maybe some floating $$ text
                    }
                    else
                    {
                        // Still reeling
                        Vector3 currentPosition;
                        currentPosition = Vector3.MoveTowards(previous, destination, delta * reelSpeed);

                        reelingCreature.transform.localPosition = currentPosition;

                        Vector3 lookAtPos = new Vector3();
                        lookAtPos.x = destination.x;
                        lookAtPos.y = reelingCreature.transform.localPosition.y;
                        lookAtPos.z = destination.z;
                        gameObject.transform.LookAt(lookAtPos);
                    }
                }
                else
                {
                    if (!aoeHighlight.activeSelf)
                    {
                        aoeHighlight.SetActive(true);
                    }

                    // Calculate the rotation
                    if (aoeRotationDirection < 0 && aoeRotation <= -maxAoeRotation) {
                        aoeRotationDirection = 1;
                    }
                    else if (aoeRotationDirection > 0 && aoeRotation >= maxAoeRotation)
                    {
                        aoeRotationDirection = -1;
                    }
                    aoeRotation += (aoeRotationDirection * aoeRotationSpeed);
                    aoeHighlight.transform.localEulerAngles = new Vector3(aoeRotation, 0, 0);

                    // rotate the aoe up and down
                    if (currentTask.gridModule.hollowGridPosition == HollowGridPosition.Left)
                    {
                        // look right
                        gameObject.transform.LookAt(Vector3.right * 1000);
                    }
                    else
                    {
                        // look left
                        gameObject.transform.LookAt(Vector3.left * 1000);
                    }


                    // check to see if it hit
                    RaycastHit hitInfo;
                    bool hit = Physics.Raycast(aoeHighlight.transform.position, aoeHighlight.transform.forward, out hitInfo, harpoonRange, LayerMask.GetMask("Creature"));

                    if (hit)
                    {
                        SeaCreatureController creature = hitInfo.collider.gameObject.GetComponentInParent<SeaCreatureController>();

                        if (creature != null)
                        {
                            // Caught something
                            reelingCreature = creature;
                            creature.Reeling();
                            aoeHighlight.SetActive(false);

                            // reeling effects (zipper sound)
                            //game.sound.playSound("hey");
                            //StartCoroutine("ResetCoroutine");
                            //game.Caught();
                        }
                    }
                }
            }
        }

        private void DoPath()
        {
            if (path != null)
            {
                float delta = Time.deltaTime;
                Vector3 previous = transform.localPosition;
                Vector3 destination = path.First.Value;

                Vector3 currentPosition;

                if (Vector3.Distance(previous, destination) <= targetDistanceTolerance)
                {
                    // We found the path node. Move to the next node.
                    if (path.Count > 1)
                    {
                        currentPosition = destination;
                        path.RemoveFirst();
                    }
                    else
                    {
                        // We're at the final destination. Shut it down.
                        currentPosition = destination;
                        path = null;
                        currentTask = pendingTask;
                        pendingTask = null;
                    }
                }
                else
                {
                    currentPosition = Vector3.MoveTowards(previous, destination, delta * walkSpeed);
                }

                transform.localPosition = currentPosition;

                Vector3 lookAtPos = new Vector3();
                lookAtPos.x = destination.x;
                lookAtPos.y = transform.localPosition.y;
                lookAtPos.z = destination.z;
                gameObject.transform.LookAt(lookAtPos);
            }
        }

        public void ShowCharacterSelection()
        {
            scene.overlay.SetFloatingText("What do you need?", transform.position);
            // disable highlights for now
            //selectionHighlight.SetActive(true);
        }
        public void HideCharacterSelection()
        {
            selectionHighlight.SetActive(false);
        }

        private void StopDoingStuff()
        {
            if (currentTask != null) {
                currentTask = null;

                // if harpooning
                aoeHighlight.SetActive(false);
            }
        }

        public void AssignTask(ModuleComponent module)
        {
            StopDoingStuff();

            currentTask = null;
            pendingTask = module;
            PathTo(module.gridModule.position);

            scene.overlay.SetFloatingText("I'm on it!", transform.position);
        }

        private void PathTo(Vector2Int destination)
        {
            Vector2Int fromPosition = scene.grid.GetGridPosition(transform.localPosition);
            Vector3[] pathNodes = scene.grid.GetPath(fromPosition, destination);

            if (pathNodes.Length == 0)
            {
                return;
            }
            path = new LinkedList<Vector3>(pathNodes);
        }
    }
}