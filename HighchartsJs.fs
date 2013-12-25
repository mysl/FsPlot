﻿module FsPlot.HighchartsJs

#if INTERACTIVE
#r """.\packages\FunScript.1.1.0.28\lib\net40\FunScript.dll"""
#r """.\packages\FunScript.1.1.0.28\lib\net40\FunScript.Interop.dll"""
#r """.\packages\FunScript.TypeScript.Binding.lib.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.lib.dll"""
#r """.\packages\FunScript.TypeScript.Binding.jquery.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.jquery.dll"""
#r """.\packages\FunScript.TypeScript.Binding.highcharts.1.1.0.13\lib\net40\FunScript.TypeScript.Binding.highcharts.dll"""
#endif

open Microsoft.FSharp.Quotations
open System
open FunScript
open Options
open DataSeries
open Expr

[<ReflectedDefinition>]
module internal Utils =

    let jq(selector:string) = Globals.Dollar.Invoke selector

    let areaChartOptions renderTo chartType inverted (options:HighchartsOptions) =
        let chartOptions = createEmpty<HighchartsChartOptions>()
        chartOptions.renderTo <- renderTo
        chartOptions._type <- chartType
        chartOptions.inverted <- inverted
        options.chart <- chartOptions

    let setChartOptions renderTo chartType (options:HighchartsOptions) =
        let chartOptions = createEmpty<HighchartsChartOptions>()
        chartOptions.renderTo <- renderTo
        chartOptions._type <- chartType
        options.chart <- chartOptions

    let setXAxisOptions xType (options:HighchartsOptions) (categories:string []) xTitle =
        let axisOptions = createEmpty<HighchartsAxisOptions>()
        let xAxisType =
            match categories.Length with
            | 0 ->
                match xType with
                | TypeCode.DateTime -> "datetime"
                | TypeCode.String -> "category"
                | _ -> "linear"
            | _ ->
                axisOptions.categories <- categories
                "category"
        axisOptions._type <- xAxisType
        let axisTitle = createEmpty<HighchartsAxisTitle>()
        axisTitle.text <- defaultArg xTitle ""
        axisOptions.title <- axisTitle
        options.xAxis <- axisOptions

    let setYAxisOptions (options:HighchartsOptions) yTitle =
        let axisOptions = createEmpty<HighchartsAxisOptions>()
        let axisTitle = createEmpty<HighchartsAxisTitle>()
        axisTitle.text <- defaultArg yTitle ""
        axisOptions.title <- axisTitle
        options.yAxis <- axisOptions

    let setTitleOptions chartTitle (options:HighchartsOptions) =
        let titleOptions = createEmpty<HighchartsTitleOptions>()
        titleOptions.text <- defaultArg chartTitle ""
        options.title <- titleOptions

    let setSeriesChartType chartType (options:HighchartsSeriesOptions) =
        let chartTypeStr = 
            match chartType with
            | Area -> "area"
            | Areaspline -> "areaspline"
            | Arearange -> "arearange"
            | Bar -> "bar"
            | Bubble -> "bubble"
            | Column -> "column"
            | Combination -> ""
            | Donut | Pie -> "pie"
            | Funnel -> "funnel"
            | Line | Radar -> "line"
            | Scatter -> "scatter"
            | Spline -> "spline"
        options._type <- chartTypeStr

    let setSeriesOptions (series:Series []) (options:HighchartsOptions) =
        let seriesOptions =
            [|
                for x in series do
                    let options = createEmpty<HighchartsSeriesOptions>()
                    options.data <- x.Values
                    options.name <- x.Name
                    
                    setSeriesChartType x.Type options
                    yield options
            |]
        options.series <- seriesOptions

    let setTooltipOptions pointFormat (options:HighchartsOptions) =
        match pointFormat with
        | None -> ()
        | Some value ->
            let tooltipOptions = createEmpty<HighchartsTooltipOptions>()
            tooltipOptions.pointFormat <- value
            options.tooltip <- tooltipOptions

    let setAreaMarker (areaChart:HighchartsAreaChart) =
        let marker = createEmpty<HighchartsMarker>()
        marker.enabled <- false
        marker.radius <- 2.
        let state = createEmpty<HighchartsMarkerState>()
        state.enabled <- true
        let states = createEmpty<AnonymousType1905>()
        states.hover <- state
        marker.states <- states
        areaChart.marker <- marker

    let setSubtitle subtitle (options:HighchartsOptions) =
        match subtitle with
        | None -> ()
        | Some value ->
            let subtitleOptions = createEmpty<HighchartsSubtitleOptions>()
            subtitleOptions.text <- value
            options.subtitle <- subtitleOptions
    
    let areaStacking stacking (areaChart:HighchartsAreaChart) =
        match stacking with
        | Disabled -> ()
        | Normal -> areaChart.stacking <- "normal"
        | Percent -> areaChart.stacking <- "percent"

