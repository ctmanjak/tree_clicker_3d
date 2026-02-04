using System;
using Cysharp.Threading.Tasks;
using Firebase;
using UnityEngine;

namespace Outgame
{
    public class FirebaseInitializer
    {
        private FirebaseAuthService _authService;
        private FirebaseStoreService _storeService;

        public IFirebaseAuthService AuthService => _authService;
        public IFirebaseStoreService StoreService => _storeService;

        public async UniTask Initialize()
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                throw new InvalidOperationException($"Firebase 초기화 실패: {dependencyStatus}");
            }

            _authService = new FirebaseAuthService();
            await _authService.Initialize();

            _storeService = new FirebaseStoreService(_authService);
            await _storeService.Initialize();

            Debug.Log("Firebase 초기화 완료");
        }
    }
}
