package bluewind.mangasuite;

import java.util.Timer;
import java.util.TimerTask;
import org.apache.cordova.DroidGap;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.view.*;
import android.widget.LinearLayout;

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
