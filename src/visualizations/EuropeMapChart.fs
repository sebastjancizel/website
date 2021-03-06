[<RequireQualifiedAccess>]
module EuropeMap

open System
open System.Text
open Feliz
open Elmish
open Feliz.ElmishComponents
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Browser

open Highcharts
open Types
open Data.OurWorldInData

type MapToDisplay = Europe | World

let europeGeoJsonUrl = "/maps/europe.geo.json"
let worldGeoJsonUrl = "/maps/world-robinson.geo.json"

type GeoJson = RemoteData<obj, string>

type OwdData = Data.OurWorldInData.OurWorldInDataRemoteData

type CountryData =
    { Country: string
      // OWD data
      TwoWeekIncidence1M: float
      TwoWeekIncidence: float []
      TwoWeekIncidenceMaxValue: float
      NewCases: int list
      OwdDate: DateTime
      // NIJZ data
      RestrictionColor: string
      RestrictionText: string
      RestrictionAltText: string
      ImportedFrom: int
      ImportedDate: DateTime }

type CountriesMap = Map<string, CountryData>

type ChartType =
    | TwoWeekIncidence
    | Restrictions

    override this.ToString() =
        match this with
        | TwoWeekIncidence -> I18N.t "charts.europe.twoWeekIncidence"
        | Restrictions -> I18N.t "charts.europe.restrictions"

type State =
    { MapToDisplay : MapToDisplay
      Countries : CountrySelection
      GeoJson: GeoJson
      OwdData: OwdData
      CountryData: CountriesMap
      ChartType: ChartType }

type Msg =
    | GeoJsonRequested
    | GeoJsonLoaded of GeoJson
    | OwdDataRequested
    | OwdDataReceived of OwdData
    | ChartTypeChanged of ChartType

let worldCountries =
    [ "AFG" ; "ALB" ; "DZA" ; "ASM" ; "AND" ; "AGO" ; "AIA" ; "ATA" ; "ATG" ; "ARG" ; "ARM" ; "ABW" ; "AUS" ; "AUT" ; "AZE" ; "BHS" ; "BHR" ; "BGD" ; "BRB" ; "BLR" ; "BEL" ; "BLZ" ; "BEN" ; "BMU" ; "BTN" ; "BOL" ; "BES" ; "BIH" ; "BWA" ; "BVT" ; "BRA" ; "IOT" ; "BRN" ; "BGR" ; "BFA" ; "BDI" ; "CPV" ; "KHM" ; "CMR" ; "CAN" ; "CYM" ; "CAF" ; "TCD" ; "CHL" ; "CHN" ; "CXR" ; "CCK" ; "COL" ; "COM" ; "COD" ; "COG" ; "COK" ; "CRI" ; "HRV" ; "CUB" ; "CUW" ; "CYP" ; "CZE" ; "CIV" ; "DNK" ; "DJI" ; "DMA" ; "DOM" ; "ECU" ; "EGY" ; "SLV" ; "GNQ" ; "ERI" ; "EST" ; "SWZ" ; "ETH" ; "FLK" ; "FRO" ; "FJI" ; "FIN" ; "FRA" ; "GUF" ; "PYF" ; "ATF" ; "GAB" ; "GMB" ; "GEO" ; "DEU" ; "GHA" ; "GIB" ; "GRC" ; "GRL" ; "GRD" ; "GLP" ; "GUM" ; "GTM" ; "GGY" ; "GIN" ; "GNB" ; "GUY" ; "HTI" ; "HMD" ; "VAT" ; "HND" ; "HKG" ; "HUN" ; "ISL" ; "IND" ; "IDN" ; "IRN" ; "IRQ" ; "IRL" ; "IMN" ; "ISR" ; "ITA" ; "JAM" ; "JPN" ; "JEY" ; "JOR" ; "KAZ" ; "KEN" ; "KIR" ; "PRK" ; "KOR" ; "KWT" ; "KGZ" ; "LAO" ; "LVA" ; "LBN" ; "LSO" ; "LBR" ; "LBY" ; "LIE" ; "LTU" ; "LUX" ; "MAC" ; "MDG" ; "MWI" ; "MYS" ; "MDV" ; "MLI" ; "MLT" ; "MHL" ; "MTQ" ; "MRT" ; "MUS" ; "MYT" ; "MEX" ; "FSM" ; "MDA" ; "MCO" ; "MNG" ; "MNE" ; "MSR" ; "MAR" ; "MOZ" ; "MMR" ; "NAM" ; "NRU" ; "NPL" ; "NLD" ; "NCL" ; "NZL" ; "NIC" ; "NER" ; "NGA" ; "NIU" ; "NFK" ; "MNP" ; "NOR" ; "OMN" ; "PAK" ; "PLW" ; "PSE" ; "PAN" ; "PNG" ; "PRY" ; "PER" ; "PHL" ; "PCN" ; "POL" ; "PRT" ; "PRI" ; "QAT" ; "MKD" ; "ROU" ; "RUS" ; "RWA" ; "REU" ; "BLM" ; "SHN" ; "KNA" ; "LCA" ; "MAF" ; "SPM" ; "VCT" ; "WSM" ; "SMR" ; "STP" ; "SAU" ; "SEN" ; "SRB" ; "SYC" ; "SLE" ; "SGP" ; "SXM" ; "SVK" ; "SVN" ; "SLB" ; "SOM" ; "ZAF" ; "SGS" ; "SSD" ; "ESP" ; "LKA" ; "SDN" ; "SUR" ; "SJM" ; "SWE" ; "CHE" ; "SYR" ; "TWN" ; "TJK" ; "TZA" ; "THA" ; "TLS" ; "TGO" ; "TKL" ; "TON" ; "TTO" ; "TUN" ; "TUR" ; "TKM" ; "TCA" ; "TUV" ; "UGA" ; "UKR" ; "ARE" ; "GBR" ; "UMI" ; "USA" ; "URY" ; "UZB" ; "VUT" ; "VEN" ; "VNM" ; "VGB" ; "VIR" ; "WLF" ; "ESH" ; "YEM" ; "ZMB" ; "ZWE" ; "ALA" ; "XKX"]

