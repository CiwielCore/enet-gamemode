using GTANetworkAPI;
using ColShapeDefault = GTANetworkAPI.ColShape;
using System;

namespace eNetwork.Framework.API.Interaction;

public  delegate void OnPlayerPressButton(ENetPlayer player, ColShapeDefault colShape);

public class Interaction
{
    private readonly static Logger _logger = new("interaction");

    public Marker Marker { get; private set; }

    public readonly Vector3 Position;
    public readonly ColShapeDefault ColShape;

    private readonly string _interactionText;
    private readonly OnPlayerPressButton _onPlayerPressButton;
    private readonly bool _isInteractTextVisible;

    public Interaction(Vector3 position, OnPlayerPressButton handler, string interactionText = "Нажмите для взаимодействия", float radius = 2f, float height = 2f, bool isTextVisible = true, uint dimension = 0)
    {
        Position = position;

        ColShape = ENet.ColShape.CreateCylinderColShape(position, radius, height, dimension);
        ColShape.SetData("INTERACTION_PARENT", this);

        ColShape.OnEntityEnterColShape += _onPlayerEnterColShape;
        ColShape.OnEntityExitColShape += _onPlayerExitColShape;

        _isInteractTextVisible = isTextVisible;
        _interactionText = interactionText;
        _onPlayerPressButton = handler;
    }

    public void CreateMarker(int type, float radius, Vector3 offset = default)
    {
        Console.WriteLine("Marker offset", offset);

        Marker = NAPI.Marker.CreateMarker(type, Position + offset, new Vector3(), new Vector3(), radius, new Color(128, 128, 128, 200), false, ColShape.Dimension);
    }

    public void SetStay(string key, object value)
    {
        ColShape.SetData(key, value);
    }

    public void Destroy()
    {
        NAPI.Task.Run(() =>
        {
            try
            {
                Marker?.Delete();
                ColShape?.Delete();
            }
            catch (Exception e) { _logger.WriteError("On destroy interaction", e); }
        });
    }

    private void _invokeEvent(ENetPlayer player)
    {
        try
        {
            if (player is null)
                return;

            _onPlayerPressButton.Invoke(player, ColShape);
        }
        catch (Exception e) { _logger.WriteError("On interact player", e); }
    }

    private void _onPlayerEnterColShape(ColShapeDefault colShape, Player player)
    {
        try
        {
            player.SetData("PLAYER_INTERACTION", this);

            if (_isInteractTextVisible)
                ClientEvent.Event(player, "client.interaction.show", "E", _interactionText);
        }
        catch (Exception e) { _logger.WriteError("On enter player", e); }
    }

    private void _onPlayerExitColShape(ColShapeDefault colShape, Player player)
    {
        try
        {
            if (!player.HasData("PLAYER_INTERACTION"))
                return;

            Interaction playerInteraction = player.GetData<Interaction>("PLAYER_INTERACTION");

            if (playerInteraction.ColShape == colShape)
                player.ResetData("PLAYER_INTERACTION");

            if (_isInteractTextVisible)
                ClientEvent.Event(player, "client.interaction.disable");
        }
        catch (Exception e) { _logger.WriteError("On exit player", e); }
    }

    public static T GetStay<T>(Player player, string data)
    {
        if (!player.HasData("PLAYER_INTERACTION"))
            return default;

        Interaction interaction = player.GetData<Interaction>("PLAYER_INTERACTION");

        return interaction.ColShape.GetData<T>(data);
    }
}
