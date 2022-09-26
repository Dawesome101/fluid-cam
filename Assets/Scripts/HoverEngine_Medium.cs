using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{
    public class HoverEngine_Medium : HoverEngine
    {
        protected override void Awake()
        {
            base.Awake();

            SetupRigidbody(mass, drag, force);
        }
    }
    
}
