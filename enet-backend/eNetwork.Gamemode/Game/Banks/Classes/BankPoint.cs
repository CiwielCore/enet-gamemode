using eNetwork.Framework;
using eNetwork.Game.Banks.Player;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Banks.Classes
{
    public class BankPoint
    {
        private static readonly Logger Logger = new Logger("bank-point");

        public Position NpcPosition { get; set; }   
        public uint NpcHash { get; set; }
        public Vector3 Position { get; set; }

        public BankPoint(Position npcPosition, uint npcHash, Vector3 position)
        {
            NpcPosition = npcPosition;
            NpcHash = npcHash;
            Position = position;
        }

        private Ped _ped { get; set; }
        private ENetColShape _colShape { get; set; }
        private Blip _blip { get; set; }

        public void GTAElements()
        {
            _ped = NAPI.Ped.CreatePed(NpcHash, NpcPosition.GetVector3(), NpcPosition.GetHeading(), false, true, true, true, 0);
            
            _colShape = ENet.ColShape.CreateCylinderColShape(Position, 2, 2, 0, ColShapeType.Bank);
            _colShape.OnEntityEnterColShape += (s, e) =>
            {
                e.SetData("bank.point", this);
            };
            _colShape.OnEntityExitColShape += (s, e) =>
            {
                e.ResetData("bank.point");
            };

            _blip = ENet.Blip.CreateBlip(500, Position, .8f, 2, "Банк", 255, 0, true, 0, 0);
        }

        public void Interaction(ENetPlayer player)
        {
            try
            {
                if (!player.GetBankAccount(out var bankAccount))
                {
                    ClientEvent.Event(player, "client.bank.open", true, JsonConvert.SerializeObject(new { Owner = "null", Number = 0000000000000000, Balance = 0, Type = "???", PinCode = 0000 }), JsonConvert.SerializeObject(new List<object>() { }));
                    return;
                }

                ClientEvent.Event(player, "client.bank.open", 
                    // Первый раз или нет
                    false,

                    // Данные о владельце карты
                    JsonConvert.SerializeObject(new
                    {
                        Owner = player.GetName(),
                        Number = bankAccount.Id,
                        Balance = bankAccount.Balance,
                        Type = "Standart",
                        PinCode = bankAccount.PinCode,
                    }), 

                    // Логи
                    JsonConvert.SerializeObject(BankManager.GetHistory(bankAccount))
                );

                // TODO: временно, пока не будет банкомат
                Quests.QuestTasksHandler.Instance.AddProgressToTask(player, Quests.QuestTaskId.OpenATM);
            }
            catch(Exception ex) { Logger.WriteError("Open", ex); }
        }
    }
}
