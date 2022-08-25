using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class FirebaseManager : MonoBehaviour
{
    //Llamar a las depencencias de la libreria firebase
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
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
    public TMP_InputField estaturaField;
    public TMP_InputField pesoField;
    public TMP_Text warningRegisterText;

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

    private void InitializeFirebase()
    {
        Debug.Log("Configuración exitosa de firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginButton()
    {
        //enviamos los parametros de registro para loguearse
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        //enviamos los parametros de registro para añadir un nuevo usuario
        StartCoroutine(Register(emailRegisterField.text,passwordRegisterField.text,usernameRegisterField.text, estaturaField.text,pesoField.text));
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
            //el usuario a podido loguearse y se envia el resultado
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Login exitoso !";
           //llamar al metodo de administrador de interfaz de usuario de datos 
           yield return new WaitForSeconds(2);
           UIManager.instance.UserGameScreen();
           confirmLoginText.text = "";
        }
    }

    private IEnumerator Register(string _email, string _password, string _username, string _estatura, string _peso)
    {
        if (_username == "")
        {
            //si está vacio el user name se envia un warning
            warningRegisterText.text = "Usuario vacío !";
        }
        else if (_estatura=="")
        {
            warningRegisterText.text = "Estatura vacía";
        }else if (_peso=="")
        {
            warningRegisterText.text = "Peso vacío";
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
                        message = "Email vación !";
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
                        //el usuario ahora es enviado
                        //retorna al login
                        UIManager.instance.LoginScreen();
                        emailLoginField.text = "";
                        passwordLoginField.text = "";
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }


}
