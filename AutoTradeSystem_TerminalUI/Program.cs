using Terminal.Gui;
using NStack;
using AutoTradeSystem_TerminalUI.Services;
using AutoTradeSystem_TerminalUI.Interfaces;
using PricingSystem.Protos;
using System.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
ConfigureServices(builder);
var host = builder.Build();
await host.StartAsync();

var pricingService = host.Services.GetRequiredService<IPricingService>();

Application.Init();
var theme = GetGreenOnBlackTheme();

var priceSource = GetPriceTable();

var strategySource = GetInitialStrategyData();

var tickerSelect = new ComboBox() { X = 1, Y = 2, Width = Dim.Fill(1), Height = 5, Source = new ListWrapper(new ustring[0]) };
var quantityInput = new TextField("") { X = 1, Y = 5, Width = Dim.Fill(1) };
var priceInput = new TextField("") { X = 1, Y = 12, Width = Dim.Fill(1) };
var actionSelect = new RadioGroup(new ustring[] { "Buy", "Sell" }) { X = 1, Y = 8 };
var priceTable = CreateTableView(priceSource, theme);
var strategyTable = CreateTableView(strategySource, theme);


var leftPane = CreateOrderPane(tickerSelect, quantityInput, actionSelect, priceInput);
var middlePane = new FrameView("MARKET PRICES") { X = Pos.Right(leftPane), Y = 0, Width = Dim.Percent(20), Height = Dim.Fill() };
var rightPane = new FrameView("CURRENT STRATEGIES") { X = Pos.Right(middlePane), Y = 0, Width = Dim.Percent(55), Height = Dim.Fill() };

middlePane.Add(priceTable);
rightPane.Add(strategyTable);

var top = new Window() { Title = "TRADING SYSTEM TUI", ColorScheme = theme };
top.Add(leftPane, middlePane, rightPane);

SetupInputValidation(quantityInput, priceInput);
SetupLayoutHandling(top, priceTable, priceSource, strategyTable, strategySource);
SetupUpdateLoop(pricingService, priceSource, priceTable, tickerSelect);

Application.Run(top);
Application.Shutdown();

void ConfigureServices(HostApplicationBuilder builder) {
    builder.Services.AddSingleton<PricingService>();
    builder.Services.AddSingleton<IPricingService>(p => p.GetRequiredService<PricingService>());
    builder.Services.AddHostedService(p => p.GetRequiredService<PricingService>());
    builder.Logging.ClearProviders();
    builder.Services.AddGrpcClient<GrpcPricingService.GrpcPricingServiceClient>(o => o.Address = new Uri("https://localhost:7001"))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
}

ColorScheme GetGreenOnBlackTheme() => new ColorScheme() {
    Normal = Terminal.Gui.Attribute.Make(Color.Green, Color.Black),
    Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.Green),
    HotNormal = Terminal.Gui.Attribute.Make(Color.BrightGreen, Color.Black),
    HotFocus = Terminal.Gui.Attribute.Make(Color.BrightGreen, Color.Green),
    Disabled = Terminal.Gui.Attribute.Make(Color.DarkGray, Color.Black)
};

FrameView CreateOrderPane(ComboBox ticker, TextField qty, RadioGroup actions, TextField price) {
    var pane = new FrameView("NEW ORDER") { X = 0, Y = 0, Width = Dim.Percent(25), Height = Dim.Fill() };
    var submitBtn = new Button("SUBMIT") { X = Pos.Center(), Y = Pos.AnchorEnd(2) };

    submitBtn.Clicked += () => {
        if (int.TryParse(qty.Text.ToString(), out int q) && decimal.TryParse(price.Text.ToString(), out decimal p)) {
            MessageBox.Query("Order Sent", $"Target: {ticker.Text}\n{actions.RadioLabels[actions.SelectedItem]} {q} @ {p}", "Ok");
        } else {
            MessageBox.ErrorQuery("Error", "Invalid Quantity or Price", "Ok");
        }
    };

    pane.Add(new Label("Ticker:") { X = 1, Y = 1 }, ticker,
             new Label("Quantity:") { X = 1, Y = 4 }, qty,
             new Label("TradeAction:") { X = 1, Y = 7 }, actions,
             new Label("ActionPrice ($):") { X = 1, Y = 11 }, price, submitBtn);
    return pane;
}

