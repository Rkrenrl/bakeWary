using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;
    public DatabaseReference DBreference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;
    bool isLogin = false;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    
    [Header("UserDataUI")]
    public TMP_Text usernameField;
    public TMP_Text levelText;
    public TMP_Text tierText;
    public Image levelBar;
    public Image tierBar;

    [Header("UserData")]
    public string username;
    public float xp;
    public float score;
    public float level;
    public float tier;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    void Update()
    {
        if(isLogin)
        {
            levelText.text = level.ToString();
            tierText.text = tier.ToString();
            levelBar.fillAmount = xp / 500;
            tierBar.fillAmount = score / 100;

            if(Input.GetKeyDown(KeyCode.F11))
            {
                xp += 10;
                StartCoroutine(UpdateXp(xp));
            }
            if(Input.GetKeyDown(KeyCode.F12))
            {
                score += 10;
                StartCoroutine(UpdateScore(score));
            }

            if(xp >= 500)
            {
                xp = 0;
                level++;
                StartCoroutine(UpdateXp(xp));
                StartCoroutine(UpdateLevel(level));
            }
            if(score >= 100)
            {
                score = 0;
                tier++;
                StartCoroutine(UpdateScore(score));
                StartCoroutine(UpdateTier(tier));
            }
        }
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;    
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;    
    }

    public void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }
    
    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void SignOutButton()
    {
        auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearLoginFields();
        ClearRegisterFields();
    }

    public void SaveDataButton()
    {
        StartCoroutine(UpdateLevel(level));
        StartCoroutine(UpdateTier(tier));
        StartCoroutine(UpdateXp(xp));
        StartCoroutine(UpdateScore(score));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }

            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            yield return new WaitForSeconds(2);
            
            StartCoroutine(LoadUserData());

            usernameField.text = User.DisplayName;
            username = usernameField.text;

            UIManager.instance.LobbyScreen();

            ClearLoginFields();
            ClearRegisterFields();

            isLogin = true;
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else 
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }

                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();                 
                        warningRegisterText.text = "";       

                        ClearLoginFields();
                        ClearRegisterFields();
                    }
                }
            }
        }
    }  

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        UserProfile profile = new UserProfile { DisplayName = _username };

        var ProfileTask = User.UpdateUserProfileAsync(profile);

        yield return new WaitUntil(predicate : () => ProfileTask.IsCompleted);

        if(ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            
        }
    }

    public IEnumerator UpdateUsernameDatabase(string _username)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate : () => DBTask.IsCompleted);

        if(DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            
        }
    }

    public IEnumerator UpdateLevel(float _level)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("level").SetValueAsync(_level);

        yield return new WaitUntil(predicate : () => DBTask.IsCompleted);

        if(DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public IEnumerator UpdateTier(float _tier)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("tier").SetValueAsync(_tier);

        yield return new WaitUntil(predicate : () => DBTask.IsCompleted);

        if(DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public IEnumerator UpdateXp(float _xp)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("xp").SetValueAsync(_xp);

        yield return new WaitUntil(predicate : () => DBTask.IsCompleted);

        if(DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public IEnumerator UpdateScore(float _score)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("score").SetValueAsync(_score);

        yield return new WaitUntil(predicate : () => DBTask.IsCompleted);

        if(DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    private IEnumerator LoadUserData()
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load data: {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            // 최초 유저
            level = 0;
            tier = 0;
            xp = 0;
            score = 0;

            SaveDataButton(); // 처음만 저장
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            level = Convert.ToSingle(snapshot.Child("level").Value);
            tier = Convert.ToSingle(snapshot.Child("tier").Value);
            xp = Convert.ToSingle(snapshot.Child("xp").Value);
            score = Convert.ToSingle(snapshot.Child("score").Value);
        }
    }

}
