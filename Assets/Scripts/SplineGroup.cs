using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SplineGroup {

    public List<SplineHandler> splineList;

    public SplineGroup() {
        splineList = new List<SplineHandler>();
    }

	public void SetRamp(ColorRamp ramp) {
		this.Clear();

        SplineHandler spline = SphereBox.instance.SpawnSpline();
		this.AddSpline(spline);

        foreach (ColorSquare square in ramp) {
            spline.Add(square.sphere);
        }
    }

	public void Clear() {
        List<SplineHandler> tempList = new List<SplineHandler>(this.splineList);
        foreach (SplineHandler spline in tempList) {
			spline.Clear();
            SphereBox.instance.DespawnSpline(spline);
        }
    }

	public void AddSpline(SplineHandler spline) {
		this.splineList.Add(spline);
        spline.splineGroup = this;
    }

	public void RemoveSpline(SplineHandler spline, bool despawn = false) {
        this.splineList.Remove(spline);
        spline.splineGroup = null;

        if (despawn) {
            SphereBox.instance.DespawnSpline(spline);
        }
    }
}

