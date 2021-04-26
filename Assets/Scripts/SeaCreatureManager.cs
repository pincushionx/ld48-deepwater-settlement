using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    // character list UI
    // set nodes by grid
    // point and click pathfinding

    public class SeaCreatureManager : MonoBehaviour
    {
        public MainSceneController scene;
        public HashSet<SeaCreature> creatures = new HashSet<SeaCreature>();

        public GameObject creaturePrefabDepth0;
        public GameObject creaturePrefabDepth1;
        public GameObject creaturePrefabDepth2;
        public GameObject creaturePrefabDepth3;

        public float seaCreatureDepth = -0.1f;

        public void AddSeaCreature(SeaCreature creature)
        {
            int depth = (int)creature.type;

            LinkedList<Vector3> path = scene.grid.GetSwimmablePath(depth);
            if (path.Count == 0)
            {
                // failed to get path
                return;
            }


            creatures.Add(creature);

            GameObject creatureGo;
            if (depth == 0)
            {
                creatureGo = Instantiate(creaturePrefabDepth0);
                creature.currencyFood = Random.Range(2, 10);
                creature.currencyBone = Random.Range(1, 5);
                creature.currencyPlastic = Random.Range(10, 20);
            }
            else if (depth == 1)
            {
                creatureGo = Instantiate(creaturePrefabDepth1);
                creature.currencyFood = Random.Range(5, 20);
                creature.currencyBone = Random.Range(3, 10);
                creature.currencyPlastic = Random.Range(1, 10);
            }
            else if (depth == 2)
            {
                creatureGo = Instantiate(creaturePrefabDepth2);
                creature.currencyFood = Random.Range(10, 30);
                creature.currencyBone = Random.Range(5, 20);
                creature.currencyPlastic = Random.Range(0, 0);
            }
            else
            {
                creatureGo = Instantiate(creaturePrefabDepth3);
                creature.currencyFood = Random.Range(20, 50);
                creature.currencyBone = Random.Range(10, 40);
                creature.currencyPlastic = Random.Range(0, 0);
            }

            creature.path = path;
            creature.controller = creatureGo.GetComponent<SeaCreatureController>();
            creature.controller.creature = creature;
            creature.controller.scene = scene; // required for pathing
            creatureGo.transform.SetParent(scene.grid.transform);

            creatureGo.transform.localPosition = path.First.Value;
        }

        public void RemoveSeaCreature(SeaCreature creature)
        {
            creatures.Remove(creature);
            Destroy(creature.controller.gameObject);
        }
    }

    public class SeaCreature
    {
        public SeaCreatureType type;
        public int currencyFood = 0;
        public int currencyBone = 0;
        public int currencyPlastic = 0;
        public LinkedList<Vector3> path;
        public SeaCreatureController controller; // if available
    }
    public enum SeaCreatureType
    {
        Fish = 0,
        BigFish = 1,
        Whale = 2,
        BigWhale = 3
    }
}