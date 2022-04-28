using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;


public class FirebaseController : MonoBehaviour

{

    public GameObject loginPanel, signupPanel, profilePanel, forgottenPasswordPanel, notificationPanel, editProfilePanel;
    
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupUserName, forgottenPasswordEmail, profileAbilities, profileCompanies, profileEmployment, profileHobbies, profileInterests, profileLinkedIn, profilePronouns, profileSocialMedia, passwordInput;

    public Text notif_Title_Text, notif_Message_Text, profileUserName_Text, profileUserEmail_Text, profileCompanies_Text, profileEmployment_Text, profileAbilities_Text, profileInterests_Text, profileLinkedIn_Text, profilePronouns_Text, profileHobbies_Text, profileSocialMedia_Text;

    public Toggle rememberMe;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    FirebaseFirestore db;
    Dictionary<string,object> profile;

    bool isSignIn = false;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) 
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();

                db = FirebaseFirestore.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            } 
            else 
            {
                UnityEngine.Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgottenPasswordPanel.SetActive(false);
        editProfilePanel.SetActive(false);
    }

    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        profilePanel.SetActive(false);
        forgottenPasswordPanel.SetActive(false);
        editProfilePanel.SetActive(false);
    }

    public void OpenProfilePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(true);
        forgottenPasswordPanel.SetActive(false);
        editProfilePanel.SetActive(false);

    }

    public void OpenforgottenPasswordPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgottenPasswordPanel.SetActive(true);
        editProfilePanel.SetActive(false);
    }

    public void OpeneditProfilePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgottenPasswordPanel.SetActive(false);
        editProfilePanel.SetActive(true);
    }


    public void LoginUser()
    {
        if(string.IsNullOrEmpty(loginEmail.text)&&string.IsNullOrEmpty(loginPassword.text))
        {
            showErrorMessage("Error", "1 or more fields empty");
            return;
        }
        //LOGIN
        SignInUser(loginEmail.text, loginPassword.text);
    }

    public void SignUpUser()
    {
        if(string.IsNullOrEmpty(signupEmail.text)&&string.IsNullOrEmpty(signupPassword.text)&&string.IsNullOrEmpty(signupCPassword.text)&&string.IsNullOrEmpty(signupUserName.text))
        {
            showErrorMessage("Error", "1 or more fields empty");
            return;
        }

        //SIGNUP
        CreateUser(signupEmail.text, signupPassword.text, signupUserName.text);
    }

    public void forgottenPassword()
    {
        if(string.IsNullOrEmpty(forgottenPasswordEmail.text))
        {
            showErrorMessage("Error", "1 or more fields empty");
            return;
        }
    }

    public void CheckPasswordConditions()
    {
        string ReceivedString = passwordInput.text;

        bool num = false;
        bool capital = false;
        bool lower = false;
        bool special = false;

        for (int i = 0; i < ReceivedString.Length; i++)
        {
            char currentChar = ReceivedString[i];
            if (char.IsDigit(currentChar))
            {
                num = true;
            }
            else if (char.IsUpper(currentChar))
            {
                capital = true;
            }
            else if (char.IsLower(currentChar))
            {
                lower = true;
            }
            else if (!char.IsLetterOrDigit(currentChar))
            {
                special = true;
            }
            if (num && capital && lower && special)
            {
                showErrorMessage("Error", "Does not fit password requirements");
                return;
            }
        }
    }

    private void showErrorMessage(string title, string message)
    {
        notif_Title_Text.text = "" + title;
        notif_Message_Text.text = "" + message;

        notificationPanel.SetActive(true);
    }

    public void CloseNotif_Panel()
    {
       // auth.SignOut();
        notif_Title_Text.text = "";
        notif_Message_Text.text = "";

        notificationPanel.SetActive(false);
    }

    public void LogOut()
    {
        auth.SignOut();
        profileUserName_Text.text = "";
        profileUserEmail_Text.text = "";
        
        OpenLoginPanel();
    }

    void CreateUser(string email, string password, string Username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            } 
            if (task.IsFaulted) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                foreach(Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showErrorMessage("Error", GetErrorMessage(errorCode)) ;
                    }    
                }
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            // Add user to database
            profile = new Dictionary<string,object> 
            {
                {"UserID", newUser.UserId},
                {"Abilities", ""},
                {"Companies", ""},
                {"Employment", ""},
                {"Hobbies", ""},
                {"Interests", ""}, 
                {"LinkedIn", ""}, 
                {"Pronouns", ""}, 
                {"Social Media", ""},
            };
            
            db.Collection("Users").Document(newUser.UserId).SetAsync(profile).ContinueWith(task => 
            {
                if(task.IsCompleted) 
                {
                    Debug.Log("Successfully added user to Firestore database");
                }
                else 
                {
                    Debug.Log("Not successful");
                }
            });

            UpdateUserProfile(Username);
        });

    }
    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                foreach(Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showErrorMessage("Error", GetErrorMessage(errorCode)) ;
                    }
                }
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            
            profileUserName_Text.text = "" + newUser.DisplayName;
            profileUserEmail_Text.text = "" + newUser.Email;

            OpenProfilePanel();    
        });

    }

    public void EditProfile()
    {
        DocumentReference ProfileRef = db.Collection("Users").Document(auth.CurrentUser.UserId);
        ProfileRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (!snapshot.Exists) {
                profile = new Dictionary<string,object> 
                {
                    {"UserID", auth.CurrentUser.UserId},
                };
                db.Collection("Users").Document(auth.CurrentUser.UserId).SetAsync(profile).ContinueWith(task => 
                {
                    if(task.IsCompleted) 
                    {
                        Debug.Log("Successfully added user to Firestore database");
                    }
                    else 
                    {
                    Debug.Log("Not successful");
                    }
                });
            }
        });

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {"Abilities", profileAbilities.text},
            {"Companies", profileCompanies.text},
            {"Employment", profileEmployment.text},
            {"Hobbies", profileHobbies.text},
            {"Interests", profileInterests.text}, 
            {"LinkedIn", profileLinkedIn.text}, 
            {"Pronouns", profilePronouns.text}, 
            {"Social Media", profileSocialMedia.text},
        };

        ProfileRef.UpdateAsync(updates).ContinueWithOnMainThread(task => {
            Debug.Log(
                "Updated the fields of the user's document in database.");
        });

        if (user != null) {
            DocumentReference docRef = db.Collection("Users").Document(auth.CurrentUser.UserId);
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));
                    Dictionary<string, object> city = snapshot.ToDictionary();
                    foreach (KeyValuePair<string, object> pair in city) {
                        switch(pair.Key.ToString()) {
                            case "Abilities":
                                profileAbilities_Text.text = pair.Value.ToString();
                                break;
                            case "Companies":
                                profileCompanies_Text.text = pair.Value.ToString();
                                break;
                            case "Employment":
                                profileEmployment_Text.text = pair.Value.ToString();
                                break;
                            case "Hobbies":
                                profileHobbies_Text.text = pair.Value.ToString();
                                break;
                            case "Interests":
                                profileInterests_Text.text = pair.Value.ToString();
                                break;
                            case "LinkedIn":
                                profileLinkedIn_Text.text = pair.Value.ToString();
                                break;
                            case "Pronouns":
                                profilePronouns_Text.text = pair.Value.ToString();
                                break;
                            case "Social Media":
                                profileSocialMedia_Text.text = pair.Value.ToString();
                                break;
                        }
                        Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                    }
                } else {
                    Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
                }
            });
        }
    }

    void InitializeFirebase() 
    {
        Debug.Log("Initialize");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs) 
    {
        if (auth.CurrentUser != user) 
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) 
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn) 
            {
                Debug.Log("Signed in " + user.UserId);
                isSignIn = true;
                
                
            }       
        }
    }

    void OnDestroy()
    {
        //auth.StateChanged -= AuthStateChanged;
        //auth = null;
    }
    

    void UpdateUserProfile(string UserName)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null) 
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile 
            {
                DisplayName = UserName,
                PhotoUrl = new System.Uri("https://via.placeholder.com/150C/O https://placeholder.com/"),
            };

            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled) 
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) 
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");

                showErrorMessage("Alert", "Account successfully created");
            });
        }
    }

    bool isSigned = false;

    void Update()
    {
        if(isSignIn)
        {
            if(!isSigned)
            {
                isSigned = true;
                profileUserName_Text.text = "" + user.DisplayName;
                profileUserEmail_Text.text = "" + user.Email;

                if (user != null) {
                    DocumentReference docRef = db.Collection("Users").Document(auth.CurrentUser.UserId);
                    docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                    {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists) {
                        Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));
                        Dictionary<string, object> city = snapshot.ToDictionary();
                        foreach (KeyValuePair<string, object> pair in city) {
                            switch(pair.Key.ToString()) {
                                case "Abilities":
                                    profileAbilities_Text.text = pair.Value.ToString();
                                    break;
                                case "Companies":
                                    profileCompanies_Text.text = pair.Value.ToString();
                                    break;
                                case "Employment":
                                    profileEmployment_Text.text = pair.Value.ToString();
                                    break;
                                case "Hobbies":
                                    profileHobbies_Text.text = pair.Value.ToString();
                                    break;
                                case "Interests":
                                    profileInterests_Text.text = pair.Value.ToString();
                                    break;
                                case "LinkedIn":
                                    profileLinkedIn_Text.text = pair.Value.ToString();
                                    break;
                                case "Pronouns":
                                    profilePronouns_Text.text = pair.Value.ToString();
                                    break;
                                case "Social Media":
                                    profileSocialMedia_Text.text = pair.Value.ToString();
                                    break;
                            }
                            Debug.Log(String.Format("{0}: {1}", pair.Key, pair.Value));
                        }
                    } else {
                        Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
                    }
                    });
                }
            }
        }

    }

    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Account already exists";
                break;
            case AuthError.MissingPassword:
                message = "Missing Password";
                break;
            case AuthError.WeakPassword:
                message = "Weak Password";
                break;
            case AuthError.WrongPassword:
                message = "Wrong Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Email is already in use";
                break;
            case AuthError.InvalidEmail:
                message = "Email invalid";
                break;
            case AuthError.MissingEmail:
                message = "Email is missing";
                break;
            default:
                message = "Error";
                break;
        }
        return message;
    }

}

