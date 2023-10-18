using PathFinding;

public static class PathFindingToric
{
    public static MapPoint[] FindBestPath(Map map, MapPoint start, MapPoint end)
    {
        return new AStartToric(map).CalculateBestPath(start, end);
    }
}

public class AStartToric
{
    private AStar aStar;

    public AStartToric(Map map)
    {
        GenerateAStart(map);
    }

    private void GenerateAStart(Map map)
    {

    }

    public MapPoint[] CalculateBestPath(MapPoint start, MapPoint end)
    {
        return aStar.CalculateBestPath(start, end);
    }
}
