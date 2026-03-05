using Terminal.Gui;

Application.Init();

var greenOnBlack = new ColorScheme() {
    Normal = Terminal.Gui.Attribute.Make(Color.Green, Color.Black),
    Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.Green),
    HotNormal = Terminal.Gui.Attribute.Make(Color.BrightGreen, Color.Black),
    HotFocus = Terminal.Gui.Attribute.Make(Color.BrightGreen, Color.Green),
    Disabled = Terminal.Gui.Attribute.Make(Color.DarkGray, Color.Black)
};

var top = new Window() { Title = "TRADING SYSTEM TUI", ColorScheme = greenOnBlack };

var leftPane = new FrameView("NEW ORDER") {
    X = 0, Y = 0, Width = Dim.Percent(25), Height = Dim.Fill()
};
leftPane.Add(
    new Label("Ticker:") { X = 1, Y = 1 },
    new TextField("") { X = 1, Y = 2, Width = Dim.Fill(1) },
    new Label("Quantity:") { X = 1, Y = 4 },
    new TextField("") { X = 1, Y = 5, Width = Dim.Fill(1) },
    new Label("TradeAction:") { X = 1, Y = 7 },
    new TextField("") { X = 1, Y = 8, Width = Dim.Fill(1) },
    new Label("ActionPrice:") { X = 1, Y = 10 },
    new TextField("") { X = 1, Y = 11, Width = Dim.Fill(1) },
    new Button("SUBMIT") { X = Pos.Center(), Y = Pos.AnchorEnd(2) }
);

var middlePane = new FrameView("MARKET PRICES") {
    X = Pos.Right(leftPane), Y = 0, Width = Dim.Percent(40), Height = Dim.Fill()
};
var priceTable = new TableView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
middlePane.Add(priceTable);

var rightPane = new FrameView("CURRENT STRATEGIES") {
    X = Pos.Right(middlePane), Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
};
var strategyTable = new TableView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
rightPane.Add(strategyTable);

top.Add(leftPane, middlePane, rightPane);

Application.Run(top);
Application.Shutdown();
