using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Passive : AI_LocationController {
    // Start is called before the first frame update
    new void Start() {
        base.Start();
        capitalUnitPercent = 0.3f;
        midLineUnitPercent = 0.5f;
        frontLineUnitPercent = 0.7f;
    }
}
