using DiffPlex.DiffBuilder.Model;
using DiffPlex.DiffBuilder;
using DiffPlex;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiffPlex.Chunkers;
using static AloeSoapSample.MainBody;

namespace AloeSoapSample
{
    /// <summary>
    /// MainBody.xaml の相互作用ロジック
    /// </summary>
    public partial class MainBody : UserControl
    {
        public MainBody()
        {
            this.InitializeComponent();
        }

        private void ClearSoapButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# SOAPをクリアしました。";

            this.SubjectiveTextBox.Clear();
            this.ObjectiveTextBox.Clear();
            this.AssessmentTextBox.Clear();
            this.PlanTextBox.Clear();
        }

        private void SetSoapSample1Button_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# SOAPサンプル1を表示します。";

            this.SubjectiveTextBox.Text =
                """
                患者は50歳男性。
                主訴は約1週間前から持続する胸やけ感と食後の胃痛。「特に夕食後がひどく、夜間就寝時に胃酸が上がってくるような感じで眠りにくい」と訴える。時折喉の奥まで酸っぱい感じがあり、不快感が強いとのこと。
                既往歴に高血圧（現在降圧剤服用中）があり、1か月前の健診で胃部異常なし。ストレスが多く仕事が忙しいと述べる。
                """;
            this.ObjectiveTextBox.Text =
                """
                バイタルサインは安定。体温 36.7℃、血圧 132/78 mmHg、脈拍 72回/分、呼吸数16回/分。
                腹部診察において心窩部（みぞおち）に軽度の圧痛あり。腸蠕動音は正常。
                胸部および心臓・肺に異常所見なし。口腔内診察で咽頭後壁に軽度の発赤を認めるが扁桃腫大なし。
                """;
            this.AssessmentTextBox.Text =
                """
                患者の訴える症状および所見より、逆流性食道炎（GERD）の疑いが強い。
                食後および就寝時に症状が増悪している点、胃酸逆流による喉の刺激症状（酸っぱい感じ）が特徴的。
                咽頭の軽度発赤は胃酸による二次的な刺激性炎症と推測される。ストレスの増加も病態を助長している可能性がある。
                """;
            this.PlanTextBox.Text =
                """
                ① プロトンポンプ阻害薬（PPI）を1日1回、朝食後に2週間処方。症状の経過を観察する。
                ② 食事指導として、夜間の飲食を控えるよう助言。脂肪分や刺激物の多い食事を避けるよう説明。
                ③ 就寝時は頭を高くする（枕を高くする）工夫を指導。
                ④ ストレス対策として適度な休息を促し、症状改善がみられない場合は上部消化管内視鏡検査を考慮。
                """;
        }

        private void ClearTalkButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# TALKをクリアしました。";

            this.RecognizedTextBox.Clear();
        }

        private SoapMessage? _soapMessage;

        private void ClearDiffButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# 差分をクリアしました。";

            this._soapMessage = null;
            this.SubjectiveRichTextBox.Document.Blocks.Clear();
            this.ObjectiveRichTextBox.Document.Blocks.Clear();
            this.AssessmentRichTextBox.Document.Blocks.Clear();
            this.PlanRichTextBox.Document.Blocks.Clear();
        }

        private void AcceptDiffButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# SOAPを確定しました。";

            this.SubjectiveTextBox.Text = this._soapMessage?.Subjective ?? "";
            this.ObjectiveTextBox.Text = this._soapMessage?.Objective ?? "";
            this.AssessmentTextBox.Text = this._soapMessage?.Assessment ?? "";
            this.PlanTextBox.Text = this._soapMessage?.Plan ?? "";

            this._soapMessage = null;
            this.SubjectiveRichTextBox.Document.Blocks.Clear();
            this.ObjectiveRichTextBox.Document.Blocks.Clear();
            this.AssessmentRichTextBox.Document.Blocks.Clear();
            this.PlanRichTextBox.Document.Blocks.Clear();
        }

        private void SetTalkSample1Button_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# TALKサンプル1を表示します。";

            this.RecognizedTextBox.Text =
                """
                [Guest-1] こんばんは。
                [Guest-2] こんばんは。お仕事帰りですか？

                [Guest-1] はいち。
                [Guest-1] ょっとだけ。早く。上がれたので。
                [Guest-2] そうなんですね。では今日はどうされましたか？

                [Guest-1] あの。最近ちょっと胃の調子が悪くて。食後に無感化する感じがあるのと、特に夜、寝る前とかに気持ち悪くなるんです。
                [Guest-2] いつ頃からですか？

                [Guest-1] 先週の多分火曜？ 月曜だったかな。
                [Guest-1] はっきり覚えてないんですけど、そのあたりからです。
                [Guest-2] 横になったときに症状が強くなる感じですか？

                [Guest-1] はい。寝ようとすると、喉の辺りが下がってもちょっと気持ち悪いというか、なんか戻ってくるような感じがあります。
                [Guest-2] 他にはリップや胸は？ 胸やけはありますか？

                [Guest-1] でっぱが出るんですよね。日によってムカムカが強くなったり、胸焼きがあったり。昨日は少しきつかったです。
                [Guest-2] わかりました。ストレスや睡眠不足はどうですか？

                [Guest-1] 仕事が忙しくて、夜遅くに食事をすることも多いんで、寝る直前までバタバタしてます。
                [Guest-2] なるほど。では、谷を測りましょう。これを右の脇に挟んでください。

                [Guest-1] はい。
                [Unknown]
                [Guest-2] 36.6度、低熱ですね。次は血圧を測ります。腕をこちらへ。

                [Guest-1] はい。
                [Guest-1] すぅ。
                [Guest-2] 寡婦を巻きますね。少し圧迫されますので。

                [Guest-1] ふぅ。
                [Unknown]
                [Guest-1] うわっ。
                [Guest-2] あ、ちょっと緩かったかもしれませんね。
                [Guest-1] ボンっていきましたけど、大丈夫ですか？
                [Guest-2] 大丈夫です。たまにカフが外れることがあるんですよ。もう一度巻き直しますね。

                [Guest-1] はい、お願いします。
                [Unknown]

                [Guest-2] 142の88。ちょっと高めですが、普段の血圧はどのくらいでしょう？
                [Guest-1] 普段はもう少し低いんですけど、病院だと緊張するみたいで…。

                [Guest-2] じゃあもう一度深呼吸してみましょう。鼻から吸って、口から吐いて。
                [Guest-1] はい。
                [Guest-1] すぅ。

                [Guest-2] 132の78。下がりましたね。これなら普段通りかもしれません。
                [Guest-1] ありがとうございます。少し安心しました。

                [Guest-2] では次、脈を測りますね。腕を出してください。
                [Guest-1] はい。
                [Guest-2] 70、70？
                [Guest-2] 二ですね。脈は問題ありません。
                [Guest-1] よかったです。

                [Guest-2] 呼吸を飲みますので、普段通り呼吸をしてください。
                [Guest-1] IT失敗？

                [Guest-2] はい、落ち着いてますね。ではお腹を見ます。後ろのベッドへ行きましょう。靴を脱いで頭をこちら側にして横になってください。
                [Guest-1] わかりました。

                [Unknown]

                [Guest-2] シャツをまくっていただいて、軽く触っていきます。痛いところや気持ち悪いところがあれば教えてください。
                [Guest-1] あそこ。ちょっと重たい感じがします。

                [Guest-2] 輸送機のあたりですね。昨日もここが気になりましたか？
                [Guest-1] 昨日はそこまででもなかったんですけど、一昨日は結構無感化が強かったです。

                [Guest-2] なるほど。では起き上がって大丈夫ですよ。
                [Guest-1] はい。

                [Guest-2] 責任と逆流性食道炎の可能性が高いですね。潰瘍のような深い所見は見当たりません。
                [Guest-1] じゃあ海洋ってわけではないんですか？

                [Guest-2] 潰瘍ほどではないと思われます。胃酸の逆流が原因でしょう。
                [Guest-1] やっぱりそうですか。

                [Guest-2] 遺産を去る釣りを出しておきますので、朝食後に一日一回服用してください。
                [Guest-1] はい。飲み方はわかりました。

                [Guest-2] あと、コーヒーや辛いもの、アルコールなどはなるべく控えてください。寝る直前の食事も避けましょう。
                [Guest-1] コーヒーは正直毎日飲んでるんですが、少し減らしてみます。

                [Guest-2] 無理のない範囲で大丈夫です。一～二週間様子を見て、改善しなければ検査を考えましょう。
                [Guest-1] そうですね。胃カメラとか苦手ですけど、治らないなら仕方ないですね。

                [Guest-2] 血圧の薬との併用は問題ないですが、時間帯を少しずらすと吸収が安定する場合もあります。
                [Guest-1] わかりました。そうしてみます。

                [Guest-2] 何か異変があれば早めに来院してください。今日はこれで終わりです。
                [Guest-1] ありがとうございます。助かりました。

                [Guest-2] お大事に。
                [Guest-1] はい。失礼します。

                [Unknown]
                """;
        }

        private void SetTalkSample2Button_OnClick(object sender, RoutedEventArgs e)
        {
            this.RecognizingTextBox.Text = "# TALKサンプル2を表示します。";

            this.RecognizedTextBox.Text =
                """
                [Unknown]
                [Unknown]
                [Guest-1] こんにちは、予約していた田中です。
                [Guest-2] こんにちは、田中さんですね。今日はどうされましたか？
                [Guest-1] ここ数日、腕と首元に赤い発疹が出てきて、かゆみがひどいんです。
                [Guest-2] かゆみが特に強いのはいつ頃ですか？
                [Guest-1] 夜です。寝ているときに目が覚めるほどかゆいです。
                [Guest-2] それはつらいですね。ちなみに、発疹はどんな感じで広がっていますか？
                [Guest-1] 最初は腕の内側だけだったんですけど、2日前くらいから首のあたりにも小さなブツブツができ始めました。
                [Guest-2] なるほど。他に熱や痛みはありませんか？
                [Guest-1] 熱はありません。痛みというよりは、とにかくかゆくて、掻いた部分がヒリヒリする感じです。
                [Guest-2] わかりました。普段、何かアレルギーはお持ちですか？
                [Guest-1] 花粉症くらいですね。あとは特にないと思います。
                [Guest-2] 食事や薬、化粧品など、新しく使い始めたものはありますか？
                [Guest-1] そういえば、ここ最近ボディーソープを変えました。あと、柔軟剤も新しいものにしてみたんです。
                [Guest-2] そうですか。そのあたりが原因の可能性も否定はできませんね。
                [Guest-1] そうなんですね。急にかぶれたりするものなんでしょうか？
                [Guest-2] 体質や肌のコンディションによっては、ある日突然合わなくなることもありますよ。
                [Guest-1] なるほど…。思い当たるとすれば、そのボディーソープと柔軟剤くらいかもしれません。
                [Guest-2] では、少し肌を診せてもらっていいですか？
                [Guest-1] はい、お願いします。
                [Guest-2] 失礼します。腕の内側は赤みが強いですね。小さな水泡のようにも見えます。
                [Guest-1] ここ数日で急に悪化した感じがします。
                [Guest-2] 首も見せていただけますか？
                [Guest-1] どうぞ。
                [Guest-2] こちらも同じように小さな発疹が散らばってますね。
                [Guest-1] ああ、やっぱりそうですか。見た目も赤くてちょっと恥ずかしいです。
                [Guest-2] 掻き崩すと色素沈着や傷が残る可能性があるので、注意してくださいね。
                [Guest-1] はい、なるべく我慢してるんですが、夜はどうしても掻いてしまいます…。
                [Guest-2] それなら、かゆみ止めの内服薬と塗り薬を処方しておきましょう。
                [Guest-1] 助かります。ちゃんと治るか心配で…。
                [Guest-2] まずは薬で症状を抑えつつ、原因となっている可能性のある製品は使用を控えてみてください。
                [Guest-1] わかりました。ボディーソープと柔軟剤を前のものに戻して様子を見ますね。
                [Guest-2] それがいいと思います。あと、入浴時はあまり熱いお湯だと肌が乾燥しやすいので、ぬるめのお湯にしましょう。
                [Guest-1] はい、いつも少し熱めにしてたかもしれません。
                [Guest-2] 保湿も大事ですので、お風呂上がりに保湿クリームを薄く塗ってください。
                [Guest-1] 保湿クリーム、家にあります。塗り方にコツはありますか？
                [Guest-2] 体を拭いた後、なるべく早めに塗ると効果的ですよ。
                [Guest-1] わかりました、ありがとうございます。
                [Guest-2] 今回のかゆみは外的要因の可能性が高いとは思いますが、念のため血液検査でアレルギー反応もチェックしておきましょうか？
                [Guest-1] そうですね、検査してもらえたら安心です。
                [Guest-2] では血液検査をしますので、看護師を呼びますね。
                [Unknown]
                [Unknown]
                [Unknown]
                [Unknown]
                [Unknown]
                [Unknown]
                [Guest-3] はい、失礼します。血液検査の準備をします。
                [Guest-1] お願いします。
                [Guest-3] では、こちらの椅子に座って、腕を出してください。
                [Guest-1] はい。
                [Guest-3] ちょっとゴムで止めますね。チクッとしますよ。
                [Guest-1] うっ…ちょっと痛いですね。
                [Guest-3] 大丈夫ですか？すぐ終わりますよ…はい、終わりました。
                [Guest-1] ありがとうございます。
                [Guest-3] この検体を検査に回しておきます。結果が出るまでしばらくお待ちください。
                [Guest-2] 結果が出るのはおそらく数日後ですね。その間は先ほどお伝えした生活上の注意を守りつつ、薬を使ってみてください。
                [Guest-1] わかりました。かゆみがひどいときは掻かずに冷やした方がいいんでしょうか？
                [Guest-2] そうですね。冷やすのも有効ですし、塗り薬をこまめに使うのもいいと思います。
                [Guest-1] 夜中に痒くて起きるのが本当にしんどいんですよね…。
                [Guest-2] もし夜のかゆみが特に強いようなら、寝る前にかゆみ止めの薬を追加で飲んでも大丈夫です。
                [Guest-1] わかりました。そうします。
                [Guest-2] ただし、飲みすぎはよくないので用法用量は守ってくださいね。
                [Guest-1] はい、気をつけます。
                [Guest-2] 他に気になる症状やお困りのことはありますか？
                [Guest-1] うーん、今のところは腕と首の発疹だけですね。
                [Guest-2] では、そこが落ち着いてくれば問題ないかと思います。万が一、発疹が増えたり熱が出たりしたらすぐに来院してください。
                [Guest-1] ありがとうございます。助かります。
                [Guest-2] お仕事などでストレスは溜まっていませんか？ ストレスも肌荒れの原因になることがありますので。
                [Guest-1] 多少はありますが、普段通りです。特に今、忙しい時期というわけでもないので…。
                [Guest-2] そうですか。睡眠と食事をしっかり取ることも肌トラブルには重要です。
                [Guest-1] そうですよね。最近あまり寝れてなかったので、気をつけます。
                [Guest-2] 十分休養を取ってくださいね。あと、シャワーで済ませるのではなく、湯船につかると血行が良くなるのでおすすめです。
                [Guest-1] そうですね、なるべくゆっくりお風呂に入るようにします。
                [Guest-2] では、血液検査の結果が出たらまたこちらからご連絡しますので、その結果をもとにさらに詳しい治療を検討しましょう。
                [Guest-1] はい、ありがとうございます。
                [Guest-2] それまでに何か変化があったら、電話でもいいのでご相談くださいね。
                [Guest-1] わかりました。
                [Guest-2] では、今日のところはこれで大丈夫です。薬の処方箋を受付でお渡しします。
                [Guest-1] ありがとうございます。早めに治ることを願ってます。
                [Guest-2] しっかりケアすれば、きっと良くなると思いますよ。
                [Guest-1] そう言っていただけると安心します。
                [Guest-2] それでは、お大事になさってください。
                [Guest-1] 失礼します。お世話になりました。
                [Guest-3] 受付から失礼します。こちら、処方箋と次回検査結果のお知らせカードになります。
                [Guest-1] ありがとうございます。次回はいつ頃来ればいいですか？
                [Guest-3] 検査結果が出るまで3〜4日かかりますので、その頃を目安にご連絡しますね。
                [Guest-1] わかりました。
                [Guest-3] もし薬がなくなったり、症状が変わったりしたらいつでもお電話ください。
                [Guest-1] はい、ありがとうございます。
                [Guest-3] お大事になさってください。
                [Guest-2] そういえば、一点補足です。シャワーのあとは肌を擦りすぎないように気をつけてくださいね。
                [Guest-1] わかりました。タオルでゴシゴシやらない方がいいんですよね。
                [Guest-2] そうです。強く擦ると余計に刺激になるので、押さえるように水分を拭き取ってください。
                [Guest-1] 了解しました。気をつけます。
                [Guest-2] あと、塗り薬は朝起きたときと寝る前、なるべく1日2～3回は塗ってみてください。
                [Guest-1] はい、わかりました。
                [Guest-2] 塗り忘れが多いと症状がなかなか良くならないので、タイミングを決めておくといいですよ。
                [Guest-1] そうですね。スマートフォンのアラームをセットしておきます。
                [Guest-2] いいアイデアですね。では、気をつけてお帰りください。
                [Guest-1] ありがとうございます。失礼します。
                [Guest-3] 会計は隣の窓口になりますので、そちらでお願いいたします。
                [Guest-1] はい。わかりました。お世話になりました。
                [Guest-3] お大事にどうぞ。
                [Guest-2] もし何かあれば遠慮なくご連絡くださいね。
                [Guest-1] はい、ありがとうございます。
                [Guest-1] それでは失礼します。
                [Unknown]
                [Unknown]
                [Unknown]
                """;

        }

        private async void StartRecognitionButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RecognizingTextBox.Text = "# 音声認識を開始しました。";

                //await this.StartSpeechRecognizer();
                await this.StartConversationTranscriber();

                this.StartRecognitionButton.Content = "認識中";
                this.StartRecognitionButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                this.UpdateRecognizedText("Error: " + ex.Message);
            }
        }

        #region SpeechRecognizer

        private SpeechRecognizer? _recognizer;

        private async Task StartSpeechRecognizer()
        {
            // 既存のrecognizerを停止して破棄
            if (this._recognizer != null)
            {
                await this._recognizer.StopContinuousRecognitionAsync();
                this._recognizer.Recognizing -= this.Recognizer_Recognizing;
                this._recognizer.Recognized -= this.Recognizer_Recognized;
                this._recognizer.Dispose();
                this._recognizer = null;
            }

            this._recognizer = this.CreateSpeechRecognizer();

            // 音量レベルをイベントから取得
            this._recognizer.Recognizing += this.Recognizer_Recognizing;
            this._recognizer.Recognized += this.Recognizer_Recognized;

            await this._recognizer.StartContinuousRecognitionAsync();
        }

        private SpeechRecognizer CreateSpeechRecognizer()
        {
            var config = App.Host.Services.GetRequiredService<IConfiguration>();

            var subscriptionKey = config["AzureSpeech:SubscriptionKey"];
            var region = config["AzureSpeech:Region"];
            var lang = config["AzureSpeech:Lang"];

            var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
            speechConfig.SpeechRecognitionLanguage = lang;
            speechConfig.SetProperty("diarization.mode", "True");

            // オーディオ入力の設定（マイク）
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            return new SpeechRecognizer(speechConfig, audioConfig);
        }

        private void Recognizer_Recognizing(object? sender, SpeechRecognitionEventArgs e)
        {
            // 音声入力中イベント（簡易的に音量の代わりにTextの長さを利用）
            this.UpdateAudioLevel(e.Result.Text.Length % 100); // 疑似的に変化を出す
            this.UpdateRecognizingText(e.Result.Text);
        }

        private void Recognizer_Recognized(object? sender, SpeechRecognitionEventArgs e)
        {
            // 確定した音声認識結果
            this.UpdateAudioLevel(0); // 音声入力終了で0に戻す
            this.UpdateRecognizedText(e.Result.Text);
        }

        private void UpdateAudioLevel(int level)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.AudioLevelBar.Value = level;
            });
        }

        private void UpdateRecognizingText(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine(text);
                this.RecognizingTextBox.Text = text;
            });
        }

        private void UpdateRecognizedText(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine(text);
                this.RecognizedTextBox.AppendText(text);
                this.RecognizedTextBox.AppendText(Environment.NewLine);
                this.RecognizedTextBox.ScrollToEnd();
            });
        }

        #endregion SpeechRecognizer

        #region ConversationTranscriber

        private ConversationTranscriber? _transcriber;

        private async Task StartConversationTranscriber()
        {
            // 既存のrecognizerを停止して破棄
            if (this._transcriber != null)
            {
                await this._transcriber.StopTranscribingAsync();
                this._transcriber.Transcribing -= this.Transcriber_Transcribing;
                this._transcriber.Transcribed -= this.Transcriber_Transcribed;
                this._transcriber.Dispose();
                this._transcriber = null;
            }

            this._transcriber = this.CreateConversationTranscriber();

            // 音量レベルをイベントから取得
            this._transcriber.Transcribing += this.Transcriber_Transcribing;
            this._transcriber.Transcribed += this.Transcriber_Transcribed;

            await this._transcriber.StartTranscribingAsync();
        }

        private ConversationTranscriber CreateConversationTranscriber()
        {
            var config = App.Host.Services.GetRequiredService<IConfiguration>();

            var subscriptionKey = config["AzureSpeech:SubscriptionKey"];
            var region = config["AzureSpeech:Region"];
            var lang = config["AzureSpeech:Lang"];

            var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
            speechConfig.SpeechRecognitionLanguage = lang;
            speechConfig.SetProperty("diarization.mode", "True");

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            return new ConversationTranscriber(speechConfig, audioConfig);
        }

        private void Transcriber_Transcribing(object? sender, ConversationTranscriptionEventArgs e)
        {
            this.UpdateAudioLevel(e.Result.Text.Length % 100);
            this.UpdateRecognizingText($"[{e.Result.SpeakerId}] {e.Result.Text}");
        }

        private void Transcriber_Transcribed(object? sender, ConversationTranscriptionEventArgs e)
        {
            this.UpdateAudioLevel(0);
            this.UpdateRecognizedText($"[{e.Result.SpeakerId}] {e.Result.Text}");
        }

        #endregion ConversationTranscriber

        private Kernel? _kernel;

        private record struct SoapMessage(
            string Subjective,
            string Objective,
            string Assessment,
            string Plan);

        public enum SoapType
        {
            Subjective,
            Objective,
            Assessment,
            Plan,
        }

        private async void BuildSoapButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RecognizingTextBox.Text = "# TALKの内容をSOAPに反映しています……";

                this.BuildSoapButton.Content = "解析中";
                this.BuildSoapButton.IsEnabled = false;
                this.AudioLevelBar.IsIndeterminate = true;

                var text = this.RecognizedTextBox.Text;
                // TODO: 前の履歴を何処かに残しておきたい
                this.RecognizedTextBox.Clear();

                var soap = new SoapMessage()
                {
                    Subjective = this.SubjectiveTextBox.Text.Trim(),
                    Objective = this.ObjectiveTextBox.Text.Trim(),
                    Assessment = this.AssessmentTextBox.Text.Trim(),
                    Plan = this.PlanTextBox.Text.Trim(),
                };

                var newSoap = await this.StartSemanticKernel(soap, text);

                this.SetSoapAll(newSoap);
            }
            catch (Exception ex)
            {
                this.UpdateRecognizedText("Error: " + ex.Message);
            }
            finally
            {
                this.RecognizingTextBox.Text = "# TALKの内容をSOAPに反映しました。";

                this.BuildSoapButton.Content = "反映";
                this.BuildSoapButton.IsEnabled = true;
                this.AudioLevelBar.IsIndeterminate = false;
            }
        }

        private async Task<SoapMessage> StartSemanticKernel(SoapMessage soap,string input)
        {
            //this._kernel ??= this.CreateKernelWithLlStudio();
            this._kernel ??= this.CreateKernelWithOpenAi();

            var responseFormat = OpenAI.Chat.ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "soap_result",
                jsonSchema: BinaryData.FromString(
                    """
                    {
                        "type": "object",
                        "properties": {
                            "Subjective": { "type": "string" },
                            "Objective": { "type": "string" },
                            "Assessment": { "type": "string" },
                            "Plan": { "type": "string" }
                        },
                        "required": ["Subjective", "Objective", "Assessment", "Plan"],
                        "additionalProperties": false
                    }
                    """),
                jsonSchemaIsStrict: true);

            // Specify response format by setting ChatResponseFormat object in prompt execution settings.
