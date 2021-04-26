using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class CastawayController : MonoBehaviour
    {
        public MainSceneController scene;

        public Castaway castaway;

        private float walkSpeed = 1;
        private float targetDistanceTolerance = 0.1f;
        public LinkedList<Vector3> path = null;

        private bool invited = false;

        private void Update()
        {
            if (!scene.GameplayPaused)
            {
                DoPath();
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

                        if (invited)
                        {
                            ConvertToCitizen();
                        }
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

        public void Invite()
        {
            Vector3 defaultModulePosition = scene.grid.GetWorldPosition(scene.defaultModule, -0.1f) + scene.grid.moduleCenterOffset;
            Vector3 topOfTheLadderPosition = new Vector3(defaultModulePosition.x, transform.localPosition.y, transform.localPosition.z);

            path = new LinkedList<Vector3>();
            path.AddFirst(topOfTheLadderPosition);
            path.AddLast(defaultModulePosition);

            scene.overlay.SetFloatingText("Thanks!", transform.position);

            invited = true;
        }

        private void ConvertToCitizen()
        {
            scene.citizens.ConvertCastawayToCitizen(castaway);
        }
    }
}