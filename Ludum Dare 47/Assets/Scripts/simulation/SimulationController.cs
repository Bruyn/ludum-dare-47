﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Rendering.PostProcessing;

public enum PlaybackMode
{
    Pause,
    PlayAndRecord,
    Rewind,
    FastForward
}

public class SimulationController : MonoBehaviour
{
    public static SimulationController Instance;

    public bool isActiveOnStart = true;

    public KeyCode pauseToggleKey = KeyCode.F;
    public KeyCode rewindKey = KeyCode.Mouse0;
    public KeyCode forwardKey = KeyCode.Mouse1;
    public KeyCode fastPlayBackKey = KeyCode.LeftShift;

    public float maxRecordingTimeSec = 60f;

    private PlaybackMode currentMode = PlaybackMode.PlayAndRecord;
    private bool isFastPlayBack = false;
    private bool isActive = false;

    private int simulationStep = -1;
    private float currentTimeInSec = 0f;
    private bool isTimeExceeded = false;

    public void ActivateSimulation()
    {
        isActive = true;
    }

    public bool IsSimulationActive()
    {
        return isActive;
    }

    public PlaybackMode GetCurrentMode()
    {
        return currentMode;
    }

    public bool IsFastPlayBack()
    {
        return isFastPlayBack;
    }

    private void Awake()
    {
        Instance = this;
    }

    public float GetCurrentTimeIn()
    {
        return Mathf.Clamp(currentTimeInSec, 0f, maxRecordingTimeSec);
    }

    public float GetSimulationProgress()
    {
        return Mathf.Clamp(currentTimeInSec, 0f, maxRecordingTimeSec) / maxRecordingTimeSec;
    }

    void Start()
    {
        if (Application.targetFrameRate != 60)
            Application.targetFrameRate = 60;

        if (isActiveOnStart)
            ActivateSimulation();
    }

    private void Update()
    {
        if (!IsSimulationActive())
            return;

        if (Input.GetKeyDown(pauseToggleKey) || isTimeExceeded)
        {
            if (isTimeExceeded)
            {
                currentMode = PlaybackMode.Pause;
            }
            else
            {
                switch (currentMode)
                {
                    case PlaybackMode.Pause:
                        currentMode = PlaybackMode.PlayAndRecord;
                        break;
                    case PlaybackMode.PlayAndRecord:
                        currentMode = PlaybackMode.Pause;
                        break;
                }
            }
        }

        if (currentMode != PlaybackMode.PlayAndRecord)
        {
            ProcessKeys();
        }
    }

    private void ProcessKeys()
    {
        if (Input.GetKey(rewindKey))
        {
            currentMode = PlaybackMode.Rewind;
        }

        if (!isTimeExceeded && Input.GetKey(forwardKey))
        {
            currentMode = PlaybackMode.FastForward;
        }

        if (Input.GetKeyUp(rewindKey) || Input.GetKeyUp(forwardKey))
        {
            currentMode = PlaybackMode.Pause;
        }

        isFastPlayBack = Input.GetKey(fastPlayBackKey);
    }

    private void FixedUpdate()
    {
        if (isFastPlayBack)
        {
            Time.timeScale = 2;
        }
        else
        {
            Time.timeScale = 1;
        }

        var simulatedObjects = GameObject.FindObjectsOfType<SimulatedEntityBase>();
        foreach (var obj in simulatedObjects)
        {
            obj.GetComponent<SimulatedEntityBase>().TriggerSimulate(currentMode);
        }

        switch (currentMode)
        {
            case PlaybackMode.PlayAndRecord:
                simulationStep++;
                break;
            case PlaybackMode.FastForward:
                simulationStep++;
                break;
            case PlaybackMode.Rewind:
                simulationStep--;
                break;
        }

        if (simulationStep < 0)
            simulationStep = 0;

        currentTimeInSec = simulationStep * Time.fixedDeltaTime;
        isTimeExceeded = FloatComparer.AreEqual(currentTimeInSec, maxRecordingTimeSec, Mathf.Epsilon) ||
                         currentTimeInSec > maxRecordingTimeSec;
    }
}