using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Microsoft.AspNet.SignalR.Client;

namespace SignalR_Drag_Tutorial
{
    [Activity(Label = "SignalR_Drag_Tutorial", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private FrameLayout mRoot;
        private View mDraggableView;
        private IHubProxy mChatHubProxy;
        private float mXInit = 0;
        private float mYInit = 0;

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mRoot = FindViewById<FrameLayout>(Resource.Id.root);
            mDraggableView = FindViewById(Resource.Id.draggableView);

            mDraggableView.Touch += MDraggableView_Touch;

            HubConnection hubConnection = new HubConnection("http://cforbeginners.com:901");
            mChatHubProxy = hubConnection.CreateHubProxy("ChatHub");

            try
            {
                await hubConnection.Start();
            }
            catch (Exception)
            {
                //handle errors
            }

            mChatHubProxy.On<float, float, float, float>("UpdateView", (rawX, initX, rawY, initY) =>
            {
                RunOnUiThread(() =>
                {
                    mDraggableView.Animate()
                            .X(rawX + initX)
                            .Y(rawY + initY)
                            .SetDuration(0)
                            .Start();

                    mRoot.Invalidate();
                });
            });
        }

        private async void MDraggableView_Touch(object sender, View.TouchEventArgs e)
        {
            float x = e.Event.RawX;
            float y = e.Event.RawY;

            View touchedView = sender as View;

            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    mXInit = touchedView.GetX() - x;
                    mYInit = touchedView.GetY() - y;
                    break;

                case MotionEventActions.Move:
                    touchedView.Animate()
                            .X(e.Event.RawX + mXInit)
                            .Y(e.Event.RawY + mYInit)
                            .SetDuration(0)
                            .Start();                            

                    break;

                default:
                    break;
            }

            mRoot.Invalidate();

            await mChatHubProxy.Invoke("DragView", new object[] { e.Event.RawX, mXInit, e.Event.RawY, mYInit });
        }
    }
}

