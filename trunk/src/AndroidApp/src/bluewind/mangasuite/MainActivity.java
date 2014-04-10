package bluewind.mangasuite;

import org.apache.cordova.DroidGap;
import android.content.pm.ActivityInfo;
import android.os.Bundle;


public class MainActivity extends DroidGap
{
	protected boolean IsMainAdLast=false;
		
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		super.setIntegerProperty("loadUrlTimeoutValue",600000);
		super.setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
		super.setIntegerProperty("splashscreen",R.drawable.splash);
		super.loadUrl("file:///android_asset/www/index.html",5000);
		
		
	}
	
}
