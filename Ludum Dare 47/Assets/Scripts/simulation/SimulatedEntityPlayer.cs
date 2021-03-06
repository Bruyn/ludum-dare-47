﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedEntityPlayer : SimulatedEntityBase
{
    public override void TriggerSimulate(PlaybackMode mode)
    {
        transform.parent.gameObject.GetComponent<InputController>().Simulate(mode);
    }
}
