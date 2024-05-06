using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Account;
using api.Interfaces;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Util;

namespace api.Service
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly FirebaseAuth _firebaseAuth;

        public FirebaseAuthService(FirebaseAuthClient firebaseAuthClient, FirebaseApp firebaseApp)
        {
            _firebaseAuthClient = firebaseAuthClient;
            _firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);
        }

        public async Task<UserDTO?> SignUp(string email, string password, string username)
        {
            var userCredentials = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);

            if (userCredentials == null) return null;

            var token = await userCredentials.User.GetIdTokenAsync();

            return new UserDTO { Username = userCredentials.User.Info.DisplayName, Token = token };
        }

        public async Task<UserDTO?> Login(string email, string password)
        {
            var userCredentials = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, password);

            if (userCredentials == null) return null;

            var token = await userCredentials.User.GetIdTokenAsync();
        
            return new UserDTO { Username = userCredentials.User.Info.DisplayName, Token = token };
        }

        public async void SignOut(string userId)
        {
            var existingUser = await _firebaseAuth.GetUserAsync(userId);
            await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(existingUser.Uid);
        }

        public async Task SetCustomClaimsAsync(string uid)
        {
            var customClaims = new Dictionary<string, object>
            {
                { "role", "user" },
            };

            await _firebaseAuth.SetCustomUserClaimsAsync(uid, customClaims);
        }

        public async Task<bool> IsTokenValid(string jwtToken)
        {
            try{
                FirebaseToken decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwtToken);
                return true;
            } catch(FirebaseAdmin.Auth.FirebaseAuthException e)
            {
                return false;
            }
            
        }

        public async Task<string?> GetUserId(string jwtToken)
        {
            try{
                FirebaseToken decodedToken = await _firebaseAuth.VerifyIdTokenAsync(jwtToken);
                Console.WriteLine(decodedToken.Uid);
                return decodedToken.Uid;
            } catch(FirebaseAdmin.Auth.FirebaseAuthException e)
            {
                return null;
            }
        }
    }
}