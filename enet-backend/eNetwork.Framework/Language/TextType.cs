using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNetwork.Framework
{
    public enum LangType
    {
        Russian,
        Ukraine,
    }
    public enum TextType
    {
        AccountNotFound,
        ErrorLogin,
        WrongPassword,
        AlreadyLogined,
        ErrorRegister,
        SocialRegister,
        LoginRegister,
        FormatLogin,
        FormatPassword,
        CheckWrongPassword,

        CharacterAgeError,
        ErrorTryGetCharacterData,
        CharacterErrorName,
        CharacterErrorSurname,
        CharacterErrorExists,

        PlayerDontExsistDynamic,
        PlayerDontExsistStatic,

        YouSendToCreator,

        WhitelistAdd,
        WhitelistAddError,
        WhitelistRemove,
        WhitelistRemoveError,

        FerrisCabTaken,

        CarHaveFullPetrol,
        PetrolStationDoesntWork,
        YouPetrolingVehicle,
        PetrolDescription,
        EnablePetrolTypes,

        NoThanks,
        NotNowDay,
        
        NoModelsLeft,
        ErrorBuyVehicle,
        YouBuyVehicle,
        TestdriveEnded,
        Testdrive,

        WelcomePDM,
        GoShowShowroom,

        PamelaFulton,

        NoInventoryPlaces,

        NoAccess,
    }
}
