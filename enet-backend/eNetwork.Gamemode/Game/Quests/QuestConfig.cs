using eNetwork.Framework.Singleton;
using System.Collections.Generic;

namespace eNetwork.Game.Quests
{
    enum QuestLineId
    {
        Beginner
    }

    enum QuestTaskId
    {
        RentAMoped,                         // ok
        OpenBankAccount,                    // ok
        OpenATM,                            // ok
        BackToBot,                          // ok
        BuyCola,                            // ok
        BuyBurger,                          // ok
        BuyClothes,                         // ok
        TakeBCategoryDriverLicenseExam,     // ok
        TakeLookAtCarDealership,            // ok
        SubstituteBotFriendAtWork,          // ok
        EarnMoneyOnSideJob,                 // ok
        TalkToHomelessBot,                  // ok
        BringBeerToHomelessBot,             // ok
        FindDogForHomelessBot,              // ok
        TellHomelessBotWhereHisDogIs        // ok
    }

    class QuestsConfig : Singleton<QuestsConfig>
    {
        private readonly Dictionary<QuestLineId, string> _questLineNames;
        private readonly Dictionary<QuestLineId, List<QuestTask>> _questLines;

        private QuestsConfig()
        {
            _questLineNames = new Dictionary<QuestLineId, string>();
            _questLines = new Dictionary<QuestLineId, List<QuestTask>>();
        }

        public string GetQuestLineName(QuestLineId questLineId)
        {
            return _questLineNames.GetValueOrDefault(questLineId) ?? questLineId.ToString("G");
        }

        public QuestTask GetQuestTaskByIndex(QuestLineId line, uint index)
        {
            if (_questLines.ContainsKey(line))
                return _questLines[line].Find(t => t.Index == index);

            return null;
        }

        public void AddQuestTask(QuestLineId line, QuestTask task)
        {
            if (_questLines.ContainsKey(line) is false)
                _questLines.Add(line, new List<QuestTask>());

            _questLines[line].Add(task);
        }
    }
}
