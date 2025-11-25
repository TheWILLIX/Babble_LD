using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using System.Diagnostics;
using System;

public class Performance : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _fps;
    [SerializeField] private TMPro.TMP_Text _maxfps;
    [SerializeField] private TMPro.TMP_Text _minfps;
    [SerializeField] private TMPro.TMP_Text _99fps;
    [SerializeField] private TMPro.TMP_Text _latency;
    [SerializeField] private TMPro.TMP_Text _cpu;
    [SerializeField] private TMPro.TMP_Text _cpuTime;
    [SerializeField] private TMPro.TMP_Text _gpu;
    private List<int> _frames = new List<int>();
    private PerformanceCounter cpuCounter;
    private PerformanceCounter gpuCounter;
    private TimeSpan lastTime;

    void Start()
    {
        lastTime = Process.GetCurrentProcess().TotalProcessorTime;
        cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true);
        gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "_Total", true);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
        }
        if (!ClemCAddons.Utilities.Timer.MinimumDelay("performanceUpdate".GetHashCode(), 100))
            return;
        _frames.Add((1 / Time.deltaTime).Round());
        if(_frames.Count > 100)
        {
            _frames.RemoveAt(0);
        }
        _fps.text = _frames[_frames.Count - 1] + " fps";
        _maxfps.text = "Max: " + _frames.Max() + " fps";
        _minfps.text = "Min: " + _frames.Min() + " fps";
        var copy = _frames;
        copy.OrderByDescending(t => t);
        if(copy.Count > 2)
            _99fps.text = "99%: " + copy[copy.Count - 2] + " fps";
        _latency.text = "Frame: " + (Time.deltaTime * 1000).Round() + " ms";
        if (ClemCAddons.Utilities.Timer.MinimumDelay("performanceUpdateCPU".GetHashCode(), 1000))
        {
            _cpuTime.text = "CPU: " + Process.GetCurrentProcess().TotalProcessorTime.Milliseconds +" ms";
            _cpu.text = "CPU: " + GetCPUUsage() + "%";
            _gpu.text = "GPU: " + gpuCounter.NextValue() + "%";
        }
    }

    private string GetCPUUsage()
    {
        var total = Process.GetCurrentProcess().TotalProcessorTime;
        var value = total.Subtract(lastTime);
        lastTime = total;
        return ((float)value.TotalMilliseconds / 10f / Environment.ProcessorCount).Round().ToString();
    }
}
