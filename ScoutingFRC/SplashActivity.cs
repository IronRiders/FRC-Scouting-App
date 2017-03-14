using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using ScoutingFRC;

[Activity(Theme = "@style/CustomTheme.Splash", MainLauncher = true, NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
public class SplashActivity : Activity
{

    public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
    {
        base.OnCreate(savedInstanceState, persistentState);
     
    }

    protected override void OnResume()
    {
        base.OnResume();
        StartActivity(new Intent(Application.Context, typeof(MainActivity)));
    }

}