using UnityEngine;
using System.Collections.Generic;
public static class ScoutIntel
{
    public static int lastSeenUnitCount = 0;
    public static bool turretSpotted = false;

    private static HashSet<Transform> seenUnits = new();
    private static HashSet<Transform> seenTurrets = new();

    public static void ReportUnit(Transform unit)
    {
        if (seenUnits.Add(unit)) lastSeenUnitCount++;
    }

    public static void ReportTurret(Transform turret)
    {
        if (seenTurrets.Add(turret)) turretSpotted = true;
    }

    public static void Clear()
    {
        seenUnits.Clear();
        seenTurrets.Clear();
        lastSeenUnitCount = 0;
        turretSpotted = false;
    }
}
