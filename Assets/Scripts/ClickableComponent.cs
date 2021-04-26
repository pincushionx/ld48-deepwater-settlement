using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class ClickableComponent : MonoBehaviour
    {
        public ClickableComponentType Type { get; set; }
    }

    public enum ClickableComponentType
    {
        AddModuleButton
    }
}