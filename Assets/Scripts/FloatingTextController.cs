using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD48
{
    public class FloatingTextController : MonoBehaviour
    {
        private float ttl = 3f;
        public TextMeshPro text;

        private readonly Vector3 velocity = new Vector3(0.025f, 0.06125f, 0f);

        private void Awake()
        {
            text = GetComponent<TextMeshPro>();
        }
        void Update()
        {
            if (ttl > 0f)
            {
                Vector3 currentPosition = transform.position;
                currentPosition += Time.deltaTime * velocity;
                transform.position = currentPosition;

                ttl -= Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetText(string s)
        {
            text.text = s;
        }
    }
}