using System;
using System.Text.RegularExpressions;

// 객체 : 어떤 대상/개념에 대한 속성(데이터) + 기능(메서드)
// 도메인: 어떤 개념에 집중해서 객체로 표현한 것
namespace Outgame
{
    public class Account
    {
        // 이메일
        // 비밀번호
        public readonly string Email;
        public readonly string Password;

  
    
        public Account(string email, string password)
        {
            var emailSpec = new EmailAccountSpecification();
            if (!emailSpec.IsSatisfiedBy(email))
            {
                throw new ArgumentException(emailSpec.ErrorMessage);
            }
        
            if(string.IsNullOrEmpty(password)) throw new ArgumentException($"비밀번호는 비어있을 수 없습니다.");
            if(password.Length < 6 || 15 < password.Length) throw new ArgumentException($"비밀번호는 6~15자 사이어야합니다.");
            if(!Regex.IsMatch(password, "[A-Z]")) throw new ArgumentException($"비밀번호는 대문자를 1개 이상 포함해야합니다.");
            if(!Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]")) throw new ArgumentException($"비밀번호는 특수문자를 1개 이상 포함해야합니다.");
        
            Email = email;
            Password = password;
        }
    
        // 이메일 규칙:
        // 0. 비어있으면 안된다.
        // 1. 올바른 이메일이어야한다.
        // 2. 동일한 이메일이면 중복 안된다..
    
        // 비밀번호 규칙
        // 0. 비어있으면 안된다.
        // 1. 6자리 이상 15자 이하  //(대문자 1개이상 포함, 특수문자 1개 이상포함)
    
    
    
    }
}