﻿using API.Identity.Shared.Configuration.Configuration.Identity;

namespace API.Identity.Helpers.Localization
{
    public static class LoginPolicyResolutionLocalizer
    {
        public static string GetUserNameLocalizationKey(LoginResolutionPolicy policy)
        {
            switch (policy)
            {
                case LoginResolutionPolicy.Username:
                    return "Username";
                case LoginResolutionPolicy.Email:
                    return "Email";
                default:
                    return "Username";
            }
        }
    }
}
