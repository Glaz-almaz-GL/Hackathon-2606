using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using TerrainScanner.Core.Models;

namespace TerrainScanner.WinForms.Components
{
    public sealed class DrawerControl(ILogger<DrawerControl>? logger = null) : Control
    {
        private readonly ILogger<DrawerControl>? _logger = logger;
        private List<Vector2> Vectors = [];

        public void AddVector(Vector2 position, Vector2 endPosition)
        {
            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation("Adding vector2 with coordinations: x({Pos1}); y({Pos2})", position, endPosition);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var graphics = e.Graphics;
        }
    }
}