let euCountries =
    [ "ALB"
      "AND"
      "AUT"
      "BLR"
      "BEL"
      "BIH"
      "BGR"
      "HRV"
      "CYP"
      "CZE"
      "DNK"
      "EST"
      "FRO"
      "FIN"
      "FRA"
      "DEU"
      "GRC"
      "HUN"
      "ISL"
      "IRL"
      "ITA"
      "LVA"
      "LIE"
      "LTU"
      "LUX"
      "MKD"
      "MLT"
      "MDA"
      "MCO"
      "MNE"
      "NLD"
      "NOR"
      "POL"
      "PRT"
      "SRB"
      "ROU"
      "RUS"
      "SMR"
      "SVK"
      "SVN"
      "ESP"
      "SWE"
      "CHE"
      "TUR"
      "UKR"
      "GBR"
      "VAT"
      "XKX"
      "NCY"
      "NMA" ]

let greenCountries =
    Map.ofList
        [ 
            ("AUT", "")
            ("CYP", "")
            ("FIN", "")
            ("KOR", "")
            ("LVA", "")
            ("LIE", "")
            ("LTU", "")
            ("NZL", "")
            ("POL", "")
            ("SRB", "")
            ("URY", "")
        ]

let redCountries =
    Map.ofList
        [ 
            ("AFG", "")
            ("ALB", "")
            ("DZA", "")
            ("AND", "")
            ("AGO", "")
            ("ARG", "")
            ("ARM", "")
            ("AUT", "administrativne enote: Dunaj, Predarlska, Tirolska")
            ("AZE", "")
            ("BAH", "")
            ("BHR", "")
            ("BGD", "")
            ("BEL", "administrativna enota: Bruselj")
            ("BLZ", "")
            ("BLR", "")
            ("BEN", "")
            ("BGR", "administrativna enota: Blagoevgrad")
            ("BOL", "")
            ("BIH", "")
            ("BRA", "")
            ("BFA", "")
            ("BDI", "")
            ("BTN", "")
            ("TCD", "")
            ("CZE", "")
            ("CHL", "")
            ("MNE", "")
            ("DNK", "administrativna enota: Hovedstaden, regija glavnega mesta")
            ("DOM", "")
            ("EGY", "")
            ("ECU", "")
            ("GNQ", "")
            ("ERI", "")
            ("SWZ", "")
            ("ETH", "")
            ("PHL", "")
            ("FRA", "administrativna enota: Auvergne-Rona-Alpe, Bretanja, Center-Val de Loire, Korzika, Hauts-de-France, Île-de-France, Normandija, Nova Akvitanija, Oksitanija, Provansa-Alpe-Azurna obala; čezmorsko ozemlje: Francoska Gvajana, Guadeloupe, Sveti Martin, La Réunion")
            ("GAB", "")
            ("GMB", "")
            ("GHA", "")
            ("GUY", "")
            ("GTM", "")
            ("GIN", "")
            ("GNB", "")
            ("HTI", "")
            ("HND", "")
            ("HRV", "administrativne enote: Brodsko-posavska, Dubrovniško-neretvanska, Liško-senjska, Požeško-slavonska, Šibensko-kninska, Splitsko-dalmatinska, Virovitiško-podravska, Zadarska")
            ("IND", "")
            ("IDN", "")
            ("IRQ", "")
            ("IRN", "")
            ("IRL", "administrativna enota: Dublin")
            ("ISR", "")
            ("JAM", "")
            ("YEM", "")
            ("ZAF", "")
            ("SSD", "")
            ("CMR", "")
            ("QAT", "")
            ("KAZ", "")
            ("KEN", "")
            ("KGZ", "")
            ("COL", "")
            ("COM", "")
            ("COG", "")
            ("COD", "")
            ("XKX", "")
            ("CRI", "")
            ("KWT", "")
            ("LSO", "")
            ("LBN", "")
            ("LBR", "")
            ("LBY", "")
            ("LUX", "")
            ("MDG", "")
            ("HUN", "administrativne enote: Budimpešta, Győr-Moson-Sopron")
            ("MWL", "")
            ("MDV", "")
            ("MLI", "")
            ("MAR", "")
            ("MRT", "")
            ("MEX", "")
            ("MDA", "")
            ("MNG", "")
            ("MOZ", "")
            ("NAM", "")
            ("NPL", "")
            ("NIG", "")
            ("NGA", "")
            ("NIC", "")
            ("NLD", "administrativna enote: Severna Nizozemska, Južna Nizozemska, Utrecht; čezmorsko ozemlje: Aruba, Saint Maarten")
            ("OMN", "")
            ("PAK", "")
            ("PAN", "")
            ("PNG", "")
            ("PRY", "")
            ("PER", "")
            ("PRT", "administrativna enota: Lizbona")
            ("ROU", "administrativne enote: Bacău, Bihor, Brăila, Brașov, Bukarešta, Caras Severin, Covasna, Neamt, Iasi, Ilfov, Prahova, Vâlcea, Vaslui")
            ("RUS", "")
            ("SLV", "")
            ("STP", "")
            ("SAU", "")
            ("SEN", "")
            ("PRK", "")
            ("MKD", "")
            ("SLE", "")
            ("SYR", "")
            ("CIV", "")
            ("SOM", "")
            ("CAF", "")
            ("SUR", "")
            ("ESP", "")
            ("CHE", "administrativne enote: Fribourg, Ženeva, Vaud")
            ("TJK", "")
            ("TZA", "")
            ("TGO", "")
            ("TTO", "")
            ("TUR", "")
            ("TKM", "")
            ("UKR", "")
            ("UZB", "")
            ("VEN", "")
            ("TLS", "")
            ("ZMB", "")
            ("USA", "")
            ("ARE", "")
            ("GBR", "čezmorsko ozemlje Gibraltar")
            ("CPV", "")
            ("ZWE", "")
        ]

