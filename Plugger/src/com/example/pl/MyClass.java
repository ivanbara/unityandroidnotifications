package com.example.pl;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.GregorianCalendar;

import android.annotation.SuppressLint;
import android.annotation.TargetApi;
import android.app.Activity;
import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.TaskStackBuilder;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.os.Build;
import android.os.SystemClock;
import android.support.v4.app.NotificationCompat;
import android.widget.Toast;

public class MyClass {
	private Class<?> unityPlayerClass;
    private Field unityCurrentActivity;
    private Method unitySendMessage;
   

    public void init () {

        //Get the UnityPlayer class using Reflection
        try {
			unityPlayerClass = Class.forName("com.unity3d.player.UnityPlayer");
		} catch (ClassNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        //Get the currentActivity field
        try {
			unityCurrentActivity= unityPlayerClass.getField("currentActivity");
		} catch (NoSuchFieldException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        //Get the UnitySendMessage method
        try {
			unitySendMessage = unityPlayerClass.getMethod("UnitySendMessage", new Class [] { String.class, String.class, String.class } );
		} catch (NoSuchMethodException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        
        
        
    }

    //Use this method to get UnityPlayer.currentActivity
    public Activity currentActivity () {

        Activity activity = null;
		try {
			activity = (Activity) unityCurrentActivity.get(unityPlayerClass);
		} catch (IllegalAccessException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IllegalArgumentException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        return activity;            
    }


    public void unitySendMessageInvoke (String gameObject, String methodName, String param) {
        //Invoke the UnitySendMessage Method
        try {
			unitySendMessage.invoke(null, new Object[] { gameObject, methodName, param} );
		} catch (IllegalAccessException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IllegalArgumentException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (InvocationTargetException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    }

    public void makeToast (final String toastText, final int duration) {
        currentActivity().runOnUiThread(new Runnable() {
            public void run() {
                Toast.makeText(currentActivity(), toastText, duration).show();
            }
        });
    }
    
    public void makeNotification(String title, String content){
		
		Notification n = this.getNotification(title, content);
		n.flags |= Notification.FLAG_AUTO_CANCEL;
		
		NotificationManager notificationManager = (NotificationManager) currentActivity().getSystemService(Context.NOTIFICATION_SERVICE);
		notificationManager.notify(0, n);
	}
    
    
    public void createScheduledNotification(String title, String content, int delay){
    	Notification notification = this.getNotification(title, content);
		notification.flags |= Notification.FLAG_AUTO_CANCEL;
		
		// 1sec = 1000
		this.scheduleNotification(notification, delay);
    }
    
    
    public void cancelScheduledNotification(){
    	Intent notificationIntent = new Intent(currentActivity(), NotificationPublisher.class);
    	PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity(), 0, notificationIntent, 
        		PendingIntent.FLAG_UPDATE_CURRENT);
    	AlarmManager alarmManager = (AlarmManager)currentActivity().getSystemService(Context.ALARM_SERVICE);
    	alarmManager.cancel(pendingIntent);
    }
    
    
    // delay, 1sec = 1000
    private void scheduleNotification(Notification notification, int delay) {
        Intent notificationIntent = new Intent(currentActivity(), NotificationPublisher.class);
        notificationIntent.putExtra(NotificationPublisher.NOTIFICATION_ID, 1);
        notificationIntent.putExtra(NotificationPublisher.NOTIFICATION, notification);
        
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity(), 0, notificationIntent, 
        		PendingIntent.FLAG_UPDATE_CURRENT);
        
        long futureInMillis = new GregorianCalendar().getTimeInMillis() + delay;
        AlarmManager alarmManager = (AlarmManager)currentActivity().getSystemService(Context.ALARM_SERVICE);
        alarmManager.set(AlarmManager.RTC_WAKEUP, futureInMillis, pendingIntent);
    }
    
    
    private void scheduleRepeatingNotificationDays(String title, String content, int delay, int amount){
    	Notification notification = this.getNotification(title, content);
		notification.flags |= Notification.FLAG_AUTO_CANCEL;
		
		// 1sec = 1000
		Intent notificationIntent = new Intent(currentActivity(), NotificationPublisher.class);
        notificationIntent.putExtra(NotificationPublisher.NOTIFICATION_ID, 1);
        notificationIntent.putExtra(NotificationPublisher.NOTIFICATION, notification);
        
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity(), 0, notificationIntent, 
        		PendingIntent.FLAG_UPDATE_CURRENT);
        
        GregorianCalendar cal = new GregorianCalendar();
        long futureInMillis = cal.getTimeInMillis() + delay;
        AlarmManager alarmManager = (AlarmManager)currentActivity().getSystemService(Context.ALARM_SERVICE);
        alarmManager.setRepeating(AlarmManager.RTC_WAKEUP, futureInMillis, AlarmManager.INTERVAL_DAY*amount, pendingIntent);
    }
    
    private void scheduleRepeatingNotificationHours(String title, String content, int delay, int amount){
    	Notification notification = this.getNotification(title, content);
		notification.flags |= Notification.FLAG_AUTO_CANCEL;
		
		// 1sec = 1000
		Intent notificationIntent = new Intent(currentActivity(), NotificationPublisher.class);
        notificationIntent.putExtra(NotificationPublisher.NOTIFICATION_ID, 1);
        notificationIntent.putExtra(NotificationPublisher.NOTIFICATION, notification);
        
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity(), 0, notificationIntent, 
        		PendingIntent.FLAG_UPDATE_CURRENT);
        
        GregorianCalendar cal = new GregorianCalendar();
        long futureInMillis = cal.getTimeInMillis() + delay;
        AlarmManager alarmManager = (AlarmManager)currentActivity().getSystemService(Context.ALARM_SERVICE);
        alarmManager.setRepeating(AlarmManager.RTC_WAKEUP, futureInMillis, AlarmManager.INTERVAL_HOUR*amount, pendingIntent);
    }
    
    
	private Notification getNotification(String title, String content) {
		Resources res = currentActivity().getResources();
		int icon = res.getIdentifier("app_icon", "drawable", currentActivity().getPackageName());
		
        NotificationCompat.Builder builder = new NotificationCompat.Builder(currentActivity());
        builder.setSmallIcon(icon);
        builder.setContentTitle(title);
        builder.setContentText(content);
        
        Intent notificationIntent = new Intent(currentActivity(), currentActivity().getClass());
        PendingIntent intent = PendingIntent.getActivity(currentActivity(), 0, notificationIntent, 0);
        builder.setContentIntent(intent);
        
        /*
         builder.setContentIntent(pending_intent)
        .setSmallIcon(R.drawable.ic_launcher)
        .setLargeIcon(BitmapFactory.decodeResource(res, R.drawable.ic_launcher))
        .setTicker("test")
        .setWhen(System.currentTimeMillis())
        .setAutoCancel(false)
        .setContentTitle("title")
        .setContentInfo("cinfo")
        .setContentText("ctext");
        */
        
        return builder.build();
    }
    
	
	// Clear existing shown notification
	public void clearNotification(){
		NotificationManager nm = (NotificationManager)currentActivity().getSystemService(Context.NOTIFICATION_SERVICE);
		nm.cancelAll();
	}
	
	
	
	
	
}