#pragma warning disable SKEXP0010
            var settings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = responseFormat,
            };
#pragma warning restore SKEXP0010

            var json = JsonSerializer.Serialize(soap, new JsonSerializerOptions { WriteIndented = true });

            // メッセージ全体を生文字列リテラルで組み立てる
            var message =
                $"""
                ステップバイステップで考えて、電カルのSOAPメッセージを変更します。
                既存のSOAPメッセージがある場合は、不必要に変更しないよう注意します。(重要)
                特に入力の内容が少ない場合や指示が書かれている場合は、関係ない箇所のもとのメッセージは1文字も変えてはいけません。
                入力内容は音声認識を行ったものなので、誤認識がある場合は正しく修正します。
                人間が読みやすいよう適度に改行します。
                電カルのSOAPとしてそのまま使えるクオリティを目指し、間違いがないように注意します。

                # 今回入力があった文字列
                {input}

                # 既存のSOAPメッセージ
                {json}
                """;

            // Send a request and pass prompt execution settings with desired response format.
            var result = await this._kernel.InvokePromptAsync(message, new(settings));
            Console.WriteLine(result);

            var newSoap = JsonSerializer.Deserialize<SoapMessage>(result.ToString());

            return newSoap;
        }

        private void SetSoapAll(SoapMessage newSoap)
        {
            if (String.IsNullOrWhiteSpace(
                    this.SubjectiveTextBox.Text
                    + this.ObjectiveTextBox.Text
                    + this.AssessmentTextBox.Text
                    + this.PlanTextBox.Text))
            {
                this.SubjectiveTextBox.Text = newSoap.Subjective;
                this.ObjectiveTextBox.Text = newSoap.Objective;
                this.AssessmentTextBox.Text = newSoap.Assessment;
                this.PlanTextBox.Text = newSoap.Plan;
            }
            else
            {
                this._soapMessage = newSoap;

                this.SetDiff_old(
                    ref this.SubjectiveRichTextBox,
                    this.SubjectiveTextBox.Text,
                    newSoap.Subjective);

                this.SetDiff_old(
                    ref this.ObjectiveRichTextBox,
                    this.ObjectiveTextBox.Text,
                    newSoap.Objective);

                this.SetDiff_old(
                    ref this.AssessmentRichTextBox,
                    this.AssessmentTextBox.Text,
                    newSoap.Assessment);

                this.SetDiff_old(
                    ref this.PlanRichTextBox,
                    this.PlanTextBox.Text,
                    newSoap.Plan);

                //DiffRenderer.SetDiff(
                //    SoapType.Subjective,
                //    ref this.SubjectiveRichTextBox,
                //    this.SubjectiveTextBox.Text,
                //    newSoap.Subjective);

                //DiffRenderer.SetDiff(
                //    SoapType.Objective,
                //    ref this.ObjectiveRichTextBox,
                //    this.ObjectiveTextBox.Text,
                //    newSoap.Objective);

                //DiffRenderer.SetDiff(
                //    SoapType.Assessment,
                //    ref this.AssessmentRichTextBox,
                //    this.AssessmentTextBox.Text,
                //    newSoap.Assessment);

                //DiffRenderer.SetDiff(
                //    SoapType.Plan,
                //    ref this.PlanRichTextBox,
                //    this.PlanTextBox.Text,
                //    newSoap.Plan);
            }
        }

        private void SetDiff_old(ref RichTextBox target, string oldText, string newText)
        {
            // InlineDiffBuilderを使って差分モデルを作成
            var differ = new Differ();
            var diffBuilder = new InlineDiffBuilder(differ);
            var diffModel = diffBuilder.BuildDiffModel(oldText, newText);

            // RichTextBoxをクリア
            target.Document.Blocks.Clear();
            var body = new Paragraph { Margin = new System.Windows.Thickness(0) };


            // 差分の各行を解析し、色分けしてRichTextBoxに追加
            foreach (DiffPiece line in diffModel.Lines)
            {
                // 表示用のRunを作成
                var run = new Run(line.Text);

                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        // 挿入された行 -> 例として緑系の色で表示
                        run.Foreground = System.Windows.Media.Brushes.Green;
                        run.Text = "+ " + run.Text;
                        break;

                    case ChangeType.Deleted:
                        // 削除された行 -> 例として赤系の色で表示
                        run.Foreground = System.Windows.Media.Brushes.Red;
                        run.Text = "- " + run.Text;
                        break;

                    case ChangeType.Unchanged:
                        // 変更なし -> 通常のまま
                        run.Foreground = System.Windows.Media.Brushes.Black;
                        break;

                    case ChangeType.Imaginary:
                        // 対応する行が空の場合もあるので、このときは薄い灰色など
                        run.Foreground = System.Windows.Media.Brushes.Gray;
                        break;
                }

                body.Inlines.Add(run);
                body.Inlines.Add(new LineBreak());
            }

            target.Document.Blocks.Add(body);
        }

        private Kernel CreateKernelWithOpenAi()
        {
            var config = App.Host.Services.GetRequiredService<IConfiguration>();

            var deploymentName = config["AzureOpenAI:DeploymentName"];
            var azureEndpoint = config["AzureOpenAI:Endpoint"];
            var azureApiKey = config["AzureOpenAI:ApiKey"];
            var modelId = config["AzureOpenAI:ModelId"];

            var builder = Kernel.CreateBuilder();
            builder.Services.AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: azureEndpoint,
                apiKey: azureApiKey,
                modelId: modelId
            );
            return builder.Build();
        }

        private Kernel CreateKernelWithLlStudio()
        {
            var config = App.Host.Services.GetRequiredService<IConfiguration>();

            var modelId = config["LMStudio:ModelId"];
            var endpoint = new Uri(config["LMStudio:Endpoint"]);

            var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0010
            builder.Services.AddOpenAIChatCompletion(
                modelId: modelId,
                endpoint: endpoint
            );
#pragma warning restore SKEXP0010

            return builder.Build();
        }

    }
}

