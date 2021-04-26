using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class SeaCreatureController : MonoBehaviour
    {
        public MainSceneController scene;
        public SeaCreature creature;

        private readonly float walkSpeed = 1f;
        private readonly float targetDistanceTolerance = 0.1f;

        public GameObject tail;
        private float tailRotation = 0f;
        private float tailRotationSpeed = 0.25f;
        private readonly float tailRotationSpeedReeling = 1f;
        private readonly float maxTailRotation = 25;
        private float tailRotationDirection = 1f;

        private bool reeling = false;

        private bool attacked = false;
        private float attackProximity = 1f;

        private void Update()
        {
            if (!scene.GameplayPaused)
            {
                if (creature.path != null)
                {
                    float delta = Time.deltaTime;
                    Vector3 previous = transform.localPosition;
                    Vector3 destination = creature.path.First.Value;

                    Vector3 currentPosition;

                    if (Vector3.Distance(previous, destination) <= targetDistanceTolerance)
                    {
                        // We found the path node. Move to the next node.
                        if (creature.path.Count > 1)
                        {
                            creature.path.RemoveFirst();
                        }
                        else
                        {
                            // We're at the final destination. Shut it down.
                            creature.path = null;

                            // Destroy this object
                            RemoveSelf();
                        }
                    }

                    currentPosition = Vector3.MoveTowards(previous, destination, delta * walkSpeed);

                    transform.localPosition = currentPosition;


                    if (currentPosition.x < destination.x)
                    {
                        transform.eulerAngles = new Vector3(0, 90f, 0);
                    }
                    else if (currentPosition.x > destination.x)
                    {
                        transform.eulerAngles = new Vector3(0, -90f, 0);
                    }

                    /*Vector3 lookAtPos = new Vector3();
                    lookAtPos.x = destination.x;
                    lookAtPos.y = destination.y;
                    lookAtPos.z = destination.z;
                    lookAtPos = gameObject.transform.TransformPoint(lookAtPos);*/
                   // gameObject.transform.LookAt(lookAtPos);


                    // Calculate the rotation
                    if (tailRotationDirection < 0 && tailRotation <= -maxTailRotation)
                    {
                        tailRotationDirection = 1;
                    }
                    else if (tailRotationDirection > 0 && tailRotation >= maxTailRotation)
                    {
                        tailRotationDirection = -1;
                    }
                    tailRotation += (tailRotationDirection * tailRotationSpeed);
                    tail.transform.localEulerAngles = new Vector3(0, tailRotation, 0);



                    DoAttack();
                }
            }
        }

        public void Reeling()
        {
            reeling = true;
            tailRotationSpeed = tailRotationSpeedReeling;
        }

        public void DoAttack()
        {
            if (!reeling && !attacked)
            {
                Vector2Int position = scene.grid.GetGridPosition(transform.localPosition);
                GridModule module = scene.grid.IsAdjacentModule(position);
                if (module != null)
                {
                    int damage = 0;

                    // attack module
                    if (creature.type == SeaCreatureType.Fish)
                    {
                        damage = -1;
                    }
                    else if (creature.type == SeaCreatureType.BigFish)
                    {
                        damage = -2;
                    }
                    else if (creature.type == SeaCreatureType.Whale)
                    {
                        damage = -5;
                    }
                    else if (creature.type == SeaCreatureType.BigWhale)
                    {
                        damage = -20;
                    }

                    module.controller.UpdateHealth(damage);
                    scene.overlay.SetFloatingText(damage.ToString(), transform.position);

                    attacked = true;
                }
            }
        }

        private void RemoveSelf()
        {
            scene.creatures.RemoveSeaCreature(creature);
        }
    }
}