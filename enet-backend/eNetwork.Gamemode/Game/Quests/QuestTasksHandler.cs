using eNetwork.Framework.Singleton;
using eNetwork.Services.Rewards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eNetwork.Game.Quests
{
    class QuestTasksHandler : Singleton<QuestTasksHandler>
    {
        private readonly QuestsConfig _config;
        private readonly QuestRepository _repository;
        private readonly Dictionary<ENetPlayer, QuestPlayerData> _questDataCache;
        private readonly object _locker;

        private QuestTasksHandler()
        {
            _config = QuestsConfig.Instance;
            _repository = new QuestRepository();
            _questDataCache = new Dictionary<ENetPlayer, QuestPlayerData>();
            _locker = new object();
        }

        public async Task InitPlayer(ENetPlayer player)
        {
            QuestPlayerData data = await _repository.GetQuestDataOfCharacter(player.GetUUID());
            if (data is null)
            {
                data = new QuestPlayerData() { CharacterId = player.GetUUID() };
                await _repository.CreateQuestDataInDB(data);
            }

            lock (_locker)
            {
                _questDataCache.Add(player, data);
            }

            QuestTask task = _config.GetQuestTaskByIndex(data.ActiveQuestLineId, data.ActiveQuestTaskIndex);
            if (task is null || task.InitAction is null)
                return;

            task.InitAction.Invoke(player);
        }

        public void OnPlayerDisconnected(ENetPlayer player)
        {
            DeleteQuestData(player);
        }

        public QuestPlayerData GetQuestData(ENetPlayer player)
        {
            lock (_locker)
            {
                return _questDataCache.GetValueOrDefault(player);
            }
        }
        
        public Task DeleteQuestData(ENetPlayer player)
        {
            lock (_locker)
            {
                _questDataCache.Remove(player);
            }

            return _repository.DeleteQuestDataFromDB(player.GetUUID());
        }

        public async Task StartQuestLine(ENetPlayer player, QuestLineId questLine)
        {   
            QuestTask firstTask = _config.GetQuestTaskByIndex(questLine, 1);
            if (firstTask is null)
                throw new System.ArgumentException();

            player.SendInfo($"Вы начали квестовую линию {_config.GetQuestLineName(questLine)}");
            SetQuestTask(player, firstTask);

            if (await _repository.ExistsQuestDataForCharacter(player.GetUUID()))
            {
                await _repository.UpdateQuestDataInDB(GetQuestData(player));
                return;
            }

            await _repository.CreateQuestDataInDB(GetQuestData(player));
        }

        public void AddProgressToTask(ENetPlayer player, QuestTaskId taskId, int value = 1)
        {
            QuestPlayerData questData = GetQuestData(player);

            if (!questData.Progress.ContainsKey(taskId))
                return;

            questData.Progress[taskId] += value;
            CheckProgressCurrentQuestTask(player);
        }

        private void CheckProgressCurrentQuestTask(ENetPlayer player)
        {
            QuestPlayerData questData = GetQuestData(player);
            QuestTask task = _config.GetQuestTaskByIndex(questData.ActiveQuestLineId, questData.ActiveQuestTaskIndex);
            bool complete = true;
            foreach (var taskProgress in questData.Progress)
            {
                if (!task.TaskIds.ContainsKey(taskProgress.Key))
                    throw new KeyNotFoundException();

                if (task.TaskIds[taskProgress.Key] > taskProgress.Value)
                {
                    complete = false;
                    break;
                }
            }

            if (complete)
                HandleCompleteQuestTask(player, task);
        }

        private void HandleCompleteQuestTask(ENetPlayer player, QuestTask questTask)
        {
            foreach (IReward reward in questTask.Rewards)
            {
                reward.GiveReward(player);
            }

            questTask.ExitAction?.Invoke(player);

            QuestTask nextTask = _config.GetQuestTaskByIndex(questTask.QuestId, questTask.Index + 1);
            if (nextTask is null)
            {
                player.SendInfo($"Вы завершили квестовую линию {_config.GetQuestLineName(questTask.QuestId)}");
                return;
            }

            SetQuestTask(player, nextTask);
        }

        private void SetQuestTask(ENetPlayer player, QuestTask task)
        {
            QuestPlayerData questData = GetQuestData(player);
            questData.ActiveQuestLineId = task.QuestId;
            questData.ActiveQuestTaskIndex = task.Index;
            questData.Progress = task.TaskIds.Keys.ToDictionary(k => k, _ => 0);
            task.InitAction?.Invoke(player);
            player.SendInfo($"У вас новая квестовая задача: {task.Title}");
        }
    }
}
