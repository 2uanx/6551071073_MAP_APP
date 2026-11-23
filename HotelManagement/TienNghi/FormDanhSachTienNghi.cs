using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.TienNghi
{
    public partial class FormDanhSachTienNghi : Form
    {
        private readonly Image _iconTienNghi = Properties.Resources.details;
        private readonly Image _iconSua = Properties.Resources.edit;
        private readonly Image _iconXoa = Properties.Resources.delete;

        public FormDanhSachTienNghi()
        {
            InitializeComponent();
        }

        private void FormDanhSachTienNghi_Load(object? sender, EventArgs e)
        {
            CauHinhLuoi();
            LoadDanhSachTienNghi();
        }

        private void CauHinhLuoi()
        {
            if (dtgDanhSachTienNghi.Columns.Count > 0)
                return;

            dtgDanhSachTienNghi.ReadOnly = true;
            dtgDanhSachTienNghi.AllowUserToAddRows = false;
            dtgDanhSachTienNghi.AllowUserToDeleteRows = false;
            dtgDanhSachTienNghi.AllowUserToResizeRows = false;
            dtgDanhSachTienNghi.MultiSelect = false;
            dtgDanhSachTienNghi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgDanhSachTienNghi.RowHeadersVisible = false;
            dtgDanhSachTienNghi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgDanhSachTienNghi.RowTemplate.Height = 40;

            dtgDanhSachTienNghi.Columns.Add(TaoCotAnh("colIcon", "", _iconTienNghi, 50));
            dtgDanhSachTienNghi.Columns.Add("MaTN", "Mã tiện nghi");
            dtgDanhSachTienNghi.Columns.Add("TenTN", "Tên tiện nghi");
            dtgDanhSachTienNghi.Columns.Add(TaoCotAnh("Sua", "Cập nhật", _iconSua, 70));
            dtgDanhSachTienNghi.Columns.Add(TaoCotAnh("Xoa", "Xóa", _iconXoa, 50));
        }

        private DataGridViewImageColumn TaoCotAnh(string ten, string tieuDe, Image anh, int doRong)
        {
            return new DataGridViewImageColumn
            {
                Name = ten,
                HeaderText = tieuDe,
                Image = anh,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = doRong,
                Resizable = DataGridViewTriState.False,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void LoadDanhSachTienNghi(string keyword = "")
        {
            try
            {
                using var db = new HotelManagementContext();

                var query = db.TienNghis
                    .AsNoTracking()
                    .Where(t => t.DaXoa != true);

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim();
                    query = query.Where(t =>
                        t.MaTn.Contains(keyword) ||
                        t.TenTn.Contains(keyword));
                }

                var dsTienNghi = query
                    .OrderBy(t => t.MaTn)
                    .Select(t => new
                    {
                        t.MaTn,
                        t.TenTn
                    })
                    .ToList();

                dtgDanhSachTienNghi.Rows.Clear();

                foreach (var tn in dsTienNghi)
                {
                    dtgDanhSachTienNghi.Rows.Add(
                        _iconTienNghi,
                        tn.MaTn,
                        tn.TenTn,
                        _iconSua,
                        _iconXoa
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            var cacDong = dtgDanhSachTienNghi.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Visible && !r.IsNewRow)
                .ToList();

            if (cacDong.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var hopLuu = new SaveFileDialog())
            {
                hopLuu.Title = "Xuất danh sách tiện nghi";
                hopLuu.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                hopLuu.FileName = $"DanhSachTienNghi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                if (hopLuu.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    using (var wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("Danh sách tiện nghi");
                        string[] tenCot = { "MaTN", "TenTN" };

                        for (int c = 0; c < tenCot.Length; c++)
                            ws.Cell(1, c + 1).Value = dtgDanhSachTienNghi.Columns[tenCot[c]].HeaderText;
                        ws.Row(1).Style.Font.Bold = true;

                        for (int i = 0; i < cacDong.Count; i++)
                        {
                            for (int c = 0; c < tenCot.Length; c++)
                            {
                                var o = ws.Cell(i + 2, c + 1);
                                o.Value = cacDong[i].Cells[tenCot[c]].Value?.ToString() ?? "";
                            }
                        }

                        ws.Columns().AdjustToContents();
                        wb.SaveAs(hopLuu.FileName);
                    }

                    var traLoi = MessageBox.Show("Xuất dữ liệu thành công!\nBạn có muốn mở file ngay không?", "Thông báo",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (traLoi == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(
                            new System.Diagnostics.ProcessStartInfo(hopLuu.FileName) { UseShellExecute = true });
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Không ghi được file. Có thể file đang được mở trong Excel, hãy đóng lại rồi thử lần nữa.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            using (FormThemTienNghi f = new FormThemTienNghi())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
                }
            }
        }

        private void dtgDanhSachTienNghi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string tenCot = dtgDanhSachTienNghi.Columns[e.ColumnIndex].Name;
            if (tenCot != "Sua" && tenCot != "Xoa")
                return;

            string? maTN = dtgDanhSachTienNghi.Rows[e.RowIndex].Cells["MaTN"].Value?.ToString();
            if (string.IsNullOrEmpty(maTN))
                return;

            if (tenCot == "Sua")
            {
                using (FormSuaTienNghi f = new FormSuaTienNghi(maTN))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
                    }
                }
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa tiện nghi này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using var db = new HotelManagementContext();

                var tienNghi = db.TienNghis.FirstOrDefault(t => t.MaTn == maTN);
                if (tienNghi == null)
                {
                    MessageBox.Show("Không tìm thấy tiện nghi cần xóa!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    tienNghi.DaXoa = true;
                    int kq = db.SaveChanges();

                    if (kq > 0)
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tiện nghi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadDanhSachTienNghi(txtTimKiem.Text.Trim());
        }
    }
}
