using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TerrainNavigation.Core.Models;
using TerrainNavigation.Core.Navigation;

namespace TerrainNavigation.Core.Services
{
    public sealed class TestDataGenerator
    {
        public TerrainPoint GetLocation(
            TerrainMap map,
            float[] heigtMap,
            double speed,
            int seconds)
        {
            var distanceToStop = speed * seconds;
            return null!;
        }
    }
}
