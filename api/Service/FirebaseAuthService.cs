using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Account;
using api.Interfaces;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Apis.Util;

namespace api.Service
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthClient _firebaseAuthClient;

        public FirebaseAuthService(FirebaseAuthClient firebaseAuthClient)
        {
            _firebaseAuthClient = firebaseAuthClient;
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

        public User GetUser() => _firebaseAuthClient.User;

        public void SignOut() => _firebaseAuthClient.SignOut();

        public bool IsUserSignedIn()
        {
            try
            {
                string user = _firebaseAuthClient.User.Uid;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}