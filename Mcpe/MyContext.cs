using System;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using GMap.NET.WindowsForms;
using LevelDB;

namespace Mcpe
{
    public class MyProvider : GMapProvider
    {
        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            Console.WriteLine($"Calling for {pos.X} {pos.Y}");
            return new GMapImage();
        }

        public override Guid Id { get; } = new Guid("B98BCFC3-6050-4F69-B04E-3B21051329C3");
        public override string Name { get; } = "MyMapProvider";
        public override PureProjection Projection { get; } = new MercatorProjection();
        public override GMapProvider[] Overlays { get; }
    }

    public class MyContext
    {
        public MyContext()
        {
            //MainMap.MapProvider = GMapProviders.OpenStreetMap;
            //MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
            Provider = GMapProviders.GoogleMap;
            Position = new PointLatLng(54.6961334816182, 25.2985095977783);

        }

        public GMapProvider Provider { get; }

        public PointLatLng Position { get; set; }
    }
}