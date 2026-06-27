namespace TerrainScanner.Core.Models
{
    public sealed record class MapOptions(
        float[] Heights,
            int Rows,
            int Columns,
            double MinLongitude,
            double MaxLongitude,
            double MinLatitude,
            double MaxLatitude,
            double LongitudeStep,
            double LatitudeStep,
            float MinHeight,
            float MaxHeight,
            double CellSizeX,
            double CellSizeY);
}
