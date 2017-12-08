using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System;
using Android.Support.V7.App;
using System.Collections.Generic;
using Map_vTest.Droid.Model;
using System.Threading.Tasks;
using Android.Views;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using static Android.Gms.Maps.GoogleMap;

namespace Map_vTest.Droid
{
    [Activity(Label = "Map_vTest", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, IInfoWindowAdapter, IOnInfoWindowClickListener
    {
        private EditText input_name, input_email;
        private ListView list_data;

        private List<Account> list_users = new List<Account>();
        private ListViewAdapter adapter; 
        private Account selectedAccount;

        private const string FirebaseURL = "https://xamarin-74fb6.firebaseio.com/";

        double posi_lat = 13.7298956 ;
        double posi_lng = 100.7771276;

        void IOnMapReadyCallback.OnMapReady(GoogleMap googleMap)
        {
            Button button = FindViewById<Button>(Resource.Id.button1);
            button.Click += delegate
            {
                button.Text = $"{posi_lat},{posi_lng}";
            };

            if (Math.Abs(posi_lat) > 0)
            {
                MarkerOptions markerOptions = new MarkerOptions();
                markerOptions.SetPosition(new LatLng(posi_lat, posi_lng));

                markerOptions.SetTitle("My Position");

                googleMap.AddMarker(markerOptions);
            }

            googleMap.SetInfoWindowAdapter(this);
            googleMap.SetOnInfoWindowClickListener(this);

            //optional
            googleMap.UiSettings.ZoomControlsEnabled = true;
            googleMap.UiSettings.CompassEnabled = true;
            if(Math.Abs(posi_lat) > 0)
            {
                googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(posi_lat, posi_lng), 17.0f));
            }
            else{
                googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(13.7448301, 100.7226308), 12.0f));
            }


        }

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            MapFragment mapFragment = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this );

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "Firebase Demo";
            SetSupportActionBar(toolbar); 

            input_name = FindViewById<EditText>(Resource.Id.name);
            input_email = FindViewById<EditText>(Resource.Id.email);
            list_data = FindViewById<ListView>(Resource.Id.list_data);
            list_data.ItemClick += (s, e) => {
                Account acc = list_users[e.Position];
                selectedAccount = acc;
                input_name.Text = acc.name;
                input_email.Text = acc.email;
            };

            await LoadDataAsync();

        }

        private async Task LoadDataAsync()
        {
            list_data.Visibility = ViewStates.Invisible;

            var firebase = new FirebaseClient(FirebaseURL);
            var items = await firebase
                .Child("users")
                .OnceAsync<Account>();
            list_users.Clear();
            adapter = null;
            foreach (var item in items)
            {
                Account acc = new Account();
                acc.uid = item.Key;
                acc.name = item.Object.name;
                acc.email = item.Object.email;

                list_users.Add(acc);
            }
            adapter = new ListViewAdapter(this, list_users);
            adapter.NotifyDataSetChanged();
            list_data.Adapter = adapter;
            list_data.Visibility = ViewStates.Visible;

        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.menu_add)
            {
                CreateUser();
            }
            else if (id == Resource.Id.menu_save) // Update
            {
                UpdateUser(selectedAccount.uid, input_name.Text, input_email.Text);
            }
            else if (id == Resource.Id.menu_remove)
            {
                DeleteUser(selectedAccount.uid);
            }
            return base.OnOptionsItemSelected(item);
        }

        private async void DeleteUser(string uid)
        {
            var firebase = new FirebaseClient(FirebaseURL);
            await firebase.Child("users").Child(uid).DeleteAsync();
            await LoadDataAsync();
        }

        private async void UpdateUser(string uid, string name, string email)
        {
            var firebase = new FirebaseClient(FirebaseURL);
            await firebase.Child("users").Child(uid).Child("name").PutAsync(name);
            await firebase.Child("users").Child(uid).Child("email").PutAsync(email);

            await LoadDataAsync();
        }

        private async void CreateUser()
        {
            Account user = new Account();
            user.uid = String.Empty;
            user.name = input_name.Text;
            user.email = input_email.Text;

            var firebase = new FirebaseClient(FirebaseURL);

            //Add item
            var item = await firebase.Child("users").PostAsync<Account>(user);

            await LoadDataAsync();
        }

        public View GetInfoContents(Marker marker)
        {
            return null;
        }

        public View GetInfoWindow(Marker marker)
        {
            View view = LayoutInflater.Inflate(Resource.Layout.info_window, null, true);
            view.FindViewById<TextView>(Resource.Id.txtName).Text = "Xamarin";
            view.FindViewById<TextView>(Resource.Id.txtAddress).Text = "Ladkrabang";
            view.FindViewById<TextView>(Resource.Id.txtHours).Text = "8.00 AM";


            return view;
        }

        public void OnInfoWindowClick(Marker marker)
        {
            Console.WriteLine("Info Window has been Click!");
        }
    }
}


