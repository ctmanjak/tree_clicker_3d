using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Outgame
{
    public class LocalAccountRepository : IAccountRepository
    {
        private const string KeyPrefix = "account:";
        private const char Separator = ':';

        public UniTask<AuthResult> Register(string email, string password)
        {
            string key = KeyPrefix + email;

            if (PlayerPrefs.HasKey(key))
            {
                return UniTask.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "중복된 계정입니다.",
                });
            }

            string salt = Crypto.GenerateSalt();
            string hashedPassword = Crypto.HashPassword(password, salt);
            string storedValue = $"{salt}{Separator}{hashedPassword}";

            PlayerPrefs.SetString(key, storedValue);

            return UniTask.FromResult(new AuthResult
            {
                Success = true,
            });
        }

        public UniTask<AuthResult> Login(string email, string password)
        {
            string key = KeyPrefix + email;

            if (!PlayerPrefs.HasKey(key))
            {
                return UniTask.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "아이디와 비밀번호를 확인해주세요.",
                });
            }

            string storedValue = PlayerPrefs.GetString(key);
            string[] parts = storedValue.Split(Separator);

            if (parts.Length != 2)
            {
                return UniTask.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "계정 데이터가 손상되었습니다.",
                });
            }

            string salt = parts[0];
            string storedHash = parts[1];

            if (!Crypto.VerifyPassword(password, storedHash, salt))
            {
                return UniTask.FromResult(new AuthResult
                {
                    Success = false,
                    ErrorMessage = "아이디와 비밀번호를 확인해주세요.",
                });
            }

            return UniTask.FromResult(new AuthResult
            {
                Success = true,
            });
        }

        public void Logout()
        {
            Debug.Log("로그아웃 됐습니다.");
        }
    }
}
