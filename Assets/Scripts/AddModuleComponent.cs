using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD48
{
    public class AddModuleComponent : MonoBehaviour
    {
        public ModuleType type = ModuleType.Room;
        public ModuleType Type { get { return type; } }

        public Vector2Int Position { get; set; }

        // For harpoons
        public HollowGridPosition hollowGridPosition { get; set; }
    }

}