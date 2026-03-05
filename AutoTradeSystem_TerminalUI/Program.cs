using Terminal.Gui;
using NStack;
using System.Data;


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

ustring[] tickers = new ustring[] { "IBM", "AAPL", "AMZN" }; //Hardcoded for testing
ustring[] actions = new ustring[] { "Buy", "Sell" };

var tickerSelect = new ComboBox() { 
    X = 1, 
    Y = 2, 
    Width = Dim.Fill(1), 
    Height = 5, 
    Source = new ListWrapper(tickers) 
};

var quantityInput = new TextField("") { X = 1, Y = 5, Width = Dim.Fill(1) };
var actionSelect = new RadioGroup(actions) { X = 1, Y = 8 };
var priceInput = new TextField("") { X = 1, Y = 12, Width = Dim.Fill(1) };
var submitBtn = new Button("SUBMIT") { X = Pos.Center(), Y = Pos.AnchorEnd(2) };

quantityInput.KeyPress += (e) => {
    var key = e.KeyEvent.Key;
    if (key == Key.Backspace || key == Key.Delete || key == Key.CursorLeft || key == Key.CursorRight) {
        return; 
    }
    
    if (!char.IsDigit((char)key)) {
        e.Handled = true;
    }
};

priceInput.KeyPress += (e) => {
    var key = e.KeyEvent.Key;
    char c = (char)key;

    if (key == Key.Backspace || key == Key.Delete || key == Key.CursorLeft || key == Key.CursorRight) {
        return;
    }

    if (!char.IsDigit(c) && c != '.') {
        e.Handled = true;
    }

    if (c == '.' && priceInput.Text.ToString().Contains(".")) {
        e.Handled = true;
    }
};


submitBtn.Clicked += () => {
    if (!int.TryParse(quantityInput.Text.ToString(), out int qty)) {
        MessageBox.ErrorQuery("Error", "Quantity must be a whole number", "Ok");
        return;
    }
    if (!decimal.TryParse(priceInput.Text.ToString(), out decimal price)) {
        MessageBox.ErrorQuery("Error", "Price must be a valid decimal", "Ok");
        return;
    }

    var order = new {
        Ticker = tickerSelect.Text,
        Quantity = qty,
        Action = actions[actionSelect.SelectedItem],
        Price = price
    };

    MessageBox.Query("Order Sent", $"Target: {order.Ticker}\n{order.Action} {order.Quantity} @ {order.Price}", "Ok");
};

leftPane.Add(
    new Label("Ticker:") { X = 1, Y = 1 }, tickerSelect,
    new Label("Quantity:") { X = 1, Y = 4 }, quantityInput,
    new Label("TradeAction:") { X = 1, Y = 7 }, actionSelect,
    new Label("ActionPrice:") { X = 1, Y = 11 }, priceInput,
    submitBtn
);

var middlePane = new FrameView("MARKET PRICES") {
    X = Pos.Right(leftPane), Y = 0, Width = Dim.Percent(20), Height = Dim.Fill()
};
var priceTable = new TableView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

priceTable.Style.AlwaysShowHeaders = true;
priceTable.Style.ShowHorizontalHeaderUnderline = true;
priceTable.ColorScheme = greenOnBlack;

var priceSource = new DataTable();
priceSource.Columns.Add("Ticker", typeof(string));
priceSource.Columns.Add("Price ($)", typeof(string));

priceSource.Rows.Add("IBM", "168.21");
priceSource.Rows.Add("AAPL", "215.76");
priceSource.Rows.Add("AMZN", "227.39");

priceTable.Table = priceSource;

middlePane.Add(priceTable);


var rightPane = new FrameView("CURRENT STRATEGIES") {
    X = Pos.Right(middlePane), Y = 0, Width = Dim.Percent(55), Height = Dim.Fill()
};
var strategyTable = new TableView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

strategyTable.Style.AlwaysShowHeaders = true;
strategyTable.Style.ShowHorizontalHeaderUnderline = true;
strategyTable.ColorScheme = greenOnBlack;

var strategySource = new DataTable();
strategySource.Columns.Add("Ticker", typeof(string));
strategySource.Columns.Add("Action", typeof(string));
strategySource.Columns.Add("Quantity", typeof(int));
strategySource.Columns.Add("ActionPrice ($)", typeof(decimal));

strategySource.Rows.Add("IBM", "Buy", 12, 175.50m);
strategySource.Rows.Add("AAPL", "Buy", 10, 230.50m);
strategySource.Rows.Add("AMZN", "Sell", 5, 245.10m);

strategyTable.Table = strategySource;

rightPane.Add(strategyTable);

top.Add(leftPane, middlePane, rightPane);

top.LayoutComplete += (e) => {
    int priceWidth = priceTable.Bounds.Width - 4; 
    if (priceWidth > 0) {
        int pColWidth = priceWidth / 2;
        priceTable.Style.ColumnStyles.Clear();
        foreach (DataColumn col in priceSource.Columns) {
            priceTable.Style.ColumnStyles.Add(col, new TableView.ColumnStyle { 
                MinWidth = pColWidth, MaxWidth = pColWidth,
                Alignment = TextAlignment.Centered
            });
        }
    }

    int strategyWidth = strategyTable.Bounds.Width - 2;
    if (strategyWidth > 0) {
        int sColWidth = strategyWidth / 4;
        strategyTable.Style.ColumnStyles.Clear();
        foreach (DataColumn col in strategySource.Columns) {
            strategyTable.Style.ColumnStyles.Add(col, new TableView.ColumnStyle { 
                MinWidth = sColWidth, MaxWidth = sColWidth,
                Alignment = TextAlignment.Centered
            });
        }
    }
};


Application.Run(top);
Application.Shutdown();
