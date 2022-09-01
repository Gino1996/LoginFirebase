using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;

public class FirebaseManager : MonoBehaviour
{
   

    private string userID;
    //Llamar a las depencencias de la libreria firebase
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBReference;
    
    //crear variables para el login
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_Text warningLoginText;
    public TMP_InputField passwordLoginField;
    public TMP_Text confirmLoginText;
    
    //variables crear nuevo registro
    [Header ("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public TMP_Text warningRegisterText;
    //

    [Header("SaveDB")] 
    public TMP_InputField SaveUsernameField;
    public TMP_InputField estaturaRegisterField;
    public TMP_InputField pesoRegisterField;
    public TMP_Text warningUsersDBField;
    [Header("Mapbox")]
    public Transform RegisterUsersContent;
    public TMP_InputField GameUsernameField;
    

    [Header("VerificationMessage")] 
    public TMP_Text verificationMessageField;

    [Header("ForgetPassword")] 
    public TMP_InputField EmailforgetPassField;

    public TMP_Text confirmMessageForgetPassField;
    public TMP_Text warningMessageForgetPassField;
    
    
    private void Awake()
    {
        //verificamos todas las dependencias de firebase que necesitamos 
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            //establece el resultado si todos estan disponible
            if (dependencyStatus == DependencyStatus.Available)
            {
                //ejectua la funcion de firebase para inicializar
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Error al resolver dependecias"+dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        Debug.Log("Configuración exitosa de firebase Auth");
       
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChange;
        AuthStateChange(this,null);
        //Referencia raiz de la instancia firebase database
        DBReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void AuthStateChange(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser!=User)
        {
            bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && User!=null)
            {
                Debug.Log("Signed out"+User.UserId);
            }

            User = auth.CurrentUser;
            if (signedIn)
            {
             Debug.Log("SignIn"+User.UserId);   
            }
        }
    }

    private void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        confirmLoginText.text = "";
        warningLoginText.text = "";
    }

    private void ClearRegisterFields()
    {
        emailRegisterField.text = "";
        usernameRegisterField.text = "";
        passwordRegisterField.text = "";
        confirmPasswordRegisterField.text = "";
        pesoRegisterField.text = "";
        estaturaRegisterField.text = "";
        warningLoginText.text = "";
    }

    private void ClearFogetPassField()
    {
        EmailforgetPassField.text = "";
        warningMessageForgetPassField.text = "";
    }

    public void ClearDatabaseUsersFields()
    {
        pesoRegisterField.text = "";
        estaturaRegisterField.text = "";
        warningLoginText.text = "";
    }
    public void LoginButton()
    {
        //enviamos los parametros de registro para loguearse
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        //enviamos los parametros de registro para añadir un nuevo usuario
        StartCoroutine(Register(emailRegisterField.text,passwordRegisterField.text,usernameRegisterField.text));
    }

    public void SignOutButton()
    {
        auth.SignOut(); //desconecta del metodo de autenticación propio de la librería firebase.auth
        UIManager.instance.LoginScreen();
        ClearRegisterFields();
        ClearLoginFields();
        //ClearDatabaseUsersFields();
       
    }

    public void BackButtonLogin()
    {
        ClearLoginFields();
        ClearRegisterFields();
        ClearFogetPassField();
        //ClearDatabaseUsersFields();
        UIManager.instance.LoginScreen();
    }
    

    public void SaveButton()
    {
        SaveData();
    }

    public void forgetPassButton()
    {
        if (string.IsNullOrEmpty(EmailforgetPassField.text))
        {
            warningMessageForgetPassField.text = "Ingrese el correo por favor !";
            return;
        }
        forgotPassword(EmailforgetPassField.text); 
       
        UIManager.instance.LoginScreen();
    }
    
    private IEnumerator Login(string _email, string _password)
    {
        //llama a firebase auth la funcion signin y envia el email y el password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);
        if (LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorcode = (AuthError)firebaseEx.ErrorCode;
            string message = "Error en Login !";
            switch (errorcode)
            {
                    
                case  AuthError.MissingEmail:
                   message = "Email vacío !";
                   //message = "Ingrese todos los datos !";
                    break;
                
                case  AuthError.MissingPassword:
                    message = "Contraseña vacía !";
                   // message = "Ingrese todos los datos !";
                    break;
                case AuthError.WrongPassword:
                    message = "Contraseña erronea !";
                    break;
                case AuthError.InvalidEmail:
                    message = "Email inválido !";
                    break;
                case AuthError.UserNotFound:
                    message = "La cuenta no existe !";
                    break;
            }
 
            warningLoginText.text = message;
        }
        else {
           if (User.IsEmailVerified)
           {
                //el usuario a podido loguearse y se envia el resultado
                User = LoginTask.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
                warningLoginText.text = "";
                confirmLoginText.text = "Login exitoso !";
                //llamar al metodo de administrador de interfaz de usuario de datos 
                yield return new WaitForSeconds(2);
                
                
                //Comparar con los datos de la base
                //Condicion si el usuario ya registró todos los datos puede acceder al juego
                
                UIManager.instance.UserGameScreen();
                GameUsernameField.text = User.DisplayName;
               
               
                //Si aun no ingresa todos los datos (registrar datos faltantes)
                
               // SaveUsernameField.text = User.DisplayName;
               // UIManager.instance.UsersDatabaScreen();
                
                confirmLoginText.text = "";
                ClearLoginFields();
                ClearRegisterFields();
           }
           else
           {
               StartCoroutine(SendVerificationEmail());
               confirmLoginText.text = "";
               ClearLoginFields();
               ClearRegisterFields();
           }
        }
    }

     private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //si está vacio el user name se envia un warning
            warningRegisterText.text = "Llene todos los datos !";
        }
   