open Utils

let private quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let seriesExpr = quoteSeriesArr series
    let chartTitleExpr = quoteStrOption chartTitle
    let legendExpr = quoteBool legend
    let categoriesExpr = quoteStringArr categories
    let xTitleExpr = quoteStrOption xTitle
    let yTitleExpr = quoteStrOption yTitle
    let pointFormatExpr = quoteStrOption pointFormat
    let subtitleExpr = quoteStrOption subtitle
    seriesExpr, chartTitleExpr, legendExpr, categoriesExpr, xTitleExpr, yTitleExpr, pointFormatExpr, subtitleExpr

[<ReflectedDefinition>]
let private areaChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle pointFormat subtitle stacking inverted =
    let options = createEmpty<HighchartsOptions>()
    areaChartOptions "chart" "area" inverted options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let areaChart = createEmpty<HighchartsAreaChart>()
    areaChart.showInLegend <- legend
    setAreaMarker areaChart
    areaStacking stacking areaChart
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.area <- areaChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let area series chartTitle legend categories xTitle yTitle pointFormat subtitle stacking inverted =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    let stackingExpr = quoteStacking stacking
    let invertedExpr = quoteBool inverted
    Compiler.Compiler.Compile(
        <@ areaChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 %%stackingExpr %%invertedExpr @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private areasplineChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle pointFormat subtitle stacking inverted =
    let options = createEmpty<HighchartsOptions>()
    areaChartOptions "chart" "areaspline" inverted options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let areaChart = createEmpty<HighchartsAreaChart>()
    areaChart.showInLegend <- legend
    setAreaMarker areaChart
    areaStacking stacking areaChart
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.areaspline <- areaChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let areaspline series chartTitle legend categories xTitle yTitle pointFormat subtitle stacking inverted =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    let stackingExpr = quoteStacking stacking
    let invertedExpr = quoteBool inverted
    Compiler.Compiler.Compile(
        <@ areasplineChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 %%stackingExpr %%invertedExpr @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private arearangeChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle pointFormat subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "arearange" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let arearangeChart = createEmpty<HighchartsAreaRangeChart>()
    arearangeChart.showInLegend <- legend
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.arearange <- arearangeChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    let tooltipOptions = createEmpty<HighchartsTooltipOptions>()
    match pointFormat with
    | None -> ()
    | Some value -> tooltipOptions.pointFormat <- value
    tooltipOptions.crosshairs <- true
    options.tooltip <- tooltipOptions

//    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let arearange series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ arearangeChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private barChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle stacking =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "bar" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let barChart = createEmpty<HighchartsBarChart>()
    barChart.showInLegend <- legend
    match stacking with
    | Disabled -> ()
    | Normal -> barChart.stacking <- "normal"
    | Percent -> barChart.stacking <- "percent"
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.bar <- barChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let bar series chartTitle legend categories xTitle yTitle pointFormat subtitle stacking =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    let stackingExpr = quoteStacking stacking
    Compiler.Compiler.Compile(
        <@ barChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 %%stackingExpr @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private bubbleChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    // chart options
    setChartOptions "chart" "bubble" options
    // x axis options
    setXAxisOptions series.[0].XType options categories xTitle
    // y axis options
    setYAxisOptions  options yTitle
    // title options
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setTooltipOptions pointFormat options    
    // series options
    setSeriesOptions series options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let bubble series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ bubbleChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private columnChart (series:Series []) chartTitle legend categories xTitle yTitle (pointFormat:string option) subtitle stacking =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "column" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let barChart = createEmpty<HighchartsBarChart>()
    barChart.showInLegend <- legend
    match stacking with
    | Disabled -> ()
    | Normal -> barChart.stacking <- "normal"
    | Percent -> barChart.stacking <- "percent"
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.column <- barChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let column series chartTitle legend categories xTitle yTitle pointFormat subtitle stacking =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    let stackingExpr = quoteStacking stacking
    Compiler.Compiler.Compile(
        <@ columnChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 %%stackingExpr @>,
        noReturn=true,
        shouldCompress=true)

[<JSEmitInline("{0}.center = {1}")>]
let pieCenter (options:HighchartsSeriesOptions) (arr:int []) : unit = failwith "never"

[<JSEmitInline("{0}.size = {1}")>]
let pieSize (options:HighchartsSeriesOptions) (size:obj) : unit = failwith "never"

[<JSEmitInline("{0}.showInLegend = false")>]
let disableLegend (options:HighchartsSeriesOptions) : unit = failwith "never"

[<JSEmitInline("{0}.dataLabels = {enabled: false}")>]
let disableLabels (options:HighchartsSeriesOptions) : unit = failwith "never"

[<ReflectedDefinition>]
let private combChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) (subtitle:string option) (pieOptions:PieOptions option)=
    let options = createEmpty<HighchartsOptions>()
    // x axis options
    setXAxisOptions series.[0].XType options categories xTitle
    // y axis options
    setYAxisOptions options yTitle
    // title options
    setTitleOptions chartTitle options
    setSubtitle subtitle options

//    let pieChart = createEmpty<HighchartsPieChart>()
//    pieChart.showInLegend <- false
//    pieChart.size <- 100
//    pieChart.center <- [|"100"; "80"|]
//    let dataLabels = createEmpty<HighchartsDataLabels>()
//    dataLabels.enabled <- false
//    pieChart.dataLabels <- dataLabels
//    let plotOptions = createEmpty<HighchartsPlotOptions>()
//    plotOptions.pie <- pieChart
//    options.plotOptions <- plotOptions

    let seriesOptions =
        [|
            for x in series do
                let options = createEmpty<HighchartsSeriesOptions>()
                options.data <- x.Values
                options.name <- x.Name
                match x.Type with
                | Pie ->
                    match pieOptions with
                    | None -> ()
                    | Some value ->
                        pieCenter options value.Center
                        pieSize options value.Size
                        disableLegend options
                        disableLabels options
                | _ -> ()    
                setSeriesChartType x.Type options
                yield options
        |]
    options.series <- seriesOptions


    // series options
//    setSeriesOptions series options

    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let comb series chartTitle legend categories xTitle yTitle pointFormat subtitle pieOptions =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    let pieOptionsExpr = quotePieOptions pieOptions
    Compiler.Compiler.Compile(
        <@ combChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 %%pieOptionsExpr @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private donutChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "pie" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let pieChart = createEmpty<HighchartsPieChart>()
    pieChart.showInLegend <- legend
    pieChart.innerSize <- "50%"
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.pie <- pieChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let donut series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ donutChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private funnelChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "funnel" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
//    let pieChart = createEmpty<Highcharts PieChart>()
//    pieChart.showInLegend <- legend
//    let plotOptions = createEmpty<HighchartsPlotOptions>()
//    plotOptions.pie <- pieChart
//    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let funnel series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ funnelChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private lineChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "line" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let lineChart = createEmpty<HighchartsLineChart>()
    lineChart.showInLegend <- legend
//    match stacking with
//    | Disabled -> ()
//    | Normal -> barChart.stacking <- "normal"
//    | Percent -> barChart.stacking <- "percent"
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.line <- lineChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let line series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ lineChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private pieChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "pie" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let pieChart = createEmpty<HighchartsPieChart>()
    pieChart.showInLegend <- legend
//    match stacking with
//    | Disabled -> ()
//    | Normal -> barChart.stacking <- "normal"
//    | Percent -> barChart.stacking <- "percent"
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.pie <- pieChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let pie series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ pieChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<JSEmitInline("{0}.gridLineInterpolation= 'polygon'")>]
let polygon(options:HighchartsAxisOptions) : unit = failwith "never"

[<ReflectedDefinition>]
let private radarChart (series:Series []) chartTitle (legend:bool) (categories:string []) xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    let chartOptions = createEmpty<HighchartsChartOptions>()
    chartOptions.renderTo <- "chart"
    chartOptions._type <- "line"
    chartOptions.polar <- true
    options.chart <- chartOptions
//    setChartOptions "chart" "line" options
//    setXAxisOptions series.[0].XType options categories xTitle
    let axisOptions = createEmpty<HighchartsAxisOptions>()
    let xAxisType =
        match categories.Length with
        | 0 ->
            match series.[0].XType with
            | TypeCode.DateTime -> "datetime"
            | TypeCode.String -> "category"
            | _ -> "linear"
        | _ ->
            axisOptions.categories <- categories
            "category"
    axisOptions._type <- xAxisType
    let axisTitle = createEmpty<HighchartsAxisTitle>()
    axisTitle.text <- defaultArg xTitle ""
    axisOptions.title <- axisTitle
    axisOptions.lineWidth <- 0.
    axisOptions.tickmarkPlacement <- "on"
    options.xAxis <- axisOptions


    let yAxisOptions = createEmpty<HighchartsAxisOptions>()
    let yAxisTitle = createEmpty<HighchartsAxisTitle>()
    yAxisTitle.text <- defaultArg yTitle ""
    yAxisOptions.title <- yAxisTitle
    yAxisOptions.lineWidth <- 0.
    yAxisOptions.min <- 0.
    polygon yAxisOptions
    options.yAxis <- yAxisOptions

//    setYAxisOptions options yTitle
    let lineChart = createEmpty<HighchartsLineChart>()
    lineChart.showInLegend <- legend
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.line <- lineChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let radar series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ radarChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private scatterChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "scatter" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let scatterChart = createEmpty<HighchartsScatterChart>()
    scatterChart.showInLegend <- legend
//    match stacking with
//    | Disabled -> ()
//    | Normal -> barChart.stacking <- "normal"
//    | Percent -> barChart.stacking <- "percent"
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.scatter <- scatterChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let scatter series chartTitle legend categories xTitle yTitle pointFormat subtitle  =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ scatterChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)

[<ReflectedDefinition>]
let private splineChart (series:Series []) chartTitle (legend:bool) categories xTitle yTitle (pointFormat:string option) subtitle =
    let options = createEmpty<HighchartsOptions>()
    setChartOptions "chart" "spline" options
    setXAxisOptions series.[0].XType options categories xTitle
    setYAxisOptions options yTitle
    let splineChart = createEmpty<HighchartsSplineChart>()
    splineChart.showInLegend <- legend
    let plotOptions = createEmpty<HighchartsPlotOptions>()
    plotOptions.spline <- splineChart
    options.plotOptions <- plotOptions
    setTitleOptions chartTitle options
    setSubtitle subtitle options
    setSeriesOptions series options
    setTooltipOptions pointFormat options
    let chartElement = Utils.jq "#chart"
    chartElement.highcharts(options) |> ignore

let spline series chartTitle legend categories xTitle yTitle pointFormat subtitle =
    let expr1, expr2, expr3, expr4, expr5, expr6, expr7, expr8 =
        quoteArgs series chartTitle legend categories xTitle yTitle pointFormat subtitle
    Compiler.Compiler.Compile(
        <@ splineChart %%expr1 %%expr2 %%expr3 %%expr4 %%expr5 %%expr6 %%expr7 %%expr8 @>,
        noReturn=true,
        shouldCompress=true)
