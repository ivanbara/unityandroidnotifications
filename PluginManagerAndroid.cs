using UnityEngine;
using System.Collections;
using System.IO;
using Soomla.Profile;

public class PluginManagerAndroid : MonoBehaviour {

    private static PluginManagerAndroid _instance;

    #if !UNITY_EDITOR && UNITY_ANDROID
    AndroidJavaObject myClass = null;
    #endif
    
    // For Screenshots
    private bool isProcessing = false;

    public static PluginManagerAndroid instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PluginManagerAndroid>();
                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            //If I am the first instance, make me the Singleton
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            //If a Singleton already exists and you find
            //another reference in scene, destroy it!
            if (this != _instance)
                Destroy(this.gameObject);
        }

    }

	// Use this for initialization
	void Start () {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (myClass == null)
            myClass = new AndroidJavaObject("com.example.pl.MyClass");

        if (myClass != null)
            myClass.Call("init", new object[0]);
        #endif
    }


    public void clickTestScheduledNotif() {
        this.makeScheduledNotification("title", "content", 10);
    }

    public void makeInstantNotification(string title, string content)
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (myClass != null)
            myClass.Call("makeNotification", new object[] { title, content });
        #endif
    }

    public void makeScheduledNotification(string title, string content, int delay)
    {
        int millisecDelay = delay * 1000;

        #if !UNITY_EDITOR && UNITY_ANDROID
        if (myClass != null)
            myClass.Call("createScheduledNotification", new object[] { title, content, millisecDelay });
        #endif

        #if !UNITY_EDITOR && UNITY_IOS
        UnityEngine.iOS.LocalNotification n = new UnityEngine.iOS.LocalNotification();
        
        n.fireDate = System.DateTime.Now.AddSeconds(delay);
        n.alertBody = content;
        n.hasAction = true; //Set that pushing the button will launch the application
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(n);
        #endif

    }

    public void makeRepeatingNotificationAndroidDays(string title, string content, int delay, int amountDays)
    {
        int millisecDelay = delay * 1000;

        #if !UNITY_EDITOR && UNITY_ANDROID
                if (myClass != null)
                    myClass.Call("scheduleRepeatingNotificationDays", 
                                    new object[] { title, content, millisecDelay, amountDays });
        #endif
    }

    public void makeRepeatingNotificationAndroidHours(string title, string content, int delay, int amountHours)
    {
        int millisecDelay = delay * 1000;

        #if !UNITY_EDITOR && UNITY_ANDROID
                if (myClass != null)
                    myClass.Call("scheduleRepeatingNotificationHours", 
                                new object[] { title, content, millisecDelay, amountHours });
        #endif
    }

    #if !UNITY_EDITOR && UNITY_IOS
        
    public void makeRepeatingNotificationiOS(string title, string content, int delay, UnityEngine.iOS.CalendarUnit u)
    {
            UnityEngine.iOS.LocalNotification n = new UnityEngine.iOS.LocalNotification();
        
            n.fireDate = System.DateTime.Now.AddSeconds(delay);
            n.repeatInterval = u;
            n.alertBody = content;
            n.hasAction = true; //Set that pushing the button will launch the application
            UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(n);
        

    }
    #endif
    
    
    public void cancelScheduledNotification()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (myClass != null)
            myClass.Call("cancelScheduledNotification");
        #endif

        #if !UNITY_EDITOR && UNITY_IOS
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
        #endif
    }

    /*
     Clear a notification that is already in the notification bar
     */
    public void cancelExistingNotification()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (myClass != null)
            myClass.Call("clearNotification");
        #endif

        #if !UNITY_EDITOR && UNITY_IOS
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        #endif
    }


    public void makeToast(string to)
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (myClass != null) {
            myClass.Call("makeToast", new object[] { to, 1000 });
        }
        #endif
    }



    public void shareImage(string caption)
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (!isProcessing)
            StartCoroutine(ShareScreenshot(caption));
        #endif


        #if !UNITY_EDITOR && UNITY_IOS
        
        #endif

        
    }


    
    public IEnumerator ShareScreenshot(string capt)
    {
        
        isProcessing = true;

        // wait for graphics to render
        yield return new WaitForEndOfFrame();
        //———————————————————————————————————————————————————————————– PHOTO
        // create the texture
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        // put buffer into texture
        screenTexture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height),0,0);
        // apply
        screenTexture.Apply();
        //———————————————————————————————————————————————————————————– PHOTO

        byte[] dataToSave = screenTexture.EncodeToPNG();
        string destination = Path.Combine(Application.persistentDataPath, 
                                System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png");
        File.WriteAllBytes(destination, dataToSave);

        #if !UNITY_EDITOR && UNITY_ANDROID
        // block to open the file and share it ————START
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse","file://" + destination);

        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), capt);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);

        //intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>(“EXTRA_SUBJECT"), "SUBJECT");
        intentObject.Call<AndroidJavaObject>("setType", "image/*");
        //intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "testo");
        //intentObject.Call<AndroidJavaObject>("text/plain");

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        // option one:
        currentActivity.Call("startActivity", intentObject);

        #endif

        // option two:
        //AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "YO BRO! WANNA SHARE?");
        //currentActivity.Call("startActivity", jChooser);

        isProcessing = false;

    }
    

    
    public IEnumerator ShareScreenshotSoomla(Provider p)
    {
        isProcessing = true;
        // wait for graphics to render
        yield return new WaitForEndOfFrame();
        //———————————————————————————————————————————————————————————– PHOTO
        // create the texture
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        // put buffer into texture
        screenTexture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
        // apply
        screenTexture.Apply();
        //———————————————————————————————————————————————————————————– PHOTO
        string appLink = "";
        
        #if UNITY_ANDROID
            appLink = Manager.instance.ANDROID_APP_URL;
        #endif
        #if UNITY_IOS
            appLink = Manager.instance.IOS_APP_URL;
        #endif

        SoomlaProfile.UploadImage(
        p,                  
        "Bear Boom top score! How high can you get?",
        "BearBoomtopscore.png", 
        screenTexture,
        "",
        null      
        );

        isProcessing = false;
        
    }
    

   

}
