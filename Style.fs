namespace SaunApp.Style


[<AutoOpen>]
module Colors =
    open Xamarin.Forms
    let Primary = Color.FromHex("#8bc34a")
    let PrimaryL = Color.FromHex("#bef67a")
    let PrimaryD = Color.FromHex("#5a9216")
    let Secondary = Color.FromHex("#ffc400")
    let SecondaryL = Color.FromHex("#fff64f")
    let SecodaryD = Color.FromHex("#c79400")
    let Text = Color.Black
    let Error = Color.FromHex("#B00020")

[<AutoOpen>]
module Design =
    open Fabulous.XamarinForms
    open Xamarin.Forms
    let materialFont =
        (match Device.RuntimePlatform with
         | Device.iOS -> "Material Design Icons"
         | Device.Android -> "materialdesignicons_webfont.ttf#Material Design Icons"
         | Device.UWP -> "materialdesignicons-webfont.ttf#Material Design Icons"
         | _ -> null)

    let materialButton materialIcon backgroundColor textColor command =
        View.Button
            (text = materialIcon, command = command, fontFamily = materialFont, fontSize = FontSize(20.),
             backgroundColor = backgroundColor, textColor = textColor, cornerRadius = 10, borderColor = backgroundColor)

[<AutoOpen>]
module Icon =
    let Refresh = "\U000F0450"
