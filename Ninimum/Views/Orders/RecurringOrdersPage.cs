using Api.Services;
using Newtonsoft.Json.Linq;
namespace Ninimum.Views.Orders;

public sealed class RecurringOrdersPage : ContentPage
{
 private readonly UserApiService api;
 private readonly Entry source=new(){Keyboard=Keyboard.Numeric},maximum=new(){Keyboard=Keyboard.Numeric};
 private readonly Label preview=new(){TextColor=Colors.Black};
 private readonly CheckBox consent=new();
 private readonly VerticalStackLayout rules=new(){Spacing=12};
 private readonly Button inspect=new(),save=new();
 private long? previewSource;private bool busy;
 private static string L(string uz,string ru,string en)=>System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch{"uz"=>uz,"ru"=>ru,_=>en};
 public RecurringOrdersPage(UserApiService api)
 {
  this.api=api;Title=L("Takroriy buyurtmalar","Повторные заказы","Recurring orders");BackgroundColor=Colors.White;
  source.Placeholder=L("Yetkazilgan buyurtma raqami","Номер доставленного заказа","Delivered order number");maximum.Placeholder=L("Eng yuqori jami summa (so‘m)","Максимальная сумма (сум)","Maximum total (UZS)");
  inspect.Text=L("Ko‘rib chiqish","Предпросмотр","Preview");save.Text=L("Roziman, rejalashtirish","Согласен, запланировать","Consent and schedule");
  source.TextChanged+=(_,_)=>{previewSource=null;consent.IsChecked=false;preview.Text="";};maximum.TextChanged+=(_,_)=>consent.IsChecked=false;
  inspect.Clicked+=async(_,_)=>await Run(async()=>{
   if(!long.TryParse(source.Text?.Trim().TrimStart('#'),out var id)||id<1)throw new InvalidOperationException(L("Buyurtma raqamini kiriting","Введите номер заказа","Enter an order number"));
   var r=await api.RecurringRequest("preview",new{source_order_id=id});var p=r["resultData"]!;
   maximum.Text=p["total"]?.ToString();preview.Text=$"{p["delivery_address"]}\n{p["total"]} UZS · {p["interval_days"]} "+L("kunda bir","дней","days");previewSource=id;consent.IsChecked=false;
  });
  save.Clicked+=async(_,_)=>await Run(async()=>{
   if(previewSource==null||!consent.IsChecked||!long.TryParse(maximum.Text,out var amount)||amount<1)throw new InvalidOperationException(L("Oldin ko‘rib chiqing va rozilik bering","Сначала выполните предпросмотр и дайте согласие","Preview first and give consent"));
   await api.RecurringRequest("save",new{source_order_id=previewSource.Value,max_total=amount,consent=true});consent.IsChecked=false;previewSource=null;preview.Text=L("Rejalashtirildi","Запланировано","Scheduled");await LoadRules();
  });
  var layout=new VerticalStackLayout{Padding=20,Spacing=14};
  layout.Add(new Label{Text=L("Faqat to‘lanmagan buyurtma yaratiladi. Har bir to‘lovni o‘zingiz tasdiqlaysiz. Narx oshsa yoki obuna tugasa, reja tekshirish uchun to‘xtaydi.","Создаётся только неоплаченный заказ. Каждую оплату подтверждаете вы. При превышении цены или окончании подписки план приостанавливается.","Creates unpaid drafts only. You confirm every payment. A price above your limit or an expired membership pauses the schedule."),TextColor=Colors.Black});
  layout.Add(source);layout.Add(inspect);layout.Add(preview);layout.Add(maximum);
  layout.Add(new HorizontalStackLayout{Children={consent,new Label{Text=L("Takroriy qoralamalarga roziman","Согласен на повторные черновики","I consent to recurring drafts"),VerticalOptions=LayoutOptions.Center,TextColor=Colors.Black}}});
  layout.Add(save);layout.Add(new Label{Text=L("Mening rejalarim","Мои планы","My schedules"),FontAttributes=FontAttributes.Bold,TextColor=Colors.Black});layout.Add(rules);Content=new ScrollView{Content=layout};
 }
 protected override async void OnAppearing(){base.OnAppearing();await Run(LoadRules);}
 private async Task LoadRules(){var response=await api.RecurringRequest("list");rules.Clear();foreach(var r in response["resultData"] as JArray??new JArray()){
  var box=new VerticalStackLayout{Spacing=6};box.Add(new Label{Text=$"#{r["source_order_id"]} · {r["status"]} · {r["interval_days"]} "+L("kun","дней","days")+$" · ≤ {r["max_total"]} UZS",TextColor=Colors.Black});
  if(DateTimeOffset.TryParse(r["next_run_at"]?.ToString(),out var date))box.Add(new Label{Text=L("Keyingi qoralama: ","Следующий черновик: ","Next draft: ")+date.ToOffset(TimeSpan.FromHours(5)).ToString("dd.MM.yyyy HH:mm"),TextColor=Colors.Gray});
  if(r["last_order_id"]!=null&&r["last_order_id"]!.Type!=JTokenType.Null)box.Add(new Label{Text=L("Buyurtmalar sahifasida to‘lang: #","Оплатите на странице заказов: #","Pay on the Orders page: #")+r["last_order_id"],TextColor=Colors.Black});
  if(r["last_order_status"]?.ToString()=="PENDING"&&r["last_payment_status"]?.ToString()=="PENDING"){
   long order=r["last_order_id"]!.Value<long>();string total=r["last_total"]?.ToString()??"";
   var pay=new Button{Text=L("To‘lash","Оплатить","Pay")+$" #{order} · {total} UZS"};
   pay.Clicked+=async(_,_)=>await Run(async()=>{
    bool confirmed=await DisplayAlertAsync(L("To‘lov","Оплата","Payment"),$"#{order} · {total} UZS",L("Davom etish","Продолжить","Continue"),L("Bekor qilish","Отмена","Cancel"));if(!confirmed)return;
    var checkout=await api.CreatePaymeCheckoutUrl(new global::Models.Requests.CreatePaymeCheckoutUrlRequest{order_id=checked((int)order)});
    if(checkout.resultCode!="100"||string.IsNullOrWhiteSpace(checkout.resultData?.payment_url))throw new InvalidOperationException(checkout.resultMsg??"Checkout unavailable");
    await Shell.Current.GoToAsync(nameof(Ninimum.Views.Payment.PaymentPage),new Dictionary<string,object>{{"PaymentUrl",checkout.resultData.payment_url},{"OrderId",order},{"PaymentType","ORDER"}});
   });box.Add(pay);
  }
  box.Add(new Label{Text=r["last_note"]?.ToString(),TextColor=Colors.Gray});
  if(r["status"]?.ToString()=="ACTIVE"){long id=r["id"]!.Value<long>();var stop=new Button{Text=L("To‘xtatish","Приостановить","Pause")};stop.Clicked+=async(_,_)=>await Run(async()=>{await api.RecurringRequest($"{id}/pause");await LoadRules();});box.Add(stop);}
  else if(r["status"]?.ToString() is "PAUSED" or "NEEDS_REVIEW"){long id=r["id"]!.Value<long>();var resume=new Button{Text=L("Davom ettirish","Возобновить","Resume")};resume.Clicked+=async(_,_)=>await Run(async()=>{await api.RecurringRequest($"{id}/resume");await LoadRules();});box.Add(resume);}
  rules.Add(box);
 }}
 private async Task Run(Func<Task> action){if(busy)return;busy=true;inspect.IsEnabled=save.IsEnabled=false;try{await action();}catch(Exception ex){await DisplayAlertAsync(L("Xabar","Сообщение","Notice"),ex.Message,"OK");}finally{busy=false;inspect.IsEnabled=save.IsEnabled=true;}}
}
