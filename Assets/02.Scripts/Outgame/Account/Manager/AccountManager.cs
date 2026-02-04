using System;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    [DefaultExecutionOrder(-50)]
    public class AccountManager : MonoBehaviour
    {
        public static AccountManager Instance { get; private set; }

        private Account _currentAccount = null;
        public bool IsLogin => _currentAccount != null;
        public string Email => _currentAccount?.Email ?? string.Empty;

        private IAccountRepository _repository;

        private void Awake()
        {
            Instance = this;
        }

        private async UniTaskVoid Start()
        {
            await GameBootstrap.Instance.Initialization;
            ServiceLocator.TryGet(out _repository);
        }

        public async UniTask<AuthResult> TryLogin(string email, string password)
        {
            Account account;
            try
            {
                account = new Account(email, password);
            }
            catch (ArgumentException ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                };
            }

            AuthResult result = await _repository.Login(email, password);
            if (result.Success)
            {
                _currentAccount = account;
                return new AuthResult
                {
                    Success = true,
                    Account = _currentAccount,
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
            };
        }

        public async UniTask<AuthResult> TryRegister(string email, string password)
        {
            try
            {
                Account account = new Account(email, password);
            }
            catch (ArgumentException ex)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                };
            }

            AuthResult result = await _repository.Register(email, password);
            if (result.Success)
            {
                return new AuthResult
                {
                    Success = true,
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
            };
        }

        public void Logout()
        {
            _currentAccount = null;
            _repository.Logout();
        }
    }
}
