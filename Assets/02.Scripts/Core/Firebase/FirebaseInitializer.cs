using System;
using Cysharp.Threading.Tasks;
#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase;
#endif
using UnityEngine;

namespace Core
{
    public class FirebaseInitializer
    {
        private FirebaseAuthService _authService;
        private FirebaseStoreService _storeService;

        public IFirebaseAuthService AuthService => _authService;
        public IFirebaseStoreService StoreService => _storeService;

        public async UniTask Initialize()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
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
#else
            throw new InvalidOperationException("Firebase는 WebGL에서 지원되지 않습니다.");
#endif
        }
    }
}
