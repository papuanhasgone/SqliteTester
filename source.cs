using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Data.SQLite;
using System.Globalization;
using System.Windows.Forms;

[assembly: AssemblyTitle("SqliteTester")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace SqliteTester {
  internal sealed class AssemblyInfo {
    internal Type a;
    internal AssemblyInfo() { a = typeof(Program); }

    internal String Title {
      get {
        return ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(
            a.Assembly, typeof(AssemblyTitleAttribute)
        )).Title;
      }
    }

    internal String MyPath {
      get {
        return Path.GetDirectoryName(
          new Uri(a.Assembly.CodeBase).LocalPath
        );
      }
    }
  } // AssemblyInfo

  internal sealed class frmMain : Form {
    public frmMain() {
      InitializeComponent();
      this.Text = new AssemblyInfo().Title;
    }

    private ToolStrip        tsStrip;
    private ToolStripLabel   tsLabel;
    private ToolStripTextBox tsPrice;
    private ToolStripButton  tsSumDo;
    private DataGridView     dgvData;

    private static readonly String me = new AssemblyInfo().MyPath;
    private static readonly String db = Path.Combine(me, "test.db");

    private SQLiteConnection sqlcon = null;
    private SQLiteCommand    sqlcmd = null;
    private SQLiteDataReader sqldbr = null;

    private void InitializeComponent() {
      this.tsStrip = new ToolStrip();
      this.tsLabel = new ToolStripLabel();
      this.tsPrice = new ToolStripTextBox();
      this.tsSumDo = new ToolStripButton();
      this.dgvData = new DataGridView();
      //
      // tsStrip
      //
      this.tsStrip.Items.AddRange(new ToolStripItem[] {
        this.tsLabel, this.tsPrice, this.tsSumDo
      });
      //
      // tsLabel
      //
      this.tsLabel.Text = "Цена для выборки:";
      //
      // tsPrice
      //
      this.tsPrice.BorderStyle = BorderStyle.FixedSingle;
      this.tsPrice.Size = new Size(210, 23);
      this.tsPrice.TextBoxTextAlign = HorizontalAlignment.Right;
      //
      // tsSumDo
      //
      this.tsSumDo.Text = "Расчитать";
      this.tsSumDo.Click += (s, e) => {
        Int32  idp;
        String itm;
        Single dis, res, tmp, dis_par = 0;

        if (String.IsNullOrEmpty(tsPrice.Text)) {
          ShowErrMessage("Не указана начальная сумма расчета.");
          return;
        }

        if (!Single.TryParse(tsPrice.Text, out res)) {
          ShowErrMessage("Указанное значение суммы имеет недопустимый формат.");
          return;
        }

        DataGridViewSelectedRowCollection cur = dgvData.SelectedRows;
        if (!Int32.TryParse(cur[0].Cells[1].Value.ToString(), out idp)) {
          ShowErrMessage("Невозожно привести к целочисленному типу.");
          return;
        }

        itm = cur[0].Cells[2].Value.ToString();

        if (!Single.TryParse(cur[0].Cells[3].Value.ToString(), out dis)) {
          ShowErrMessage("Невозможно привести к вещественному типу.");
          return;
        }

        if (0 != idp) { // вверх по дереву, вплоть до корня
          while (0 != idp) {
            DataGridViewRow par = dgvData.Rows.Cast<DataGridViewRow>()
            .Where(x => x.Cells[0].Value.ToString().Equals(idp.ToString(
              CultureInfo.InvariantCulture
            ))).First();

            if (!Int32.TryParse(par.Cells[1].Value.ToString(), out idp)) {
              ShowErrMessage("Невозможно привести к целочисленному типу.");
              return;
            }

            if (!Single.TryParse(par.Cells[3].Value.ToString(), out tmp)) {
              ShowErrMessage("Невозможно привести к вещественному типу.");
              return;
            }

            dis_par += tmp;
          }
        }
        // собственно, расчет суммы по формуле
        tmp = res; // цена
        res = res - res * ((dis + dis_par) / 100);

        ShowErrMessage(res.ToString(CultureInfo.InvariantCulture));
        // запись итогов в таблицу тестов
        String req = String.Format(
          CultureInfo.InvariantCulture,
          "INSERT INTO tests (choose, discount, discount_parent, price, result) " +
          "VALUES (\"{0}\", {1}, {2}, {3}, {4})", itm, dis, dis_par, tmp, res
        );

        try {
          sqlcon = new SQLiteConnection("Data Source=" + db + ";Version=3;");
          sqlcmd = new SQLiteCommand(req, sqlcon);

          sqlcon.Open();
          sqlcmd.ExecuteNonQuery();
        }
        catch (SQLiteException se) { ShowErrMessage(se.Message); }
        finally {
          if (null != sqlcmd) sqlcmd.Dispose();
          if (null != sqlcon) sqlcon.Dispose();
        }
      };
      //
      // dgvData
      //
      this.dgvData.AllowUserToAddRows = false;
      this.dgvData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
      this.dgvData.Dock = DockStyle.Fill;
      this.dgvData.EditMode = DataGridViewEditMode.EditProgrammatically;
      this.dgvData.MultiSelect = false;
      this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dgvData.CellClick += (s, e) => { tsPrice.Text = String.Empty; };
      //
      // frmMain
      //
      this.ClientSize = new Size(643, 150);
      this.Controls.AddRange(new Control[] {
        this.dgvData, this.tsStrip
      });
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Load += (s, e) => {
        String sql = Path.Combine(me, "init.sql");

        if (!File.Exists(db)) {
          if (!File.Exists(sql)) {
            ShowErrMessage("Сценарий SQL не найден.");
            return;
          }

          SQLiteConnection.CreateFile(db);
          try {
            sqlcon = new SQLiteConnection("Data Source=" + db + ";Version=3;");
            sqlcmd = new SQLiteCommand(File.ReadAllText(sql), sqlcon);

            sqlcon.Open();
            sqlcmd.ExecuteNonQuery();
          }
          catch (SQLiteException se) { ShowErrMessage(se.Message); }
          finally {
            if (null != sqlcmd) sqlcmd.Dispose();
            if (null != sqlcon) sqlcon.Dispose();
          }

          JustShowTable();
        }
        else JustShowTable();
      };
    }

    private void ShowErrMessage(String err) {
      MessageBox.Show(
        err, this.Text, MessageBoxButtons.OK,
        MessageBoxIcon.Exclamation,
        MessageBoxDefaultButton.Button1, (MessageBoxOptions)0
      );
    }

    private void JustShowTable() {
      try {
        sqlcon = new SQLiteConnection("Data Source=" + db + ";Version=3;");
        sqlcmd = new SQLiteCommand("SELECT * FROM dummy;", sqlcon);
        
        sqlcon.Open();
        sqldbr = sqlcmd.ExecuteReader();
        
        DataTable dt = new DataTable();
        dt.Locale = CultureInfo.InvariantCulture;
        dt.Load(sqldbr);
        dgvData.DataSource = dt;
      }
      catch (SQLiteException se) { ShowErrMessage(se.Message); }
      finally {
        if (null != sqldbr) sqldbr.Dispose();
        if (null != sqlcmd) sqlcmd.Dispose();
        if (null != sqlcon) sqlcon.Dispose();
      }
    }
  } // frmMain

  internal sealed class Program {
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.Run(new frmMain());
    }
  } // Program
}
