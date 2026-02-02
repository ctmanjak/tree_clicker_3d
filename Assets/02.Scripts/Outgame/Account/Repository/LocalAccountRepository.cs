using System;
using UnityEngine;

public class LocalAccountRepository : IAccountRepository
{
    private const char SEPARATOR = ':';

    public bool IsEmailAvailable(string email)
    {
        // 이메일 검사
        if (PlayerPrefs.HasKey(email))
        {
            return false;
        }

        return true;
    }

    public AuthResult Register(string email, string password)
    {
        // 1. 이메일 중복검사
        if (!IsEmailAvailable(email))
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "중복된 계정입니다.",
            };
        }
        
        string salt = Crypto.GenerateSalt();
        string hashedPassword = Crypto.HashPassword(password, salt);
        string storedValue = $"{salt}{SEPARATOR}{hashedPassword}";

        PlayerPrefs.SetString(email, storedValue);

        return new AuthResult()
        {
            Success = true,
        };
    }

    public AuthResult Login(string email, string password)
    {
        // 2. 가입한적 없다면 실패!
        if (!PlayerPrefs.HasKey(email))
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "아이디와 비밀번호를 확인해주세요.",
            };
        }
        
        // 3. 저장된 값에서 솔트와 해시 분리
        string storedValue = PlayerPrefs.GetString(email);
        string[] parts = storedValue.Split(SEPARATOR);

        if (parts.Length != 2)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "계정 데이터가 손상되었습니다.",
            };
        }

        string salt = parts[0];
        string storedHash = parts[1];

        // 4. 비밀번호 검증
        if (!Crypto.VerifyPassword(password, storedHash, salt))
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "아이디와 비밀번호를 확인해주세요.",
            };
        }

        return new AuthResult()
        {
            Success = true,
        };
    }

    public void Logout()
    {
        Debug.Log("로그아웃 됐습니다.");
    }
}