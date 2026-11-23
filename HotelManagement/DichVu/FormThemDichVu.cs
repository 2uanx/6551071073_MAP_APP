using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.DichVu
{
    public partial class FormThemDichVu : Form
    {
        private static readonly string[] LoaiMacDinh = { "Thức uống", "Đồ ăn", "Dịch vụ" };

        public FormThemDichVu()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AcceptButton = btnThemMoi;
            CancelButton = btnThoat;
        }

        private void FormThemDichVu_Load(object? sender, EventArgs e)
        {
            txtMaDV.MaxLength = 5;
            txtTenDV.MaxLength = 20;

            try
            {
                using var db = new HotelManagementContext();

                var dsLoai = new HashSet<string>(LoaiMacDinh);
                var loaiTrongDb = db.DichVus
                    .AsNoTracking()
                    .Where(d => d.DaXoa != true && !string.IsNullOrEmpty(d.LoaiDv))
                    .Select(d => d.LoaiDv)
                    .Distinct()
                    .ToList();

                foreach (var ten in loaiTrongDb)
                {
                    if (!string.IsNullOrWhiteSpace(ten))
                        dsLoai.Add(ten);
                }

                cbLoaiDV.Items.Clear();
                cbLoaiDV.Items.AddRange(dsLoai.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải loại dịch vụ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cbLoaiDV.Items.Clear();
                cbLoaiDV.Items.AddRange(LoaiMacDinh);
            }

            if (cbLoaiDV.Items.Count > 0)
                cbLoaiDV.SelectedIndex = 0;

            txtSoLuongConLai.Text = "-1";
            txtMaDV.Focus();
        }

        private void btnThemMoi_Click(object sender, EventArgs e)
        {
            string maDV = txtMaDV.Text.Trim().ToUpper();
            string tenDV = txtTenDV.Text.Trim();
            string donGiaText = txtDonGia.Text.Trim();
            string soLuongText = txtSoLuongConLai.Text.Trim();
            string loaiDV = cbLoaiDV.Text.Trim();

            if (maDV.Length == 0)
            {
                MessageBox.Show("Vui lòng nhập mã dịch vụ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaDV.Focus();
                return;
            }

            if (tenDV.Length == 0)
            {
                MessageBox.Show("Vui lòng nhập tên dịch vụ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenDV.Focus();
                return;
            }

            if (loaiDV.Length == 0)
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập loại dịch vụ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbLoaiDV.Focus();
                return;
            }

            if (!decimal.TryParse(donGiaText, out decimal donGia) || donGia <= 0)
            {
                MessageBox.Show("Đơn giá không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGia.Focus();
                return;
            }

            int soLuongConLai = -1;
            if (soLuongText.Length > 0 && (!int.TryParse(soLuongText, out soLuongConLai) || soLuongConLai < 0))
            {
                MessageBox.Show("Số lượng còn lại không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuongConLai.Focus();
                return;
            }

            try
            {
                using var db = new HotelManagementContext();

                var daCo = db.DichVus.FirstOrDefault(d => d.MaDv == maDV);

                if (daCo != null)
                {
                    if (daCo.DaXoa != true)
                    {
                        MessageBox.Show("Mã dịch vụ đã tồn tại. Vui lòng nhập mã khác.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtMaDV.Focus();
                        return;
                    }

                    var traLoi = MessageBox.Show(
                        $"Mã {maDV} từng thuộc dịch vụ \"{daCo.TenDv}\" đã bị xóa.\nBạn có muốn khôi phục mã này với thông tin vừa nhập không?",
                        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (traLoi != DialogResult.Yes)
                    {
                        txtMaDV.Focus();
                        return;
                    }

                    daCo.TenDv = tenDV;
                    daCo.DonGia = donGia;
                    daCo.SlconLai = soLuongConLai;
                    daCo.LoaiDv = loaiDV;
                    daCo.DaXoa = false;
                }
                else
                {
                    db.DichVus.Add(new Models.DichVu
                    {
                        MaDv = maDV,
                        TenDv = tenDV,
                        DonGia = donGia,
                        SlconLai = soLuongConLai,
                        LoaiDv = loaiDV,
                        DaXoa = false
                    });
                }

                int rows = db.SaveChanges();
                if (rows > 0)
                {
                    MessageBox.Show("Thêm dịch vụ thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Thêm thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message), "Lỗi khi thêm",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi khi thêm", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtDonGia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txtSoLuongConLai_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
