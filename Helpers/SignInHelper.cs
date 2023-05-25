using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using System.Threading;
//using System.Collections.Generic;
//using System.Threading.Tasks;

namespace TeamsMessagingExtensionsSearchAuthConfig.Helpers
{
    public static class SignInHelper
    {
        public static string GetTokenAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {

            //var state = action.State; // Check the state value
            //var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
            //if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            //{
            //    // There is no token, so the user has not signed in yet.

            //    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
            //    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

            //    return new MessagingExtensionActionResponse
            //    {
            //        ComposeExtension = new MessagingExtensionResult
            //        {
            //            Type = "auth",
            //            SuggestedActions = new MessagingExtensionSuggestedAction
            //            {
            //                Actions = new List<CardAction>
            //                    {
            //                        new CardAction
            //                        {
            //                            Type = ActionTypes.OpenUrl,
            //                            Value = signInLink,
            //                            Title = "Bot Service OAuth",
            //                        },
            //                    },
            //            },
            //        },
            //    };
            //}
            return "";
        }
    }
}
