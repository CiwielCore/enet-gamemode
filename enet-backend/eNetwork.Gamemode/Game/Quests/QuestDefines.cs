using eNetwork.Services.Rewards;
using System;
using System.Collections.Generic;

namespace eNetwork.Game.Quests
{
    class QuestPlayerData
    {
        public int CharacterId { get; set; }
        public QuestLineId ActiveQuestLineId { get; set; }
        public uint ActiveQuestTaskIndex { get; set; }
        public Dictionary<QuestTaskId, int> Progress { get; set; } = new Dictionary<QuestTaskId, int>();
    }

    class QuestTask
    {
        public QuestLineId QuestId { get; set; }
        public uint Index { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public Dictionary<QuestTaskId, int> TaskIds { get; set; }
        public IReward[] Rewards { get; set; } = Array.Empty<IReward>();

        public Action<ENetPlayer> InitAction { get; set; } = null;
        public Action<ENetPlayer> ExitAction { get; set; } = null;
    }
}
