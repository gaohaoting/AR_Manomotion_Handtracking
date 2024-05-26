using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class BrushSettings
{
    public string name = "";
    public bool didChange = false;
    public Material brushMaterial = null;
    public Color color = new Color(0, 0, 0, 0);
    public ExtrudeShapeSettings extrudeShapeSettings = new ExtrudeShapeSettings();
    public BezierSpline.SplinePathSettings splinePathSettings = new BezierSpline.SplinePathSettings();
    public BezierSpline.SplineWidthSettings splineWidthSettings = new BezierSpline.SplineWidthSettings();
    public BezierPoint.PointWidthSettings pointWidthSettings = new BezierPoint.PointWidthSettings();
    public float[] customParameters = new float[] { };

    public BrushSettings GetChangeCompareCopy()
    {
        BrushSettings copy = new BrushSettings();
        copy.splinePathSettings.simplifyStraightSectionsBelowAngle = splinePathSettings.simplifyStraightSectionsBelowAngle;
        copy.splinePathSettings.simplifyStraightSectionsBelowWidthDiff = splinePathSettings.simplifyStraightSectionsBelowWidthDiff;

        copy.splineWidthSettings = new BezierSpline.SplineWidthSettings(splineWidthSettings);

        copy.pointWidthSettings = new BezierPoint.PointWidthSettings(pointWidthSettings);

        return copy;
    }

    private bool SetDidChange()
    {
        didChange = true;
        return true;
    }
    public bool CheckChange(BrushSettings changeCompareCopy)
    {
        didChange = false;

        if (changeCompareCopy == null)
            return SetDidChange();

        if (splinePathSettings.simplifyStraightSectionsBelowAngle != changeCompareCopy.splinePathSettings.simplifyStraightSectionsBelowAngle)
            return SetDidChange();
        if (splinePathSettings.simplifyStraightSectionsBelowWidthDiff != changeCompareCopy.splinePathSettings.simplifyStraightSectionsBelowWidthDiff)
            return SetDidChange();

        if (splineWidthSettings.keepAnimatingAfterSplineEnded != changeCompareCopy.splineWidthSettings.keepAnimatingAfterSplineEnded)
            return SetDidChange();

        if (pointWidthSettings.widthAnimationTime != changeCompareCopy.pointWidthSettings.widthAnimationTime)
            return SetDidChange();
        //if (pointWidthSettings.customPostClampWidthMultiplier != changeCompareCopy.pointWidthSettings.customPostClampWidthMultiplier)
        //    return SetDidChange();
        if (pointWidthSettings.minWidth != changeCompareCopy.pointWidthSettings.minWidth)
            return SetDidChange();
        if (pointWidthSettings.maxWidth != changeCompareCopy.pointWidthSettings.maxWidth)
            return SetDidChange();

        return false;
    }
}


public class SettingsPlain
{

    public bool keepTrackOfChangesSelective = true;

    public List<BrushSettings> brushes = new List<BrushSettings>();
}

public class Settings : MonoBehaviour
{

    public bool keepTrackOfChangesSelective = true;

    protected static Settings theInstance = null;
    public static Settings GetInstance()
    {
        if (theInstance == null)
        {
            theInstance = GameObject.FindObjectOfType<Settings>();
            if (theInstance == null)
            {
                Debug.LogWarning("No settings in scene! Creating new by defaults.");
                GameObject settingsGameObject = new GameObject("Settings");
                theInstance = settingsGameObject.AddComponent<Settings>();
            }
        }
        return theInstance;
    }

    [SerializeField]
    private List<BrushSettings> brushes = new List<BrushSettings>();
    public int BrushCount
    {
        get
        {
            return brushes.Count;
        }
    }
    private List<BrushSettings> prevBrushes = new List<BrushSettings>();

    public string[] GetListOfBrushNames()
    {
        string[] list = new string[brushes.Count];
        for (int i = 0; i < brushes.Count; ++i)
        {
            list[i] = brushes[i].name;
        }
        return list;
    }

    public BrushSettings GetBrushByName(string name)
    {
        for (int i = 0; i < brushes.Count; ++i)
        {
            if (brushes[i].name == name)
            {
                return brushes[i];
            }
        }
        return null;
    }
    public BrushSettings GetBrushById(int i)
    {
        if (i >= 0 && i < brushes.Count)
        {
            return brushes[i];
        }
        return null;
    }

    public void CreateNewBrush()
    {
        brushes.Add(new BrushSettings());
    }
    /*public void DeleteBrushByName(string name) {
		for (int i = 0; i < brushes.Count; ++i) {
			if (brushes [i].name == name) {
				brushes.RemoveAt (i);
			}
		}
	}*/
    public void DeleteBrush(BrushSettings brush)
    {
        brushes.Remove(brush);
    }

    public void Update()
    {
        // did values change?
        if (keepTrackOfChangesSelective)
        {
            for (int i = 0; i < BrushCount; ++i)
            {
                BrushSettings brush = brushes[i];
                brush.didChange = false;
                if (prevBrushes.Count <= i)
                {
                    brush.didChange = true;
                    prevBrushes.Add(brush.GetChangeCompareCopy());
                }
                else
                {
                    brush.didChange = brush.CheckChange(prevBrushes[i]);
                    if (brush.didChange)
                    {
                        prevBrushes[i] = brush.GetChangeCompareCopy();
                    }
                }
            }
        }
    }

    public SettingsPlain GetSettingsPlainCopy()
    {
        SettingsPlain plain = new SettingsPlain();
        plain.keepTrackOfChangesSelective = keepTrackOfChangesSelective;

        plain.brushes.Clear();
        foreach (BrushSettings brush in brushes)
        {
            plain.brushes.Add(brush);
        }

        return plain;
    }

    public void SetFromSettingsPlain(SettingsPlain plain)
    {
        if (plain == null)
            return;

        keepTrackOfChangesSelective = plain.keepTrackOfChangesSelective;

        brushes.Clear();
        foreach (BrushSettings brush in plain.brushes)
        {
            brushes.Add(brush);
        }
    }
}