TableView CreateTableView(DataTable source, ColorScheme theme) {
    return new TableView() {
        X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
        Table = source,
        ColorScheme = theme,
        Style = new TableView.TableStyle { AlwaysShowHeaders = true, ShowHorizontalHeaderUnderline = true }
    };
}

DataTable GetPriceTable() {
    var dt = new DataTable();
    dt.Columns.Add("Ticker", typeof(string));
    dt.Columns.Add("Price ($)", typeof(string));
    return dt;
}

DataTable GetInitialStrategyData() {
    var dt = new DataTable();
    dt.Columns.Add("Ticker", typeof(string));
    dt.Columns.Add("Action", typeof(string));
    dt.Columns.Add("Quantity", typeof(int));
    dt.Columns.Add("ActionPrice ($)", typeof(decimal));
    //Add in Dummy Data for testing purposes
    dt.Rows.Add("IBM", "Buy", 12, 175.50m);
    dt.Rows.Add("AAPL", "Buy", 10, 230.50m);
    dt.Rows.Add("AMZN", "Sell", 5, 245.10m);
    return dt;
}

void SetupInputValidation(TextField qty, TextField price) {
    qty.KeyPress += (e) => e.Handled = !char.IsDigit((char)e.KeyEvent.Key) && !IsNavKey(e.KeyEvent.Key);
    price.KeyPress += (e) => {
        char c = (char)e.KeyEvent.Key;
        bool isDot = c == '.' && !price.Text.ToString().Contains(".");
        e.Handled = !char.IsDigit(c) && !isDot && !IsNavKey(e.KeyEvent.Key);
    };
}

bool IsNavKey(Key key) => key == Key.Backspace || key == Key.Delete || key == Key.CursorLeft || key == Key.CursorRight;

void SetupLayoutHandling(Window top, TableView pTab, DataTable pSrc, TableView sTab, DataTable sSrc) {
    int margin = 4;
    top.LayoutComplete += (e) => {
        AdjustColumnWidths(pTab, pSrc, margin, 2);
        AdjustColumnWidths(sTab, sSrc, margin, 4);
    };
}

void AdjustColumnWidths(TableView table, DataTable source, int margin, int colCount) {
    int available = table.Bounds.Width - margin;
    if (available <= 0) return;
    int colWidth = available / colCount;
    table.Style.ColumnStyles.Clear();
    foreach (DataColumn col in source.Columns) {
        table.Style.ColumnStyles.Add(col, new TableView.ColumnStyle { MinWidth = colWidth, MaxWidth = colWidth, Alignment = TextAlignment.Centered });
    }
}

void SetupUpdateLoop(IPricingService service, DataTable pSrc, TableView pTab, ComboBox tickerSelect) {
    Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(250), (_) => {
        var freshPrices = service.GetLatestPrices();
        bool changed = false;
        foreach (var kvp in freshPrices) {
            var row = pSrc.AsEnumerable().FirstOrDefault(r => r.Field<string>("Ticker") == kvp.Key);
            string newVal = kvp.Value.ToString("N2");
            if (row != null) {
                if (row["Price ($)"].ToString() != newVal) { row["Price ($)"] = newVal; changed = true; }
            } else { pSrc.Rows.Add(kvp.Key, newVal); changed = true; }
        }
        if (changed) pTab.SetNeedsDisplay();

        var tickers = service.GetLatestTickers().Select(t => (ustring)t).ToList();
        if (tickers.Count != tickerSelect.Source.Count) {
            tickerSelect.Source = new ListWrapper(tickers);
            tickerSelect.SetNeedsDisplay();
        }
        return true;
    });
}
