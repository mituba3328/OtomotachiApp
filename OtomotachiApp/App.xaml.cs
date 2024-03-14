using Plugin.LocalNotification;

namespace OtomotachiApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
        protected override Window CreateWindow(IActivationState activationState)//オーバーライドしているので、そのメソッドを呼び出す。
        {
            Window window = base.CreateWindow(activationState);//Windowクラスのインスタンスを作成する
            window.Stopped += (s, e) =>   //←window.ライフサイクルイベント名(今回はStopped)でイベントハンドラーを作成
            {
                //通知の処理
                var request = new NotificationRequest //ライブラリのNotificationRequestクラスを作成
                {
                    NotificationId = 1337,//通知のID(なんでもよい)
                    Title = "寝てますよ",//(通知のタイトル)
                    Subtitle = "あなたはStoppedによって寝ていました",//(通知のサブタイトル)
                    Description = "It's Me",//(説明など)
                    BadgeNumber = 0,//(通知バッジのナンバー)
                                    //通知のスケジュールを設定する
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(5),//イベントが起こった5秒あとに通知をする処理
                    }
                };
                LocalNotificationCenter.Current.Show(request);//通知センターに通知を表示するメソッド
            };
            return window;//windowを返すようにする
        }
    }
}
