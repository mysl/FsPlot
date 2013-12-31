﻿module FsPlot.Charting

open Options
open DataSeries

type ChartData =
    {
        Categories : string []
        Data : Series []
        Legend : bool
        PointFormat : string option
        Subtitle : string option
        Title : string option
        Type : ChartType
        XTitle : string option
        YTitle : string option
    }

    static member New a b c d e f g h i=
        {
            Categories = a
            Data = b
            Legend = c
            PointFormat = d
            Subtitle = e
            Title = f
            Type = g
            XTitle = h
            YTitle = i
        }

    member __.Fields =
        __.Categories,
        __.Data,
        __.Legend,
        __.PointFormat,
        __.Subtitle,
        __.Title,
        __.Type,
        __.XTitle,
        __.YTitle

module private Js =

    let highcharts (x:ChartData) =
        let a, b, c, d, e, f, g, h, i = x.Fields
        match g with
        | Area -> HighchartsJs.area b f c a h i d e Disabled false
        | Areaspline -> HighchartsJs.areaspline b f c a h i d e Disabled false
        | Arearange -> HighchartsJs.arearange b f c a h i d e
        | Bar -> HighchartsJs.bar b f c a h i d e Disabled
        | Bubble -> HighchartsJs.bubble b f c a h i d e
        | Column -> HighchartsJs.column b f c a h i d e Disabled
        | Combination -> HighchartsJs.combine b f c a h i d e None
        | Donut -> HighchartsJs.donut b f c a h i d e
        | Funnel -> HighchartsJs.funnel b f c a h i d e
        | Line -> HighchartsJs.line b f c a h i d e
        | PercentArea -> HighchartsJs.percentArea b f c a h i d e false
        | PercentBar -> HighchartsJs.percentBar b f c a h i d e
        | PercentColumn -> HighchartsJs.percentColumn b f c a h i d e
        | Pie -> HighchartsJs.pie b f c a h i d e
        | Radar -> HighchartsJs.radar b f c a h i d e
        | Scatter -> HighchartsJs.scatter b f c a h i d e
        | Spline -> HighchartsJs.spline b f c a h i d e
        | StackedArea -> HighchartsJs.stackedArea b f c a h i d e false
        | StackedBar -> HighchartsJs.stackedBar b f c a h i d e
        | StackedColumn -> HighchartsJs.stackedColumn b f c a h i d e

module private Html =
    
    let highcharts chartType =
        match chartType with
        | Arearange | Bubble | Radar -> HighchartsHtml.more
        | Combination -> HighchartsHtml.combine
        | Funnel -> HighchartsHtml.funnel
        | _ -> HighchartsHtml.common


type GenericChart() as chart =
    
    [<DefaultValue>] val mutable private chartData : ChartData    
