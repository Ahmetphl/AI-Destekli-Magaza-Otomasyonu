#nullable enable

namespace MagazaOtomasyonu;

public partial class FrmAna
{
    private System.ComponentModel.IContainer? components = null;
    private Panel pnlMenu = null!;
    private Panel pnlBrand = null!;
    private Label lblBrand = null!;
    private Label lblBrandSubtitle = null!;
    private FlowLayoutPanel NavPanel = null!;
    private Button BtnHome = null!;
    private Button BtnSales = null!;
    private Button BtnStock = null!;
    private Button BtnProducts = null!;
    private Button BtnCategories = null!;
    private Button BtnReports = null!;
    private Button BtnDebt = null!;
    private Button BtnReturns = null!;
    private Button BtnSettings = null!;
    private Button BtnBackup = null!;
    private Button BtnAbout = null!;
    private Button BtnExit = null!;
    private Panel PnlHeader = null!;
    private Panel PnlFooter = null!;
    private Label lblFooterStatus = null!;
    private Panel PnlContainer = null!;
    private Label LblSayfaBaslik = null!;
    private Label LblTarih = null!;
    private FlowLayoutPanel pnlDesignCards = null!;
    private Label lblCardTodaySales = null!;
    private Label lblCardMonthSales = null!;
    private Label lblCardStock = null!;
    private Label lblCardProfit = null!;
    private TableLayoutPanel pnlDesignColumns = null!;
    private Panel pnlDesignQuickSale = null!;
    private Label lblQuickSaleTitle = null!;
    private Label lblQuickSaleFields = null!;
    private DataGridView gridDesignCart = null!;
    private Label lblFinishSale = null!;
    private TableLayoutPanel pnlDesignReports = null!;
    private Panel pnlDesignBestSellers = null!;
    private Panel pnlDesignRecentSales = null!;
    private Panel pnlDesignLowStock = null!;
    private Label lblBestSellersTitle = null!;
    private Label lblRecentSalesTitle = null!;
    private Label lblLowStockTitle = null!;
    private System.Windows.Forms.Timer timer1 = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlMenu = new Panel();
        NavPanel = new FlowLayoutPanel();
        BtnHome = new Button();
        BtnSales = new Button();
        BtnStock = new Button();
        BtnProducts = new Button();
        BtnCategories = new Button();
        BtnReports = new Button();
        BtnDebt = new Button();
        BtnReturns = new Button();
        BtnSettings = new Button();
        BtnBackup = new Button();
        BtnAbout = new Button();
        BtnExit = new Button();
        pnlBrand = new Panel();
        lblBrandSubtitle = new Label();
        lblBrand = new Label();
        PnlHeader = new Panel();
        LblTarih = new Label();
        LblSayfaBaslik = new Label();
        PnlFooter = new Panel();
        lblFooterStatus = new Label();
        PnlContainer = new Panel();
        pnlDesignCards = new FlowLayoutPanel();
        lblCardTodaySales = new Label();
        lblCardMonthSales = new Label();
        lblCardStock = new Label();
        lblCardProfit = new Label();
        pnlDesignColumns = new TableLayoutPanel();
        pnlDesignQuickSale = new Panel();
        lblQuickSaleTitle = new Label();
        lblQuickSaleFields = new Label();
        gridDesignCart = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        lblFinishSale = new Label();
        pnlDesignReports = new TableLayoutPanel();
        pnlDesignBestSellers = new Panel();
        lblBestSellersTitle = new Label();
        pnlDesignRecentSales = new Panel();
        lblRecentSalesTitle = new Label();
        pnlDesignLowStock = new Panel();
        lblLowStockTitle = new Label();
        timer1 = new System.Windows.Forms.Timer(components);
        pnlMenu.SuspendLayout();
        NavPanel.SuspendLayout();
        pnlBrand.SuspendLayout();
        PnlHeader.SuspendLayout();
        PnlFooter.SuspendLayout();
        PnlContainer.SuspendLayout();
        pnlDesignCards.SuspendLayout();
        pnlDesignColumns.SuspendLayout();
        pnlDesignQuickSale.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridDesignCart).BeginInit();
        pnlDesignReports.SuspendLayout();
        pnlDesignBestSellers.SuspendLayout();
        pnlDesignRecentSales.SuspendLayout();
        pnlDesignLowStock.SuspendLayout();
        SuspendLayout();
        // 
        // pnlMenu
        // 
        pnlMenu.BackColor = Color.FromArgb(27, 43, 63);
        pnlMenu.Controls.Add(NavPanel);
        pnlMenu.Controls.Add(pnlBrand);
        pnlMenu.Dock = DockStyle.Left;
        pnlMenu.Location = new Point(0, 0);
        pnlMenu.Name = "pnlMenu";
        pnlMenu.Padding = new Padding(10, 8, 10, 12);
        pnlMenu.Size = new Size(214, 900);
        pnlMenu.TabIndex = 3;
        // 
        // NavPanel
        // 
        NavPanel.AutoScroll = true;
        NavPanel.BackColor = Color.FromArgb(27, 43, 63);
        NavPanel.Controls.Add(BtnHome);
        NavPanel.Controls.Add(BtnSales);
        NavPanel.Controls.Add(BtnStock);
        NavPanel.Controls.Add(BtnProducts);
        NavPanel.Controls.Add(BtnCategories);
        NavPanel.Controls.Add(BtnReports);
        NavPanel.Controls.Add(BtnDebt);
        NavPanel.Controls.Add(BtnReturns);
        NavPanel.Controls.Add(BtnSettings);
        NavPanel.Controls.Add(BtnBackup);
        NavPanel.Controls.Add(BtnAbout);
        NavPanel.Controls.Add(BtnExit);
        NavPanel.Dock = DockStyle.Fill;
        NavPanel.FlowDirection = FlowDirection.TopDown;
        NavPanel.Location = new Point(10, 90);
        NavPanel.Name = "NavPanel";
        NavPanel.Padding = new Padding(0, 8, 0, 0);
        NavPanel.Size = new Size(194, 798);
        NavPanel.TabIndex = 0;
        NavPanel.WrapContents = false;
        // 
        // BtnHome
        // 
        BtnHome.BackColor = Color.FromArgb(27, 43, 63);
        BtnHome.FlatAppearance.BorderSize = 0;
        BtnHome.FlatStyle = FlatStyle.Flat;
        BtnHome.Font = new Font("Segoe UI", 10F);
        BtnHome.ForeColor = Color.FromArgb(235, 241, 249);
        BtnHome.Location = new Point(3, 11);
        BtnHome.Name = "BtnHome";
        BtnHome.Size = new Size(222, 50);
        BtnHome.TabIndex = 0;
        BtnHome.Tag = "Ana Sayfa";
        BtnHome.Text = "  ⌂    Ana Sayfa";
        BtnHome.TextAlign = ContentAlignment.MiddleLeft;
        BtnHome.UseVisualStyleBackColor = false;
        // 
        // BtnSales
        // 
        BtnSales.BackColor = Color.FromArgb(27, 43, 63);
        BtnSales.FlatAppearance.BorderSize = 0;
        BtnSales.FlatStyle = FlatStyle.Flat;
        BtnSales.Font = new Font("Segoe UI", 10F);
        BtnSales.ForeColor = Color.FromArgb(235, 241, 249);
        BtnSales.Location = new Point(3, 67);
        BtnSales.Name = "BtnSales";
        BtnSales.Size = new Size(222, 50);
        BtnSales.TabIndex = 1;
        BtnSales.Tag = "Satış İşlemleri";
        BtnSales.Text = "  \U0001f6d2    Satış İşlemleri";
        BtnSales.TextAlign = ContentAlignment.MiddleLeft;
        BtnSales.UseVisualStyleBackColor = false;
        // 
        // BtnStock
        // 
        BtnStock.BackColor = Color.FromArgb(27, 43, 63);
        BtnStock.FlatAppearance.BorderSize = 0;
        BtnStock.FlatStyle = FlatStyle.Flat;
        BtnStock.Font = new Font("Segoe UI", 10F);
        BtnStock.ForeColor = Color.FromArgb(235, 241, 249);
        BtnStock.Location = new Point(3, 123);
        BtnStock.Name = "BtnStock";
        BtnStock.Size = new Size(222, 50);
        BtnStock.TabIndex = 2;
        BtnStock.Tag = "Stok Yönetimi";
        BtnStock.Text = "  ◇    Stok Yönetimi";
        BtnStock.TextAlign = ContentAlignment.MiddleLeft;
        BtnStock.UseVisualStyleBackColor = false;
        // 
        // BtnProducts
        // 
        BtnProducts.BackColor = Color.FromArgb(27, 43, 63);
        BtnProducts.FlatAppearance.BorderSize = 0;
        BtnProducts.FlatStyle = FlatStyle.Flat;
        BtnProducts.Font = new Font("Segoe UI", 10F);
        BtnProducts.ForeColor = Color.FromArgb(235, 241, 249);
        BtnProducts.Location = new Point(3, 179);
        BtnProducts.Name = "BtnProducts";
        BtnProducts.Size = new Size(222, 50);
        BtnProducts.TabIndex = 3;
        BtnProducts.Tag = "Ürün Yönetimi";
        BtnProducts.Text = "  ♢    Ürün Yönetimi";
        BtnProducts.TextAlign = ContentAlignment.MiddleLeft;
        BtnProducts.UseVisualStyleBackColor = false;
        // 
        // BtnCategories
        // 
        BtnCategories.BackColor = Color.FromArgb(27, 43, 63);
        BtnCategories.FlatAppearance.BorderSize = 0;
        BtnCategories.FlatStyle = FlatStyle.Flat;
        BtnCategories.Font = new Font("Segoe UI", 10F);
        BtnCategories.ForeColor = Color.FromArgb(235, 241, 249);
        BtnCategories.Location = new Point(3, 235);
        BtnCategories.Name = "BtnCategories";
        BtnCategories.Size = new Size(222, 50);
        BtnCategories.TabIndex = 4;
        BtnCategories.Tag = "Kategori Yönetimi";
        BtnCategories.Text = "  ▦    Kategori Yönetimi";
        BtnCategories.TextAlign = ContentAlignment.MiddleLeft;
        BtnCategories.UseVisualStyleBackColor = false;
        // 
        // BtnReports
        // 
        BtnReports.BackColor = Color.FromArgb(27, 43, 63);
        BtnReports.FlatAppearance.BorderSize = 0;
        BtnReports.FlatStyle = FlatStyle.Flat;
        BtnReports.Font = new Font("Segoe UI", 10F);
        BtnReports.ForeColor = Color.FromArgb(235, 241, 249);
        BtnReports.Location = new Point(3, 291);
        BtnReports.Name = "BtnReports";
        BtnReports.Size = new Size(222, 50);
        BtnReports.TabIndex = 5;
        BtnReports.Tag = "Raporlar";
        BtnReports.Text = "  ▥    Raporlar";
        BtnReports.TextAlign = ContentAlignment.MiddleLeft;
        BtnReports.UseVisualStyleBackColor = false;
        // 
        // BtnDebt
        // 
        BtnDebt.BackColor = Color.FromArgb(27, 43, 63);
        BtnDebt.FlatAppearance.BorderSize = 0;
        BtnDebt.FlatStyle = FlatStyle.Flat;
        BtnDebt.Font = new Font("Segoe UI", 10F);
        BtnDebt.ForeColor = Color.FromArgb(235, 241, 249);
        BtnDebt.Location = new Point(3, 347);
        BtnDebt.Name = "BtnDebt";
        BtnDebt.Size = new Size(222, 50);
        BtnDebt.TabIndex = 6;
        BtnDebt.Tag = "Borç Takibi";
        BtnDebt.Text = "  ₺    Borç Takibi";
        BtnDebt.TextAlign = ContentAlignment.MiddleLeft;
        BtnDebt.UseVisualStyleBackColor = false;
        // 
        // BtnReturns
        // 
        BtnReturns.BackColor = Color.FromArgb(27, 43, 63);
        BtnReturns.FlatAppearance.BorderSize = 0;
        BtnReturns.FlatStyle = FlatStyle.Flat;
        BtnReturns.Font = new Font("Segoe UI", 10F);
        BtnReturns.ForeColor = Color.FromArgb(235, 241, 249);
        BtnReturns.Location = new Point(3, 403);
        BtnReturns.Name = "BtnReturns";
        BtnReturns.Size = new Size(222, 50);
        BtnReturns.TabIndex = 7;
        BtnReturns.Tag = "İade / Değişim";
        BtnReturns.Text = "  ↶    İade / Değişim";
        BtnReturns.TextAlign = ContentAlignment.MiddleLeft;
        BtnReturns.UseVisualStyleBackColor = false;
        // 
        // BtnSettings
        // 
        BtnSettings.BackColor = Color.FromArgb(27, 43, 63);
        BtnSettings.FlatAppearance.BorderSize = 0;
        BtnSettings.FlatStyle = FlatStyle.Flat;
        BtnSettings.Font = new Font("Segoe UI", 10F);
        BtnSettings.ForeColor = Color.FromArgb(235, 241, 249);
        BtnSettings.Location = new Point(3, 459);
        BtnSettings.Name = "BtnSettings";
        BtnSettings.Size = new Size(222, 50);
        BtnSettings.TabIndex = 8;
        BtnSettings.Tag = "Ayarlar";
        BtnSettings.Text = "  ⚙    Ayarlar";
        BtnSettings.TextAlign = ContentAlignment.MiddleLeft;
        BtnSettings.UseVisualStyleBackColor = false;
        // 
        // BtnBackup
        // 
        BtnBackup.BackColor = Color.FromArgb(27, 43, 63);
        BtnBackup.FlatAppearance.BorderSize = 0;
        BtnBackup.FlatStyle = FlatStyle.Flat;
        BtnBackup.Font = new Font("Segoe UI", 10F);
        BtnBackup.ForeColor = Color.FromArgb(235, 241, 249);
        BtnBackup.Location = new Point(3, 515);
        BtnBackup.Name = "BtnBackup";
        BtnBackup.Size = new Size(222, 50);
        BtnBackup.TabIndex = 9;
        BtnBackup.Tag = "Yedekleme";
        BtnBackup.Text = "  ▤    Yedekleme";
        BtnBackup.TextAlign = ContentAlignment.MiddleLeft;
        BtnBackup.UseVisualStyleBackColor = false;
        // 
        // BtnAbout
        // 
        BtnAbout.BackColor = Color.FromArgb(27, 43, 63);
        BtnAbout.FlatAppearance.BorderSize = 0;
        BtnAbout.FlatStyle = FlatStyle.Flat;
        BtnAbout.Font = new Font("Segoe UI", 10F);
        BtnAbout.ForeColor = Color.FromArgb(235, 241, 249);
        BtnAbout.Location = new Point(3, 571);
        BtnAbout.Name = "BtnAbout";
        BtnAbout.Size = new Size(222, 50);
        BtnAbout.TabIndex = 10;
        BtnAbout.Tag = "Hakkında";
        BtnAbout.Text = "  ⓘ    Hakkında";
        BtnAbout.TextAlign = ContentAlignment.MiddleLeft;
        BtnAbout.UseVisualStyleBackColor = false;
        // 
        // BtnExit
        // 
        BtnExit.BackColor = Color.FromArgb(27, 43, 63);
        BtnExit.FlatAppearance.BorderSize = 0;
        BtnExit.FlatStyle = FlatStyle.Flat;
        BtnExit.Font = new Font("Segoe UI", 10F);
        BtnExit.ForeColor = Color.FromArgb(235, 241, 249);
        BtnExit.Location = new Point(3, 627);
        BtnExit.Name = "BtnExit";
        BtnExit.Size = new Size(222, 50);
        BtnExit.TabIndex = 11;
        BtnExit.Tag = "Çıkış";
        BtnExit.Text = "  \u23fb    Çıkış";
        BtnExit.TextAlign = ContentAlignment.MiddleLeft;
        BtnExit.UseVisualStyleBackColor = false;
        // 
        // pnlBrand
        // 
        pnlBrand.Controls.Add(lblBrandSubtitle);
        pnlBrand.Controls.Add(lblBrand);
        pnlBrand.Dock = DockStyle.Top;
        pnlBrand.Location = new Point(10, 8);
        pnlBrand.Name = "pnlBrand";
        pnlBrand.Size = new Size(194, 82);
        pnlBrand.TabIndex = 1;
        // 
        // lblBrandSubtitle
        // 
        lblBrandSubtitle.AutoSize = true;
        lblBrandSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblBrandSubtitle.ForeColor = Color.White;
        lblBrandSubtitle.Location = new Point(10, 44);
        lblBrandSubtitle.Name = "lblBrandSubtitle";
        lblBrandSubtitle.Size = new Size(203, 20);
        lblBrandSubtitle.TabIndex = 0;
        lblBrandSubtitle.Text = "Mağaza Otomasyon Sistemi";
        // 
        // lblBrand
        // 
        lblBrand.AutoSize = true;
        lblBrand.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblBrand.ForeColor = Color.White;
        lblBrand.Location = new Point(8, 4);
        lblBrand.Name = "lblBrand";
        lblBrand.Size = new Size(189, 35);
        lblBrand.TabIndex = 1;
        lblBrand.Text = "♧  İMAJ GİYİM";
        // 
        // PnlHeader
        // 
        PnlHeader.BackColor = Color.White;
        PnlHeader.Controls.Add(LblTarih);
        PnlHeader.Controls.Add(LblSayfaBaslik);
        PnlHeader.Dock = DockStyle.Top;
        PnlHeader.Location = new Point(214, 0);
        PnlHeader.Name = "PnlHeader";
        PnlHeader.Padding = new Padding(20, 0, 24, 0);
        PnlHeader.Size = new Size(1286, 62);
        PnlHeader.TabIndex = 2;
        // 
        // LblTarih
        // 
        LblTarih.Dock = DockStyle.Right;
        LblTarih.Font = new Font("Segoe UI", 10F);
        LblTarih.ForeColor = Color.FromArgb(31, 41, 55);
        LblTarih.Location = new Point(902, 0);
        LblTarih.Name = "LblTarih";
        LblTarih.Size = new Size(360, 62);
        LblTarih.TabIndex = 0;
        LblTarih.Text = "4 Ekim 2026     ◷  17:16";
        LblTarih.TextAlign = ContentAlignment.MiddleRight;
        // 
        // LblSayfaBaslik
        // 
        LblSayfaBaslik.Dock = DockStyle.Left;
        LblSayfaBaslik.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        LblSayfaBaslik.ForeColor = Color.FromArgb(30, 76, 145);
        LblSayfaBaslik.Location = new Point(20, 0);
        LblSayfaBaslik.Name = "LblSayfaBaslik";
        LblSayfaBaslik.Size = new Size(300, 62);
        LblSayfaBaslik.TabIndex = 1;
        LblSayfaBaslik.Text = "☰   İMAJ GİYİM";
        LblSayfaBaslik.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // PnlFooter
        // 
        PnlFooter.BackColor = Color.White;
        PnlFooter.Controls.Add(lblFooterStatus);
        PnlFooter.Dock = DockStyle.Bottom;
        PnlFooter.Location = new Point(214, 802);
        PnlFooter.Name = "PnlFooter";
        PnlFooter.Size = new Size(1286, 98);
        PnlFooter.TabIndex = 1;
        // 
        // lblFooterStatus
        // 
        lblFooterStatus.Dock = DockStyle.Right;
        lblFooterStatus.Font = new Font("Segoe UI", 8F);
        lblFooterStatus.ForeColor = Color.SeaGreen;
        lblFooterStatus.Location = new Point(956, 0);
        lblFooterStatus.Name = "lblFooterStatus";
        lblFooterStatus.Size = new Size(330, 98);
        lblFooterStatus.TabIndex = 0;
        lblFooterStatus.Text = "● Veritabanı: Magaza.db     |     Sürüm: 1.0.0";
        lblFooterStatus.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // PnlContainer
        // 
        PnlContainer.BackColor = Color.FromArgb(245, 247, 250);
        PnlContainer.Controls.Add(pnlDesignCards);
        PnlContainer.Controls.Add(pnlDesignColumns);
        PnlContainer.Dock = DockStyle.Fill;
        PnlContainer.Location = new Point(214, 62);
        PnlContainer.Name = "PnlContainer";
        PnlContainer.Padding = new Padding(10, 6, 10, 6);
        PnlContainer.Size = new Size(1286, 740);
        PnlContainer.TabIndex = 0;
        // 
        // pnlDesignCards
        // 
        pnlDesignCards.BackColor = Color.FromArgb(245, 247, 250);
        pnlDesignCards.Controls.Add(lblCardTodaySales);
        pnlDesignCards.Controls.Add(lblCardMonthSales);
        pnlDesignCards.Controls.Add(lblCardStock);
        pnlDesignCards.Controls.Add(lblCardProfit);
        pnlDesignCards.Dock = DockStyle.Top;
        pnlDesignCards.Location = new Point(10, 6);
        pnlDesignCards.Name = "pnlDesignCards";
        pnlDesignCards.Size = new Size(1266, 92);
        pnlDesignCards.TabIndex = 0;
        pnlDesignCards.WrapContents = false;
        // 
        // lblCardTodaySales
        // 
        lblCardTodaySales.BackColor = Color.White;
        lblCardTodaySales.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblCardTodaySales.ForeColor = Color.FromArgb(37, 99, 235);
        lblCardTodaySales.Location = new Point(0, 0);
        lblCardTodaySales.Margin = new Padding(0, 0, 12, 8);
        lblCardTodaySales.Name = "lblCardTodaySales";
        lblCardTodaySales.Size = new Size(205, 90);
        lblCardTodaySales.TabIndex = 0;
        lblCardTodaySales.Text = "\U0001f6d2  Bugünkü Satış\n0,00 ₺";
        lblCardTodaySales.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblCardMonthSales
        // 
        lblCardMonthSales.BackColor = Color.White;
        lblCardMonthSales.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblCardMonthSales.ForeColor = Color.FromArgb(37, 99, 235);
        lblCardMonthSales.Location = new Point(217, 0);
        lblCardMonthSales.Margin = new Padding(0, 0, 12, 8);
        lblCardMonthSales.Name = "lblCardMonthSales";
        lblCardMonthSales.Size = new Size(205, 90);
        lblCardMonthSales.TabIndex = 1;
        lblCardMonthSales.Text = "↗  Bu Ayki Satış\n0,00 ₺";
        lblCardMonthSales.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblCardStock
        // 
        lblCardStock.BackColor = Color.White;
        lblCardStock.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblCardStock.ForeColor = Color.FromArgb(37, 99, 235);
        lblCardStock.Location = new Point(434, 0);
        lblCardStock.Margin = new Padding(0, 0, 12, 8);
        lblCardStock.Name = "lblCardStock";
        lblCardStock.Size = new Size(205, 90);
        lblCardStock.TabIndex = 2;
        lblCardStock.Text = "◇  Toplam Stok\n0 adet";
        lblCardStock.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblCardProfit
        // 
        lblCardProfit.BackColor = Color.White;
        lblCardProfit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblCardProfit.ForeColor = Color.FromArgb(37, 99, 235);
        lblCardProfit.Location = new Point(651, 0);
        lblCardProfit.Margin = new Padding(0, 0, 12, 8);
        lblCardProfit.Name = "lblCardProfit";
        lblCardProfit.Size = new Size(205, 90);
        lblCardProfit.TabIndex = 3;
        lblCardProfit.Text = "₺  Tahmini Kâr\n0,00 ₺";
        lblCardProfit.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlDesignColumns
        // 
        pnlDesignColumns.BackColor = Color.FromArgb(245, 247, 250);
        pnlDesignColumns.ColumnCount = 2;
        pnlDesignColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61F));
        pnlDesignColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39F));
        pnlDesignColumns.Controls.Add(pnlDesignQuickSale, 0, 0);
        pnlDesignColumns.Controls.Add(pnlDesignReports, 1, 0);
        pnlDesignColumns.Dock = DockStyle.Fill;
        pnlDesignColumns.Location = new Point(10, 6);
        pnlDesignColumns.Name = "pnlDesignColumns";
        pnlDesignColumns.RowCount = 1;
        pnlDesignColumns.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        pnlDesignColumns.Size = new Size(1266, 728);
        pnlDesignColumns.TabIndex = 1;
        // 
        // pnlDesignQuickSale
        // 
        pnlDesignQuickSale.BackColor = Color.White;
        pnlDesignQuickSale.Controls.Add(lblQuickSaleTitle);
        pnlDesignQuickSale.Controls.Add(lblQuickSaleFields);
        pnlDesignQuickSale.Controls.Add(gridDesignCart);
        pnlDesignQuickSale.Controls.Add(lblFinishSale);
        pnlDesignQuickSale.Dock = DockStyle.Fill;
        pnlDesignQuickSale.Location = new Point(0, 0);
        pnlDesignQuickSale.Margin = new Padding(0, 0, 8, 0);
        pnlDesignQuickSale.Name = "pnlDesignQuickSale";
        pnlDesignQuickSale.Size = new Size(764, 728);
        pnlDesignQuickSale.TabIndex = 0;
        // 
        // lblQuickSaleTitle
        // 
        lblQuickSaleTitle.Dock = DockStyle.Top;
        lblQuickSaleTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblQuickSaleTitle.ForeColor = Color.FromArgb(30, 69, 124);
        lblQuickSaleTitle.Location = new Point(0, 92);
        lblQuickSaleTitle.Name = "lblQuickSaleTitle";
        lblQuickSaleTitle.Padding = new Padding(14, 6, 0, 0);
        lblQuickSaleTitle.Size = new Size(764, 34);
        lblQuickSaleTitle.TabIndex = 0;
        lblQuickSaleTitle.Text = "Hızlı Satış";
        // 
        // lblQuickSaleFields
        // 
        lblQuickSaleFields.Dock = DockStyle.Top;
        lblQuickSaleFields.ForeColor = Color.DimGray;
        lblQuickSaleFields.Location = new Point(0, 0);
        lblQuickSaleFields.Name = "lblQuickSaleFields";
        lblQuickSaleFields.Padding = new Padding(14, 8, 0, 0);
        lblQuickSaleFields.Size = new Size(764, 92);
        lblQuickSaleFields.TabIndex = 1;
        lblQuickSaleFields.Text = "Ürün seçin                 Müşteri\nAdet       Birim fiyat ₺       Tahsil edilen ₺";
        // 
        // gridDesignCart
        // 
        gridDesignCart.AllowUserToAddRows = false;
        gridDesignCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridDesignCart.BackgroundColor = Color.White;
        gridDesignCart.BorderStyle = BorderStyle.None;
        gridDesignCart.ColumnHeadersHeight = 29;
        gridDesignCart.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
        gridDesignCart.Dock = DockStyle.Fill;
        gridDesignCart.Location = new Point(0, 0);
        gridDesignCart.Name = "gridDesignCart";
        gridDesignCart.ReadOnly = true;
        gridDesignCart.RowHeadersVisible = false;
        gridDesignCart.RowHeadersWidth = 51;
        gridDesignCart.Size = new Size(764, 679);
        gridDesignCart.TabIndex = 2;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.HeaderText = "Ürün";
        dataGridViewTextBoxColumn1.MinimumWidth = 6;
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.HeaderText = "Adet";
        dataGridViewTextBoxColumn2.MinimumWidth = 6;
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        dataGridViewTextBoxColumn2.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn3
        // 
        dataGridViewTextBoxColumn3.HeaderText = "Birim Fiyat";
        dataGridViewTextBoxColumn3.MinimumWidth = 6;
        dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        dataGridViewTextBoxColumn3.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewTextBoxColumn4.HeaderText = "Toplam";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        dataGridViewTextBoxColumn4.ReadOnly = true;
        // 
        // lblFinishSale
        // 
        lblFinishSale.BackColor = Color.FromArgb(42, 158, 81);
        lblFinishSale.Dock = DockStyle.Bottom;
        lblFinishSale.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblFinishSale.ForeColor = Color.White;
        lblFinishSale.Location = new Point(0, 679);
        lblFinishSale.Name = "lblFinishSale";
        lblFinishSale.Size = new Size(764, 49);
        lblFinishSale.TabIndex = 3;
        lblFinishSale.Text = "✓  Satışı Tamamla";
        lblFinishSale.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlDesignReports
        // 
        pnlDesignReports.ColumnCount = 1;
        pnlDesignReports.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        pnlDesignReports.Controls.Add(pnlDesignBestSellers, 0, 0);
        pnlDesignReports.Controls.Add(pnlDesignRecentSales, 0, 1);
        pnlDesignReports.Controls.Add(pnlDesignLowStock, 0, 2);
        pnlDesignReports.Dock = DockStyle.Fill;
        pnlDesignReports.Location = new Point(775, 3);
        pnlDesignReports.Name = "pnlDesignReports";
        pnlDesignReports.Padding = new Padding(8, 0, 0, 0);
        pnlDesignReports.RowCount = 3;
        pnlDesignReports.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
        pnlDesignReports.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
        pnlDesignReports.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
        pnlDesignReports.Size = new Size(488, 722);
        pnlDesignReports.TabIndex = 1;
        // 
        // pnlDesignBestSellers
        // 
        pnlDesignBestSellers.BackColor = Color.White;
        pnlDesignBestSellers.Controls.Add(lblBestSellersTitle);
        pnlDesignBestSellers.Dock = DockStyle.Fill;
        pnlDesignBestSellers.Location = new Point(8, 0);
        pnlDesignBestSellers.Margin = new Padding(0, 0, 0, 9);
        pnlDesignBestSellers.Name = "pnlDesignBestSellers";
        pnlDesignBestSellers.Size = new Size(480, 236);
        pnlDesignBestSellers.TabIndex = 0;
        // 
        // lblBestSellersTitle
        // 
        lblBestSellersTitle.Dock = DockStyle.Top;
        lblBestSellersTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblBestSellersTitle.ForeColor = Color.FromArgb(30, 69, 124);
        lblBestSellersTitle.Location = new Point(0, 0);
        lblBestSellersTitle.Name = "lblBestSellersTitle";
        lblBestSellersTitle.Padding = new Padding(10, 5, 0, 0);
        lblBestSellersTitle.Size = new Size(480, 29);
        lblBestSellersTitle.TabIndex = 0;
        lblBestSellersTitle.Text = "En Çok Satan Ürünler (Bu Ay)";
        // 
        // pnlDesignRecentSales
        // 
        pnlDesignRecentSales.BackColor = Color.White;
        pnlDesignRecentSales.Controls.Add(lblRecentSalesTitle);
        pnlDesignRecentSales.Dock = DockStyle.Fill;
        pnlDesignRecentSales.Location = new Point(8, 245);
        pnlDesignRecentSales.Margin = new Padding(0, 0, 0, 9);
        pnlDesignRecentSales.Name = "pnlDesignRecentSales";
        pnlDesignRecentSales.Size = new Size(480, 229);
        pnlDesignRecentSales.TabIndex = 1;
        // 
        // lblRecentSalesTitle
        // 
        lblRecentSalesTitle.Dock = DockStyle.Top;
        lblRecentSalesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblRecentSalesTitle.ForeColor = Color.FromArgb(30, 69, 124);
        lblRecentSalesTitle.Location = new Point(0, 0);
        lblRecentSalesTitle.Name = "lblRecentSalesTitle";
        lblRecentSalesTitle.Padding = new Padding(10, 5, 0, 0);
        lblRecentSalesTitle.Size = new Size(480, 29);
        lblRecentSalesTitle.TabIndex = 0;
        lblRecentSalesTitle.Text = "Son Yapılan Satışlar";
        // 
        // pnlDesignLowStock
        // 
        pnlDesignLowStock.BackColor = Color.White;
        pnlDesignLowStock.Controls.Add(lblLowStockTitle);
        pnlDesignLowStock.Dock = DockStyle.Fill;
        pnlDesignLowStock.Location = new Point(8, 483);
        pnlDesignLowStock.Margin = new Padding(0, 0, 0, 9);
        pnlDesignLowStock.Name = "pnlDesignLowStock";
        pnlDesignLowStock.Size = new Size(480, 230);
        pnlDesignLowStock.TabIndex = 2;
        // 
        // lblLowStockTitle
        // 
        lblLowStockTitle.Dock = DockStyle.Top;
        lblLowStockTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblLowStockTitle.ForeColor = Color.FromArgb(30, 69, 124);
        lblLowStockTitle.Location = new Point(0, 0);
        lblLowStockTitle.Name = "lblLowStockTitle";
        lblLowStockTitle.Padding = new Padding(10, 5, 0, 0);
        lblLowStockTitle.Size = new Size(480, 29);
        lblLowStockTitle.TabIndex = 0;
        lblLowStockTitle.Text = "Stok Durumu (Azalan Stoklar)";
        // 
        // timer1
        // 
        timer1.Interval = 1000;
        timer1.Tick += timer1_Tick;
        // 
        // FrmAna
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 250);
        ClientSize = new Size(1500, 900);
        Controls.Add(PnlContainer);
        Controls.Add(PnlFooter);
        Controls.Add(PnlHeader);
        Controls.Add(pnlMenu);
        MinimumSize = new Size(1180, 760);
        Name = "FrmAna";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "İmaj Giyim | Mağaza Yönetimi";
        WindowState = FormWindowState.Maximized;
        Load += FrmAna_Load;
        pnlMenu.ResumeLayout(false);
        NavPanel.ResumeLayout(false);
        pnlBrand.ResumeLayout(false);
        pnlBrand.PerformLayout();
        PnlHeader.ResumeLayout(false);
        PnlFooter.ResumeLayout(false);
        PnlContainer.ResumeLayout(false);
        pnlDesignCards.ResumeLayout(false);
        pnlDesignColumns.ResumeLayout(false);
        pnlDesignQuickSale.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridDesignCart).EndInit();
        pnlDesignReports.ResumeLayout(false);
        pnlDesignBestSellers.ResumeLayout(false);
        pnlDesignRecentSales.ResumeLayout(false);
        pnlDesignLowStock.ResumeLayout(false);
        ResumeLayout(false);
    }

    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1 = null!;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2 = null!;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3 = null!;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4 = null!;
}
