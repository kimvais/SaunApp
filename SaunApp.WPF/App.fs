namespace SaunApp.WPF

open System

open System.ComponentModel.Design
open System.Windows
open Xamarin.Forms
open Xamarin.Forms.Platform.WPF

type MainWindow() = 
    inherit FormsApplicationPage()

module Main = 
    [<EntryPoint>]
    [<STAThread>]
    let main(_args) =

        let app = new System.Windows.Application()
        Forms.Init()
        let window = MainWindow()
        window.Width <- 300.
        window.Height <- 300.
        window.WindowStyle <- WindowStyle.ThreeDBorderWindow
        window.Title <- "SaunaApp"
        
        window.LoadApplication(new SaunApp.App())

        app.Run(window)
