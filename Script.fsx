﻿#r """.\packages\FunScript.1.1.0.28\lib\net40\FunScript.dll"""
#r """.\packages\FunScript.1.1.0.28\lib\net40\FunScript.Interop.dll"""
#r """.\packages\FunScript.TypeScript.Binding.lib.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.lib.dll"""
#r """.\packages\FunScript.TypeScript.Binding.jquery.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.jquery.dll"""
#r """.\packages\FunScript.TypeScript.Binding.highcharts.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.highcharts.dll"""
#r """.\bin\release\FsPlot.dll"""

open FsPlot.Charting
open FsPlot.DataSeries

let data = ["Chrome", 233; "Firefox", 141; "IE", 256]
    
// Create a pie chart
let chart = Highcharts.Pie data


// Display a legend
chart.ShowLegend()


// Update the chart's data
chart.SetData ["Chrome", 233; "Firefox", 141; "IE", 256; "Safari", 208]


// Update the chart's data in a more structured way
["Chrome", 233; "Firefox", 141; "IE", 256; "Safari", 208; "Others", 75]
|> Series.New "Browser Share" ChartType.Pie
|> chart.SetData


// Add a title
chart.SetTitle "Website Visitors By Browser"

module Pie =
    
    let pie1 =
        let data = ["Chrome", 233; "Firefox", 141; "IE", 256; "Safari", 208; "Others", 75]
        Highcharts.Pie(data, "Website Visitors By Browser", true)

    let pie2 =
        ["Chrome", 233; "Firefox", 141; "IE", 256; "Safari", 208; "Others", 75]
        |> Series.New "Browser Share" ChartType.Pie
        |> fun x -> Highcharts.Pie(x, "Website Visitors By Browser", true)

open System

module Area =
    
    let area1 =
        let salesData = ["2010", 1000; "2011", 1170; "2012", 560; "2013", 1030]
        Highcharts.Area(salesData, "Company Sales", true)

    let area2 =
        ["2010", 1000; "2011", 1170; "2012", 560; "2013", 1030]
        |> Series.New "Sales" ChartType.Area
        |> fun x -> Highcharts.Area(x, "Company Sales", true)

    let area3 =
        let salesData = ["2010", 1000; "2011", 1170; "2012", 560; "2013", 1030]        
        let expensesData = ["2010", 600; "2011", 760; "2012", 420; "2013", 540]
        Highcharts.Area([salesData; expensesData], "Company Performance", true)

    let area4 =
        let sales =
            ["2010", 1000; "2011", 1170; "2012", 560; "2013", 1030]
            |> Series.New "Sales" ChartType.Area
        let expenses =
            ["2010", 600; "2011", 760; "2012", 420; "2013", 540]
            |> Series.New "Expenses" ChartType.Area
        Highcharts.Area([sales; expenses], "Company Performance", true)

    // datetime x axis
    let area5 =
        [
            DateTime.Now, 1000
            DateTime.Now.AddDays(1.), 1170
            DateTime.Now.AddDays(4.), 560
            DateTime.Now.AddDays(8.), 1030
        ]
        |> Highcharts.Area
        
