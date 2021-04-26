using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    // character list UI
    // set nodes by grid
    // point and click pathfinding

    public class CitizenManager : MonoBehaviour
    {
        public MainSceneController scene;

        public Dictionary<string, Castaway> castaways = new Dictionary<string, Castaway>();
        public Dictionary<string, Citizen> citizens = new Dictionary<string, Citizen>();

        public GameObject citizenPrefab;
        public GameObject castawayPrefab;

        public void AddCitizen(Citizen citizen)
        {
            citizens.Add(citizen.name, citizen);

            scene.overlay.AddCitizen(citizen);

            GameObject citizenGo = Instantiate(citizenPrefab);
            citizen.controller = citizenGo.GetComponent<CitizenController>();
            citizen.controller.citizen = citizen;
            citizen.controller.scene = scene; // required for pathing
            citizenGo.transform.SetParent(scene.grid.transform);

            Vector3 defaultModulePosition = scene.grid.GetWorldPosition(scene.defaultModule, -0.1f) + scene.grid.moduleCenterOffset;
            citizenGo.transform.localPosition = defaultModulePosition;

            GridModule module = scene.grid.GetModule(scene.defaultModule);
            citizen.controller.AssignTask(module.controller);
        }

        public void RemoveCitizen(Citizen citizen)
        {
            citizens.Remove(citizen.name);
        }

        public void ClearAllCitizenHighlights()
        {
            foreach (KeyValuePair<string, Citizen> kv in citizens)
            {
                kv.Value.controller.HideCharacterSelection();
            }
        }

        public Castaway InviteCastaway()
        {
            if (castaways.Count > 0)
            {
                foreach (KeyValuePair<string, Castaway> kv in castaways)
                {
                    Castaway castaway = kv.Value;
                    castaway.controller.Invite();
                    return castaway;
                }
            }
            return null;
        }

        public void AddCastaway(Castaway castaway)
        {
            castaways.Add(castaway.name, castaway);

            GameObject citizenGo = Instantiate(castawayPrefab);
            castaway.controller = citizenGo.GetComponent<CastawayController>();
            castaway.controller.castaway = castaway;
            castaway.controller.scene = scene; // required for pathing
            citizenGo.transform.SetParent(scene.grid.transform);

            citizenGo.transform.localPosition = scene.defaultCastawayWaitingPosition;
        }

        public void ConvertCastawayToCitizen(Castaway castaway)
        {
            castaways.Remove(castaway.name);
            Destroy(castaway.controller.gameObject);

            Citizen citizen = new Citizen();
            citizen.name = castaway.name;
            AddCitizen(citizen);
        }
    }

    public class Citizen
    {
        public string name;
        public CitizenController controller; // if available
    }
    public class Castaway
    {
        public string name;
        public CastawayController controller; // if available
    }
}