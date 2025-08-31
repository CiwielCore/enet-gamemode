using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using eNetwork.Framework;
using eNetwork.Framework.API.Interaction;

namespace eNetwork.Framework.API.ColShape.Methods
{
    public class ColShapeController : Script
    {
        private readonly static Logger _logger = new Logger("colshape-interactions");
        private readonly static string ColshapeData = "COLSHAPE";

        [ServerEvent(Event.PlayerEnterColshape)]
        private void OnPlayerEnterColshape(GTANetworkAPI.ColShape rShape, Player rPlayer)
        {
            ENetPlayer player = rPlayer as ENetPlayer;
            if (player == null)
                return;

            ENetColShape shape = rShape as ENetColShape;
            if (player == null)
                return;

            try
            {
                if (shape.Interaction == ColShapeType.Default) return;

                if (shape.Predicate != null && !shape.Predicate(shape, player)) return;

                player.SetData(ColshapeData, shape.Interaction);
                InteractionManagerDeprecated.Call("ENTER-" + shape.Interaction.ToString(), player, shape);

                if (!string.IsNullOrEmpty(shape.InteractionText))
                    ClientEvent.Event(player, "client.interaction.show", "E", shape.InteractionText);
            }
            catch (Exception e) { _logger.WriteError("OnPlayerEnterColshape", e); }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        private void OnPlayerExitColshape(GTANetworkAPI.ColShape rShape, Player rPlayer)
        {
            ENetPlayer player = rPlayer as ENetPlayer;
            if (player == null)
                return;

            ENetColShape shape = rShape as ENetColShape;
            if (player == null)
                return;

            try
            {
                if (shape.Interaction == ColShapeType.Default) return;

                player.ResetData(ColshapeData);
                InteractionManagerDeprecated.Call("LEAVE-" + shape.Interaction.ToString(), player, shape);

                if (!String.IsNullOrEmpty(shape.InteractionText))
                    ClientEvent.Event(player, "client.interaction.disable");
            }
            catch (Exception e) { _logger.WriteError("OnPlayerExitColshape", e); }
        }
        [CustomEvent("server.interaction")]
        private void KeyPress(ENetPlayer player)
        {
            try
            {
                if (!player.HasData(ColshapeData) || !player.IsTimeouted("INTERACTION", 1)) return;
                var colData = player.GetData<ColShapeType>(ColshapeData);

                InteractionManagerDeprecated.Call(colData.ToString(), player);
            }
            catch (Exception e) { _logger.WriteError("KeyPress", e); }
        }
    }
}
