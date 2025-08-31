using eNetwork.Framework.Collections;
using eNetwork.GameUI;
using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace eNetwork.Services.CarRental
{
    internal class CarRentalPoint
    {
        public uint Id { get; set; }
        public Vector3 Location { get; set; }
        public float PedHeading { get; set; }

        private IReadOnlyList<Position> _vehicleSpawns;
        private IterableList<Position> _vehicleSpawnsIterable;

        private Ped _ped;
        private Blip _blip;
        private Marker _marker;
        private ColShape _colShape;

        private Dialog _dialog;

        public CarRentalPoint(uint id, float pedHeading, Vector3 position, IEnumerable<Position> vehicleSpawns)
        {
            Id = id;
            Location = position;
            PedHeading = pedHeading;

            _vehicleSpawns = vehicleSpawns.ToList();
            _vehicleSpawnsIterable = new IterableList<Position>(_vehicleSpawns).IsLooped();

            CreateElements();
            CreateDialog();
        }

        public void OpenDialogWithPed(ENetPlayer player)
        {
            _dialog.Open(player, _ped);
        }

        public Position GetCarSpawnPosition()
        {
            Position position = _vehicleSpawnsIterable.GetNext();
            return position;
        }

        private void CreateElements()
        {
            if (CarRentalService.Instance.Config.PointConfig.BlipsEnable)
            {
                _blip = ENet.Blip.CreateBlip(
                    CarRentalService.Instance.Config.PointConfig.BlipSprite,
                    Location,
                    CarRentalService.Instance.Config.PointConfig.BlipScale,
                    CarRentalService.Instance.Config.PointConfig.BlipColor,
                    CarRentalService.Instance.Config.PointConfig.BlipName,
                    255,
                    0,
                    true,
                    0,
                    0);
            }

            if (CarRentalService.Instance.Config.PointConfig.MarkerEnable)
            {
                _marker = NAPI.Marker.CreateMarker(
                    CarRentalService.Instance.Config.PointConfig.MarkerType,
                    Location,
                    new Vector3(),
                    new Vector3(),
                    CarRentalService.Instance.Config.PointConfig.MarkerScale,
                    CarRentalService.Instance.Config.PointConfig.MarkerColor,
                    false,
                    0);
            }

            uint pedModel = CarRentalService.Instance.Config.PointConfig.PedModelHash;
            _ped = NAPI.Ped.CreatePed(pedModel, Location, PedHeading, false, true, true, true, 0);
            _ped.SetData("POSITION_DATA", new Position(Location.X, Location.Y, Location.Z, PedHeading));

            _colShape = ENet.ColShape.CreateCylinderColShape(
                Location,
                CarRentalService.Instance.Config.PointConfig.ColShapeRange,
                CarRentalService.Instance.Config.PointConfig.ColShapeHeight,
                0,
                ColShapeType.CarRental);

            _colShape.OnEntityEnterColShape += (_, player) => player.SetData(nameof(CarRentalPoint), this);
            _colShape.OnEntityExitColShape += (_, player) => player.ResetData(nameof(CarRentalPoint));
        }

        private void CreateDialog()
        {
            _dialog = new Dialog()
            {
                ID = (int)Id,
                Name = CarRentalService.Instance.Config.DialogConfig.NameText,
                Description = CarRentalService.Instance.Config.DialogConfig.DescriptionText,
                Text = CarRentalService.Instance.Config.DialogConfig.ContainerText,
                Answers = new List<DialogAnswer>()
                {
                    new DialogAnswer(CarRentalService.Instance.Config.DialogConfig.AcceptButtonText,
                        (p, _) => CarRentalService.Instance.ShowCarRentalMenu(p, this), "accept"),
                    new DialogAnswer(CarRentalService.Instance.Config.DialogConfig.CancelButtonText, null, "close")
                }
            };
        }
    }
}
