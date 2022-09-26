using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky
{ 
    public class HoverEngine_Small : HoverEngine
    {
        protected override void Awake()
        {
            base.Awake();

            SetupRigidbody(mass, drag, force);
        }
    }
}
