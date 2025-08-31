using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework
{
    public class Language
    {
        private readonly static LangType CurrentLanguage = LangType.Russian;

        private readonly static Dictionary<LangType, Dictionary<TextType, string>> List = new Dictionary<LangType, Dictionary<TextType, string>>()
        {
            // Русский
            { LangType.Russian, new Dictionary<TextType, string>() {
                { TextType.AccountNotFound, "Аккаунт {0} не надйен!" },
                { TextType.ErrorLogin, "Ошибка при входе в аккаунт!" },
                { TextType.WrongPassword, "Неправильный пароль, у вас осталось {0} {1}" },
                { TextType.AlreadyLogined, "Игрок под вашим аккаунтом уже играет на сервере. Если это не вы, смените быстро пароль и обратитесь в администрацию!" },
                { TextType.CheckWrongPassword, "Пароли не совпадают!" },
                { TextType.ErrorRegister, "Непредвиденная ошибка при регистрации. Обратитесь в поддержку сервера!" },
                { TextType.FormatLogin, "Неправильный формат логина" },
                { TextType.SocialRegister, "Аккаунт с вашим SocialClub уже зарегистрирован" },
                { TextType.LoginRegister, "Данный логин уже занят" },
                { TextType.FormatPassword, "Пароль должен содержать минимум 3 символа. Можно использовать только Английские буквы и цифры!" },
                { TextType.PlayerDontExsistDynamic, "Игрок ({0}) не найден" },
                { TextType.PlayerDontExsistStatic, "Игрок #{0} не найден!" },
                { TextType.YouSendToCreator, "Вы отправили игрока {0} в создание персонажа" },
                { TextType.FerrisCabTaken, "Кабинка #{0} занята, дождитесь другой кабинки" },

                { TextType.WhitelistAdd, "Вы добавил нового пользователя в белый список: {0}" },
                { TextType.WhitelistAddError, "Не удалось добавить нового пользователя в белый список" },
                { TextType.WhitelistRemove, "Вы удалили пользователя из белого списка: {0}" },
                { TextType.WhitelistRemoveError, "Не удалось удалить {0} из белого списка. Скорее всего его там нет!" },

                { TextType.CharacterAgeError, "Возраст может быть не меньше {0} лет и не больше {1}" },
                { TextType.ErrorTryGetCharacterData, "Не удалось получить данные о пресонаже" },
                { TextType.CharacterErrorName, "Ошибка в правильности имени" },
                { TextType.CharacterErrorSurname, "Ошибка в правильности фамилии" },
                { TextType.CharacterErrorExists, "Данное имя уже занято. Придумайте другое" },

                { TextType.NotNowDay, "Не сегодня..." },
                { TextType.NoThanks, "Нет, спасибо" },
                { TextType.WelcomePDM, "Привет, {0}! Ты попал{1} в автосалон - \"Premium Motosport\", чем могу быть полезна?" },
                { TextType.GoShowShowroom, "Покажи мне список транспорта" },
                { TextType.NoModelsLeft, "Недостаточно моделей на складе" },
                { TextType.ErrorBuyVehicle, "Ошибка при покупка транспорта в салоне, обратитесь в тех. поддержку" },
                { TextType.YouBuyVehicle, "Вы приобрели транспортное средство {0}" },
                { TextType.TestdriveEnded, "Время тестдрайва завершилось" },
                { TextType.Testdrive, "Тестдрайв" },

                { TextType.CarHaveFullPetrol, "У машины и так полный бак" },
                { TextType.PetrolStationDoesntWork, "Данная заправочная станция временно не работает!" },
                { TextType.YouPetrolingVehicle, "Вы заправили транспорт на {0} литров!" },
                { TextType.PetrolDescription, "Данный бизнес представляет собой место, где водители могут заправить свои автомобили, мотоциклы и другие транспортные средства." },
                { TextType.EnablePetrolTypes, "Доступные виды" },

                { TextType.NoInventoryPlaces, "Недостаточно места в инвентаре" },
                { TextType.NoAccess, "Нет доступа!" },
            }},

            // Украинский
            { LangType.Ukraine, new Dictionary<TextType, string>() {
                
            }},
        };
        public static string GetText(TextType type, params object[] arguments)
        {
            if (List[CurrentLanguage].TryGetValue(type, out string text))
            {
                return String.Format(text, arguments);
            }
            return "undefined";
        } 
    }
}
