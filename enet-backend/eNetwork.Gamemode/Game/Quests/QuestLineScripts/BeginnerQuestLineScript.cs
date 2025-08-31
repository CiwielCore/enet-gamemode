using eNetwork.Framework.API.InteractionDepricated.Data;
using eNetwork.GameUI;
using eNetwork.Inv;
using eNetwork.Services.Rewards;
using eNetwork.Services.Rewards.RewardKinds;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eNetwork.Game.Quests.QuestLineScripts
{
    class BeginnerQuestLineScript : Script
    {
        private readonly Vector3 _homelessPedPosition = new Vector3();
        private readonly Vector3 _dogPosition = new Vector3();

        private readonly QuestsConfig _questConfig;
        private readonly QuestTasksHandler _questHandler;

        public BeginnerQuestLineScript()
        {
            _questConfig = QuestsConfig.Instance;
            _questHandler = QuestTasksHandler.Instance;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            AddBeginnerQuestLine();

            Services.DrivingLicensing.DrivingLicenseService.PlayerGiveDriverLicenseEvent += DrivingLicenseService_PlayerGiveDriverLicenseEvent;
            Services.CarRental.CarRentalService.PlayerRentCarEvent += CarRentalService_PlayerRentACar;
            Businesses.List.ProductShop.BuyProductEvent += ProductShop_BuyProductEvent;
            Jobs.JobsManager.PlayerGetJobEvent += JobsManager_PlayerGetJobEvent;

            CreateStartBot();
            CreateHomelessBot();
        }


        private Ped _startPed;
        private void CreateStartBot()
        {
            ENet.Blip.CreateBlip(1, new Vector3(), 1.0f, 1, "Bot", 255, 0, true, 0, 0);
            _startPed = NAPI.Ped.CreatePed((uint)PedHash.Abigail, new Vector3(), 0, false, true, true, true, 0);
            _startPed.SetData("POSITION_DATA", new Position(new Vector3().X, new Vector3().Y, new Vector3().Z, 0));
            ColShape shape = ENet.ColShape.CreateCylinderColShape(new Vector3(), 2f, 1.2f, 0, ColShapeType.QuestBotInteraction);

            shape.OnEntityEnterColShape += (_, player) => player.SetData<Action<ENetPlayer, QuestPlayerData>>(
                nameof(ColShapeType.QuestBotInteraction), StartBotInteraction);
            shape.OnEntityExitColShape += (_, player) => player.ResetData(nameof(ColShapeType.QuestBotInteraction));
        }

        private Ped _homelessPed;
        private void CreateHomelessBot()
        {
            ENet.Blip.CreateBlip(1, _homelessPedPosition, 1.0f, 1, "Bot-Bomj", 255, 0, true, 0, 0);
            _homelessPed = NAPI.Ped.CreatePed((uint)PedHash.Abigail, _homelessPedPosition, 0, false, true, true, true, 0);
            _homelessPed.SetData("POSITION_DATA", new Position(_homelessPedPosition.X, _homelessPedPosition.Y, _homelessPedPosition.Z, 0));
            ColShape shape = ENet.ColShape.CreateCylinderColShape(_homelessPedPosition, 2f, 1.2f, 0, ColShapeType.QuestBotInteraction);

            shape.OnEntityEnterColShape += (_, player) => player.SetData<Action<ENetPlayer, QuestPlayerData>>(
                nameof(ColShapeType.QuestBotInteraction), HomelessBotInteraction);
            shape.OnEntityExitColShape += (_, player) => player.ResetData(nameof(ColShapeType.QuestBotInteraction));

            // create homeless dog
            NAPI.Ped.CreatePed((uint)PedHash.Chop, _dogPosition, 0, false, true, true, true, 0);
            ColShape dogShape = ENet.ColShape.CreateCylinderColShape(_dogPosition, 4f, 1.2f, 0, ColShapeType.QuestBotInteraction);
            dogShape.OnEntityEnterColShape += (_, player) =>
            {
                if (player is not ENetPlayer ePlayer)
                    return;

                QuestPlayerData questPlayerData = _questHandler.GetQuestData(ePlayer);
                if (questPlayerData.Progress.ContainsKey(QuestTaskId.FindDogForHomelessBot))
                {
                    ePlayer.SendInfo("Вы нашли собаку бота-бомжа, отправляйтесь к нему и сообщите ему");
                    _questHandler.AddProgressToTask(ePlayer, QuestTaskId.FindDogForHomelessBot);
                }
            };
        }

        [InteractionDeprecated(ColShapeType.QuestBotInteraction)]
        private void OnInteraction(ENetPlayer player)
        {
            if (IsValidSender(player) is false)
                return;

            if (player.HasData(nameof(ColShapeType.QuestBotInteraction)) is false)
                return;

            Action<ENetPlayer, QuestPlayerData> interactionHandler = player.GetData<Action<ENetPlayer, QuestPlayerData>>(
                nameof(ColShapeType.QuestBotInteraction));
            interactionHandler?.Invoke(player, _questHandler.GetQuestData(player));
        }

        private void StartBotInteraction(ENetPlayer player, QuestPlayerData questData)
        {
            QuestTask questTask = _questConfig.GetQuestTaskByIndex(questData.ActiveQuestLineId, questData.ActiveQuestTaskIndex);
            if (questTask is null)
                return;

            if (questTask.TaskIds.ContainsKey(QuestTaskId.BackToBot))
            {
                player.SendInfo("Молодец, что пришел ко мне");
                _questHandler.AddProgressToTask(player, QuestTaskId.BackToBot);
                return;
            }
        }

        private void HomelessBotInteraction(ENetPlayer player, QuestPlayerData questData)
        {
            QuestTask questTask = _questConfig.GetQuestTaskByIndex(questData.ActiveQuestLineId, questData.ActiveQuestTaskIndex);
            if (questTask is null)
                return;

            Dialog dialog = new Dialog();
            dialog.Name = "Бом-Бомж";
            dialog.Description = "Выполните пару заданий у этого бота";

            if (questTask.TaskIds.ContainsKey(QuestTaskId.TalkToHomelessBot))
            {
                dialog.Text = "Слышь, привет. Привези мне пивка, окей?";
                dialog.Answers = new List<DialogAnswer>()
                {
                    new DialogAnswer("Хорошо", (p, _) => _questHandler.AddProgressToTask(p, QuestTaskId.TalkToHomelessBot), "accept"),
                    new DialogAnswer("Да пошел ты со своим пивом", null, "close")
                };
                dialog.Open(player, _homelessPed);
                return;
            }

            if (questTask.TaskIds.ContainsKey(QuestTaskId.BringBeerToHomelessBot))
            {
                dialog.Text = "О, ты привез мне пивка?";
                dialog.Answers = new List<DialogAnswer>()
                {
                    new DialogAnswer("Да, вот, бери..", (p, _) =>
                    {
                        // check beer
                        if (!player.GetInventory(out Storage storage))
                        {
                            Dialogs.Close(p);
                            return;
                        }
                        
                        // TODO: replace water on beer
                        Item beerItem = storage.Items.FirstOrDefault(i => i.Type == ItemId.Water);
                        if (beerItem == null)
                        {
                            Dialogs.Close(p);
                            player.SendInfo("Бля, где пиво, епта? Сгоняй быстро");
                            return;
                        }

                        Inventory.RemoveItem(player, beerItem, 1);
                        _questHandler.AddProgressToTask(player, QuestTaskId.BringBeerToHomelessBot);
                        dialog.Text = "Дружище, будь дорб, сыщи моего пса. Эта сучка уже 3 день не возвращается";
                        dialog.Answers = new List<DialogAnswer>()
                        {
                            new DialogAnswer("Базару ноль, бомжара", null, "close")
                        };
                        dialog.Open(p, _homelessPed);
                    }, "accept"),
                    new DialogAnswer("Да пошел ты со своим пивом", null, "close")
                };
                dialog.Open(player, _homelessPed);
                return;
            }

            if (questTask.TaskIds.ContainsKey(QuestTaskId.FindDogForHomelessBot))
            {
                dialog.Text = "Ты ещё не нашел моего пса? Ну и лоботряс";
                dialog.Answers = new List<DialogAnswer>()
                {
                    new DialogAnswer("Да иди ты со своей сучкой", null, "close")
                };
                dialog.Open(player, _homelessPed);
                return;
            }

            if (questTask.TaskIds.ContainsKey(QuestTaskId.TellHomelessBotWhereHisDogIs))
            {
                player.SendInfo("Благодарю тебя, ты мне очень помог!");
                _questHandler.AddProgressToTask(player, QuestTaskId.TellHomelessBotWhereHisDogIs);
                return;
            }
        }

        private void JobsManager_PlayerGetJobEvent(object sender, Jobs.GetJobEventArgs e)
        {
            if (IsValidSender(sender) is false)
                return;

            ENetPlayer player = sender as ENetPlayer;
            QuestPlayerData data = _questHandler.GetQuestData(player);
            if (data.Progress.ContainsKey(QuestTaskId.SubstituteBotFriendAtWork) is false)
                return;

            _questHandler.AddProgressToTask(player, QuestTaskId.SubstituteBotFriendAtWork);
        }

        private void DrivingLicenseService_PlayerGiveDriverLicenseEvent(object sender, Services.DrivingLicensing.GiveDriverLicenseArgs e)
        {
            if (IsValidSender(sender) is false)
                return;

            if (e.LicenseClass == Framework.Classes.DrivingLicenseClass.Car)
                _questHandler.AddProgressToTask(sender as ENetPlayer, QuestTaskId.TakeBCategoryDriverLicenseExam);
        }

        private void ProductShop_BuyProductEvent(object sender, Businesses.Models.BuyProductArgs e)
        {
            if (IsValidSender(sender) is false)
                return;

            if (e.Item == ItemId.eCola)
                _questHandler.AddProgressToTask(sender as ENetPlayer, QuestTaskId.BuyCola);

            if (e.Item == ItemId.Burger)
                _questHandler.AddProgressToTask(sender as ENetPlayer, QuestTaskId.BuyBurger);
        }

        private void CarRentalService_PlayerRentACar(object sender, Services.CarRental.CarRentalModels.CarRentArgs e)
        {
            if (IsValidSender(sender) is false)
                return;

            if (e.CarModel == "faggio")
                _questHandler.AddProgressToTask(sender as ENetPlayer, QuestTaskId.RentAMoped);
        }

        private bool IsValidSender(object sender)
        {
            if (sender is not ENetPlayer player)
                return false;

            QuestPlayerData data = _questHandler.GetQuestData(player);
            if (data is null)
                return false;

            if (data.ActiveQuestLineId != QuestLineId.Beginner)
                return false;

            return true;
        }

        private void AddBeginnerQuestLine()
        {
            List<QuestTask> questTasks = new List<QuestTask>()
            {
                new QuestTask()
                {
                    Index = 1,
                    Title = "Арендовать мопед",
                    Text = "Найдите любую арендную площадку и арендуйте мопед",
                    TaskIds = new() { [QuestTaskId.RentAMoped] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 2,
                    Title = "Открыть банковский счёт",
                    Text = "Езжайте в ближайшее отделения банка и откройте себе банковский счёт",
                    TaskIds = new() { [QuestTaskId.OpenBankAccount] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 3,
                    Title = "Воспользуйтесь банкоматом",
                    Text = "Езжайте к ближайшему банкомату и откройте его меню",
                    TaskIds = new() { [QuestTaskId.OpenATM] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 4,
                    Title = "Вернитесь к боту",
                    Text = "Вам необходимо вернуться к боту",
                    TaskIds = new() { [QuestTaskId.BackToBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    Rewards = new IReward[] { new MoneyReward() { Amount = 2_000_000 } }
                },
                new QuestTask()
                {
                    Index = 5,
                    Title = "Купить колу и бургер",
                    Text = "Отправляйтесь в ближайший магазин за колой и бургером",
                    TaskIds = new() { [QuestTaskId.BuyCola] = 1, [QuestTaskId.BuyBurger] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 6,
                    Title = "Вернитесь к боту",
                    Text = "Вам необходимо вернуться к боту",
                    TaskIds = new() { [QuestTaskId.BackToBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    Rewards = new IReward[] { new CaseReward() { Amount = 1, CaseName = "Обычный кейс" } }
                },
                new QuestTask()
                {
                    Index = 7,
                    Title = "Купить одежду",
                    Text = "Посетите магазин одежды и приобретите любую вещь",
                    TaskIds = new() { [QuestTaskId.BuyClothes] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 8,
                    Title = "Вернитесь к боту",
                    Text = "Вам необходимо вернуться к боту",
                    TaskIds = new() { [QuestTaskId.BackToBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    Rewards = new IReward[] { new MoneyReward() { Amount = 2_000_000 } }
                },
                new QuestTask()
                {
                    Index = 9,
                    Title = "Получить водительские права",
                    Text = "Отправляйтесь в автошколу и сдайте экзамен на категорию B",
                    TaskIds = new() { [QuestTaskId.TakeBCategoryDriverLicenseExam] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 10,
                    Title = "Вернитесь к боту",
                    Text = "Вам необходимо вернуться к боту",
                    TaskIds = new() { [QuestTaskId.BackToBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    Rewards = new IReward[] { new MoneyReward() { Amount = 1_500_000 } }
                },
                new QuestTask()
                {
                    Index = 11,
                    Title = "Подменить друга бота",
                    Text = "Бот просит вас подменить своего друга на его работе",
                    TaskIds = new() { [QuestTaskId.SubstituteBotFriendAtWork] = 1 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 12,
                    Title = "Заработать $100.000",
                    Text = "Работая на разных подработках заработайте сумму $100.000",
                    TaskIds = new() { [QuestTaskId.EarnMoneyOnSideJob] = 100_000 },
                    QuestId = QuestLineId.Beginner
                },
                new QuestTask()
                {
                    Index = 13,
                    Title = "Вернитесь к боту",
                    Text = "Вам необходимо вернуться к боту",
                    TaskIds = new() { [QuestTaskId.BackToBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    Rewards = new IReward[] { new CaseReward() { Amount = 1, CaseName = "Обычный кейс" } }
                },
                new QuestTask()
                {
                    Index = 14,
                    Title = "Поговорить с ботом-бомжом",
                    Text = "Отправляйтесь к боту-бомжу, чтобы с ним поговорить и получить задания",
                    TaskIds = new() { [QuestTaskId.TalkToHomelessBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    InitAction = player =>
                    {
                        player.SetWaypoint(_homelessPed.Position.X, _homelessPed.Position.Y);
                    }
                },
                new QuestTask()
                {
                    Index = 15,
                    Title = "Принести пиво для бота-бомжа",
                    Text = "Посетите ближаший магазин и купите любое пиво для бота-бомжа",
                    TaskIds = new() { [QuestTaskId.BringBeerToHomelessBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                },
                new QuestTask()
                {
                    Index = 16,
                    Title = "Бот-бомж ищет свою собаку",
                    Text = "Найдите собаку бота-бомжа",
                    TaskIds = new() { [QuestTaskId.FindDogForHomelessBot] = 1 },
                    QuestId = QuestLineId.Beginner,
                    InitAction = player =>
                    {
                        // Create marker
                        player.SetWaypoint(_dogPosition.X, _dogPosition.Y);
                    },
                    ExitAction = player =>
                    {
                        // delete marker
                    }
                },
                new QuestTask()
                {
                    Index = 17,
                    Title = "Вернуться к боту-бомжу",
                    Text = "Сообщите боту-бомжу, где его собака",
                    TaskIds = new() { [QuestTaskId.TellHomelessBotWhereHisDogIs] = 1 },
                    QuestId = QuestLineId.Beginner,
                    Rewards = new IReward[]
                    {
                        new CaseReward() { Amount = 1, CaseName = "Премиум кейс" },
                        new MoneyReward() { Amount = 3_000_000 }
                    }
                },
            };

            questTasks.ForEach(task => _questConfig.AddQuestTask(QuestLineId.Beginner, task));
        }
    }
}
