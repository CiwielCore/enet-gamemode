using System;
using System.Collections.Generic;
using System.Text;
using eNetwork.Framework;
using eNetwork.Framework.API.InteractionDepricated.Data;
using GTANetworkAPI;

namespace eNetwork.Modules
{
    public class TeleportsPoints
    {
        private static readonly Logger Logger = new Logger("teleport-points");
        public static void Initialize()
        {
            try
            {
                var priton = new Point(new Vector3(-43.26266, -1231.4489, 29.335021), 0, new Vector3(116.54515, -1024.4744, -99.69107), 6666);
            }
            catch (Exception ex) { Logger.WriteError("Initialize", ex); }
        }

        [InteractionDeprecated(ColShapeType.Teleports)]
        private static void Interaction(ENetPlayer player)
        {
            try
            {
                if (!player.GetData("TELEPORT_DATA", out Point point)) return;
                point.Teleport(player);
            }
            catch (Exception e) { Logger.WriteError("Interaction", e); }
        }

        public class Point
        {
            public Vector3 From;
            public uint FromDimension;

            public Vector3 To;
            public uint ToDimension;

            private ENetColShape From_ColShape;
            private ENetColShape To_ColShape;
            public Point(Vector3 from, uint fromDim, Vector3 to, uint toDim)
            {
                From = from;
                FromDimension = fromDim;
                To = to;
                ToDimension = toDim;

                GTAElements();
            }

            public void GTAElements()
            {
                try
                {
                    From_ColShape = ENet.ColShape.CreateCylinderColShape(From, 2, 2, FromDimension);
                    From_ColShape.SetIntraction(ColShapeType.Teleports);
                    From_ColShape.OnEntityEnterColShape += (s, e) =>
                    {
                        e.SetData("TELEPORT_DATA", this);
                        e.SetData("TELEPORT_TYPE", "FROM");
                    };
                    From_ColShape.OnEntityExitColShape += (s, e) =>
                    {
                        e.ResetData("TELEPORT_DATA");
                        e.ResetData("TELEPORT_TYPE");
                    };
                    NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, From - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), .7f, Helper.GTAColor, false, FromDimension);

                    To_ColShape = ENet.ColShape.CreateCylinderColShape(To, 2, 2, ToDimension);
                    To_ColShape.SetIntraction(ColShapeType.Teleports);
                    To_ColShape.OnEntityEnterColShape += (s, e) =>
                    {
                        e.SetData("TELEPORT_DATA", this);
                        e.SetData("TELEPORT_TYPE", "TO");
                    };
                    To_ColShape.OnEntityExitColShape += (s, e) =>
                    {
                        e.ResetData("TELEPORT_DATA");
                        e.ResetData("TELEPORT_TYPE");
                    };
                    NAPI.Marker.CreateMarker(MarkerType.VerticalCylinder, To - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), .7f, Helper.GTAColor, false, ToDimension);
                }
                catch (Exception ex) { Logger.WriteError("GTAElements", ex); }
            }

            public void Teleport(ENetPlayer player)
            {
                try
                {
                    if (!player.GetData("TELEPORT_TYPE", out string type)) return;
                    if (type == "TO")
                    {
                        player.Position = From;
                        player.Dimension = FromDimension;
                    }
                    else
                    {
                        player.Position = To;
                        player.Dimension = ToDimension;
                    }
                }
                catch (Exception ex) { Logger.WriteError("Teleport", ex); }
            }
        }
    }
}
