using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int tries = 5)
    {
        if(AuthState == AuthState.Authenticated)
            return AuthState;

        if(AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticating!");
            return await Authenticating();
        }

        await SignInAnonymouslyAsync(tries);

        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while(AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int tries)
    {
        AuthState = AuthState.Authenticating;
        while (AuthState == AuthState.Authenticating && tries > 0)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch(AuthenticationException ex)
            {
                Debug.LogError(ex);
                AuthState = AuthState.Error;
            }
            catch(RequestFailedException ex)
            {
                Debug.LogError(ex);
                AuthState = AuthState.Error;
            }

            --tries;

            await Task.Delay(1000);
        }

        if(AuthState != AuthState.Authenticated)
        {
            Debug.LogError("Signing In Timed Out!");
            AuthState = AuthState.TimeOut;
        }
    }
}

public enum AuthState
{
    NotAuthenticated, Authenticated, Authenticating, Error, TimeOut
}