public class DiffColorTheme
{
    public Brush LineDeletedBackground { get; init; }
    public Brush LineInsertedBackground { get; init; }
    public Brush WordDeletedBackground { get; init; }
    public Brush WordInsertedBackground { get; init; }
}

public static class DiffRenderer
{
    private static readonly Dictionary<SoapType, Brush> SoapForegrounds = new()
    {
        { SoapType.Subjective, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a0522d")) },
        { SoapType.Objective,   new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e90ff")) },
        { SoapType.Assessment,  new SolidColorBrush((Color)ColorConverter.ConvertFromString("#228b22")) },
        { SoapType.Plan,        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b22222")) }
    };

    private static readonly Dictionary<SoapType, DiffColorTheme> SoapThemes = new()
    {
        {
            SoapType.Subjective,
            new DiffColorTheme
            {
                LineDeletedBackground = BrushFrom("#ffd6d6"),
                LineInsertedBackground = BrushFrom("#fff0cc"),
                WordDeletedBackground = BrushFrom("#ff6666"),
                WordInsertedBackground = BrushFrom("#ffcc66"),
            }
        },
        {
            SoapType.Objective,
            new DiffColorTheme
            {
                LineDeletedBackground = BrushFrom("#d6eaff"),
                LineInsertedBackground = BrushFrom("#d6ffd6"),
                WordDeletedBackground = BrushFrom("#66b2ff"),
                WordInsertedBackground = BrushFrom("#66ff66"),
            }
        },
        {
            SoapType.Assessment,
            new DiffColorTheme
            {
                LineDeletedBackground = BrushFrom("#ffeecc"),
                LineInsertedBackground = BrushFrom("#f0ffe6"),
                WordDeletedBackground = BrushFrom("#ffaa33"),
                WordInsertedBackground = BrushFrom("#66cc66"),
            }
        },
        {
            SoapType.Plan,
            new DiffColorTheme
            {
                LineDeletedBackground = BrushFrom("#ffccff"),
                LineInsertedBackground = BrushFrom("#ccf5ff"),
                WordDeletedBackground = BrushFrom("#ff66cc"),
                WordInsertedBackground = BrushFrom("#66e0ff"),
            }
        }
    };

    private static Brush BrushFrom(string hex) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    public static void SetDiff(SoapType section, ref RichTextBox target, string oldText, string newText)
    {
        var builder = new InlineDiffBuilder(new Differ());
        var diff = builder.BuildDiffModel(oldText, newText);
        var document = new FlowDocument();
        var body = new Paragraph { Margin = new System.Windows.Thickness(0) };
        var theme = SoapThemes[section];
        var foreground = SoapForegrounds[section];

        for (int i = 0; i < diff.Lines.Count; i++)
        {
            var line = diff.Lines[i];

            if (line.Type == ChangeType.Modified &&
                i + 1 < diff.Lines.Count &&
                diff.Lines[i + 1].Type == ChangeType.Inserted)
            {
                var deleted = BuildWordInline(section, line.Text, diff.Lines[i + 1].Text, isDeleted: true);
                var inserted = BuildWordInline(section, line.Text, diff.Lines[i + 1].Text, isDeleted: false);
                body.Inlines.AddRange(deleted);
                body.Inlines.Add(new LineBreak());
                body.Inlines.AddRange(inserted);
                body.Inlines.Add(new LineBreak());
                i++;
                continue;
            }

            var run = new Run(line.Text)
            {
                Foreground = foreground,
                Background = line.Type switch
                {
                    ChangeType.Inserted => theme.LineInsertedBackground,
                    ChangeType.Deleted => theme.LineDeletedBackground,
                    _ => null
                }
            };

            body.Inlines.Add(run);
            body.Inlines.Add(new LineBreak());
        }

        document.Blocks.Clear();
        document.Blocks.Add(body);
        target.Document = document;
    }

    private static InlineCollection BuildWordInline(SoapType section, string oldLine, string newLine, bool isDeleted)
    {
        var builder = new InlineDiffBuilder(new Differ());
        var inner = builder.BuildDiffModel(oldLine, newLine).Lines;
        var inlines = new Paragraph().Inlines;
        var theme = SoapThemes[section];
        var foreground = SoapForegrounds[section];

        foreach (var line in inner)
        {
            foreach (var piece in line.SubPieces ?? new List<DiffPiece> { line })
            {
                var run = new Run(piece.Text)
                {
                    Foreground = foreground
                };

                if (piece.Type == ChangeType.Deleted && isDeleted)
                {
                    run.Background = theme.WordDeletedBackground;
                    run.FontWeight = FontWeights.Bold;
                }
                else if (piece.Type == ChangeType.Inserted && !isDeleted)
                {
                    run.Background = theme.WordInsertedBackground;
                    run.FontWeight = FontWeights.Bold;
                }

                inlines.Add(run);
            }
        }

        return inlines;
    }
}