//    [<DefaultValue>] val mutable private jsFun : ChartData -> string
//    [<DefaultValue>] val mutable private htmlFun : string -> string    

    let mutable jsFun = Js.highcharts    
    let mutable htmlFun = Html.highcharts

    let wnd, browser = ChartWindow.show()

    let ctx = System.Threading.SynchronizationContext.Current

    let agent =
        MailboxProcessor<ChartData>.Start(fun inbox ->
            let rec loop() =
                async {
                    let! msg = inbox.Receive()
                    match inbox.CurrentQueueLength with
                    | 0 ->
                        let js = jsFun msg
                        match inbox.CurrentQueueLength with
                        | 0 ->
                            let html = htmlFun msg.Type js
                            match inbox.CurrentQueueLength with
                            | 0 ->
                                do! Async.SwitchToContext ctx
                                browser.NavigateToString html
                                return! loop()
                            | _ -> return! loop()
                        | _ -> return! loop()
                    | _ -> return! loop()
                }
            loop())

    member __.Close() = wnd.Close()

    static member internal Create x (f:unit -> #GenericChart) =
        let gc = f()
        gc.chartData <- x
        gc.Navigate()
        gc

    member __.HideLegend() =
        chart.chartData <- { chart.chartData with Legend = false }
        __.Navigate()

    member internal __.Navigate() = agent.Post chart.chartData

    member __.SetCategories(categories) =
        chart.chartData <- { chart.chartData with Categories = Seq.toArray categories}
        __.Navigate()

    member __.SetData series =
        chart.chartData <- { chart.chartData with Data = [|series|] }
        __.Navigate()

    member __.SetData (data:seq<Series>) =
        let series = Seq.toArray data
        chart.chartData <- { chart.chartData with Data = series }
        __.Navigate()

    member internal __.SetHtmlFun f = htmlFun <- f

    member internal __.SetJsFun(f) = jsFun <- f

    member __.SetTooltip(format) =
        chart.chartData <- { chart.chartData with PointFormat = Some format }
        __.Navigate()

    member __.SetSubtitle subtitle =
        chart.chartData <- { chart.chartData with Subtitle = Some subtitle }
        __.Navigate()

    member __.SetTitle title =
        chart.chartData <- { chart.chartData with Title = Some title }
        __.Navigate()

    member __.SetXTitle(title) =
        chart.chartData <- { chart.chartData with XTitle = Some title }
        __.Navigate()

    member __.SetYTitle(title) =
        chart.chartData <- { chart.chartData with YTitle = Some title }
        __.Navigate()

    member __.ShowLegend() =
        chart.chartData <- { chart.chartData with Legend = true }
        __.Navigate()

type HighchartsArea() =
    inherit GenericChart()

    let mutable stacking = Disabled
    let mutable inverted = false

    let compileJs (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.area b f c a h i d e stacking inverted

    do base.SetJsFun compileJs

    member __.SetStacking(x) =
        stacking <- x
        base.Navigate()

    member __.SetInverted(x) =
        inverted <- x
        base.Navigate()

type HighchartsAreaspline() =
    inherit GenericChart()

    let mutable stacking = Disabled
    let mutable inverted = false

    let compileJs (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.areaspline b f c a h i d e stacking inverted

    do base.SetJsFun compileJs

    member __.SetStacking(x) =
        stacking <- x
        base.Navigate()

    member __.SetInverted(x) =
        inverted <- x
        base.Navigate()

type HighchartsArearange() =
    inherit GenericChart()

type HighchartsBar() =
    inherit GenericChart()

    let mutable stacking = Disabled

    let compileJs (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.bar b f c a h i d e stacking

    do base.SetJsFun compileJs

    member __.SetStacking(x) =
        stacking <- x
        base.Navigate()

type HighchartsBubble() =
    inherit GenericChart()

type HighchartsColumn() =
    inherit GenericChart()

    let mutable stacking = Disabled

    let compileJs (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.column b f c a h i d e stacking

    do base.SetJsFun compileJs

    member __.SetStacking(x) =
        stacking <- x
        base.Navigate()

type HighchartsCombination() =
    inherit GenericChart()

    let mutable pieOptions = None

    let js (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.combine b f c a h i d e pieOptions

    do base.SetJsFun js

    member __.SetPieOptions x =
        pieOptions <- Some x
        base.Navigate()

type HighchartsDonut() =
    inherit GenericChart()

type HighchartsFunnel() =
    inherit GenericChart()

type HighchartsLine() =
    inherit GenericChart()

type HighchartsPercentArea() =
    inherit GenericChart()

    let mutable inverted = false

    let compileJs (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.percentArea b f c a h i d e inverted

    do base.SetJsFun compileJs

    member __.SetInverted(x) =
        inverted <- x
        base.Navigate()

type HighchartsPercentBar() =
    inherit GenericChart()

type HighchartsPercentColumn() =
    inherit GenericChart()

type HighchartsPie() =
    inherit GenericChart()

type HighchartsRadar() =
    inherit GenericChart()

type HighchartsScatter() =
    inherit GenericChart()

type HighchartsSpline() =
    inherit GenericChart()

type HighchartsStackedArea() =
    inherit GenericChart()

    let mutable inverted = false

    let compileJs (x:ChartData) =
        let a, b, c, d, e, f, _, h, i = x.Fields
        HighchartsJs.stackedArea b f c a h i d e inverted

    do base.SetJsFun compileJs

    member __.SetInverted(x) =
        inverted <- x
        base.Navigate()

type HighchartsStackedBar() =
    inherit GenericChart()

type HighchartsStackedColumn() =
    inherit GenericChart()

let private newChartData a b c d e f g h i =
    let c' = defaultArg c false
    let a' =
        match a with 
        | None -> [||]
        | Some value -> Seq.toArray value
    ChartData.New a' b c' d e f g h i

type Highcharts =

    /// <summary>Creates an area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 

    /// <summary>Creates an area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Area data
        let chartData = newChartData categories [|series|] legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 

    /// <summary>Creates an area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Area data
        let chartData = newChartData categories [|series|] legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 
        
    /// <summary>Creates an area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Area
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 

    /// <summary>Creates an area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Area
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 

    /// <summary>Creates an area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 

    /// <summary>Creates an area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Area(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Area
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Area xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArea()) 

    /// <summary>Creates an areaspline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 

    /// <summary>Creates an area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Areaspline data
        let chartData = newChartData categories [|series|] legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 

    /// <summary>Creates an areaspline chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Areaspline data
        let chartData = newChartData categories [|series|] legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 
        
    /// <summary>Creates an areaspline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Areaspline
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 

    /// <summary>Creates an areaspline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Areaspline
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 

    /// <summary>Creates an areaspline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 

    /// <summary>Creates an areaspline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Areaspline(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Areaspline
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Areaspline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsAreaspline()) 

    /// <summary>Creates an arearange chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Arearange(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Arearange xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArearange())

    /// <summary>Creates an arearange chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Arearange(data:seq<#key*#value*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Arearange data
        let chartData = newChartData categories [|series|] legend None None title Arearange xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArearange())
        
    /// <summary>Creates an arearange chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Arearange(data:seq<(#key*#value*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Arearange
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Arearange xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArearange())

    /// <summary>Creates an arearange chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Arearange(data:seq<(#key*#value*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Arearange
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Arearange xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArearange())

    /// <summary>Creates an arearange chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Arearange(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Arearange xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArearange())

    /// <summary>Creates an arearange chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Arearange(data:seq<seq<#key*#value*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Arearange
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Arearange xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsArearange())

    /// <summary>Creates a bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 

    /// <summary>Creates a bar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Bar data
        let chartData = newChartData categories [|series|] legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 

    /// <summary>Creates a bar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Bar data
        let chartData = newChartData categories [|series|] legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 
        
    /// <summary>Creates a bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 

    /// <summary>Creates a bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 

    /// <summary>Creates a bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 

    /// <summary>Creates a bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bar(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBar()) 

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<#key*#value*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Bubble data
        let chartData = newChartData categories [|series|] legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<#value*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Bubble data
        let chartData = newChartData categories [|series|] legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<(#key*#value*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bubble
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())
        
    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<(#value*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bubble
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<(#key*#value*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bubble
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<(#value*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bubble
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<seq<#key*#value*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bubble
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a bubble chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Bubble(data:seq<seq<#value*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Bubble
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Bubble xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsBubble())

    /// <summary>Creates a column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 

    /// <summary>Creates a column chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Column data
        let chartData = newChartData categories [|series|] legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 

    /// <summary>Creates a column chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Column data
        let chartData = newChartData categories [|series|] legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 
        
    /// <summary>Creates a column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Column
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 

    /// <summary>Creates a column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Column
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 

    /// <summary>Creates a column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 

    /// <summary>Creates a column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Column(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Column
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Column xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsColumn()) 

    /// <summary>Creates a combination chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Combine(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories (Seq.toArray data) legend None None title Combination xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsCombination())

    /// <summary>Creates a donut chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Donut(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Donut xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsDonut()) 

    /// <summary>Creates a donut chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Donut(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories (Seq.toArray data) legend None None title Donut xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsDonut()) 

    /// <summary>Creates a donut chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Donut(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Donut data
        let chartData = newChartData categories [|series|] legend None None title Donut xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsDonut()) 

    /// <summary>Creates a donut chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Donut(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Donut data
        let chartData = newChartData categories [|series|] legend None None title Donut xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsDonut()) 

    /// <summary>Creates a funnel chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Funnel(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Funnel xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsFunnel())

    /// <summary>Creates a funnel chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Funnel(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories (Seq.toArray data) legend None None title Funnel xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsFunnel())

    /// <summary>Creates a funnel chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Funnel(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Funnel data
        let chartData = newChartData categories [|series|] legend None None title Funnel xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsFunnel())

    /// <summary>Creates a funnel chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Funnel(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Funnel data
        let chartData = newChartData categories [|series|] legend None None title Funnel xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsFunnel())

    /// <summary>Creates a line chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 

    /// <summary>Creates a line chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Line data
        let chartData = newChartData categories [|series|] legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 

    /// <summary>Creates a line chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Line data
        let chartData = newChartData categories [|series|] legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 
        
    /// <summary>Creates a line chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Line
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 

    /// <summary>Creates a line chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Line
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 

    /// <summary>Creates a line chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 

    /// <summary>Creates a line chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Line(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Line
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Line xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsLine()) 

    /// <summary>Creates a percent area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 

    /// <summary>Creates a percent area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.PercentArea data
        let chartData = newChartData categories [|series|] legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 

    /// <summary>Creates a percent area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.PercentArea data
        let chartData = newChartData categories [|series|] legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 
        
    /// <summary>Creates a percent area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentArea
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 

    /// <summary>Creates a percent area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentArea
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 

    /// <summary>Creates a percent area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 

    /// <summary>Creates a percent area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentArea(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentArea
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentArea()) 

    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 

    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.PercentBar data
        let chartData = newChartData categories [|series|] legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 

    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.PercentBar data
        let chartData = newChartData categories [|series|] legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 
        
    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentBar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 

    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentBar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 

    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 

    /// <summary>Creates a percent bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentBar(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentBar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentBar()) 

    /// <summary>Creates a percent column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 

    /// <summary>Creates a percent column chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.PercentColumn data
        let chartData = newChartData categories [|series|] legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 

    /// <summary>Creates a percent column chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.PercentColumn data
        let chartData = newChartData categories [|series|] legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 
        
    /// <summary>Creates a percent column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentColumn
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 

    /// <summary>Creates a percent column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentColumn
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 

    /// <summary>Creates a percent column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 

    /// <summary>Creates a percent column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member PercentColumn(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.PercentColumn
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title PercentColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPercentColumn()) 

    /// <summary>Creates a pie chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Pie(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Pie xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPie()) 

    /// <summary>Creates a pie chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Pie(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories (Seq.toArray data) legend None None title Pie xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPie()) 

    /// <summary>Creates a pie chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Pie(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Pie data
        let chartData = newChartData categories [|series|] legend None None title Pie xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPie()) 

    /// <summary>Creates a pie chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Pie(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Pie data
        let chartData = newChartData categories [|series|] legend None None title Pie xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsPie()) 

    /// <summary>Creates a radar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())

    /// <summary>Creates a radar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Radar data
        let chartData = newChartData categories [|series|] legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())

    /// <summary>Creates a Radar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Radar data
        let chartData = newChartData categories [|series|] legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())
        
    /// <summary>Creates a radar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Radar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())

    /// <summary>Creates a radar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Radar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())

    /// <summary>Creates a radar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())

    /// <summary>Creates a radar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Radar(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Radar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Radar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsRadar())
        
    /// <summary>Creates a scatter chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 

    /// <summary>Creates a scatter chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Scatter data
        let chartData = newChartData categories [|series|] legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 

    /// <summary>Creates a scatter chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Scatter data
        let chartData = newChartData categories [|series|] legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 
        
    /// <summary>Creates a scatter chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Scatter
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 

    /// <summary>Creates a scatter chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Scatter
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 

    /// <summary>Creates a scatter chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 

    /// <summary>Creates a scatter chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Scatter(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Scatter
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Scatter xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsScatter()) 

    /// <summary>Creates a spline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 

    /// <summary>Creates a spline chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Spline data
        let chartData = newChartData categories [|series|] legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 

    /// <summary>Creates a spline chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Spline data
        let chartData = newChartData categories [|series|] legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 
        
    /// <summary>Creates a spline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Spline
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 

    /// <summary>Creates a spline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Spline
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 

    /// <summary>Creates a spline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 

    /// <summary>Creates a spline chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member Spline(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Spline
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title Spline xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsSpline()) 

    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 

    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Area data
        let chartData = newChartData categories [|series|] legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 

    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.Area data
        let chartData = newChartData categories [|series|] legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 
        
    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Area
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 

    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Area
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 

    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 

    /// <summary>Creates a stacked area chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedArea(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.Area
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedArea xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedArea()) 

    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 

    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.StackedBar data
        let chartData = newChartData categories [|series|] legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 

    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.StackedBar data
        let chartData = newChartData categories [|series|] legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 
        
    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.StackedBar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 

    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.StackedBar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 

    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 

    /// <summary>Creates a stacked bar chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedBar(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.StackedBar
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedBar xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedBar()) 

    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:Series, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let chartData = newChartData categories [|data|] legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 

    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:seq<#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.StackedColumn data
        let chartData = newChartData categories [|series|] legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 

    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="data">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:seq<#key*#value>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let series = Series.StackedColumn data
        let chartData = newChartData categories [|series|] legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 
        
    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:seq<(#key*#value) list>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.StackedColumn
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 

    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:seq<(#key*#value) []>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.StackedColumn
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 

    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:seq<Series>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data = Seq.toArray data
        let chartData = newChartData categories data legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 

    /// <summary>Creates a stacked column chart.</summary>
    /// <param name="series">The chart's data.</param>
    /// <param name="categories">The X-axis categories.</param>
    /// <param name="legend">Whether to display a legend or not.</param>
    /// <param name="title">The chart's title.</param>
    /// <param name="xTitle">The X-axis title.</param>
    /// <param name="yTitle">The Y-axis title.</param>
    static member StackedColumn(data:seq<seq<#key*#value>>, ?categories, ?legend, ?title, ?xTitle, ?yTitle) =
        let data =
            data
            |> Seq.map Series.StackedColumn
            |> Seq.toArray
        let chartData = newChartData categories data legend None None title StackedColumn xTitle yTitle
        GenericChart.Create chartData (fun () -> HighchartsStackedColumn()) 

type Chart =

    static member categories categories (chart:#GenericChart) =
        chart.SetCategories categories
        chart

    static member close (chart:#GenericChart) = chart.Close()

    static member data (series:seq<Series>) (chart:#GenericChart) = chart.SetData series

    static member hideLegend (chart:#GenericChart) =
        chart.HideLegend()
        chart

    static member plot (series:Series) =
        match series.Type with
        | Area -> Highcharts.Area series :> GenericChart
        | Areaspline -> Highcharts.Areaspline series :> GenericChart
        | Arearange -> Highcharts.Arearange series :> GenericChart
        | Bar -> Highcharts.Bar series :> GenericChart
        | Bubble -> Highcharts.Bubble series :> GenericChart
        | Column -> Highcharts.Column series :> GenericChart
        | Donut -> Highcharts.Donut series :> GenericChart
        | Funnel -> Highcharts.Funnel series :> GenericChart
        | Line -> Highcharts.Line series :> GenericChart
        | PercentArea -> Highcharts.PercentArea series :> GenericChart
        | PercentBar -> Highcharts.PercentBar series :> GenericChart
        | PercentColumn -> Highcharts.PercentColumn series :> GenericChart
        | Pie -> Highcharts.Pie series :> GenericChart
        | Radar -> Highcharts.Radar series :> GenericChart
        | Scatter -> Highcharts.Scatter series :> GenericChart
        | Spline -> Highcharts.Spline series :> GenericChart
        | StackedArea -> Highcharts.StackedArea series :> GenericChart
        | StackedBar -> Highcharts.StackedBar series :> GenericChart
        | _ -> Highcharts.StackedColumn series :> GenericChart

    static member plot (series:seq<Series>) =
        let types =
            series
            |> Seq.map (fun x -> x.Type)
            |> Seq.distinct
        match Seq.length types with
        | 1 ->
            match Seq.nth 0 types with
            | Area -> Highcharts.Area series :> GenericChart
            | Areaspline -> Highcharts.Areaspline series :> GenericChart
            | Arearange -> Highcharts.Arearange series :> GenericChart
            | Bar -> Highcharts.Bar series :> GenericChart
            | Bubble -> Highcharts.Bubble series :> GenericChart
            | Column -> Highcharts.Column series :> GenericChart
            | Donut -> Highcharts.Donut series :> GenericChart
            | Funnel -> Highcharts.Funnel series :> GenericChart
            | Line -> Highcharts.Line series :> GenericChart
            | PercentArea -> Highcharts.PercentArea series :> GenericChart
            | PercentBar -> Highcharts.PercentBar series :> GenericChart
            | PercentColumn -> Highcharts.PercentColumn series :> GenericChart
            | Pie -> Highcharts.Pie series :> GenericChart
            | Radar -> Highcharts.Radar series :> GenericChart
            | Scatter -> Highcharts.Scatter series :> GenericChart
            | Spline -> Highcharts.Spline series :> GenericChart
            | StackedArea -> Highcharts.StackedArea series :> GenericChart
            | StackedBar -> Highcharts.StackedBar series :> GenericChart
            | _ -> Highcharts.StackedColumn series :> GenericChart
        | _ -> Highcharts.Combine series :> GenericChart

    static member showLegend (chart:#GenericChart) =
        chart.ShowLegend()
        chart

    static member subtitle subtitle (chart:#GenericChart) =
        chart.SetSubtitle subtitle
        chart

    static member title title (chart:#GenericChart) =
        chart.SetTitle title
        chart

    static member tooltip format (chart:#GenericChart) =
        chart.SetTooltip format
        chart

    static member xTitle xTitle (chart:#GenericChart) =
        chart.SetXTitle xTitle
        chart

    static member yTitle yTitle (chart:#GenericChart) =
        chart.SetYTitle yTitle
        chart