using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    private enum SceneMode
    {
        Login,
        Register
    }

    private SceneMode _mode = SceneMode.Login;
        
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private GameObject _passwordConfirmObject;
    [SerializeField] private Button _gotoRegisterButton;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _gotoLoginButton;
    [SerializeField] private Button _registerButton;
    
    [SerializeField] private TMP_InputField _idInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _repeatPasswordInputField;

    private void Start()
    {
        _messageText.text = "";
        AddButtonEvents();
        Refresh();
    }

    private void AddButtonEvents()
    {
        _gotoRegisterButton.onClick.AddListener(GotoRegister);
        _gotoLoginButton.onClick.AddListener(GotoLogin);
        _loginButton.onClick.AddListener(Login);
        _registerButton.onClick.AddListener(Register);
    }

    private void Refresh()
    {
        _passwordConfirmObject.SetActive(_mode == SceneMode.Register);
        _gotoLoginButton.gameObject.SetActive(_mode == SceneMode.Register);
        _registerButton.gameObject.SetActive(_mode == SceneMode.Register);
            
        _gotoRegisterButton.gameObject.SetActive(_mode == SceneMode.Login);
        _loginButton.gameObject.SetActive(_mode == SceneMode.Login);
    }

    private void Login()
    {
        string email = _idInputField.text;
        string password = _passwordInputField.text;

        AuthResult result = AccountManager.Instance.TryLogin(email, password);

        if (!result.Success)
        {
            _messageText.text = result.ErrorMessage;
            return;
        }

        SceneManager.LoadScene("GameScene");
    }

    private void Register()
    {
        string email = _idInputField.text;
        string password = _passwordInputField.text;
        string repeatPassword = _repeatPasswordInputField.text;

        if (password != repeatPassword)
        {
            _messageText.text = "Passwords do not match";
            return;
        }

        AuthResult result = AccountManager.Instance.TryRegister(email, password);

        if (!result.Success)
        {
            _messageText.text = result.ErrorMessage;
            return;
        }

        _messageText.text = "Registered";
        GotoLogin();
    }

    private void GotoLogin()
    {
        _mode = SceneMode.Login;
        Refresh();
    }
        
    private void GotoRegister()
    {
        _mode = SceneMode.Register;
        Refresh();
    }
}