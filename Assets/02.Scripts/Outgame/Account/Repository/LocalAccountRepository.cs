using System;
using UnityEngine;

public class LocalAccountRepository : IAccountRepository
{
    private const string SALT = "kk23";
    
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
        
        string hashedPassword = Crypto.HashPassword(password, SALT);
        // 해싱문자열: 문자열을 특정 알고리즘을 이용해서 변경된 고정된 길이의 문자열 
        // 성훈씨 아이디 : tjdgnd1004
        //      비밀번호: 10041004!
        
        PlayerPrefs.SetString(email, hashedPassword);
        
        return new AuthResult()
        {
            Success = true,
            Account = new Account(email, hashedPassword),
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
        
        // 3. 비밀번호 틀렸다면 실패.
        string myPassword = PlayerPrefs.GetString(email);
        if (Crypto.VerifyPassword(password, myPassword, SALT))
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
            Account = new Account(email, myPassword),
        };
    }

    public void Logout()
    {
        Debug.Log("로그아웃 됐습니다.");
    }
}