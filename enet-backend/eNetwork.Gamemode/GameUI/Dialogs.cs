using eNetwork.Framework;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.GameUI
{
    public static class Dialogs
    {
        private static readonly Logger Logger = new Logger("dialogs");

        public static void SetDialog(this ENetPlayer player, Dialog dialog)
        {
            try
            {
                player.SetData("_dialog.data", dialog);
            }
            catch(Exception ex) { Logger.WriteError("SetDialog", ex); }
        }

        public static bool IsDialog(this ENetPlayer player)
        {
            return player.HasData("_dialog.data");
        }

        public static void ResetDialog(this ENetPlayer player)
        {
            player.ResetData("_dialog.data");
        }

        public static bool GetDialog(this ENetPlayer player, out Dialog dialog)
        {
            try
            {
                return player.GetData("_dialog.data", out dialog);
            }
            catch(Exception ex) { Logger.WriteError("GetDialog", ex); dialog = null; return false; }
        }

        [CustomEvent("server.dialog.answer")]
        public static void Event_Answer(ENetPlayer player, string callBack)
        {
            try
            {
                if (!player.GetDialog(out Dialog dialog)) return;
                dialog.Answer(player, callBack);
            }
            catch(Exception ex) { Logger.WriteError("Event_Answer", ex); }
        }

        public static void Close(ENetPlayer player, int transition = 700)
        {
            ClientEvent.Event(player, "client.dialogs.close", transition);
        }
    }

    public class Dialog
    {
        private static readonly Logger Logger = new Logger("dialog-model");

        public int ID;
        public string Name;
        public string Description;
        public string Text;
        public List<DialogAnswer> Answers;

        public void Open(ENetPlayer player, string pedName)
        {
            try
            {
                ClientEvent.Event(player, "client.dialogs.open", pedName, JsonConvert.SerializeObject(this));
                player.SetDialog(this);
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        public void Open(ENetPlayer player, Ped ped)
        {
            try
            {
                if (!ped.HasData("POSITION_DATA")) return;
                ClientEvent.Event(player, "client.dialogs.open.ext", 
                    JsonConvert.SerializeObject(ped.GetData<Position>("POSITION_DATA")), 
                    JsonConvert.SerializeObject(this)
                );

                player.SetDialog(this);
            }
            catch (Exception ex) { Logger.WriteError("Open", ex); }
        }

        public void Answer(ENetPlayer player, string callBack)
        {
            try
            {
                var answer = Answers.Find(x => x.CallBack == callBack);
                if (answer is null) return;

                if (callBack == "close")
                    player.ResetDialog();
                else
                    answer.Action.Invoke(player, answer.Params);
            }
            catch (Exception ex) { Logger.WriteError("Answer", ex); }
        }
    }

    public class DialogAnswer
    {
        public string Text;
        public string CallBack;
        public object[] Params;

        [JsonIgnore]
        public Action<ENetPlayer, object[]> Action;

        public DialogAnswer(string text, Action<ENetPlayer, object[]> action, string callback, params object[] args)
        {
            Text = text;
            Action = action;
            CallBack = callback;
            Params = args;
        }
    }

    public enum DialogType
    {
        Showroom,
        AutoschoolExamInteraction,
        Trucker
    }
}
