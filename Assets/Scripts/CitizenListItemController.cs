using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD48
{
    public class CitizenListItemController : MonoBehaviour
    {
        public OverlayController overlay;

        private Citizen citizen;
        public Citizen Citizen
        {
            get
            {
                return citizen;
            }
            set
            {
                citizen = value;
                GetComponentInChildren<Text>().text = value.name;
                gameObject.name = value.name;
            }
        }

        private void Awake()
        {
            // Callback to overlay with this information
            GetComponent<Button>().onClick.AddListener(() => overlay.CitizenClicked(this));
        }
    }
}