        else if (passwordRegisterField.text != confirmPasswordRegisterField.text)
        {
            //si el password no coincide
            warningRegisterText.text = "Contraseña no coincide !";
        }
        else
        {
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);
            
            if (RegisterTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                    
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorcode = (AuthError)firebaseEx.ErrorCode;
                string message = "Registro fallido !";
                //continue
                switch (errorcode)
                {
                   
                    case AuthError.MissingEmail:
                        message = "Email vació !";
                        break;
                    case AuthError.MissingPassword:
                        message = "Contraseña vacía !";
                        break;
                    case AuthError.WeakPassword:
                        message = "Contraseña dévil !";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email en uso !";
                        break;
                }

                warningRegisterText.text = message;
            }
            else
            {
                //el usuario a sido creado
                User = RegisterTask.Result;
                if (User!= null)
                {
                    //crea un usuario y envia el username
                    UserProfile profile = new UserProfile { DisplayName = _username };
                    //llama a firebase auth update profile en y sus funciones y envia el profile al username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //espera mientras se completa la tarea
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
                    
                    if (ProfileTask.Exception != null)
                    {
                        //control de errores 
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx= ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorcode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username set failed";
                    }
                    else
                    {
                        if (User.IsEmailVerified)
                        {
                           //Continue 
                        }
                        else
                        {
                            //el usuario ahora es enviado
                            //retorna al login
                            //UIManager.instance.LoginScreen();
                            warningRegisterText.text = "";
                            //  emailLoginField.text = "";
                            //  passwordLoginField.text = "";
                            ClearLoginFields();
                            ClearRegisterFields();
                            StartCoroutine(SendVerificationEmail());

                        }
                        
                    }
                }
            }
        }
    }

    

    private void AwaitVerification(bool _confirm, string _email, string _message)
    {
        UIManager.instance.VerificationScreen();
        if (_confirm)
        {
            verificationMessageField.text = $"Verificación enviada ! \n Verifique en su email {_email} caso contrario ingrese un email válido";
        }
        else
        {
            verificationMessageField.text = $"Correo electrónico no verificado \n Ingrese a su buzón y verifique su cuenta o ingrese un correo válido {_email}";
        }
    }
    private IEnumerator SendVerificationEmail()
    {
        //verificamos si hay un usuario que llame a la funcion, espera que se complete la prueba y mira si se obtiene un error
        if (User!=null)
        {
           var emailTask = User.SendEmailVerificationAsync();
            yield return new WaitUntil(predicate:(() =>emailTask.IsCompleted)) ;
            if (emailTask.Exception!=null)
            {
                FirebaseException firebaseEx=(FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseEx.ErrorCode;
                string message="Error desconocido!";
                switch (error)
                {
                    case AuthError.Cancelled:
                        message = "La verificación de la tarea fue cancelada";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        message = "Email inválido";
                        break;
                    case AuthError.TooManyRequests:
                        message = "Muchas solicitudes";
                        break;
                }
                
                AwaitVerification(false, User.Email, message);
            }
            else
            {
                //se confirma el mensaje de vefification con un true se envia y se envia el email
                AwaitVerification(true, User.Email, null);
                Debug.Log("Task Verification email successfully");
            }
        }
    }
    
    //Enviar solicitud de correo para cambiar contraseña
    private void forgotPassword(string _sendEmail)
    {
        auth.SendPasswordResetEmailAsync(_sendEmail).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SendpassEmail async Canceled");
            }

            if (task.IsFaulted)
            {
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseException= exception as Firebase.FirebaseException;
                    if (firebaseException!=null)
                    {
                        var errorCode = (AuthError)firebaseException.ErrorCode;
                        Debug.LogError(errorCode);
                        confirmMessageForgetPassField.text = "Solicitud de cambio de contraseña enviada, verifique su correo";
                    }
                }
            }
        });
    }
    
    
    
    
    
    
    
    
    
    
    //METODOS PARA GUARDAR EN LA BASE DE DATOS AL ESTAR LOGUEADOS CON EL USUARIO
    
    
    
    private void SaveData()
    {
        //User us = new User(usernameRegisterField.text,double.Parse(pesoRegisterField.text),double.Parse(estaturaRegisterField.text));
        User us = new User();
        us.username = User.DisplayName;
        us.estatura = double.Parse(estaturaRegisterField.text);
        us.peso = double.Parse(pesoRegisterField.text);
 
        string json= JsonUtility.ToJson(us);
        
        DBReference.Child("User").Child(User.UserId).SetRawJsonValueAsync(json).ContinueWith(task =>
            //DBReference.Child("User").Child(userID).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Task save data complete");
            }
            else
            {
                Debug.Log("Error !");
            }     
        });
    }
    
    
    public void SaveDataUsersButton()
    {
        
        StartCoroutine(UpdateUsernameAuth(usernameRegisterField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameRegisterField.text));
        StartCoroutine(UpdateEstaturaDatabase(double.Parse(estaturaRegisterField.text)));
        StartCoroutine(UpdatePesoDatabase(double.Parse(pesoRegisterField.text)));
    }
    
    
    
    
    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //creamos un userprofile y enviamos el nombre de usuario
        UserProfile profile = new UserProfile { DisplayName = _username };
        //invovamos a firebase auth update user profile funtion para enviar el profile con el username
        if (User != null)
        {
            var ProfileTask = User.UpdateUserProfileAsync(profile);
            //esperamos hasta que la tarea se complete
            yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
            if (ProfileTask.Exception!=null)
            {
                Debug.LogWarning(message:$"Failed to register task with update auth {ProfileTask.Exception}");
            }
            else
            {
                //el usuario ya esta actualizado 
            }
        }
    }
    //BASE DE DATOS

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        var DBTask = DBReference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);
        yield return new WaitUntil(predicate:()=>DBTask.IsCompleted);
        if (DBTask.Exception!=null)
        {
            Debug.LogWarning(message:$"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //La base de datos actualizó el username (now)
        }
    }
    private IEnumerator UpdatePesoDatabase(double _peso)
    {
        var DBTask = DBReference.Child("users").Child(User.UserId).Child("peso").SetValueAsync(_peso);
        yield return new WaitUntil(predicate:()=>DBTask.IsCompleted);
        if (DBTask.Exception!=null)
        {
            Debug.LogWarning(message:$"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //La base de datos actualizó el peso (now)
        }
    }
    private IEnumerator UpdateEstaturaDatabase(double _estatura)
    {
        var DBTask = DBReference.Child("users").Child(User.UserId).Child("estatura").SetValueAsync(_estatura);
        yield return new WaitUntil(predicate:()=>DBTask.IsCompleted);
        if (DBTask.Exception!=null)
        {
            Debug.LogWarning(message:$"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //La base de datos actualizó la estatura (now)
        }
    }
}
