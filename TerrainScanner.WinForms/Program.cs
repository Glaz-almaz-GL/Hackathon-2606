using Microsoft.Extensions.Logging;
using OSGeo.GDAL;
using System.Globalization;

namespace TerrainScanner.WinForms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Gdal.AllRegister();
            Application.Run(new FrmMain());
        }
    }
}