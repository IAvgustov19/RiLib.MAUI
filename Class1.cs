using CommunityToolkit.Maui.Alerts;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RiLib.MAUI
{
    // All the code in this file is included in all platforms.
    public class Main
    {
        public static bool IsOpenPage { get; set; } = false;
        public enum TypeState
        {
            UNKNOWN_ERROR, NOT_INTERNET_CONNECTION, SUCCESS, TASK_CANCELLED
        }
        private static CancellationTokenSource cancellationTokenSource;

        internal static async Task<Response> SentRequestToYDB(string requestElement, string url)
        {
            try
            {
                //При каждом новом запросе создаётся новый экземпляр, т.к. старый уже отменен
                cancellationTokenSource = new CancellationTokenSource();
                var Response = await new HttpClient().PostAsync(
                url,
                     new StringContent(requestElement, Encoding.UTF8, "application/json"), cancellationTokenSource.Token);

                //Если отменен, то вызывается обрабатываемое исключение
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                string res = await Response.Content.ReadAsStringAsync();
                return new MAUI.Response { typeState = TypeState.SUCCESS, textResponse = res};
            }
            catch (TaskCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {
                return new Response { typeState = TypeState.TASK_CANCELLED, textResponse = string.Empty };
            }
            catch (WebException) when (cancellationTokenSource.IsCancellationRequested)
            {
                return new Response { typeState = TypeState.TASK_CANCELLED, textResponse = string.Empty};
            }
            catch (WebException)
            {
                return new Response { textResponse = string.Empty, typeState = TypeState.NOT_INTERNET_CONNECTION};
            }
            catch
            {
                return new Response { textResponse = string.Empty, typeState = TypeState.UNKNOWN_ERROR };
            }
        }
        internal static void CancellToken()
        {
            cancellationTokenSource?.Cancel();
        }
        public static async void CallToast(string message, CommunityToolkit.Maui.Core.ToastDuration duration, double textSize = 14)
        {
            await Toast.Make(message, duration, textSize).Show();
        }

        //Контроллер правильного, контролируемого перехода на страницу,
        //чтобы при открытии другие не открывались
        public static async Task MoveAndBlockOtherOpeningPage(Page current, Page to)
        {
            if (!IsOpenPage)
            {
                IsOpenPage = true;
                await current.Navigation.PushAsync(to);
                IsOpenPage = false;
            }
        }
    }
    public class Response : Main
    {
        public TypeState typeState { get; set; }
        public string textResponse {  get; set; }
    }
}