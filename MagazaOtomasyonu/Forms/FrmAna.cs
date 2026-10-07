using System.Data;
using MagazaOtomasyonu.Database;
using MagazaOtomasyonu.Models;
using MagazaOtomasyonu.Services;

namespace MagazaOtomasyonu;

public partial class FrmAna : Form
{
    private readonly Color ink = Color.FromArgb(31, 41, 55);
    private readonly Color accent = Color.FromArgb(37, 99, 235);
    private readonly Color canvas = Color.FromArgb(245, 247, 250);
    private readonly IStoreRepository storeRepository;

    public FrmAna() : this(StoreRepositoryFactory.Create())
    {
    }

    public FrmAna(IStoreRepository storeRepository)
    {
        this.storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        InitializeComponent();
        var assistantButton=new Button{Text="  ✨    Akıllı Asistan",Tag="Akıllı Asistan",Size=new Size(222,50),BackColor=Color.FromArgb(27,43,63),ForeColor=Color.FromArgb(235,241,249),FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI",10),TextAlign=ContentAlignment.MiddleLeft,UseVisualStyleBackColor=false};
        assistantButton.FlatAppearance.BorderSize=0;NavPanel.Controls.Add(assistantButton);NavPanel.Controls.SetChildIndex(assistantButton,8);
        var settings = StoreSettingsManager.Load();
        Text = settings.StoreName + " | Mağaza Yönetimi";
        LblSayfaBaslik.Text = "☰   " + settings.StoreName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
        var databaseOptions = DatabaseOptions.Load();
        lblFooterStatus.Text = "● Veritabanı: " + (databaseOptions.Provider.Equals("SQLServer", StringComparison.OrdinalIgnoreCase) ? "SQL Server" : databaseOptions.SqliteFileName) + "     |     Sürüm: 1.0.0";
        MinimumSize = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        foreach (Control control in NavPanel.Controls)
        {
            if (control is not Button button || button.Tag is not string page)
                continue;

            button.Click += (_, _) =>
            {
                if (page == "Çıkış")
                    Close();
                else
                    ShowPage(page);
            };
        }
        Shown += (_, _) => ShowPage("Ana Sayfa");
    }

    private void ShowPage(string page)
    {
        PnlContainer.Controls.Clear();
        // The form has a minimum window size and the page layouts use fill rows.
        // AutoScroll on this outer panel shifted dock-filled children on some DPI
        // settings, which made page titles and grid headers appear clipped.
        var root = new Panel { Dock = DockStyle.Fill, AutoScroll = false, BackColor = canvas, Padding = new Padding(22) };
        PnlContainer.Controls.Add(root);
        switch (page)
        {
            case "Ürünler": ProductsPage(root); break;
            case "Satış İşlemleri": SalesPage(root); break;
            case "Stok Yönetimi": StockPage(root); break;
            case "Ürün Yönetimi": ProductsPage(root); break;
            case "Kategori Yönetimi": CategoryPage(root); break;
            case "Raporlar": ReportsPage(root); break;
            case "İade / Değişim": ReturnsPage(root); break;
            case "Ayarlar": SettingsPage(root); break;
            case "Yedekleme": BackupPage(root); break;
            case "Hakkında": AboutPage(root); break;
            case "Akıllı Asistan": AssistantPage(root); break;
            case "Borç Takibi": DebtPage(root); break;
            default: Dashboard(root); break;
        }
    }

    private void CategoryPage(Panel root)
    {
        var page = CreatePageLayout(root, 74, 82);
        AddTitle(page, "Kategori Yönetimi", "Ürünleri mağazanızın ürün gruplarına göre düzenleyin.");
        var form = FormPanel(page, 1); var name = Input(form, "Kategori adı", 12, 8, 280);
        var grid = Grid(); grid.Dock = DockStyle.Fill; page.Controls.Add(grid, 0, 2);
        void Refresh() => grid.DataSource = storeRepository.GetCategories();
        Refresh();
        AddButton(form, "Kategori ekle", 310, 8, 135, () => { if (string.IsNullOrWhiteSpace(name.Text)) { Warn("Kategori adı girin."); return; } storeRepository.AddCategory(name.Text.Trim()); name.Clear(); Refresh(); });
    }

    private void ReportsPage(Panel root)
    {
        var page = CreatePageLayout(root, 74, 112);
        AddTitle(page, "Raporlar", "Satış, tahsilat ve açık borç özeti.");
        var cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 112, WrapContents = false, BackColor = canvas };
        var summary = storeRepository.GetDashboardSummary();
        AddCard(cards, "Bugünkü Ciro", Money(summary.TodaySales));
        AddCard(cards, "Bu Ay Tahsilat", Money(summary.MonthCollections));
        AddCard(cards, "Açık Borç", Money(summary.OpenDebt));
        page.Controls.Add(cards, 0, 1);
        var grid = Grid(); grid.Dock = DockStyle.Fill; page.Controls.Add(grid, 0, 2);
        grid.DataSource = storeRepository.GetSalesReport();
        FormatMoney(grid, "Satış", "Tahsilat", "Borç");
    }

