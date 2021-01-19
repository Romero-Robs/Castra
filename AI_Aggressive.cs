using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Aggressive : AI_LocationController
{
    new void Start() {
        base.Start();
        capitalUnitPercent = 0.9f;
        midLineUnitPercent = 0.9f;
        frontLineUnitPercent = 0.9f;
    }
}
