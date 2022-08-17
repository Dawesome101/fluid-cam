using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky
{
    public class HoverDrive_Small : HoverDrive
    {
        protected override void Awake()
        {
            base.Awake();

            SetupRigidbody(mass, drag, force);
        }
    }
}
