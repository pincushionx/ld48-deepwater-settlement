using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class GridController : MonoBehaviour
    {
        public MainSceneController scene;
        private Dictionary<Vector2Int, GridModule> modules = new Dictionary<Vector2Int, GridModule>();


        // Gridsize
        private int gridSize = 10;
        private int gridWidth = 10;

        private int bottomY = 9; // there are 10 levels. 9 is the bottom


        private Vector3 moduleDimensions = new Vector3(1.5f, -1f, -0.1f); // these are factors
        public readonly Vector3 moduleCenterOffset = new Vector3(0.75f, 0.5f, 0); // z doesn't matter much

        // Prefabs
        public GameObject modulePrefab;
        public GameObject addModulePrefab;

        public GameObject harpoonPrefab_right;
        public GameObject harpoonPrefab_left;

        public GameObject harpoonPrefab;
        public GameObject addHarpoonPrefab;


        private static int[][] depths = {
            new int[] { 0, 0 },
            new int[] { 1, 2 },
            new int[] { 3, 5 },
            new int[] { 6, 9 }
        };

        public GridModule GetModule(Vector2Int position)
        {
            if (modules.ContainsKey(position))
            {
                return modules[position];
            }
            return null;
        }


        // is there an adjacent module?
        public GridModule IsAdjacentModule(Vector2Int position)
        {
            foreach (Vector2Int neighbourOffset in neighbours)
            {
                GridModule module = GetModule(position + neighbourOffset);
                if (module != null)
                {
                    return module;
                }
            }
            return null;
        }

        public void AddModule(Vector2Int position, AddModuleComponent addModuleComponent)
        {
            Destroy(addModuleComponent.gameObject);

            GridModule module = new GridModule();
            module.type = addModuleComponent.Type;
            module.hollowGridPosition = addModuleComponent.hollowGridPosition;

            SetModule(position, module);
        }
        public void SetModule(Vector2Int position, GridModule module)
        {
            modules.Add(position, module);

            GameObject moduleGo;

            if (module.type == ModuleType.Harpoon)
            {
                if (module.hollowGridPosition == HollowGridPosition.Right)
                {
                    moduleGo = Instantiate(harpoonPrefab_left);
                }
                else
                {
                    moduleGo = Instantiate(harpoonPrefab_right);
                }
            }
            else
            {
                moduleGo = Instantiate(modulePrefab);
            }

            moduleGo.transform.SetParent(gameObject.transform);
            moduleGo.transform.localPosition = new Vector3(position.x * moduleDimensions.x, position.y * moduleDimensions.y, moduleDimensions.z);

            module.controller = moduleGo.GetComponent<ModuleComponent>();
            module.position = position;
            module.controller.gridModule = module;
            module.controller.scene = scene;


            // check win condition
            if (position.y >= bottomY)
            {
                scene.WinCondition();
            }

            GridChanged();
        }

        /*public void UpdateAvailableModules()
        {
            if (IsShowingAvailableModules())
            {
                HideAvailableModules();
                ShowAvailableModules();
            }
        }*/

        public void RemoveModule(GridModule module)
        {
            modules.Remove(module.position);
            Destroy(module.controller.gameObject);

            // Recalculate paths
            GridChanged();

            // maybe animate
            // kill anybody in it?
        }


        public void GridChanged()
        {
            // recalculate paths
            foreach (SeaCreature creature in scene.creatures.creatures)
            {
                if (creature.path != null)
                {
                    List<Vector3> pathsToRemove = new List<Vector3>();
                    foreach (Vector3 pathPosition in creature.path)
                    {
                        Vector2Int gridPosition = GetGridPosition(pathPosition);
                        GridModule module = GetModule(gridPosition);
                        if (module != null)
                        {
                            // there's something in the way. remove the node
                            pathsToRemove.Add(pathPosition);
                        }
                    }

                    // remove paths
                    if (pathsToRemove.Count > 0)
                    {
                        creature.path.Clear();

                        // just make the fish return
                        if (creature.controller.transform.localPosition.x < pathsToRemove[0].x)
                        {
                            Vector3 outPath = creature.controller.transform.localPosition;
                            outPath.x -= 10f;
                            creature.path.AddFirst(outPath);
                        }
                        else
                        {
                            Vector3 outPath = creature.controller.transform.localPosition;
                            outPath.x += 10f;
                            creature.path.AddFirst(outPath);
                        }

                        foreach (Vector3 pathToRemove in pathsToRemove)
                        {
                            creature.path.Remove(pathToRemove);
                        }
                    }
                }
            }
        }

        // Depths 0-3
        private List<Vector2Int> GetSwimmableSection(int depth)
        {
            List<Vector2Int> availableModule = new List<Vector2Int>();

            int[] depthLevels = depths[depth];

            int startingLevel = UnityEngine.Random.Range(depthLevels[0], depthLevels[1]+1);

            int side = UnityEngine.Random.Range(0, 2);

            if (side == 0)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Vector2Int position = new Vector2Int(x, startingLevel);
                    if (!modules.ContainsKey(position) && !availableModule.Contains(position))
                    {
                        availableModule.Add(position);
                    }
                    else
                    {
                        // stop when a wall is hit
                        break;
                    }
                }
            }
            else 
            {
                for (int x = gridWidth-1; x >= 0; x--)
                {
                    Vector2Int position = new Vector2Int(x, startingLevel);
                    if (!modules.ContainsKey(position) && !availableModule.Contains(position))
                    {
                        availableModule.Add(position);
                    }
                    else
                    {
                        // stop when a wall is hit
                        break;
                    }
                }
            }

            return availableModule;
        }

        public LinkedList<Vector3> GetSwimmablePath(int depth)
        {
            List<Vector2Int> section = GetSwimmableSection(depth);            
            List<Vector2Int> path = new List<Vector2Int>(section);

            // add the reversed list
            if (section.Count > 0)
            {
                for (int i = section.Count - 2; i >= 0; i--)
                {
                    path.Add(section[i]);
                }
            }

            LinkedList<Vector3> pathV3 = new LinkedList<Vector3>();

            foreach (Vector2Int position in path)
            {
                Vector3 worldPositionZero = GetWorldPosition(position, -0.2f);
                Vector3 worldPositionCenter = worldPositionZero + moduleCenterOffset;
                pathV3.AddLast(worldPositionCenter);
            }

            return pathV3;
        }

        public void ShowAvailableModules(ModuleType type)
        {
            HideAvailableModules();

            HashSet<Vector2Int> availableModule = new HashSet<Vector2Int>();

            foreach (KeyValuePair<Vector2Int, GridModule> kv in modules)
            {
                Vector2Int position = kv.Key;
                GridModule module = kv.Value;

                // Can only append to rooms
                if (module.type == ModuleType.Room)
                {
                    foreach (Vector2Int neighbourOffset in neighbours)
                    {
                        Vector2Int neighbour = position + neighbourOffset;
                        if (neighbour.x >= 0 && neighbour.x < gridSize
                         && neighbour.y >= 0 && neighbour.y < gridSize)
                        {
                            if (!modules.ContainsKey(neighbour) && !availableModule.Contains(neighbour))
                            {
                                availableModule.Add(neighbour);
                            }
                        }
                    }
                }
            }

            foreach (Vector2Int position in availableModule)
            {
                if (type == ModuleType.Room)
                {
                    GameObject moduleGo = Instantiate(addModulePrefab);
                    AddModuleComponent addModuleComponent = moduleGo.GetComponent<AddModuleComponent>();
                    addModuleComponent.Position = position;

                    moduleGo.transform.SetParent(gameObject.transform);

                    moduleGo.transform.localPosition = new Vector3(position.x * moduleDimensions.x, position.y * moduleDimensions.y, moduleDimensions.z);
                }
                else if (type == ModuleType.Harpoon)
                {
                    GameObject moduleGo;
                    AddModuleComponent addModuleComponent;

                    // Add harpoon slots
                    if (modules.ContainsKey(position + new Vector2Int(-1, 0)))
                    {
                        // left
                        moduleGo = Instantiate(addHarpoonPrefab);
                        addModuleComponent = moduleGo.GetComponent<AddModuleComponent>();
                        addModuleComponent.Position = position;
                        addModuleComponent.type = ModuleType.Harpoon;
                        addModuleComponent.hollowGridPosition = HollowGridPosition.Left;

                        moduleGo.transform.SetParent(gameObject.transform);
                        moduleGo.transform.localPosition = new Vector3(position.x * moduleDimensions.x, position.y * moduleDimensions.y, moduleDimensions.z + -0.1f);
                    }
                    if (modules.ContainsKey(position + new Vector2Int(1, 0)))
                    {
                        // right
                        moduleGo = Instantiate(addHarpoonPrefab);
                        addModuleComponent = moduleGo.GetComponent<AddModuleComponent>();
                        addModuleComponent.Position = position;
                        addModuleComponent.type = ModuleType.Harpoon;
                        addModuleComponent.hollowGridPosition = HollowGridPosition.Right;

                        moduleGo.transform.SetParent(gameObject.transform);
                        moduleGo.transform.localPosition = new Vector3(position.x * moduleDimensions.x, position.y * moduleDimensions.y, moduleDimensions.z + -0.1f);
                    }
                }
            }
        }

        public Vector3 GetWorldPosition(Vector2Int position, float zOffset)
        {
            return new Vector3(position.x * moduleDimensions.x, position.y * moduleDimensions.y, moduleDimensions.z + zOffset);
        }
        public Vector2Int GetGridPosition(Vector3 position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x / moduleDimensions.x), Mathf.CeilToInt(position.y / moduleDimensions.y));
        }

        public bool IsShowingAvailableModules()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject childGo = transform.GetChild(i).gameObject;
                AddModuleComponent childComponent = childGo.GetComponent<AddModuleComponent>();
                if (childComponent != null)
                {
                    return true;
                }
            }
            return false;
        }

        public void HideAvailableModules()
        {
            for (int i = transform.childCount-1; i >= 0; i--)
            {
                GameObject childGo = transform.GetChild(i).gameObject;
                AddModuleComponent childComponent = childGo.GetComponent<AddModuleComponent>();
                if (childComponent != null)
                {
                    Destroy(childGo);
                }
            }
        }

		public Vector3[] GetPath(Vector2Int fromPosition, Vector2Int destination)
        {
            List<Vector3> path = new List<Vector3>();

            SimplePriorityQueue<Vector2Int> frontier = new SimplePriorityQueue<Vector2Int>();
            Dictionary<Vector2Int, float> testedCells = new Dictionary<Vector2Int, float>();
            Dictionary<Vector2Int, Vector2Int> parentCells = new Dictionary<Vector2Int, Vector2Int>();

            Vector2Int currentCell = fromPosition;

            frontier.Enqueue(fromPosition, 0);
            testedCells.Add(fromPosition, 0);

            while (frontier.Count > 0)
            {

                currentCell = frontier.Dequeue();

                if (currentCell.Equals(destination))
                {
                    // found the destination
                    break;
                }

                // Get neighbours
                for (int i = 0; i < neighbours.Length; i++)
                {
                    //TODO voxel nodes need to return siblings. this will break
                    Vector2Int neighbour = currentCell + neighbours[i];
                    float neighbouringCellPathScore = modules.ContainsKey(neighbour) ? 1 : 0;

                    // test if the neighboring cell exists, is walkable 
                    if (neighbouringCellPathScore > 0)// && !IsNodeBlocked(neighbouringCell))
                    {
                        float newCost = testedCells[currentCell] + neighbouringCellPathScore;

                        // test if the neighboring cell hasn't been tested or has a better score than previously tested
                        if (!testedCells.ContainsKey(neighbour) || newCost < testedCells[neighbour])
                        {
                            // update the cell cost
                            testedCells[neighbour] = newCost;
                            parentCells[neighbour] = currentCell;

                            float guessedCost = newCost + GetHeuristic(neighbour, destination);
                            if (frontier.Contains(neighbour))
                            {
                                frontier.UpdatePriority(neighbour, guessedCost);
                            }
                            else
                            {
                                frontier.Enqueue(neighbour, guessedCost);
                            }
                        }
                    }
                }
            }


            if (currentCell.Equals(destination))
            {
                // build the path from parent cells
                Vector3 modulePosition = modules[currentCell].controller.transform.localPosition + modules[currentCell].controller.pathTarget.transform.localPosition;
                path.Add(modulePosition);

                while (parentCells.ContainsKey(currentCell))
                {
                    currentCell = parentCells[currentCell];
                    modulePosition = modules[currentCell].controller.transform.localPosition + modules[currentCell].controller.pathTarget.transform.localPosition;
                    path.Add(modulePosition);
                }
            }

            // reverse the path, so the first step is first
            path.Reverse();
            
            return path.ToArray();
        }

        private int GetHeuristic(Vector2Int currentPosition, Vector2Int destination)
        {
            Vector2Int diff = destination - currentPosition;
            return Math.Abs(diff.x) + Math.Abs(diff.y);
        }

        private Vector2Int[] neighbours = {
            new Vector2Int( 0,  1),
            new Vector2Int( 1,  0),
            new Vector2Int( 0, -1),
            new Vector2Int(-1,  0),
        };
    }

    public class GridModule
    {
        public Vector2Int position;
        public ModuleType type;
        public HollowGridPosition hollowGridPosition;
        public ModuleComponent controller; // if available
    }

    public class PathNode
    {
        public Vector3 position;
        public GridModule gridModule;
    }
    
    public enum ModuleType
    {
        Room,
        Harpoon
    }

    public enum HollowGridPosition
    {
        None,
        Left,
        Right
    }
}