    private void PlaceholderPage(Panel root, string title, string description)
    {
        var page = CreatePageLayout(root, 74, 150);
        AddTitle(page, title, description);
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(22), Margin = new Padding(0) };
        card.Controls.Add(new Label { Text = "Bu sayfa şu anda bilgilendirme amaçlıdır. Yukarıda belirtilen işlemler kapsam olarak belirlendi; modül henüz işlem yapmıyor.", Dock = DockStyle.Top, Height = 46, Font = new Font("Segoe UI", 11), ForeColor = ink, TextAlign = ContentAlignment.MiddleLeft });
        page.Controls.Add(card, 0, 1);
    }

    private void Dashboard(Panel root)
    {
        root.BackColor = canvas;
        var page = CreatePageLayout(root, 112);
        var cards = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = canvas, Padding = new Padding(0, 0, 0, 8), Margin = new Padding(0) };
        var summary = storeRepository.GetDashboardSummary();
        AddCard(cards, "🛒  Bugünkü Satış", Money(summary.TodaySales));
        AddCard(cards, "↗  Bu Ayki Satış", Money(summary.MonthSales));
        AddCard(cards, "◇  Toplam Stok", summary.TotalStock + " adet");
        AddCard(cards, "₺  Tahmini Kâr", Money(summary.EstimatedProfit));
        page.Controls.Add(cards, 0, 0);
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = canvas, Padding = new Padding(0, 8, 0, 0), Margin = new Padding(0) };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61)); split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39)); split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.Controls.Add(split, 0, 1);
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = canvas, Padding = new Padding(0, 0, 8, 0), Margin = new Padding(0) };
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); left.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        var right = new Panel { Dock = DockStyle.Fill, BackColor = canvas, Padding = new Padding(8, 0, 0, 0) };
        split.Controls.Add(left, 0, 0); split.Controls.Add(right, 1, 0);
        QuickSale(left);
        var shortcuts = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true, BackColor = canvas, Padding = new Padding(0, 5, 0, 0), Margin = new Padding(0) };
        AddShortcut(shortcuts, "＋ Yeni Satış", "Satış İşlemleri"); AddShortcut(shortcuts, "☷ Satışlar", "Raporlar"); AddShortcut(shortcuts, "♢ Ürünler", "Ürün Yönetimi"); AddShortcut(shortcuts, "◇ Stoklar", "Stok Yönetimi"); AddShortcut(shortcuts, "▦ Kategoriler", "Kategori Yönetimi"); AddShortcut(shortcuts, "▥ Raporlar", "Raporlar"); AddShortcut(shortcuts, "↶ İadeler", "İade / Değişim");
        left.Controls.Add(shortcuts, 0, 1);
        var rightStack = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = canvas };
        rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 34)); rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 33)); rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 33)); right.Controls.Add(rightStack);
        AddDashboardGrid(rightStack, "En Çok Satan Ürünler (Bu Ay)", storeRepository.GetBestSellingProducts(5), 0);
        AddDashboardGrid(rightStack, "Son Yapılan Satışlar", storeRepository.GetRecentSales(5), 1, "Tutar");
        AddDashboardGrid(rightStack, "Stok Durumu (Azalan Stoklar)", storeRepository.GetLowestStockProducts(5), 2);
    }

    private void QuickSale(TableLayoutPanel parent)
    {
        var box = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14), Margin = new Padding(0, 0, 8, 8) }; parent.Controls.Add(box, 0, 0);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = new Padding(0), Padding = new Padding(0), BackColor = Color.White };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 165));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        box.Controls.Add(layout);
        layout.Controls.Add(new Label { Text = "Hızlı Satış", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(30, 69, 124), TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) }, 0, 0);
        var fields = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        layout.Controls.Add(fields, 0, 1);
        var product = Combo(fields, "Ürün seçin", 0, 0, 300); FillProducts(product);
        var customer = Input(fields, "Müşteri (boşsa perakende)", 315, 0, 220);
        var qty = Input(fields, "Adet", 0, 60, 90); qty.Text = "1";
        var price = Input(fields, "Birim fiyat ₺", 105, 60, 130); if (product.SelectedItem is SaleProductOption initial) price.Text = MoneyText(initial.Price);
        var paid = Input(fields, "Tahsil edilen ₺", 245, 60, 150); paid.Text = "0";
        product.SelectedIndexChanged += (_, _) => { if (product.SelectedItem is SaleProductOption p) price.Text = MoneyText(p.Price); };
        var cart = new DataGridView { Dock = DockStyle.Fill, Margin = new Padding(0), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false };
        cart.Columns.Add("UrunID", "ID"); cart.Columns[0].Visible = false; cart.Columns.Add("Ürün", "Ürün"); cart.Columns.Add("Adet", "Adet"); cart.Columns.Add("Fiyat", "Birim Fiyat"); cart.Columns.Add("Toplam", "Toplam"); layout.Controls.Add(cart, 0, 2);
        var totalLabel = new Label { Dock = DockStyle.Fill, Text = "Toplam Tutar     0,00 ₺", TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.SeaGreen, Margin = new Padding(0) }; layout.Controls.Add(totalLabel, 0, 3);
        decimal Total() => cart.Rows.Cast<DataGridViewRow>().Sum(r => Convert.ToDecimal(r.Cells[4].Value));
        void UpdateTotal() => totalLabel.Text = "Toplam Tutar     " + MoneyText(Total());
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(0), Padding = new Padding(0, 4, 0, 0) };
        fields.Controls.Add(actions);
        var add = new Button { Text = "+  Sepete Ekle", Size = new Size(140, 34), BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 10, 0) };
        var remove = new Button { Text = "Sepetten Çıkar", Size = new Size(140, 34), BackColor = Color.White, ForeColor = ink, FlatStyle = FlatStyle.Flat, Margin = new Padding(0) };
        actions.Controls.Add(add); actions.Controls.Add(remove);
        var finish = new Button { Text = "✓  Satışı Tamamla", Dock = DockStyle.Fill, BackColor = Color.FromArgb(42, 158, 81), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0) };
        layout.Controls.Add(finish, 0, 4);
        add.Click += (_, _) => { if (product.SelectedItem is not SaleProductOption p) { Warn("Önce ürün ekleyin."); return; } int count = Int(qty.Text); if (count < 1 || count > p.Stock) { Warn($"Stok yetersiz. Mevcut: {p.Stock}"); return; } decimal unit = Num(price.Text); cart.Rows.Add(p.Id, p.Name, count, unit, count * unit); UpdateTotal(); };
        remove.Click += (_, _) => { if (cart.CurrentRow != null) cart.Rows.Remove(cart.CurrentRow); UpdateTotal(); };
        finish.Click += (_, _) =>
        {
            decimal total = Total(), amountPaid = Num(paid.Text); if (cart.Rows.Count == 0) { Warn("Sepete ürün ekleyin."); return; } if (amountPaid < 0 || amountPaid > total) { Warn("Tahsilat tutarını kontrol edin."); return; }
            var lines = cart.Rows.Cast<DataGridViewRow>()
                .Select(row => new SaleLine(Convert.ToInt32(row.Cells[0].Value), Convert.ToString(row.Cells[1].Value) ?? "", Convert.ToInt32(row.Cells[2].Value), Convert.ToDecimal(row.Cells[3].Value)))
                .ToArray();
            try
            {
                storeRepository.CompleteSale(new SaleRequest(customer.Text.Trim(), "", amountPaid, lines));
                cart.Rows.Clear(); customer.Clear(); paid.Text = "0"; UpdateTotal(); FillProducts(product);
                MessageBox.Show("Satış kaydedildi.", "İmaj Giyim", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Satış kaydedilemedi: " + ex.Message, "İmaj Giyim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
    }

    private void AddDashboardGrid(TableLayoutPanel parent, string title, DataTable data, int row, params string[] moneyColumns)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 0, 9), Padding = new Padding(10) };
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0), Padding = new Padding(0), BackColor = Color.White };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(content);
        content.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 69, 124), TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0) }, 0, 0);
        var grid = Grid(); grid.Dock = DockStyle.Fill; content.Controls.Add(grid, 0, 1); grid.DataSource = data; FormatMoney(grid, moneyColumns);
        parent.Controls.Add(panel, 0, row);
    }

    private void AddShortcut(Control parent, string text, string page)
    {
        var button = new Button { Text = text, Width = 96, Height = 52, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = accent, Font = new Font("Segoe UI", 8, FontStyle.Bold), Margin = new Padding(0, 0, 7, 0), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = Color.FromArgb(221, 227, 236); button.Click += (_, _) => ShowPage(page); parent.Controls.Add(button);
    }

    private void ProductsPage(Panel root)
    {
        var page = CreatePageLayout(root, 74, 125);
        AddTitle(page, "Ürünler", "Ürün bilgilerini ve fiyatları elle girin. Barkod alanı kullanılmaz.");
        var form = FormPanel(page, 1);
        var name = Input(form, "Ürün adı", 12, 10, 190); var cat = Input(form, "Kategori", 215, 10, 150);
        var cost = Input(form, "Alış ₺", 375, 10, 110); var price = Input(form, "Satış ₺", 495, 10, 110); var stock = Input(form, "Stok", 615, 10, 90);
        var grid = Grid(); grid.Dock = DockStyle.Fill; page.Controls.Add(grid, 0, 2); RefreshGrid();
        grid.SelectionChanged += (_, _) => { if (grid.CurrentRow?.DataBoundItem is DataRowView r) { name.Text = Convert.ToString(r["Ürün"]) ?? ""; cat.Text = Convert.ToString(r["Kategori"]) ?? ""; cost.Text = Convert.ToString(r["Alış"]) ?? "0"; price.Text = Convert.ToString(r["Satış"]) ?? "0"; stock.Text = Convert.ToString(r["Stok"]) ?? "0"; } };
        AddButton(form, "Yeni / Kaydet", 12, 70, 125, () => { if (string.IsNullOrWhiteSpace(name.Text)) { Warn("Ürün adı girin."); return; } storeRepository.SaveProduct(new ProductSaveRequest(SelectedId(grid), name.Text, cat.Text, "", "", Num(cost.Text), Num(price.Text), Int(stock.Text))); RefreshGrid(); });
        AddButton(form, "Seçileni pasife al", 150, 70, 155, () => { int id = SelectedId(grid); if (id == 0) return; storeRepository.DeactivateProduct(id); RefreshGrid(); });
        void RefreshGrid() { grid.DataSource = storeRepository.GetProducts(); FormatMoney(grid, "Alış", "Satış"); }
    }

    private void StockPage(Panel root)
    {
        var page = CreatePageLayout(root, 74, 90);
        AddTitle(page, "Stok durumu", "Sayım farklarını ürün seçerek elle düzeltin.");
        var form = FormPanel(page, 1); var quantity = Input(form, "Yeni stok miktarı", 12, 6, 180);
        var grid = Grid(); grid.Dock = DockStyle.Fill; page.Controls.Add(grid, 0, 2); grid.DataSource = storeRepository.GetStockProducts();
        grid.SelectionChanged += (_, _) => { if (grid.CurrentRow?.DataBoundItem is DataRowView r) quantity.Text = Convert.ToString(r["Mevcut stok"]); };
        AddButton(form, "Stoku güncelle", 210, 6, 150, () => { int id = SelectedId(grid); if (id == 0) { Warn("Önce ürün seçin."); return; } if (Int(quantity.Text) < 0) { Warn("Stok eksi olamaz."); return; } storeRepository.UpdateStock(id, Int(quantity.Text)); grid.DataSource = storeRepository.GetStockProducts(); });
    }

    private void SalesPage(Panel root)
    {
        var page = CreatePageLayout(root, 74, 190);
        AddTitle(page, "Satış işlemleri", "Ürünü, müşteriyi ve tahsil edilen tutarı seçin. Ödenmeyen kısım müşteri borcuna eklenir.");
        var form = FormPanel(page, 1); var product = Combo(form, "Ürün", 12, 8, 230); FillProducts(product);
        var qty = Input(form, "Adet", 252, 8, 90); qty.Text = "1"; var price = Input(form, "Birim fiyat ₺", 350, 8, 130);
        product.SelectedIndexChanged += (_, _) => { if (product.SelectedItem is SaleProductOption p) price.Text = MoneyText(p.Price); };
        var customer = Input(form, "Müşteri (boş bırakılabilir)", 492, 8, 240); var totalPaid = Input(form, "Tahsil edilen ₺", 742, 8, 150); totalPaid.Text = "0";
        var note = Input(form, "Not", 12, 68, 500); var recent = Grid(); recent.Dock = DockStyle.Fill; page.Controls.Add(recent, 0, 2);
        AddButton(form, "Satışı kaydet", 530, 68, 150, () =>
        {
            if (product.SelectedItem is not SaleProductOption p) { Warn("Ürün seçin."); return; }
            int n = Int(qty.Text); decimal unit = Num(price.Text), paid = Num(totalPaid.Text), total = n * unit; if (n < 1 || n > p.Stock) { Warn($"Stok yetersiz. Mevcut: {p.Stock}"); return; }
            if (unit < 0 || paid < 0 || paid > total) { Warn("Fiyat veya tahsilat tutarını kontrol edin."); return; }
            var saleLine = new SaleLine(p.Id, p.Name, n, unit);
            storeRepository.CompleteSale(new SaleRequest(customer.Text.Trim(), note.Text, paid, new[] { saleLine }));
            customer.Clear(); note.Clear(); totalPaid.Text = "0"; FillProducts(product); recent.DataSource = storeRepository.GetRecentSalesForEntry(30); FormatMoney(recent, "Toplam", "Ödenen", "Kalan");
        });
        recent.DataSource = storeRepository.GetRecentSalesForEntry(30); FormatMoney(recent, "Toplam", "Ödenen", "Kalan");
    }

    private void DebtPage(Panel root)
    {
        var page = CreatePageLayout(root, 74, 105);
        AddTitle(page, "Borç ve raporlar", "Müşterilerin açık bakiyelerini görün ve seçili borca tahsilat işleyin.");
        var form = FormPanel(page, 1); var amount = Input(form, "Tahsilat ₺", 12, 8, 170); var note = Input(form, "Açıklama", 195, 8, 300);
        var grid = Grid(); grid.Dock = DockStyle.Fill; page.Controls.Add(grid, 0, 2); Refresh();
        grid.SelectionChanged += (_, _) => { if (grid.CurrentRow?.DataBoundItem is DataRowView r) note.Text = "Tahsilat - " + Convert.ToString(r["Müşteri"]); };
        AddButton(form, "Tahsilat kaydet", 515, 8, 155, () => { int id = SelectedId(grid); decimal value = Num(amount.Text); if (id == 0) { Warn("Açık borç satırı seçin."); return; } if (value <= 0) { Warn("Tahsilat pozitif olmalı."); return; } decimal debt = storeRepository.GetOutstandingBalance(id) ?? 0; if (value > debt) { Warn($"Kalan borç {MoneyText(debt)}. Daha yüksek tutar girilemez."); return; } storeRepository.RecordPayment(id, value, note.Text); amount.Clear(); Refresh(); });
        void Refresh() { grid.DataSource = storeRepository.GetOpenDebts(); FormatMoney(grid, "Toplam", "Ödenen", "Kalan"); }
    }

    private void ReturnsPage(Panel root)
    {
        var page=CreatePageLayout(root,74,310); AddTitle(page,"İade / Değişim","Fiş numarasını yükleyin. İade edilen ürün stoğa eklenir; değişim farkı borç, tahsilat veya iade olarak kaydedilir.");
        var form=FormPanel(page,1); var sale=Input(form,"Fiş no",12,5,90); var load=new Button{Text="Fişi yükle",Location=new Point(112,26),Size=new Size(105,30),BackColor=accent,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};form.Controls.Add(load);
        var details=Grid();details.Location=new Point(12,62);details.Size=new Size(560,105);details.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;form.Controls.Add(details);
        var qty=Input(form,"İade adedi",585,5,100);qty.Text="1";var product=Combo(form,"Yeni ürün (boş: sadece iade)",700,5,280);FillProducts(product);product.SelectedIndex=-1;var newQty=Input(form,"Yeni adet",990,5,90);newQty.Text="1";
        var settlement=Combo(form,"Farkın kapatılması",12,175,220);settlement.Items.AddRange(new object[]{"Borç","Tahsilat","Para üstü","Müşteri alacağı"});settlement.SelectedIndex=0;
        var reason=Input(form,"İade nedeni",250,175,320);var save=new Button{Text="İade / değişimi kaydet",Location=new Point(590,196),Size=new Size(190,34),BackColor=Color.FromArgb(42,158,81),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};form.Controls.Add(save);
        load.Click+=(_,_)=>{int id=Int(sale.Text);if(id<=0){Warn("Geçerli fiş numarası girin.");return;}details.DataSource=storeRepository.GetSaleDetails(id);FormatMoney(details,"Birim fiyat");};
        product.SelectedIndexChanged+=(_,_)=>{if(product.SelectedItem is SaleProductOption p && p.Stock==0)Warn("Seçilen üründe stok yok.");};
        var history=Grid();history.Dock=DockStyle.Fill;page.Controls.Add(history,0,2);history.DataSource=storeRepository.GetRecentReturns(100);FormatMoney(history,"İade ₺","Yeni ürün ₺","Fark ₺");
        save.Click+=(_,_)=>{try{var id=Int(sale.Text);var detail=SelectedId(details);if(id<=0||detail<=0){Warn("Fiş yükleyip iade edilecek satırı seçin.");return;}int n=Int(qty.Text);int? replace=product.SelectedItem is SaleProductOption p?p.Id:null;int replacementQty=replace.HasValue?Int(newQty.Text):0;if(n<1|| (replace.HasValue&&replacementQty<1)){Warn("Adet bilgilerini kontrol edin.");return;}storeRepository.CompleteReturnExchange(new ReturnExchangeRequest(id,detail,n,replace,replacementQty,Convert.ToString(settlement.SelectedItem)??"Borç",reason.Text));details.DataSource=storeRepository.GetSaleDetails(id);history.DataSource=storeRepository.GetRecentReturns(100);FormatMoney(history,"İade ₺","Yeni ürün ₺","Fark ₺");Warn("İade / değişim kaydedildi; stok ve satış bakiyesi güncellendi.");}catch(Exception ex){MessageBox.Show("İşlem tamamlanamadı: "+ex.Message,"Mağaza",MessageBoxButtons.OK,MessageBoxIcon.Error);}};
    }

    private void SettingsPage(Panel root)
    {
        var page=CreatePageLayout(root,74,300);AddTitle(page,"Ayarlar","Mağaza bilgilerini düzenleyin. Bu bilgiler bu Windows kullanıcı hesabında saklanır.");
        var form=FormPanel(page,1);var settings=StoreSettingsManager.Load();
        var name=Input(form,"Mağaza adı",14,14,360);name.Text=settings.StoreName;
        var phone=Input(form,"Telefon",14,82,360);phone.Text=settings.Phone;
        var address=Input(form,"Adres",14,150,600);address.Text=settings.Address;
        var config=DatabaseOptions.Load();var dbInfo=config.Provider.Equals("SQLServer",StringComparison.OrdinalIgnoreCase)?$"SQL Server · {config.SqlServer.Server} / {config.SqlServer.Database}":$"SQLite · {config.SqliteFileName}";
        form.Controls.Add(new Label{Text="Etkin veritabanı (bilgi amaçlı)",Location=new Point(14,218),AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold),ForeColor=Color.FromArgb(75,85,99)});
        form.Controls.Add(new Label{Text=dbInfo,Location=new Point(14,240),AutoSize=true,Font=new Font("Segoe UI",10),ForeColor=ink});
        AddButton(form,"Ayarları kaydet",680,236,155,()=>{var storeName=name.Text.Trim();if(storeName.Length==0){Warn("Mağaza adı boş olamaz.");return;}if(storeName.Length>100||phone.Text.Trim().Length>40||address.Text.Trim().Length>250){Warn("Alan uzunluklarını kontrol edin.");return;}StoreSettingsManager.Save(new StoreSettings(storeName,phone.Text.Trim(),address.Text.Trim()));Text=storeName+" | Mağaza Yönetimi";LblSayfaBaslik.Text="☰   "+storeName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));Warn("Mağaza ayarları kaydedildi.");});
    }

    private void BackupPage(Panel root)
    {
        var page=CreatePageLayout(root,74,230);AddTitle(page,"Yedekleme ve geri yükleme","Yedek dosyasını uygulamadan ayrı bir diskte veya güvenilir bulut klasöründe saklayın.");
        var card=FormPanel(page,1);var options=DatabaseOptions.Load();bool sqlServer=options.Provider.Equals("SQLServer",StringComparison.OrdinalIgnoreCase);var backupService=new BackupService();string suggested;
        try{suggested=backupService.GetSuggestedBackupDirectory();}catch(Exception ex){suggested="Klasör alınamadı: "+ex.Message;}
        var providerLabel=sqlServer?"Etkin kaynak: SQL Server · "+options.SqlServer.Server+" / "+options.SqlServer.Database:"Etkin kaynak: SQLite · "+options.SqliteFileName;
        card.Controls.Add(new Label{Text=providerLabel,Location=new Point(14,12),AutoSize=true,Font=new Font("Segoe UI",10,FontStyle.Bold),ForeColor=ink});
        card.Controls.Add(new Label{Text=sqlServer?"Önerilen SQL Server yedek klasörü: "+suggested+"\nSQL Server hizmet hesabının seçtiğiniz klasöre yazma izni olmalıdır.":"SQLite yedeği çevrimiçi ve tutarlı alınır; yedek dosyası için kaydetme konumu seçebilirsiniz.",Location=new Point(14,42),Size=new Size(850,48),Font=new Font("Segoe UI",9),ForeColor=Color.FromArgb(75,85,99)});
        AddButton(card,"Şimdi yedek al",14,112,150,()=>{var ext=sqlServer?"bak":"db";using var dialog=new SaveFileDialog{Title="Veritabanı yedeğini kaydet",Filter=sqlServer?"SQL Server yedeği (*.bak)|*.bak":"SQLite yedeği (*.db)|*.db",FileName=(sqlServer?options.SqlServer.Database:"Magaza")+"_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"."+ext,InitialDirectory=Directory.Exists(suggested)?suggested:Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),AddExtension=true,OverwritePrompt=true};if(dialog.ShowDialog(this)!=DialogResult.OK)return;backupService.CreateBackup(dialog.FileName);MessageBox.Show("Yedek alındı ve bütünlük kontrolü başarılı.\n\n"+dialog.FileName,"Yedekleme",MessageBoxButtons.OK,MessageBoxIcon.Information);});
        AddButton(card,"Yedekten geri yükle",180,112,180,()=>{using var dialog=new OpenFileDialog{Title="Geri yüklenecek veritabanı yedeğini seç",Filter=sqlServer?"SQL Server yedeği (*.bak)|*.bak":"SQLite yedeği (*.db)|*.db",CheckFileExists=true,InitialDirectory=Directory.Exists(suggested)?suggested:Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)};if(dialog.ShowDialog(this)!=DialogResult.OK)return;var answer=MessageBox.Show("Seçilen yedek etkin veritabanının üzerine yüklenecek. Önce mevcut veritabanının otomatik güvenlik yedeği alınır. Devam edilsin mi?","Geri yüklemeyi onayla",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2);if(answer!=DialogResult.Yes)return;var safety=backupService.Restore(dialog.FileName);MessageBox.Show("Geri yükleme tamamlandı. Geri dönüş için mevcut veritabanının güvenlik kopyası alındı:\n\n"+safety,"Geri yükleme",MessageBoxButtons.OK,MessageBoxIcon.Information);});
    }

    private void AboutPage(Panel root)
    {
        var page=CreatePageLayout(root,74);var settings=StoreSettingsManager.Load();AddTitle(page,"Hakkında",settings.StoreName+" · Tarz, özen ve ulaşılabilir hizmet");
        var content=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,BackColor=canvas,Padding=new Padding(0,8,0,0)};page.Controls.Add(content,0,1);
        AddAboutBanner(content,settings.StoreName);
        AddAboutCard(content,"🏬","Mağazamız","Günlük stilinize eşlik edecek giyim ürünleriyle hizmetinizdeyiz.\n\nDurum: Faaliyette\n"+(string.IsNullOrWhiteSpace(settings.Address)?"Adres bilgisi Ayarlar bölümünden eklenebilir.":settings.Address));
        AddAboutCard(content,"✨","Hizmet anlayışımız","Kendinizi iyi hissettiren parçaları kolayca bulmanız için buradayız. Ürün seçiminden satış sonrasına kadar güler yüzlü, özenli ve güvenilir bir alışveriş deneyimi sunmayı amaçlıyoruz.\n\nHer ziyaretinizde stilinize yakışanı birlikte bulalım.");
        AddAboutCard(content,"💬","WhatsApp ile ulaşın","İletişim bilgileri mağaza yöneticisi tarafından eklenecek.");
        AddAboutCard(content,"📱","Bizi sosyal medyada bulun","Instagram\nimajgiyim_\n\nDolap\nHesap bilgisi eklenecek.");
        content.Controls.Add(new Label{Text="İmaj Giyim Mağaza Otomasyon Sistemi  ·  Sürüm 1.0.0",Width=850,Height=32,Margin=new Padding(4,2,4,10),ForeColor=Color.DimGray,TextAlign=ContentAlignment.MiddleCenter});
    }

    private void AssistantPage(Panel root)
    {
        var page=CreatePageLayout(root,74);AddTitle(page,"Mağaza Asistanı","Satış, kâr hedefi, borç ve stok hakkında mağazanın güncel kayıtlarına dayalı yanıtlar.");
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,BackColor=canvas,Padding=new Padding(0,8,0,0),Margin=new Padding(0)};
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,44));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,58));page.Controls.Add(layout,0,1);
        var messages=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,BackColor=Color.White,Padding=new Padding(14),Margin=new Padding(0)};layout.Controls.Add(messages,0,0);
        var assistant=new StoreChatbot(storeRepository);
        AddChatMessage(messages,assistant.OpeningMessage(),true);
        var prompts=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.LeftToRight,WrapContents=false,AutoScroll=true,BackColor=canvas,Padding=new Padding(0,5,0,2),Margin=new Padding(0)};layout.Controls.Add(prompts,0,1);
        var promptExamples=new[]{"Bu ay nasıl gidiyor?","Azalan stoklar","Açık borçları listele","Bu ayki kârım ne kadar?"};
        var inputRow=new Panel{Dock=DockStyle.Fill,BackColor=canvas,Padding=new Padding(0,8,0,0)};layout.Controls.Add(inputRow,0,2);
        var input=new TextBox{Location=new Point(0,8),Height=38,Font=new Font("Segoe UI",11),Anchor=AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right};var send=new Button{Text="Gönder",Size=new Size(105,38),Location=new Point(0,7),BackColor=accent,ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI",9,FontStyle.Bold),Anchor=AnchorStyles.Top|AnchorStyles.Right};inputRow.Controls.Add(input);inputRow.Controls.Add(send);
        void ResizeInput(){send.Location=new Point(inputRow.ClientSize.Width-send.Width,7);input.Size=new Size(Math.Max(200,send.Left-10),38);}inputRow.Resize+=(_,_)=>ResizeInput();ResizeInput();
        void SendChat(string text){if(string.IsNullOrWhiteSpace(text))return;AddChatMessage(messages,text.Trim(),false);input.Clear();try{AddChatMessage(messages,assistant.Reply(text),true);}catch(Exception ex){AddChatMessage(messages,"Mağaza verilerini okurken sorun oluştu: "+ex.Message,true);}}
        foreach(var prompt in promptExamples){var chip=new Button{Text=prompt,AutoSize=true,Height=32,BackColor=Color.White,ForeColor=accent,FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI",9),Margin=new Padding(0,0,8,0),Cursor=Cursors.Hand};chip.FlatAppearance.BorderColor=Color.FromArgb(210,220,232);chip.Click+=(_,_)=>SendChat(prompt);prompts.Controls.Add(chip);}
        send.Click+=(_,_)=>SendChat(input.Text);input.KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;SendChat(input.Text);}};
        input.Focus();
    }

    private static void AddChatMessage(FlowLayoutPanel conversation,string text,bool fromAssistant)
    {
        var width=Math.Max(420,conversation.ClientSize.Width-SystemInformation.VerticalScrollBarWidth-38);
        var row=new Panel{Width=width,Height=12,BackColor=Color.White,Margin=new Padding(0,0,0,10)};
        var bubble=new Label{Text=text,AutoSize=true,MaximumSize=new Size(Math.Min(760,width-30),0),Padding=new Padding(13,9,13,9),Font=new Font("Segoe UI",10),ForeColor=Color.FromArgb(31,41,55),BackColor=fromAssistant?Color.FromArgb(239,244,250):Color.FromArgb(225,239,255)};
        row.Controls.Add(bubble);bubble.Location=new Point(fromAssistant?0:Math.Max(0,width-bubble.PreferredWidth),0);row.Height=bubble.PreferredHeight;
        conversation.Controls.Add(row);conversation.ScrollControlIntoView(row);
    }

    private static void AddAboutBanner(Control parent,string storeName)
    {
        var banner=new Panel{Size=new Size(850,124),BackColor=Color.FromArgb(30,55,86),Margin=new Padding(0,0,12,16),Padding=new Padding(22)};
        banner.Controls.Add(new Label{Text="♧  "+storeName.ToUpper(new System.Globalization.CultureInfo("tr-TR")),Location=new Point(22,16),AutoSize=true,Font=new Font("Segoe UI",19,FontStyle.Bold),ForeColor=Color.White});
        banner.Controls.Add(new Label{Text="Gününüzü güzelleştiren seçimler, özenle sunulan bir alışveriş deneyimi.",Location=new Point(25,61),AutoSize=true,Font=new Font("Segoe UI",11),ForeColor=Color.FromArgb(225,235,247)});
        var status=new Label{Text="●  FAALİYETTE",Location=new Point(660,24),Size=new Size(150,32),BackColor=Color.FromArgb(41,116,92),ForeColor=Color.White,Font=new Font("Segoe UI",9,FontStyle.Bold),TextAlign=ContentAlignment.MiddleCenter};banner.Controls.Add(status);parent.Controls.Add(banner);
    }

    private static void AddAboutCard(Control parent,string icon,string title,string body)
    {
        var panel=new Panel{Size=new Size(410,205),BackColor=Color.White,Margin=new Padding(0,0,14,14),Padding=new Padding(18)};
        panel.Paint+=(_,e)=>ControlPaint.DrawBorder(e.Graphics,panel.ClientRectangle,Color.FromArgb(228,233,240),ButtonBorderStyle.Solid);
        panel.Controls.Add(new Label{Text=icon,Location=new Point(18,13),Size=new Size(38,38),Font=new Font("Segoe UI Emoji",18),TextAlign=ContentAlignment.MiddleCenter,ForeColor=Color.FromArgb(37,99,235)});
        panel.Controls.Add(new Label{Text=title,Location=new Point(64,18),AutoSize=true,Font=new Font("Segoe UI",12,FontStyle.Bold),ForeColor=Color.FromArgb(30,69,124)});
        panel.Controls.Add(new Label{Text=body,Location=new Point(20,62),Size=new Size(368,128),Font=new Font("Segoe UI",10),ForeColor=Color.FromArgb(55,65,81)});parent.Controls.Add(panel);
    }

    private TableLayoutPanel CreatePageLayout(Panel root, params int[] fixedRows)
    {
        var page = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = fixedRows.Length + 1, Margin = new Padding(0), Padding = new Padding(0), BackColor = canvas };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var height in fixedRows)
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(page);
        return page;
    }
    private void AddTitle(TableLayoutPanel parent, string title, string subtitle)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = canvas, Margin = new Padding(0) };
        panel.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = ink, AutoSize = true, Location = new Point(0, 0) });
        panel.Controls.Add(new Label { Text = subtitle, Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray, AutoSize = true, Location = new Point(2, 42) });
        parent.Controls.Add(panel, 0, 0);
    }
    private static Panel FormPanel(TableLayoutPanel parent, int row) { var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0) }; parent.Controls.Add(p, 0, row); return p; }
    private static TextBox Input(Control parent, string label, int x, int y, int width) { var l = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(75, 85, 99) }; var t = new TextBox { Location = new Point(x, y + 21), Width = width, Height = 30, Font = new Font("Segoe UI", 10) }; parent.Controls.Add(l); parent.Controls.Add(t); return t; }
    private static ComboBox Combo(Control parent, string label, int x, int y, int width) { var l = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold) }; var c = new ComboBox { Location = new Point(x, y + 21), Width = width, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) }; parent.Controls.Add(l); parent.Controls.Add(c); return c; }
    private void AddButton(Control p, string text, int x, int y, int width, Action action) { var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 34), BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; b.Click += (_, _) => { try { action(); } catch (Exception ex) { MessageBox.Show("İşlem tamamlanamadı: " + ex.Message, "Mağaza", MessageBoxButtons.OK, MessageBoxIcon.Error); } }; p.Controls.Add(b); }
    private static DataGridView Grid() => new() { BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, ColumnHeadersHeight = 34, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, RowTemplate = { Height = 30 }, EnableHeadersVisualStyles = false, ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(31, 41, 55), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), WrapMode = DataGridViewTriState.False }, DefaultCellStyle = { Font = new Font("Segoe UI", 9), SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) }, AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(248, 250, 252) } };
    private static void AddCard(Control parent, string title, string value) { var p = new Panel { Size = new Size(205, 90), BackColor = Color.White, Margin = new Padding(0, 0, 12, 8) }; p.Controls.Add(new Label { Text = title, Location = new Point(12, 12), AutoSize = true, ForeColor = Color.DimGray }); p.Controls.Add(new Label { Text = value, Location = new Point(12, 39), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) }); parent.Controls.Add(p); }
    private void FillProducts(ComboBox combo)
    {
        combo.Items.Clear();
        foreach (var product in storeRepository.GetAvailableProducts())
            combo.Items.Add(product);
        if (combo.Items.Count > 0)
            combo.SelectedIndex = 0;
    }
    private static int SelectedId(DataGridView grid) { try { return Convert.ToInt32(grid.CurrentRow?.Cells[0].Value ?? 0); } catch { return 0; } }
    private static void FormatMoney(DataGridView g, params string[] names) { var culture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR"); foreach (var n in names) if (g.Columns.Contains(n)) { g.Columns[n].DefaultCellStyle.Format = "N2"; g.Columns[n].DefaultCellStyle.FormatProvider = culture; } if (g.Columns.Contains("#")) g.Columns["#"].Visible = false; }
    private static decimal Num(string s) => decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("tr-TR"), out var v) ? v : decimal.TryParse(s, out v) ? v : 0;
    private static int Int(string s) => int.TryParse(s, out var v) ? v : 0;
    private static string Money(object value) => MoneyText(Convert.ToDecimal(value));
    private static string MoneyText(decimal value) => value.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " ₺";
    private static void Warn(string text) => MessageBox.Show(text, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    private void FrmAna_Load(object? sender, EventArgs e) { UpdateClock(); timer1.Interval = 1000; timer1.Start(); }
    private void timer1_Tick(object? sender, EventArgs e) => UpdateClock();
    private void UpdateClock() => LblTarih.Text = DateTime.Now.ToString("d MMMM yyyy dddd", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + "     ◷  " + DateTime.Now.ToString("HH:mm");

}