let importedFrom =
    Map.ofList
        [  
            ("ITA", 6)
            ("HRV", 4)
            ("AUT", 4)
            ("BIH", 3)
            ("DEU", 3)
            ("FRA", 3) 
            ("XKX", 2)
            ("SRB", 2)
            ("RUS", 1)
            ("HUN", 1)
            ("CZE", 1)
            ("MNE", 1)
            ("ESP", 1)
            ("EST", 1)
            ("BGR", 1)
            ("CHE", 1)
            ("TUR", 1)
            ("GBR", 1)
        ]

let importedDate = DateTime(2020, 9, 27)

let loadEuropeGeoJson =
    async {
        let! (statusCode, response) = Http.get europeGeoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded
                       (sprintf "Error loading map: %d" statusCode
                        |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded(data |> Success)
            with ex ->
                return GeoJsonLoaded
                           (sprintf "Error loading map: %s" ex.Message
                            |> Failure)
    }

let loadWorldGeoJson =
    async {
        let! (statusCode, response) = Http.get worldGeoJsonUrl

        if statusCode <> 200 then
            return GeoJsonLoaded
                       (sprintf "Error loading map: %d" statusCode
                        |> Failure)
        else
            try
                let data = response |> Fable.Core.JS.JSON.parse
                return GeoJsonLoaded(data |> Success)
            with ex ->
                return GeoJsonLoaded
                           (sprintf "Error loading map: %s" ex.Message
                            |> Failure)
    }

let init (mapToDisplay: MapToDisplay): State * Cmd<Msg> =
    let cmdGeoJson = Cmd.ofMsg GeoJsonRequested
    let cmdOwdData = Cmd.ofMsg OwdDataRequested
    { MapToDisplay = mapToDisplay
      Countries =
        match mapToDisplay with
        | Europe -> CountrySelection.Countries euCountries
        | World -> All
      GeoJson = NotAsked
      OwdData = NotAsked
      CountryData = Map.empty
      ChartType =
        match mapToDisplay with
        | Europe -> Restrictions
        | World -> TwoWeekIncidence },
    (cmdGeoJson @ cmdOwdData)

let prepareCountryData (data: Data.OurWorldInData.DataPoint list) =
    data
    |> List.groupBy (fun dp -> dp.CountryCode)
    |> List.map (fun (code, dps) ->
        let fixedCode =
            if code = "OWID_KOS" then "XKX" else code // hack for Kosovo code

        let country = I18N.tt "country" code // TODO: change country code in i18n for Kosovo

        let incidence1M =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.sum

        let incidence =
            dps
            |> List.map (fun dp -> dp.NewCasesPerMillion)
            |> List.choose id
            |> List.toArray

        let incidenceMaxValue =
            if incidence.Length = 0
            then 0.
            else incidence |> Array.toList |> List.max

        let newCases = dps |> List.map (fun dp -> dp.NewCases)

        let owdDate =
            dps |> List.map (fun dp -> dp.Date) |> List.max

        let red, green =
            redCountries.TryFind(fixedCode),
            greenCountries.TryFind(fixedCode)
        let rText, rColor, rAltText =
            match fixedCode with
            | "SVN" -> I18N.t "charts.europe.statusNone", "#10829a", ""
            | _ -> 
                match red with
                | Some redNote -> 
                    if redNote.Length > 0
                    then I18N.t "charts.europe.statusRed", "#FF9057", redNote
                    else I18N.t "charts.europe.statusRed", "#FF5348", redNote
                | _ ->
                    match green with
                    | Some greenNote -> I18N.t "charts.europe.statusGreen", "#C4DE6F", greenNote
                    | _ -> I18N.t "charts.europe.statusOrange", "#FFC65A", ""

        let imported =
            importedFrom.TryFind(fixedCode)
            |> Option.defaultValue 0

        let cd: CountryData =
            { CountryData.Country = country
              CountryData.TwoWeekIncidence1M = incidence1M
              CountryData.TwoWeekIncidence = incidence
              CountryData.TwoWeekIncidenceMaxValue = incidenceMaxValue
              CountryData.NewCases = newCases
              CountryData.OwdDate = owdDate
              CountryData.RestrictionColor = rColor
              CountryData.RestrictionText = rText 
              CountryData.RestrictionAltText = rAltText
              CountryData.ImportedFrom = imported
              CountryData.ImportedDate = importedDate }

        (fixedCode, cd))
    |> Map.ofList

let update (msg: Msg) (state: State): State * Cmd<Msg> =

    let owdCountries =
        match state.Countries with
        | All ->
            All
        | Countries countries ->
            countries
            |> List.map (fun code -> if code = "XKX" then "OWID_KOS" else code) // hack for Kosovo code
            |> Countries

    match msg with
    | GeoJsonRequested ->
        let cmd =
            match state.MapToDisplay with
            | Europe -> Cmd.OfAsync.result loadEuropeGeoJson
            | World -> Cmd.OfAsync.result loadWorldGeoJson
        { state with GeoJson = Loading }, cmd
    | GeoJsonLoaded geoJson -> { state with GeoJson = geoJson }, Cmd.none
    | OwdDataRequested ->
        let twoWeeksAgo = DateTime.Today.AddDays(-14.0)
        { state with OwdData = Loading },
        Cmd.OfAsync.result (Data.OurWorldInData.loadCountryIncidence owdCountries twoWeeksAgo OwdDataReceived)
    | OwdDataReceived result ->
        let ret =
            match result with
            | Success owdData ->
                { state with
                      OwdData = result
                      CountryData = prepareCountryData owdData }
            | _ -> { state with OwdData = result }

        ret, Cmd.none
    | ChartTypeChanged chartType -> { state with ChartType = chartType }, Cmd.none


let mapData state =
    let countries =
        match state.Countries with
        | Countries countries -> countries
        | All -> worldCountries

    countries
    |> List.map (fun code ->
        match state.CountryData.TryFind(code) with
        | Some cd ->
            let incidence1M = cd.TwoWeekIncidence1M |> int
            let incidence = cd.TwoWeekIncidence
            let incidenceMaxValue = cd.TwoWeekIncidenceMaxValue

            let nc =
                cd.NewCases
                |> List.tryLast
                |> Option.defaultValue 0

            let ncDate =
                (I18N.tOptions "days.date" {| date = cd.OwdDate |})

            let impDate =
                (I18N.tOptions "days.date" {| date = cd.ImportedDate |})

            let baseRec =
                {| code = code
                   country = cd.Country
                   incidence1M = incidence1M
                   incidence = incidence
                   incidenceMaxValue = incidenceMaxValue
                   newCases = nc
                   ncDate = ncDate
                   rType = cd.RestrictionText
                   rAltText = cd.RestrictionAltText
                   imported = cd.ImportedFrom
                   impDate = impDate |}

            match state.ChartType with
            | TwoWeekIncidence ->
                {| baseRec with
                       value = incidence1M
                       color = null
                       dataLabels = {| enabled = false |} |}
            | Restrictions ->
                {| baseRec with
                       value = cd.ImportedFrom
                       color = cd.RestrictionColor
                       dataLabels = {| enabled = cd.ImportedFrom > 0 |} |}
        | _ ->
            {| code = code
               country = ""
               value = 0
               color = null
               dataLabels = {| enabled = false |}
               incidence1M = 0
               incidence = null
               incidenceMaxValue = 0.0
               newCases = 0
               ncDate = ""
               rType = ""
               rAltText = ""
               imported = 0
               impDate = "" |})
    |> List.toArray

let renderMap state geoJson owdData =

    let legend =
        let enabled = state.ChartType = TwoWeekIncidence
        {| enabled = enabled
           title = {| text = null |}
           align = "left"
           verticalAlign = "bottom"
           layout = "vertical"
           floating = true
           borderWidth = 1
           backgroundColor = "white"
           valueDecimals = 0 |}
        |> pojo

    let colorAxis =
        {| dataClassColor = "category"
           dataClasses =
               [| {| from = 0; color = "#ffffcc" |}
                  {| from = 25; color = "#ffeda0" |}
                  {| from = 50; color = "#fed976" |}
                  {| from = 100; color = "#feb24c" |}
                  {| from = 200; color = "#fd8d3c" |}
                  {| from = 400; color = "#fc4e2a" |}
                  {| from = 800; color = "#e31a1c" |}
                  {| from = 1600; color = "#b10026" |} |] |}
        |> pojo

    let tooltipFormatter jsThis =
        let points = jsThis?point
        let twoWeekIncidence = points?incidence
        let twoWeekIncidenceMaxValue = Math.Ceiling(float points?incidenceMaxValue)
        let country = points?country
        let incidence1M = points?incidence1M
        let newCases = points?newCases
        let ncDate = points?ncDate
        let imported = points?imported
        let impDate = points?impDate
        let rType = points?rType
        let rAltText = points?rAltText

        let s = StringBuilder()
        let barMaxHeight = 50

        let textHtml =
            sprintf "<b>%s</b><br/>
            %s: <b>%s<br/>%s</br></br></b>
            %s: <b>%s</b> (%s)<br/><br/>
            %s: <b>%s</b><br/>
            %s: <b>%s</b> (%s)<br/>"
                country
                (I18N.t "charts.europe.countryStatus") rType rAltText
                (I18N.t "charts.europe.importedCases") imported impDate
                (I18N.t "charts.europe.incidence1M") incidence1M
                (I18N.t "charts.europe.newCases") newCases ncDate

        s.Append textHtml |> ignore

        s.Append "<div class='bars'>" |> ignore

        match twoWeekIncidence with
        | null -> I18N.t "charts.europe.noData"
        | _ ->
            twoWeekIncidence
            |> Array.iter (fun country ->
                let barHeight = Math.Ceiling(float country * float barMaxHeight / twoWeekIncidenceMaxValue)
                let barHtml = sprintf "<div class='bar-wrapper'><div class='bar' style='height: %Apx'></div></div>" (int barHeight)
                s.Append barHtml |> ignore)
            s.Append "</div>" |> ignore
            s.ToString()

    let series geoJson =
        {| visible = true
           mapData = geoJson
           data = mapData state
           joinBy = [| "iso-a3"; "code" |]
           nullColor = "white"
           borderColor = "#888"
           borderWidth = 0.5
           mapline = {| animation = {| duration = 0 |} |}
           states =
               {| normal = {| animation = {| duration = 0 |} |}
                  hover =
                      {| borderColor = "black"
                         animation = {| duration = 0 |} |} |} |}
        |> pojo

    {| optionsWithOnLoadEvent "covid19-europe-map" with
           title = null
           series = [| series geoJson |]
           colorAxis = colorAxis
           legend = legend
           tooltip =
               pojo
                   {| formatter = fun () -> tooltipFormatter jsThis
                      useHTML = true
                      distance = 50 |}
           credits =
               pojo
                   {| enabled = true
                      text =
                          sprintf "%s: %s, %s" (I18N.t "charts.common.dataSource") (I18N.t "charts.common.dsNIJZ")
                              (I18N.t "charts.common.dsOWD")
                      mapTextFull = ""
                      mapText = ""
                      href = "https://ourworldindata.org/coronavirus" |} |}
    |> map


let renderChartTypeSelectors (activeChartType: ChartType) dispatch =
    let renderChartSelector (chartSelector: ChartType) =
        let active = chartSelector = activeChartType
        Html.div [
            prop.onClick (fun _ -> dispatch chartSelector)
            Utils.classes
                [(true, "chart-display-property-selector__item")
                 (active, "selected")]
            prop.text (chartSelector.ToString())
        ]

    Html.div
        [ prop.className "chart-display-property-selector"
          prop.children
              [ Html.text (I18N.t "charts.common.view")
                renderChartSelector Restrictions
                renderChartSelector TwoWeekIncidence ] ]


let render (state: State) dispatch =

    let chart =
        match state.GeoJson, state.OwdData with
        | Success geoJson, Success owdData -> renderMap state geoJson owdData
        | Failure err, _ -> Utils.renderErrorLoading err
        | _, Failure err -> Utils.renderErrorLoading err
        | _ -> Utils.renderLoading

    Html.div
        [ prop.children
            [ Utils.renderChartTopControls [ renderChartTypeSelectors state.ChartType (ChartTypeChanged >> dispatch) ]
              Html.div
                  [ prop.style [ style.height 550 ]
                    prop.className "map"
                    prop.children [ chart ] ] ] ]

let mapChart (mapToDisplay: MapToDisplay) =
    React.elmishComponent ("EuropeMapChart", init mapToDisplay, update, render)
