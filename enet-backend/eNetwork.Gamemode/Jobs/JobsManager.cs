using eNetwork.Framework.Singleton;
using System;

namespace eNetwork.Jobs
{
    class JobsManager : Singleton<JobsManager>
    {
        public static event EventHandler<GetJobEventArgs> PlayerGetJobEvent;

        // Вызывать при выдаче зарплаты игроку за какую-то работу
        public void Pay(ENetPlayer player, int amount)
        {
            player.ChangeWallet(amount);
            Game.Quests.QuestTasksHandler.Instance.AddProgressToTask(player, Game.Quests.QuestTaskId.EarnMoneyOnSideJob, amount);
        }

        // Вызывать при найме игрока на какую-то работу
        public void EmployPlayerToJob(ENetPlayer player, int jobId)
        {
            PlayerGetJobEvent?.Invoke(player, new GetJobEventArgs(jobId));
        }
    }
}
