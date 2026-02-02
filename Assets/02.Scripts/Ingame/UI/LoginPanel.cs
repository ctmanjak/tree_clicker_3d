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
        string id = _idInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _messageText.text = "Enter your ID!";
            return;
        }
            
        string password = _passwordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _messageText.text = "Enter your Password!";
            return;
        }

        if (!PlayerPrefs.HasKey(id))
        {
            _messageText.text = "ID Not Found!";
            return;
        }

        string myPassword = PlayerPrefs.GetString(id);

        if (myPassword != password)
        {
            _messageText.text = "Invalid Password!";
        }
            
        SceneManager.LoadScene("GameScene");
    }

    private void Register()
    {
        string id = _idInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _messageText.text = "Enter your ID";
            return;
        }
            
        string password = _passwordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _messageText.text = "Enter your Password";
            return;
        }
            
        string password2 = _passwordInputField.text;
        if (string.IsNullOrEmpty(password2) || password != password2)
        {
            _messageText.text = "Invalid Password";
            return;
        }
            
        if (PlayerPrefs.HasKey(id))
        {
            _messageText.text = "Duplicated ID";
            return;
        }

        PlayerPrefs.SetString(id, password);
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