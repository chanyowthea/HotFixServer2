using System;
using GCommon;
using System.IO;
using LitJson;
using System.Collections.Generic;

namespace message
{
    //----------------------------------------------------------------------
    //HTTP
    enum ServiceMessageTypeHTTP
    {
        LogEvent,
        Login,
        Register,
        FacebookLogin,
        FacebookRegister,
        PlatformLogin,
        ChooseRegion,
        PlatformRegister,
        GetPlatformProfile,
        AccountMatchStats,
        GetWallet,
        GetBackpack,
        //ChooseAvatar,
        //ChooseClothes,
        Billboard,
        GetFriend,
        GetFriendRequestList,
        RemoveFriend,
        DeclineFriendRequest,
        ConfirmFriendRequest,
        RequestAddingFriend,
        FuzzySearchAccountByName,
        GetAccountInfoByAccountID,
        GetFacebookFriend,
        GetPlatformFriends,
        GetStore,
        Purchase,
        OpenTreasureBox,
        GetRecommendedFriend,
        Leaderboard,
        GetPlayerStats,
        GetAttendanceList,
        AttendanceSignin,
        ChooseLoadout,
        LinkFacebookAccount,
        PlatformGuestBind,
        Logout,
        GetTopNDailyLeaderboard,
        GetDailyLeaderboardByAccountID,
        GetAnnouncement,
        GetSplashBanner,
        GetPresenceByAccountIds,
        GetCBTGiftsAnnouncement,
        ReadCBTGiftsAnnouncement,
        NewPlayerSignin,
        GetNewPlayerRewardsList,
        GetActivityDesc,
        GetActivityInfo,
        GetActivityRewards,
        CSVConfig,
        GetMatchDailyBonus,
        CreateClan,
        RequestJoinClan,
        GetClanApplicantListByClanID,
        ApproveJoinClanApplication,
        DeclineJoinClanApplication,
        InviteToClan,
        ApproveJoinClanInvitation,
        ChangeMemberTypeByCaptain,
        ReassignCaptain,
        RemoveClanMember,
        QuitClan,
        GetClanInfoByClanID,
        GetClanInfoByClanName,
        FuzzySearchClanByName,
        GetClanMembers,
        ModifyClanInfo,
        GetRandomClanList,
        OpenBundle,
        GetProfiles,
        GetSkills,
        SelectProfile,
        UnlockProfile,
        EquipSkill,
        ChangeClothes,
        ModifyNickname,
        DismissClan,
        GetPlayerRankingInfo,
        GetMailList,
        ReadMail,
        GetMatchStatsHistory,
        ChooseBanner,
        ChooseHeadPic,
        ChooseSlots,
    };

    public enum EServiceCommonErrorCode
    {
        COMMON_ERROR = 400,
    }

    public enum EClanErrorCode
    {
        OK = 0,
        COMMON_EROOR = 400,
    }

    public enum ECreateClanErrorCode
    {
        COMMON_ERROR = 400,
        MONEY_NOT_ENOUGH = 500,
    }

    public enum EJoinClanErrorCode
    {
        COMMON_ERROR = 400,
    }

    public enum EModifyNickNameErrorCode
    {
        COMMON_ERROR = 400,
    }

    public static class ServiceMessageError
    {
        public const uint Login_BadRequest = 400;
        public const uint Login_Forbidden = 403;
        public const uint Login_UserNotExisted = 404;
        public const uint Register_StatusBadRequest = 400;

        // garena sdk 错误�        
        public const uint InvalidMsdkErrorCode = 900;

        public const uint Match_Result_NoResultFound = 404;
    }

    public class HTTP_BaseRequest
    {
        protected virtual bool RequestEquals(HTTP_BaseRequest other_request)
        {
            if (other_request == null)
                return false;
            return GetType() == other_request.GetType();
        }

        public override bool Equals(object obj)
        {
            if (obj != null
                && obj is HTTP_BaseRequest)
            {
                return RequestEquals(obj as HTTP_BaseRequest);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }
    }

    public class HTTP_BaseResponse
    {

    }
}
